using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FooReflection.Domain.Dto.User;
using Microsoft.AspNetCore.Mvc;

namespace FooReflection.Web.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ApiController
    {
        [HttpGet("{id}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public async Task<UserDto> Get(int id)
        {
            try
            {
                return await Task.FromResult(new UserDto
                {
                    Id  = id, 
                    Username = "Username"
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        [HttpGet]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public async Task<UserListDto> GetAll()
        {
            try
            {
                var usersDto = new UserListDto
                    {
                        Users = new List<UserDto>
                        {
                            new UserDto
                            {
                                Id = 1,
                                Username = "Admin"
                            },
                            new UserDto
                            {
                                Id = 2,
                                Username = "Guest"
                            }
                        },
                        Page = 1,
                        PageSize = 10,
                        PageSizeTotal = 2
                    };
                return await Task.FromResult(usersDto);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        [HttpPost]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public async Task<bool> Create([FromBody] UserDto userDto)
        {
            try
            {
                return await Task.FromResult(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        [HttpPatch]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public async Task<bool> Patch([FromBody] UserDto userDto)
        {
            try
            {
                return await Task.FromResult(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        [HttpDelete("{id}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public async Task<bool> Delete()
        {
            try
            {
                return await Task.FromResult(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}