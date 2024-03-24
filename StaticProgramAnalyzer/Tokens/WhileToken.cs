using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace StaticProgramAnalyzer.Tokens
{
    internal class WhileToken : StatementToken
    {
        public string VariableName { get; internal set; }
        public List<StatementToken> StatementList { get; internal set; }
        //public override string ToString()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine($"while_{VariableName}(while) --> stmtLst_while_{VariableName}(stmtLst)");
        //    foreach (var statement in StatementList)
        //    {
        //        sb.AppendLine(statement.ToString());
        //    }
        //    return sb.ToString();
        //}
    }
}
