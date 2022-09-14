using Microsoft.ALTA.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Microsoft.ALTA
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
            LoadAllAssemblies();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
        }

        public void LoadAllAssemblies()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblies = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
            foreach (var assemblyName in assemblies)
            {
                try
                {
                    var assembly = InitializeAssembly(Assembly.LoadFrom(assemblyName));
                    var assemblyShortName = Path.GetFileName(assemblyName).Split(".dll")[0];
                    var testClasses = GetTestClasses(assembly);
                    foreach (var testClass in testClasses)
                    {
                        string instanceNameKey = assemblyShortName + testClass.FullName;
                        ALTAController.instances[instanceNameKey] = InitializeClass(testClass);
                        InitializeTest(testClass, ALTAController.instances[instanceNameKey]);
                        RegisterMethods(testClass, ALTAController.instances[instanceNameKey], assemblyShortName);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Failed to load assembly {assemblyName}");
                }
            }
        }

        public static List<Type> GetTestClasses(Assembly assembly)
        {
            return assembly.GetTypes().Where(x => x.IsDefined(typeof(TestClassAttribute))).ToList();
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


        private static Assembly InitializeAssembly(Assembly assembly)
        {
            var assemblyType = assembly.GetTypes().FirstOrDefault(y => y.IsDefined(typeof(AssemblyInitializeAttribute)));

            if (assemblyType != null)
            {
                var initializeAssembly = assemblyType.GetMethods().FirstOrDefault(x => x.GetCustomAttributes<AssemblyInitializeAttribute>().Any());
                initializeAssembly.Invoke(assemblyType, new object[] { null });
            }
            return assembly;
        }

        private static void InitializeTest(Type t, object testInstance)
        {
            var initializeTest = t.GetMethods().FirstOrDefault(x => x.GetCustomAttributes<TestInitializeAttribute>().Any());
            if (initializeTest != null)
            {
                if(initializeTest.GetParameters().Count() > 0)
                {
                    initializeTest.Invoke(testInstance, new object[] { null });
                }
                else
                {
                    initializeTest.Invoke(testInstance, null);
                }
            }
        }

        private static void RegisterMethods(Type t, object testInstance, string assemblyName)
        {
            var methods = t.GetMethods().Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false).Length > 0)
                      .ToArray();
            foreach(var method in methods)
            {
                ALTAController.methods[assemblyName + t.FullName + method.Name] = method;
            }
        }

    }
}