using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer
{
    public class ProgramKnowledgeBase
    {
        public List<ProcedureToken> ProceduresTree { get; internal set; }
        public IEnumerable<IToken> TokenList { get; internal set; }
    }
}
