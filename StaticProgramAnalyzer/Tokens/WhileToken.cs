using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace StaticProgramAnalyzer.Tokens
{
    internal class WhileToken : StatementToken
    {
        public WhileToken(IToken parent, ParserToken source) : base(parent)
        {
            Source = source;
        }

        public string VariableName { get; internal set; }
        public List<StatementToken> StatementList { get; internal set; }
        public ParserToken Source { get; }

        public override IEnumerable<IToken> GetChildren()
        {
            return StatementList.Concat(
                StatementList.SelectMany(x => x.GetChildren()));
        }
        public override string ToString()
        {
            return Source.LineNumber.ToString();
        }
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
