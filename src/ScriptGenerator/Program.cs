using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ScriptGeneratorApplication
{
    class JsonTest // contains all inputs for the tests in the json file provided
    {
        public string endpoint { get; set; }
        public string method { get; set; }
        public string className { get; set; }
        public string nameSpace { get; set; }
        public List<string> parameters { get; set; }
        public int weight { get; set; }
        public int ramp_time { get; set; }
        public int duration { get; set; }
        public int delay { get; set; }

        public string[] splitURL(JsonTest test, JsonScript json) // takes in endpoint, method, class, assembly name, and parameters to make corresponding url 
        {
            // create parameter string
            string parString = string.Empty;
            foreach (string param in test.parameters)
            {
                parString += $"&query={param}";
            }

            // format URL
            string url = $@"{test.endpoint}/api/ALTA?methodName={test.method}&assemblyName={json.script_name}&className={test.nameSpace}.{test.className}{parString}";

            // split into domain and path
            string[] url_parts = url.Split(new[] { '/' }, 2);
            string domain = url_parts[0];
            string path = (url_parts.Length > 1) ? url_parts[1] : string.Empty;
            string[] result = { domain, path};
            return result;
        }
    }
    class JsonScript // input json file
    {
        public string script_name { get; set; }
        public int total_threads { get; set; }
        public JsonTest[] tests {get ; set; }

    }

    class ScriptCreator
    {
        public JsonScript JsonCreator(string path, string endpoint, int numThreads)
        {
            // load the .dll file
            string dll = path.Split(@"\").Last();

            Assembly testAssembly = Assembly.LoadFile(path);

            // load tests into json test array
            List<JsonTest> tests = new List<JsonTest>();

            // loop through all classes in dll
            foreach (Type type in testAssembly.GetExportedTypes())
            {
                   // loop through all tests in class
                   MethodInfo[] methodInfos = type.GetMethods();
                   foreach (MethodInfo method in methodInfos)
                   {
                        // only add test methods to json
                        object[] attrs = method.GetCustomAttributes(true);
                        foreach (object attr in attrs)
                        {
                            if (attr is TestMethodAttribute)
                            {
                                JsonTest newTest = new JsonTest();

                                ParameterInfo[] pars = method.GetParameters();
                                string[] parInput = new string[pars.Length];

                                // ask users for parameter inputs
                                for (int i = 0; i < pars.Length; i++)
                                {
                                    Console.WriteLine($"Enter value for {pars[i].Name} in {method.Name}");
                                    parInput[i] = Console.ReadLine();
                                }

                                // populate json with test info
                                newTest.endpoint = endpoint;
                                newTest.nameSpace = type.Namespace;
                                newTest.weight = 1; // default values
                                newTest.ramp_time = 0;
                                newTest.delay = 0;
                                newTest.duration = 60;

                                newTest.className = type.Name;
                                newTest.method = method.Name;
                                newTest.parameters = new List<string>();

                                foreach (string value in parInput)
                                {
                                    newTest.parameters.Add(value);
                                }

                                tests.Add(newTest);
                            }
                        }
                    }
            }

            JsonScript script = new JsonScript();

            script.script_name = dll.Remove(dll.Length - 4);
            script.total_threads = numThreads;
            script.tests = tests.ToArray();

            return script;
        }

        public List<XDocument> JmxScriptCreator(JsonScript json)
        {
            List<XDocument> result = new List<XDocument>();

            JsonTest[] tests = json.tests;
            int weightTotal = 0;

            foreach (JsonTest test in tests)
            {
                weightTotal += test.weight;
            }

            foreach (JsonTest test in tests)
            {
                // creates an individual jmeter script for each test
                XDocument script = new XDocument(
                    new XElement("jmeterTestPlan",
                        new XAttribute("version", "1.2"),
                        new XAttribute("properties", "5.0"),
                        new XAttribute("jmeter", "5.4.1"),
                        new XElement("hashTree",
                            new XElement("TestPlan",
                                new XAttribute("guiclass", "TestPlanGui"),
                                new XAttribute("testclass", "TestPlan"),
                                new XAttribute("testname", json.script_name),
                                new XAttribute("enabled", "true"),
                                new XElement("stringProp",
                                    new XAttribute("name", "TestPlan.comments"),
                                    string.Empty),
                                new XElement("boolProp",
                                    new XAttribute("name", "TestPlan.functional_mode"),
                                    "false"),
                                new XElement("boolProp",
                                    new XAttribute("name", "TestPlan.tearDown_on_shutdown"),
                                    "true"),
                                new XElement("boolProp",
                                    new XAttribute("name", "TestPlan.serialize_threadgroups"),
                                    "false"),
                                new XElement("elementProp",
                                    new XAttribute("name", "TestPlan.user_defined_variables"),
                                    new XAttribute("elementType", "Arguments"),
                                    new XAttribute("guiclass", "ArgumentsPanel"),
                                    new XAttribute("testclass", "Arguments"),
                                    new XAttribute("testname", "User Defined Variables"),
                                    new XAttribute("enabled", "true"),
                                    new XElement("collectionProp",
                                        new XAttribute("name", "Arguments.arguments")
                                    )
                                ),
                                new XElement("stringProp",
                                    new XAttribute("name", "TestPlan.user_define_classpath"), string.Empty)),
                            new XElement("hashTree"))));

            XElement hashTree = script.Elements("jmeterTestPlan").Elements("hashTree").Elements("hashTree").First();
            string[] urlParts = test.splitURL(test, json);
            int currentWeight = test.weight;

            hashTree.Add(new XElement("ThreadGroup",
                            new XAttribute("guiclass", "ThreadGroupGui"),
                            new XAttribute("testclass", "ThreadGroup"),
                            new XAttribute("testname", test.method),
                            new XAttribute("enabled", "true"),
                            new XElement("stringProp",
                                new XAttribute("name", "ThreadGroup.on_sample_error"),
                                "continue"),
                            new XElement("elementProp",
                                new XAttribute("name", "ThreadGroup.main_controller"),
                                new XAttribute("elementType", "LoopController"),
                                new XAttribute("guiclass", "LoopControlPanel"),
                                new XAttribute("testclass", "LoopController"),
                                new XAttribute("testname", "Loop Controller"),
                                new XAttribute("enabled", "true"),
                                new XElement("boolProp",
                                    new XAttribute("name", "LoopController.continue_forever"),
                                    "false"),
                                new XElement("intProp",
                                    new XAttribute("name", "LoopController.loops"),
                                    "-1")
                            ),
                            new XElement("stringProp",
                                new XAttribute("name", "ThreadGroup.num_threads"),
                                $"{(json.total_threads / weightTotal) * currentWeight}"), // can be edited in json
                            new XElement("stringProp",
                                new XAttribute("name", "ThreadGroup.ramp_time"),
                                $"{test.ramp_time}"), // can be edited in json
                            new XElement("boolProp",
                                new XAttribute("name", "ThreadGroup.scheduler"),
                                "true"),
                            new XElement("stringProp",
                                new XAttribute("name", "ThreadGroup.duration"),
                                $"{test.duration}"), // can be edited in json
                            new XElement("stringProp",
                                new XAttribute("name", "ThreadGroup.delay"),
                                $"{test.delay}"), // can be edited in json
                            new XElement("boolProp",
                                new XAttribute("name", "ThreadGroup.same_user_on_next_iteration"),
                                "true")));

            hashTree.Add(new XElement("hashTree",
                            new XElement("HTTPSamplerProxy",
                                new XAttribute("guiclass", "HttpTestSampleGui"),
                                new XAttribute("testclass", "HTTPSamplerProxy"),
                                new XAttribute("testname", "HTTP request"),
                                new XAttribute("enabled", "true"),
                                new XElement("elementProp",
                                    new XAttribute("name", "HTTPsampler.Arguments"),
                                    new XAttribute("elementType", "Arguments"),
                                    new XAttribute("guiclass", "HTTPArgumentsPanel"),
                                    new XAttribute("testclass", "Arguments"),
                                    new XAttribute("testname", "User Defined Variables"),
                                    new XAttribute("enabled", "true"),
                                    new XElement("collectionProp",
                                        new XAttribute("name", "Arguments.arguments"))),
                                new XElement("stringProp",
                                    new XAttribute("name", "HTTPSampler.domain"),
                                    urlParts[0]), // user input 
                                new XElement("stringProp",
                                    new XAttribute("name", "HTTPSampler.port"), string.Empty),
                                new XElement("stringProp",
                                    new XAttribute("name", "HTTPSampler.protocol"), string.Empty),
                                new XElement("stringProp",
                                    new XAttribute("name", "HTTPSampler.contentEncoding"), string.Empty),
                                new XElement("stringProp",
                                    new XAttribute("name", "HTTPSampler.path"),
                                    urlParts[1]), // user input
                                new XElement("stringProp",
                                    new XAttribute("name", "HTTPSampler.method"),
                                    "GET"),
                                new XElement("boolProp",
                                    new XAttribute("name", "HTTPSampler.follow_redirects"),
                                    "true"),
                                new XElement("boolProp",
                                    new XAttribute("name", "HTTPSampler.auto_redirects"),
                                    "false"),
                                new XElement("boolProp",
                                    new XAttribute("name", "HTTPSampler.use_keepalive"),
                                    "true"),
                                new XElement("boolProp",
                                    new XAttribute("name", "HTTPSampler.DO_MULTIPART_POST"),
                                    "false"),
                                new XElement("stringProp",
                                    new XAttribute("name", "HTTPSampler.embedded_url_re"),
                                    string.Empty),
                                new XElement("stringProp",
                                    new XAttribute("name", "HTTPSampler.connect_timeout"),
                                    string.Empty),
                                new XElement("stringProp",
                                    new XAttribute("name", "HTTPSampler.response_timeout"),
                                    string.Empty)),
                            new XElement("hashTree")));
            script.Declaration = new XDeclaration("1.0", "UTF-8", "true");
            result.Add(script);
            }

            return result;

        }
    }

    class Program
    {
        static void Main(string[] args) // path to .dll OR .json files
        {
            ScriptCreator creator = new ScriptCreator();
            foreach (string arg in args)
            {
                string input = arg.Trim(new Char[] { '"' });
                if (input.Contains(".json"))
                {
                    string jsonString = File.ReadAllText(input);

                    // convert json string to JsonScript and plug into jmx script generator
                    JsonScript testScript = JsonConvert.DeserializeObject<JsonScript>(jsonString);
                    List<XDocument> generatedScripts = creator.JmxScriptCreator(testScript);

                    // create folder in /bin/Debug to store scripts
                    string path = Directory.GetCurrentDirectory();
                    string folderPath = $@"\{testScript.script_name}";

                    Directory.CreateDirectory($"{path}{folderPath}Scripts");

                    for (int i = 0; i < generatedScripts.Count; i++)
                    {
                        generatedScripts[i].Save($@"{path}{folderPath}\{testScript.tests[i].method}.jmx");
                        string text = File.ReadAllText($@"{path}{folderPath}\{testScript.tests[i].method}.jmx");
                        text = text.Replace("&amp;", "&");
                        File.WriteAllText($@"{path}{folderPath}\{testScript.tests[i].method}.jmx", text);

                    }
                    Console.WriteLine($@"Scripts saved to {path}{folderPath}");

                }
                else if (input.Contains(".dll"))
                {
                    string dllName = input.Split(@"\").Last();
                    string scriptName = dllName.Remove(dllName.Length - 4);

                    // get endpoint, and total number of threads from user input
                    Console.WriteLine("Enter endpoint url:");
                    string endpoint = Console.ReadLine();

                    Console.WriteLine("Enter total number of threads:");
                    int numThreads = int.Parse(Console.ReadLine());

                    // create json and plug it into jmx script generator
                    JsonScript json = creator.JsonCreator(input, endpoint, numThreads);
                    List<XDocument> generatedScripts = creator.JmxScriptCreator(json);

                    // create folder in /bin/Debug/net6.0 to store scripts
                    string path = Directory.GetCurrentDirectory();
                    string folderPath = $@"\{scriptName}Scripts";

                    Directory.CreateDirectory($"{path}{folderPath}");

                    for (int i = 0; i < generatedScripts.Count; i++)
                    {
                        generatedScripts[i].Save($@"{path}{folderPath}\{json.tests[i].method}.jmx");
                        string text = File.ReadAllText($@"{path}{folderPath}\{json.tests[i].method}.jmx");
                        text = text.Replace("&amp;", "&");
                        File.WriteAllText($@"{path}{folderPath}\{json.tests[i].method}.jmx", text);

                    }
                    Console.WriteLine($@"Scripts saved to {path}{folderPath}");

                    // saves json so user can edit later
                    string output = JsonConvert.SerializeObject(json);
                    File.WriteAllText($@"{path}{folderPath}\{json.script_name}.json", output);

                }
                else
                {
                    Console.WriteLine("Invalid path. Please input the path to a .dll or .json file.");
                }
            }
        }
    }
}