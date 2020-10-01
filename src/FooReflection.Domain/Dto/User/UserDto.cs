using FooReflection.Domain.Dto.Role;
using FooReflection.Domain.Dto.Common;

namespace FooReflection.Domain.Dto.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public RoleDto Role { get; set; }
        public DateTimeInfoDto DateTimeInfo { get; set; }
    }
}