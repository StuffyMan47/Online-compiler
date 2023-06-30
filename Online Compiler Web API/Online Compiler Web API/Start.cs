using Microsoft.CodeAnalysis;

namespace Online_Compiler_Web_API
{
	public class Start
	{
		public Start() {
			string[] trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);

			List<PortableExecutableReference> references = new List<PortableExecutableReference>();

			foreach (var refAsm in trustedAssembliesPaths)
			{
				references.Add(MetadataReference.CreateFromFile(refAsm));
			}

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
}
