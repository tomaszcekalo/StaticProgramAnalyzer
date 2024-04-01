using StaticProgramAnalyzer.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace StaticProgramAnalyzer.Tokens
{
    internal class WhileToken : StatementToken
    {
        public WhileToken(IToken parent, ParserToken source) : base(parent, source)
        {
        }

        public string VariableName { get; internal set; }
        public List<StatementToken> StatementList { get; internal set; }

        public override IEnumerable<IToken> GetChildren()
        {
            return StatementList;
        }

        public override IEnumerable<IToken> GetDescentands()
        {
            return StatementList.Concat(
                StatementList.SelectMany(x => x.GetDescentands()));
        }
        public override string ToString()
        {
            return Source.LineNumber.ToString();
        }
    }
}
