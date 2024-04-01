using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class ModifyVariableToken : VariableToken
    {
        public ModifyVariableToken(IToken parent, string name) 
            : base(parent, name)
        {
        }
    }
}
