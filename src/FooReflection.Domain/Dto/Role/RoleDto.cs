using FooReflection.Domain.Dto.Common;

namespace FooReflection.Domain.Dto.Role
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTimeInfoDto DateTimeInfo { get; set; }
    }
}