using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Analyzer;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Runtime.Loader;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using System.IO;
using System;

namespace OnlineCompiler.Pages
{
	[IgnoreAntiforgeryToken]
	public class TestModel : PageModel
	{
		public bool isRu = false;
		public string codeFromePage;
		public string consoleOnPage;
		public string staticAnalyzerWarnings;
		public string staticAnalyzerWarningsRu;
		public string staticAnalyzerWarningsEng;


		public void ChangeLanguage()
		{
			Console.WriteLine("isRu = "+isRu);
			if (isRu == false)
			{
				isRu = true;
				staticAnalyzerWarnings = staticAnalyzerWarningsRu;
			}
			else
			{
				isRu = false;
				staticAnalyzerWarnings = staticAnalyzerWarningsEng;
			}
		}
		public void OnGet()
		{
			codeFromePage = @"using System;
namespace First
{
    public class Program
    {
        public static void Main()
        {
	    int a = 9;
	    int b = 10;
	    int c = a + b;
	    Console.WriteLine(""ВКР Фазлеев Марсель ТРП-2-19"");
	    Console.WriteLine(""c = "" + c);
	    if (c==10)
                Console.WriteLine(""false"");
            else
                Console.WriteLine(""false"");
        }
	public static int sum()
	{
	    int a = 9;
	    int b = 10;
	    return a + b;
	}
    }
}";
		}

		public async Task OnPostAsync(string code2)
		{
			codeFromePage = code2;

			if (string.IsNullOrEmpty(code2))
			{
				consoleOnPage = "Введите код для анализа и компиляции";
			}
			else
			{
				Code codeForAnalyse = new Code(code2);
				codeForAnalyse.StaticAnalyzer();
				staticAnalyzerWarningsEng = codeForAnalyse.Warnings.ToString();
				staticAnalyzerWarningsRu = codeForAnalyse.WarningsRu.ToString();
				staticAnalyzerWarnings = staticAnalyzerWarningsRu;
			}
			//consoleOnPage = model.code2;

			string[] trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);

			List<PortableExecutableReference> references = new List<PortableExecutableReference>();

			foreach (var refAsm in trustedAssembliesPaths)
			{
				references.Add(MetadataReference.CreateFromFile(refAsm));
			}

			Console.WriteLine(code2);
			var compiler = new RoslynCompiler("First.Program", code2, new[] { typeof(Console) });
			var type = compiler.Compile();
			if (!string.IsNullOrEmpty(compiler.error))
				consoleOnPage = compiler.error;

			if (type == null) 
			{
				Console.WriteLine(consoleOnPage);
			}
			else
			{
				TextWriter oldWriter = Console.Out;
				FileStream fs = new FileStream("Test.txt", FileMode.Create);
				// First, save the standard output.
				TextWriter tmp = Console.Out;
				StreamWriter sw = new StreamWriter(fs);
				Console.SetOut(sw);
				type.GetMethod("Main").Invoke(null, null);
				Console.SetOut(oldWriter);
                sw.Close();
                using (StreamReader file = new StreamReader("Test.txt"))
                {
                    string fileString = file.ReadToEnd();
					consoleOnPage = fileString;
                }

			}

		}

		public class RoslynCompiler
		{
			readonly CSharpCompilation _compilation;
			Assembly _generatedAssembly;
			Type? _proxyType;
			string _assemblyName;
			string _typeName;
			public string error;

			public RoslynCompiler(string typeName, string code, Type[] typesToReference)
			{
				_typeName = typeName;
				var refs = typesToReference.Select(h => MetadataReference.CreateFromFile(h.Assembly.Location) as MetadataReference).ToList();

				//some default refeerences
				refs.Add(MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll")));
				refs.Add(MetadataReference.CreateFromFile(typeof(Object).Assembly.Location));

				//generate syntax tree from code and config compilation options
				var syntax = CSharpSyntaxTree.ParseText(code);
				var options = new CSharpCompilationOptions(
					OutputKind.DynamicallyLinkedLibrary,
					allowUnsafe: true,
					optimizationLevel: OptimizationLevel.Release);

				_compilation = CSharpCompilation.Create(_assemblyName = Guid.NewGuid().ToString(), new List<SyntaxTree> { syntax }, refs, options);
			}

			public Type Compile()
			{
				if (_proxyType != null) return _proxyType;

				using (var ms = new MemoryStream())
				{
					var result = _compilation.Emit(ms);
					if (!result.Success)
					{
						var compilationErrors = result.Diagnostics.Where(diagnostic =>
								diagnostic.IsWarningAsError ||
								diagnostic.Severity == DiagnosticSeverity.Error)
							.ToList();
						if (compilationErrors.Any())
						{
							var firstError = compilationErrors.First();
							var lineOfError = firstError.Location.GetLineSpan().StartLinePosition.Line+1;
							var errorNumber = firstError.Id;
							var errorDescription = firstError.GetMessage();
							var firstErrorMessage = $"{errorNumber}: {errorDescription}";
							var exception = $"Compilation failed at {lineOfError} error is: {firstErrorMessage}";
							error = exception;
						}
					}
					else
					{
						ms.Seek(0, SeekOrigin.Begin);

						_generatedAssembly = AssemblyLoadContext.Default.LoadFromStream(ms);

						_proxyType = _generatedAssembly.GetType(_typeName);
					}
					return _proxyType;
					
				}
			}
		}
	}
}
