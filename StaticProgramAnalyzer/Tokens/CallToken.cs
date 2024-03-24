using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class CallToken : StatementToken
    {
        public string ProcedureName { get; internal set; }
    }
}
