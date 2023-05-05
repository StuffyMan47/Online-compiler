using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    internal class MethodWalker: CSharpSyntaxWalker
    {
        public StringBuilder Warnings { get; } = new StringBuilder();

        const string warningMessageFormat = "Method name '{0}' does not start with capital letter " + 
            "at {1} line";

        static bool ApplyRuleUpperChar(MethodDeclarationSyntax node, out string methodName)
        {
            methodName = node.Identifier.Text;
            return methodName.Length != 0 && !char.IsUpper(methodName[0]);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (ApplyRuleUpperChar(node, out string methodName))
            {
                int lineNumber = node.GetLocation()
                    .GetLineSpan()
                    .StartLinePosition.Line + 1;

                Warnings.AppendLine(String.Format(warningMessageFormat, methodName, lineNumber));
            }
            base.VisitMethodDeclaration(node);
        }
    }
}