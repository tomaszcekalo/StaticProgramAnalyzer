using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class ProcedureToken : IHasProcedureName
    {
        public string ProcedureName { get; set; }
        public List<StatementToken> StatementList { get; set; }
        public ParserToken Source { get; set; }

        public IToken Parent => null;

        public IEnumerable<IToken> GetChildren()
        {
            return StatementList;
        }

        public IEnumerable<IToken> GetDescentands()
        {
            return StatementList.Concat(
                StatementList.SelectMany(x => x.GetDescentands()));
        }

        public override string ToString()
        {
            return ProcedureName;
        }
    }
}