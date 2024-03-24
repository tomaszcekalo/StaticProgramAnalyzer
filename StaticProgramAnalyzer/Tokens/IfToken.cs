using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    internal class IfToken : StatementToken
    {
        public string VariableName { get; set; }

        public List<StatementToken> Then { get; internal set; }

        public List<StatementToken> Else { get; internal set; }
    }
}
