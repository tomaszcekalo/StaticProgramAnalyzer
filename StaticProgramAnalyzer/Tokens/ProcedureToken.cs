using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class ProcedureToken : IToken
    {
        public string Name { get; internal set; }
        public List<StatementToken> StatementList { get; internal set; }

        public IEnumerable<IToken> GetChildren()
        {
            return StatementList.Concat(
                StatementList.SelectMany(x => x.GetChildren()));
        }
        public override string ToString()
        {
            return Name;
        }

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
