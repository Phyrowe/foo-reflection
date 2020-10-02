using FooReflection.Domain.Dto.Common;
using FooReflection.Domain.Enum.Common;

namespace FooReflection.Domain.Dto.Role
{
    public class RoleDto
    {
        public int Id { get; set; }
        public RoleType Type { get; set; }
        public string Name { get; set; }
        public DateTimeInfoDto DateTimeInfo { get; set; }
    }
}