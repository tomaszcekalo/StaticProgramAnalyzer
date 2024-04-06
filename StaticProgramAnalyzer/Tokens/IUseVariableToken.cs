using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public interface IUseVariableToken
    {
        public string VariableName { get; set; }
    }
}
