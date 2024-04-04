using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public interface IHasProcedureName : IToken
    {
        public string ProcedureName { get; set; }
    }
}
