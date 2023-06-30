using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    internal class IfWalker: CSharpSyntaxWalker
    {
        public StringBuilder Warnings { get; } = new StringBuilder();
        public StringBuilder WarningsRu { get; } = new StringBuilder();

        const string warningMessageFormatRu = "В условном операторе на строке {0} одинаковые операции в 'if' и в 'else'";

		const string warningMessageFormat = "'if' with equal 'then' and 'else' blocks is found at line {0}";

        //Метод с проверкой на равенство then и else
        static bool ApplyRuleIfEqualElsr(IfStatementSyntax ifStatement)
        {
            if (ifStatement?.Else == null)
                return false;
            StatementSyntax thenBody = ifStatement.Statement;
            StatementSyntax elseBody = ifStatement.Else.Statement;

            return SyntaxFactory.AreEquivalent(thenBody, elseBody);
        }

        //Метод с записью информации об ошибке в StringBuilder
        public override void VisitIfStatement(IfStatementSyntax node)
        {
            if (ApplyRuleIfEqualElsr(node))
            {
                int lineNumber = node.GetLocation()
                    .GetLineSpan()
                    .StartLinePosition.Line + 1;

                Warnings.AppendLine(String.Format(warningMessageFormat, lineNumber));
				WarningsRu.AppendLine(String.Format(warningMessageFormatRu, lineNumber));
			}

            base.VisitIfStatement(node);
        }
    }
}
