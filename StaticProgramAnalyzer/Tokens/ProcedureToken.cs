using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class ProcedureToken
    {
        public string Name { get; internal set; }
        public List<StatementToken> StatementList { get; internal set; }

        //public override string ToString()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine($"procedure_{Name}(procedure {Name}) --> stmtLst{Name}");
        //    foreach (var statement in StatementList)
        //    {
        //        sb.AppendLine(statement.ToString());
        //    }
        //    return sb.ToString();
        //}
    }
}
