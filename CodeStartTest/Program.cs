using System.Text;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace CodeStartTest
{
    internal class Program
    {
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
            //System.Collections.Specialized.StringCollection assemblies = new System.Collections.Specialized.StringCollection();
            
            //починить
            var newCSC = codeDomProvider.CompileAssemblyFromSource(newParameters, code);
            var go = result.CompiledAssembly.GetType("Test.TestClass").GetMethod("Main").Invoke(null, null);
            Console.WriteLine("\n\n\n\nТЕСТ\n\n" + go.ToString() + "\n\n\n\nТЕСТ\n\n\n");


            if (String.IsNullOrEmpty(code))
            {
                Warnings.AppendLine("program text cannot be null or empty");
            }
            foreach (CompilerError error in newCSC.Errors)
            {
                Warnings.AppendLine(String.Format(warningMessageFormat, error.Line, error.ErrorNumber, error.ErrorText));
            }
            Console.WriteLine(code);
            Console.WriteLine(Warnings.ToString());
            return newCSC;
        }

        static void Main(string[] args)
        {
            string code = @"E:\analys\code.txt";
            Console.WriteLine(CodeStart(code));
            
        }
    }
}