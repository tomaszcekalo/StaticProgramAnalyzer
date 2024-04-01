using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class AssignToken : StatementToken
    {
        public AssignToken(IToken parent) : base(parent)
        {
        }

        public VariableToken Left;
        public ExpressionToken Right;
        internal HashSet<string> Modifies;
        internal HashSet<string> UsesConstants;
        internal HashSet<string> UsesVariables;
        internal int LineNumber;

        public string VariableName { get; internal set; }
        public string FakeExpression { get; internal set; }

        public override IEnumerable<IToken> GetChildren()
        {
            return new List<IToken>();
        }
        public override string ToString()
        {
            return String.Format("{0}={1}",VariableName, Right.Content);
        }
        //checks if provided tree exists in the assigment tree
        public bool ContainsTree(AssignToken checkTree)
        {
            return Right.Content.Contains(checkTree.Right.Content);
        }
        //checks if the assigment tree equals provided one
        public bool EqualsTree(AssignToken checkTree)
        {
            return Right.Content.Equals(checkTree.Right.Content);
        }

        //public override string ToString()
        //{
        //    var sb= new StringBuilder();
        //    var assignNodeName= $"assign_{VariableName}_{FakeExpression.GetHashCode()}";
        //    sb.AppendLine($"{assignNodeName}(assign)");
        //    sb.AppendLine($"{assignNodeName} --> {assignNodeName}_{VariableName}({VariableName})");

        //    return sb.ToString();
        //}
    }
}
