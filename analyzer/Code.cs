using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace Analyzer
{
	public class Code
	{
		public string codeString;
		private CSharpSyntaxTree syntaxTree;
		public string[] strArrayOfOutput;

		//предупреждения статического анализатора
		public StringBuilder Warnings { get; } = new StringBuilder();
		public StringBuilder WarningsRu { get; } = new StringBuilder();

		//ошибоки компиляции
		public StringBuilder CompilationErrors { get; } = new StringBuilder();
		const string compilationErrorsMessageFormat = "Line {0} Error No:{1} -  {2}";

		public Code (string codeString)
		{
			this.codeString = codeString;
			//проверка на наличие текста в переменной code

			this.syntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(this.codeString);
		}

		public void StaticAnalyzer()
		{
			StartIfWalker(this.syntaxTree);
			StartMethodWalker(this.syntaxTree);
		}

		/// <summary>
		/// вспомогательная функция для запуска правил анализа в условном операторе if
		/// </summary>
		/// <param name="syntaxTree">Код в формате CSharpSyntaxTree</param>
		private void StartIfWalker(CSharpSyntaxTree syntaxTree)
		{
			var ifWalker = new IfWalker();

			ifWalker.Warnings.Clear();
			ifWalker.WarningsRu.Clear();
			ifWalker.Visit(syntaxTree.GetRoot());

			var warningsIf = ifWalker.Warnings;
			var warningsIfRu = ifWalker.WarningsRu;
			if (warningsIf.Length != 0)
			{
				Warnings.AppendLine(warningsIf.ToString());
				WarningsRu.AppendLine(warningsIfRu.ToString());
			}
		}

		/// <summary>
		/// вспомогательная функция для запуска правил анализа в методах
		/// </summary>
		/// <param name="syntaxTree">Код в формате CSharpSyntaxTree</param>
		private void StartMethodWalker(CSharpSyntaxTree syntaxTree)
		{
			var methodWalker = new MethodWalker();

			methodWalker.Warnings.Clear();
			methodWalker.WarningsRu.Clear();
			methodWalker.Visit(syntaxTree.GetRoot());

			var warningsMethod = methodWalker.Warnings;
			var warningsMethodRu = methodWalker.WarningsRu;
			if (warningsMethod.Length != 0)
			{
				Warnings.AppendLine(warningsMethod.ToString());
				WarningsRu.AppendLine(warningsMethodRu.ToString());
			}
		}
	}
}
