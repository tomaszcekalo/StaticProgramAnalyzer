using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class AssignToken : StatementToken
    {
        public string VariableName { get; internal set; }
        public string FakeExpression { get; internal set; }

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
