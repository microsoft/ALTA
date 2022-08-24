using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Microsoft.ALTA.Controllers
{
    [Route("api/[controller]")]
    public class ALTAController : Controller
    {
        private Type type;
        private object testInstance;
        private static ConcurrentDictionary<string, Assembly> assemblies = new ConcurrentDictionary<string, Assembly>();
        private static ConcurrentDictionary<string, object> instances = new ConcurrentDictionary<string, object>();
        private static ConcurrentDictionary<string, Type> types = new ConcurrentDictionary<string, Type>();
        private static ConcurrentDictionary<string, MethodInfo> methods = new ConcurrentDictionary<string, MethodInfo>();


        public async Task<ActionResult> runTest(string methodName, string assemblyName, string className, string[] query = null)
        {
            MethodInfo method;
            
            if (methods.ContainsKey(assemblyName + className + methodName))
            {
                method = methods[assemblyName + className + methodName];
                testInstance = instances[assemblyName + className];

                // running the test method
                var task = (Task)method.Invoke(testInstance, query);
                await task;

                return new HttpStatusCodeResult(200);
            }
            else
            {
                Assembly testAssembly;
                if (!assemblies.ContainsKey(assemblyName))
                {
                   // testAssembly = Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.RelativeSearchPath), $@"Debug\{assemblyName}.dll"));
                    testAssembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, $"{assemblyName}.dll"));
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
                    Type[] typesarr = testAssembly.GetTypes();
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

                return new HttpStatusCodeResult(200);
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