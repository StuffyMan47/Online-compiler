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

        const string warningMessageFormat = "'if' with equal 'then' and 'else' blocks is found at line {1}";

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
            }

            base.VisitIfStatement(node);
        }
    }
}
