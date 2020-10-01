using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FooReflection.Web.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ValuesController : ApiController
    {
        [HttpGet]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public async Task<bool> Get()
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