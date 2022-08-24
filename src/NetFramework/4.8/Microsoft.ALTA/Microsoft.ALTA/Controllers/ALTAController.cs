namespace Microsoft.ALTA.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [Route("api/[controller]")]
    public class ALTAController : Controller
    {
        private static ConcurrentDictionary<string, Assembly> assemblies = new ConcurrentDictionary<string, Assembly>();
        private static ConcurrentDictionary<string, object> instances = new ConcurrentDictionary<string, object>();
        private static ConcurrentDictionary<string, Type> types = new ConcurrentDictionary<string, Type>();
        private static ConcurrentDictionary<string, MethodInfo> methods = new ConcurrentDictionary<string, MethodInfo>();

        public async Task<ActionResult> RunTest(string methodName, string assemblyName, string className, string[] query = null)
        {
            MethodInfo method;
            Type type;
            object testInstance;
            if (methods.ContainsKey(assemblyName + className + methodName))
            {
                method = methods[assemblyName + className + methodName];
                testInstance = instances[assemblyName + className];
                type = types[assemblyName + className];

                // running the test method
                InitializeTest(type, testInstance);
                await (Task)method.Invoke(testInstance, query);

                return new HttpStatusCodeResult(200);
            }
            else
            {
                Assembly testAssembly;
                if (!assemblies.ContainsKey(assemblyName))
                {
                    testAssembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, $"{assemblyName}.dll"));
                    InitializeAssembly(testAssembly);
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
                    testInstance = InitializeClass(type);
                    instances[assemblyName + className] = testInstance;
                }
                method = type.GetMethod(methodName);
                methods[assemblyName + className + methodName] = method;
                InitializeTest(type, testInstance);

                await (Task)method.Invoke(testInstance, query);

                return new HttpStatusCodeResult(200);
            }
        }

        private static object InitializeClass(Type t)
        {
            object test_Instance = Activator.CreateInstance(t);
            var initializeClass = t.GetMethods().FirstOrDefault(x => x.GetCustomAttributes<ClassInitializeAttribute>().Any());

            if (initializeClass != null)
            {
                initializeClass.Invoke(test_Instance, new object[] { null });
            }

            return test_Instance;
        }

        private static void InitializeAssembly(Assembly assembly)
        {
            var assemblyType = assembly.GetTypes().FirstOrDefault(y => y.IsDefined(typeof(AssemblyInitializeAttribute)));

            if (assemblyType != null)
            {
                var initializeAssembly = assemblyType.GetMethods().FirstOrDefault(x => x.GetCustomAttributes<AssemblyInitializeAttribute>().Any());
                initializeAssembly.Invoke(assemblyType, new object[] { null });
            }
        }

        private static void InitializeTest(Type t, object testInstance)
        {
            var initializeTest = t.GetMethods().FirstOrDefault(x => x.GetCustomAttributes<TestInitializeAttribute>().Any());
            if (initializeTest != null)
            {
                initializeTest.Invoke(testInstance, new object[] { null });
            }
        }
    }
}