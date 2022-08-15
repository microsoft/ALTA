using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Reflection;
using System.Web.Http;

namespace Microsoft.TestEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestEngineController : Controller
    {

        private Type type;
        private object testInstance;
        // path for the assembly test file
        private string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static ConcurrentDictionary<string, Assembly> assemblies = new ConcurrentDictionary<string, Assembly>();
        private static ConcurrentDictionary<string, object> instances = new ConcurrentDictionary<string, object>();
        private static ConcurrentDictionary<string, Type> types = new ConcurrentDictionary<string, Type>();
        private static ConcurrentDictionary<string, MethodInfo> methods = new ConcurrentDictionary<string, MethodInfo>();


        [HttpGet]
        public async Task<ActionResult> runTest(string methodName, string assemblyName, string className, [FromQuery] string[] query = null)
        {

            MethodInfo method;
            if (methods.ContainsKey(assemblyName + className + methodName))
            {
                method = methods[assemblyName + className + methodName];
                testInstance = instances[assemblyName + className];

                // running the test method
                var task = (Task)method.Invoke(testInstance, query);
                await task;

                return StatusCode(200);

            }
            else
            {
                Assembly testAssembly;
                if (!assemblies.ContainsKey(assemblyName))
                {
                    testAssembly = Assembly.LoadFrom(Path.Combine(path, $"{assemblyName}.dll"));
                    assemblies[assemblyName] = testAssembly;
                }
                else
                {
                    testAssembly = assemblies[assemblyName];
                }

                if (types.ContainsKey(assemblyName + className))
                {
                    type = types[assemblyName + className];
                }
                else
                {
                    type = testAssembly.GetType(className);
                    types[assemblyName + className] = type;
                }

                if (instances.ContainsKey(assemblyName + className))
                {
                    testInstance = instances[assemblyName + className];
                }
                else
                {
                    testInstance = InitializeClass(type, testInstance);
                    instances[assemblyName + className] = testInstance;

                }

                method = type.GetMethod(methodName);
                methods[assemblyName + className + methodName] = method;

                var task = (Task)method.Invoke(testInstance, query);
                await task;

                return StatusCode(200);

            }

        }

        private static object InitializeClass(Type t, object test_Instance)
        {
            test_Instance = Activator.CreateInstance(t);
            var initializeClass = t.GetMethods().FirstOrDefault(x => x.GetCustomAttributes<ClassInitializeAttribute>().Any());
            if (initializeClass != null)
            {
                initializeClass.Invoke(test_Instance, new object[] { null });
            }
            return test_Instance;
        }

    }
}
