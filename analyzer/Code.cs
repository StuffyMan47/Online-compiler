using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
	public class Code
	{
		public string codeString;
		private CSharpSyntaxTree syntaxTree;
		public string[] strArrayOfOutput;
		//предупреждения статического анализатора
		public StringBuilder Warnings { get; } = new StringBuilder();
		//Стрингбилдер для коллекции ошибок компиляции
		public StringBuilder CompilationErrors { get; } = new StringBuilder();
		const string compilationErrorsMessageFormat = "Line {0} Error No:{1} -  {2}";

		public Code(string codeString)
		{
			this.codeString = codeString;
			//проверка на наличие текста в переменной code
			if (String.IsNullOrEmpty(codeString))
			{
				//переделать под вывод алёрта о незаполненном текстовом поле
				CompilationErrors.AppendLine("program text cannot be null or empty");
			}

			this.syntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(this.codeString);
		}

		/// <summary>
		/// Функция выполняет компиляцию кода, заполняет CompilationErrors ошибками компиляции
		/// </summary>
		/// <param name="code">C# в формате текста</param>
		/// <returns>Результат компиляции</returns>
		//public CompilerResults CodeCompilation(string code)
		public void CodeCompilation(string code)
		{
			//Запуск компиляции
			CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
			var Parameters = new CompilerParameters
			{
				GenerateInMemory = true,
				IncludeDebugInformation = true
			};
			Parameters.GenerateExecutable = false;
			//var newParameters = new CompilerParameters();
			//System.Collections.Specialized.StringCollection assemblies = new System.Collections.Specialized.StringCollection();
			var compilationResult = codeDomProvider.CompileAssemblyFromSource(Parameters, code);
			var output = compilationResult.Output;
			this.strArrayOfOutput = new string[output.Count];
			output.CopyTo(this.strArrayOfOutput, 0);

			//Запись всех ошибок компиляции
			foreach (CompilerError error in compilationResult.Errors)
			{
				CompilationErrors.AppendLine(String.Format(compilationErrorsMessageFormat, error.Line, error.ErrorNumber, error.ErrorText));
			}
			//return compilationResult;
		}

		public void StaticAnalyzer(string code)
		{
			StartIfWalker(this.syntaxTree);
			StartMethodWalker(this.syntaxTree);
		}

		/// <summary>
		/// вспомогательная функция для запуска правил анализа в условным операторе if
		/// </summary>
		/// <param name="syntaxTree">Код в формате CSharpSyntaxTree</param>
		private void StartIfWalker(CSharpSyntaxTree syntaxTree)
		{
			var ifWalker = new IfWalker();

			ifWalker.Warnings.Clear();
			ifWalker.Visit(syntaxTree.GetRoot());

			var warningsIf = ifWalker.Warnings;
			if (warningsIf.Length != 0) 
				Warnings.AppendLine(warningsIf.ToString());
		}

		/// <summary>
		/// вспомогательная функция для запуска правил анализа в методах
		/// </summary>
		/// <param name="syntaxTree">Код в формате CSharpSyntaxTree</param>
		private void StartMethodWalker(CSharpSyntaxTree syntaxTree)
		{
			var methodWalker = new MethodWalker();

			methodWalker.Warnings.Clear();
			methodWalker.Visit(syntaxTree.GetRoot());

			var warningsMethod = methodWalker.Warnings;
			if (warningsMethod.Length != 0)
				Warnings.AppendLine(warningsMethod.ToString());
		}
	}
}
