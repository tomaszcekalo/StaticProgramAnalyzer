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
        public Dictionary<string, HashSet<string>> CallsDirectly { get; set; }
        public Dictionary<string, HashSet<string>> AllCalls { get; set; }
        public Dictionary<string, HashSet<IToken>> AllModifies { get; internal set; }
        public Dictionary<string, HashSet<IToken>> AllUses { get; internal set; }
    }
}
