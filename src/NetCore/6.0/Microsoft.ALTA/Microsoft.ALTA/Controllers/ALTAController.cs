namespace Microsoft.ALTA.Controllers
{
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Web.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [Route("api/[controller]")]
    [ApiController]
    public class ALTAController : Controller
    {
        //public static ConcurrentDictionary<string, Assembly> assemblies = new ConcurrentDictionary<string, Assembly>();
        public static ConcurrentDictionary<string, object> instances = new ConcurrentDictionary<string, object>();
        //public static ConcurrentDictionary<string, Type> types = new ConcurrentDictionary<string, Type>();
        public static ConcurrentDictionary<string, MethodInfo> methods = new ConcurrentDictionary<string, MethodInfo>();
        private string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        [HttpGet]
        public async Task<ActionResult> RunTest(string methodName, string assemblyName, string className, [FromQuery] string[] query = null)
        {
            object testInstance;
            MethodInfo method;
            method = methods[assemblyName + className + methodName];
            testInstance = instances[assemblyName + className];
            await (Task)method.Invoke(testInstance, query);
            return this.StatusCode(200);
        }
    }
}
