using System.CodeDom.Compiler;

namespace WebSharpCompilerBusiness
{
    public class WebSharpCompiler
    {
        public List<string> Compile(string programText)
        {
            List<string> messages = new List<string>();
            if (String.IsNullOrEmpty(programText))
            {
                messages.Add("program text cannot be null or empty");
            }
            CompilerResults compilerResults = ProcessCompilation(programText);
            foreach (CompilerError error in compilerResults.Errors)
            {
                messages.Add(String.Format("Line {0} Error No:{1} -  {2}", error.Line, error.ErrorNumber, error.ErrorText));
            }

            return messages;
        }

        public CompilerResults ProcessCompilation(string programText)
        {
            CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
            System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            System.Collections.Specialized.StringCollection assemblies = new System.Collections.Specialized.StringCollection();
            return codeDomProvider.CompileAssemblyFromSource(parameters, programText);
        }
    }
}
