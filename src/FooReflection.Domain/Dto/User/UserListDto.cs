using System.Collections.Generic;

namespace FooReflection.Domain.Dto.User
{
    public class UserListDto
    {
        public UserDto User { get; set; }
        public List<UserDto> Users { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int PageSizeTotal { get; set; }
    }
}