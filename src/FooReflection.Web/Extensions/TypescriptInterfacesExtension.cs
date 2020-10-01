using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace FooReflection.Web.Extensions
{
    public static class TypeScriptInterfacesExtension
    {
        private static readonly Type[] NonPrimitivesExcludeList = {
                typeof(object),
                typeof(string),
                typeof(decimal),
                typeof(void),
                typeof(System.DateTime)
            };

        private static readonly IDictionary<Type, string> ConvertedTypes = new Dictionary<Type, string>
        {
            [typeof(string)] = "string",
            [typeof(char)] = "string",
            [typeof(byte)] = "number",
            [typeof(sbyte)] = "number",
            [typeof(short)] = "number",
            [typeof(ushort)] = "number",
            [typeof(int)] = "number",
            [typeof(uint)] = "number",
            [typeof(long)] = "number",
            [typeof(ulong)] = "number",
            [typeof(float)] = "number",
            [typeof(double)] = "number",
            [typeof(decimal)] = "number",
            [typeof(bool)] = "boolean",
            [typeof(object)] = "any",
            [typeof(void)] = "void",
            [typeof(DateTime)] = "Date"
        };


        public static void GenerateTypeScriptInterfaces(this IApplicationBuilder app, string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Type[] typesToConvert = GetTypesToConvert(Assembly.GetExecutingAssembly());

            foreach (Type type in typesToConvert)
            {
                var tsType = ConvertCs2Ts(type);
                string fullPath = Path.Combine(path, tsType.Name);

                string directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory ?? string.Empty);
                }

                File.WriteAllLines(fullPath, tsType.Lines);
            }
        }

        private static Type[] GetTypesToConvert(Assembly assembly)
        {
            Type controllerBaseType = typeof(Microsoft.AspNetCore.Mvc.ControllerBase);

            ISet<Type> actionAttributeTypes = new HashSet<Type>()
                {
                    typeof(Microsoft.AspNetCore.Mvc.HttpGetAttribute),
                    typeof(Microsoft.AspNetCore.Mvc.HttpPostAttribute),
                };

            var controllers = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(controllerBaseType));

            var actions = controllers.SelectMany(c => c.GetMethods()
                .Where(m => m.IsPublic &&
                            m.GetCustomAttributes().Any(a => actionAttributeTypes.Contains(a.GetType())))
            );

            var actionTypes = actions.SelectMany(m =>
                new Type[1] { m.ReturnType }.Concat(m.GetParameters().Select(p => p.ParameterType)));

            var types = actionTypes
                .Select(ReplaceByGenericArgument)
                .Where(t => !t.IsPrimitive && !NonPrimitivesExcludeList.Contains(t))
                .Distinct()
                .ToArray();

            var additionalTypes = GetAdditionalTypes(types);
            /*foreach (var type in types)
            {
                additionalTypes.AddRange(GetAdditionalTypes(type));
                additionalTypes.AddRange(type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(s => s.PropertyType)
                    .Select(ReplaceByGenericArgument)
                    .Where(t => !t.IsPrimitive && !NonPrimitivesExcludeList.Contains(t)));
            }*/

            return types.Concat(additionalTypes).ToArray();
        }

        private static IEnumerable<Type> GetAdditionalTypes(IEnumerable<Type> types)
        {
            var additionalTypes = new List<Type>();
            var mergeTypes = new List<Type>();
            foreach (var type in types)
            {
                mergeTypes.AddRange(type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(s => s.PropertyType)
                    .Select(ReplaceByGenericArgument)
                    .Where(t => !t.IsPrimitive && !NonPrimitivesExcludeList.Contains(t)));
            }
            if (mergeTypes.Count == 0)
                return additionalTypes;
            additionalTypes = additionalTypes.Concat(mergeTypes).ToList();
            return additionalTypes.Concat(GetAdditionalTypes(mergeTypes)).ToList();
        }

        private static Type ReplaceByGenericArgument(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (!type.IsConstructedGenericType)
            {
                return type;
            }

            var genericArgument = type.GenericTypeArguments.First();

            var isTask = type.GetGenericTypeDefinition() == typeof(Task<>);
            var isActionResult = type.GetGenericTypeDefinition() == typeof(Microsoft.AspNetCore.Mvc.ActionResult<>);
            var isEnumerable = typeof(IEnumerable<>).MakeGenericType(genericArgument).IsAssignableFrom(type);

            if (!isTask && !isActionResult && !isEnumerable)
            {
                throw new InvalidOperationException();
            }

            if (genericArgument.IsConstructedGenericType)
            {
                return ReplaceByGenericArgument(genericArgument);
            }

            return genericArgument;
        }

        private static (string Name, string[] Lines) ConvertCs2Ts(Type type)
        {
            string filename = $"{type.Namespace?.Replace(".", "/")}/{type.Name}.d.ts";

            Type[] types = GetAllNestedTypes(type);

            var lines = new List<string>();

            foreach (Type t in types)
            {
                lines.Add($"");

                if (t.IsClass || t.IsInterface)
                {
                    ConvertClassOrInterface(lines, t);
                }
                else if (t.IsEnum)
                {
                    ConvertEnum(lines, t);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            return (filename, lines.ToArray());
        }

        private static void ConvertClassOrInterface(IList<string> lines, Type type)
        {
            lines.Add($"export interface {type.Name} {{");

            foreach (var property in type.GetProperties().Where(p => p.GetMethod.IsPublic))
            {
                Type propertyType = property.PropertyType;
                if ((propertyType.IsClass || propertyType.IsInterface)
                    && !propertyType.IsGenericType && !NonPrimitivesExcludeList.Contains(propertyType))
                {
                    string importPath = Regex.Replace(propertyType.Namespace,
                        @"(\w+)|(\.\w+)", "../",
                        RegexOptions.Compiled) + propertyType.Namespace?.Replace(".", "/");
                    string import = $"import {{ {propertyType.Name} }} from '{importPath}/{propertyType.Name}';";
                    if (lines.Contains(import)) continue;
                    lines.Insert(0, import);
                }
                if (propertyType.IsGenericType)
                {
                    foreach (var propertyArguments in propertyType.GetGenericArguments())
                    {
                        string importPath = Regex.Replace(propertyArguments.Namespace,
                            @"(\w+)|(\.\w+)", "../",
                            RegexOptions.Compiled) + propertyArguments.Namespace?.Replace(".", "/");
                        string import = $"import {{ {propertyArguments.Name} }} from '{importPath}/{propertyArguments.Name}';";
                        if (lines.Contains(import)) continue;
                        lines.Insert(0, import);
                    }
                }
                Type arrayType = GetArrayOrEnumerableType(propertyType);
                Type nullableType = GetNullableType(propertyType);

                Type typeToUse = nullableType ?? arrayType ?? propertyType;


                var convertedType = ConvertType(typeToUse);

                string suffix = "";
                suffix = arrayType != null ? "[]" : suffix;
                suffix = nullableType != null ? "|null" : suffix;

                lines.Add($"  {CamelCaseName(property.Name)}: {convertedType}{suffix};");
            }

            lines.Add($"}}");
        }

        private static string ConvertType(Type typeToUse)
        {
            if (ConvertedTypes.ContainsKey(typeToUse))
            {
                return ConvertedTypes[typeToUse];
            }

            if (typeToUse.IsConstructedGenericType &&
                typeToUse.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                var keyType = typeToUse.GenericTypeArguments[0];
                var valueType = typeToUse.GenericTypeArguments[1];
                return $"{{ [key: {ConvertType(keyType)}]: {ConvertType(valueType)} }}";
            }

            return typeToUse.Name;
        }

        private static void ConvertEnum(IList<string> lines, Type type)
        {
            var enumValues = type.GetEnumValues().Cast<int>().ToArray();
            var enumNames = type.GetEnumNames();

            lines.Add($"export enum {type.Name} {{");

            for (int i = 0; i < enumValues.Length; i++)
            {
                lines.Add($"  {enumNames[i]} = {enumValues[i]},");
            }

            lines.Add($"}}");
        }

        private static Type[] GetAllNestedTypes(Type type)
        {
            return new[] { type }
                .Concat(type.GetNestedTypes().SelectMany(GetAllNestedTypes))
                .ToArray();
        }

        private static Type GetArrayOrEnumerableType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type.IsConstructedGenericType)
            {
                Type typeArgument = type.GenericTypeArguments.First();

                if (typeof(IEnumerable<>).MakeGenericType(typeArgument).IsAssignableFrom(type))
                {
                    return typeArgument;
                }
            }

            return null;
        }

        private static Type GetNullableType(Type type)
        {
            if (type.IsConstructedGenericType)
            {
                Type typeArgument = type.GenericTypeArguments.First();

                if (typeArgument.IsValueType &&
                    typeof(Nullable<>).MakeGenericType(typeArgument).IsAssignableFrom(type))
                {
                    return typeArgument;
                }
            }

            return null;
        }

        private static string CamelCaseName(string pascalCaseName)
        {
            if (string.IsNullOrEmpty(pascalCaseName))
                throw new ArgumentNullException(nameof(pascalCaseName));
            return pascalCaseName[0].ToString().ToLower() + pascalCaseName.Substring(1);
        }
    }
}