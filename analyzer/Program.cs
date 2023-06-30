using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Runtime.Remoting.Messaging;
using Analyzer;
using System.Runtime.Loader;
using System.Reflection.Metadata;

namespace analyzer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string solutionPath = @"E:\Analyzer\analyzer\TestAnalyzator\TestAnalyzator.sln";
            string logPath = @"E:\analys\warnings.txt";
            string inputPath = @"E:\analys\Input.txt";
            string code = @"E:\analys\code.txt";


            MSBuildLocator.RegisterDefaults();
            using (var workspace = MSBuildWorkspace.Create())
            {
                Project project = GetProjectFromSolution(solutionPath, workspace);

                ////Ввод данных в методы анализа из json файла
                //CSharpSyntaxTree jsonTest = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(json);
                //Console.WriteLine("jsonTest.GetRoot():\n" + jsonTest.GetRoot() + "\n----------------------------------------------------------");
                //Console.WriteLine("jsonTest.GetRoot() type:\n" + jsonTest.GetRoot().GetType());
                //var ifWalkerTestJson = new IfWalker();
                //StartWalker(ifWalkerTestJson, jsonTest.GetRoot());
                //var warningsTestJson = ifWalkerTestJson.Warnings;
                //if (warningsTestJson.Length != 0)
                //    File.AppendAllText(logPath, warningsTestJson.ToString());

                //тестовая запись ошибок из .txt в лог файл
                ////////////////////////////////////////////////////////////////////File.AppendAllText(logPath, Anal(code));

                //Console.WriteLine("///////////////\n"+messages+"\n/////////////////////////////");
                //File.AppendAllText(logPath, compilerResults.ToString());

                ////////////////////////////////////////////////////////////////////Console.WriteLine(("----------------------------\n"+CodeStart(code)+"\n-----------------------------"));
				//Не работает
				//File.AppendAllText(logPath, Anal1(inputPath));

				//Проверка решения (всех файлов проекта с помощью файла .sln)
				foreach (var document in project.Documents)
                {
                    var tree = document.GetSyntaxTreeAsync().Result;
                    var ifWalker = new IfWalker();
                    StartWalker(ifWalker, tree.GetRoot());

                   var warningsIf = ifWalker.Warnings;
                    if (warningsIf.Length != 0)
                        File.AppendAllText(logPath,warningsIf.ToString());

                    var MethodWalker = new MethodWalker();
                    StartWalker(MethodWalker, tree.GetRoot());

                    var warningsMethod = MethodWalker.Warnings;
                    if (warningsMethod.Length != 0)
                        File.AppendAllText(logPath, warningsMethod.ToString());
                }

				string[] trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);

				List<PortableExecutableReference> references = new List<PortableExecutableReference>();

				foreach (var refAsm in trustedAssembliesPaths)
				{
					references.Add(MetadataReference.CreateFromFile(refAsm));
				}

				var compilation = CSharpCompilation.Create("a")
					.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
					.AddReferences(references)
					.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
						@"
                        using System;
 
                        public static class C
                        {
                            public static void M(ff)
                            {
                                Console.WriteLine(""Hello Roslyn."");
                            }
                        }
                    "));

				var fileName = "MyNewDLL.dll";

				var result = compilation.Emit(fileName);

				if (!result.Success)
				{
					foreach (var diag in result.Diagnostics)
						Console.WriteLine(diag);
					return;
				}

				var MyNewDLL = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(fileName));

				MyNewDLL.GetType("C").GetMethod("M").Invoke(null, null);

				Console.ReadLine();
			}
		}

        public static StringBuilder Warnings { get; } = new StringBuilder();

        const string warningMessageFormat = "Line {0} Error No:{1} -  {2}";

        public static CompilerResults CodeStart(string inputPath)
        {
            //Как по видео
            var path = @"E:\analys\result.exe";
            var csc = new CSharpCodeProvider();
            var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, path, true);
            parameters.GenerateExecutable = true;

            var code = File.ReadAllText(inputPath);
            var result = csc.CompileAssemblyFromSource(parameters, code);

            //как на сайте
            CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
            var newParameters1 = new CompilerParameters
            {
                GenerateInMemory = true,
                IncludeDebugInformation = true
            };
            newParameters1.GenerateExecutable = false;
            var newParameters = new CompilerParameters();
            System.Collections.Specialized.StringCollection assemblies = new System.Collections.Specialized.StringCollection();
            
            //починить
            var newCSC = codeDomProvider.CompileAssemblyFromSource(newParameters, code);
            //var instance = result.CompiledAssembly.CreateInstance("Test.TestClass");
            //Console.WriteLine("instance:\n" + instance.ToString()+ "\n/instance\n");
            //var go = result.CompiledAssembly.GetType("Test.TestClass").GetMethod("Sum").Invoke(null, null);
            //var go1 = newCSC.CompiledAssembly.GetType("Test.TestClass").GetMethods();
            //foreach (var item in go1)
            {
                //Console.WriteLine(item.ToString());
            }
            //Console.WriteLine("\n\n\n\nТЕСТ\n\n" + go.ToString() + "\n\n\n\nТЕСТ\n\n\n");
            //Console.WriteLine("\n\n\n\nТЕСТ1\n\n" + go1 + "\n\n\n\nТЕСТ1\n\n\n");


            if (String.IsNullOrEmpty(code))
            {
                Warnings.AppendLine("program text cannot be null or empty");
            }
            foreach (CompilerError error in result.Errors)
            {
                Warnings.AppendLine(String.Format(warningMessageFormat, error.Line, error.ErrorNumber, error.ErrorText));
            }
            Console.WriteLine(code);
            Console.WriteLine(Warnings.ToString());
            return result;
        }

        public static CompilerResults ProcessCompilation(string inputPath)
        {
            string programText = ReadFile(inputPath);
            CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            System.Collections.Specialized.StringCollection assemblies = new System.Collections.Specialized.StringCollection();
            return codeDomProvider.CompileAssemblyFromSource(parameters, programText);
        }


        //метод для чтения файлов с кодом из решения проекта .sln 
        static Project GetProjectFromSolution(string solutionPath, MSBuildWorkspace workspace)
        {
            Solution currSolution = workspace.OpenSolutionAsync(solutionPath).Result;
            return currSolution.Projects.Single();
        }

        //метод прохода проверки правил по узлам кода if
        public static void StartWalker(IfWalker ifWalker, SyntaxNode syntaxNode)
        {
            ifWalker.Warnings.Clear();
            ifWalker.Visit(syntaxNode);
        }

        //метод прохода проверки правил по узлам кода Method
        public static void StartWalker(MethodWalker methodWalker, SyntaxNode syntaxNode)
        {
            methodWalker.Warnings.Clear();
            methodWalker.Visit(syntaxNode);
        }

        //метод для чтение файлов, которые содержат код для анализа
        public static string ReadFile(string filePath)
        {
            using (StreamReader file = new StreamReader(filePath))
            {
                string fileString = file.ReadToEnd();
                return fileString;
            }
        }

        //метод для анализа кода в формате string
        public static string Anal(string filePath)
        {
            string code = ReadFile(filePath);
            CSharpSyntaxTree MethodTest = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(code);
            var ifWalkerTestMethod = new IfWalker();
            StartWalker(ifWalkerTestMethod, MethodTest.GetRoot());
            var warningsTestMethod = ifWalkerTestMethod.Warnings;
            if (warningsTestMethod.Length != 0)
                return warningsTestMethod.ToString();
            else
                return "Ошибок нет";
        }

        //Не работает 
        public static string Anal1(string filePath)
        {
            string code = ReadFile(filePath);
            CSharpSyntaxTree MethodTest = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(code);
            var methodWalker = new MethodWalker();
            StartWalker(methodWalker, MethodTest.GetRoot());
            var warningsMethodTest = methodWalker.Warnings;
            if (warningsMethodTest.Length != 0)
                return warningsMethodTest.ToString();
            else return "Ошибок нет";
        }
    }
}
