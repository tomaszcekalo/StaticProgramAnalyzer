using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class UseVariableToken : VariableToken, IUseVariableToken
    {
        public UseVariableToken(IToken parent, string name)
            : base(parent, name)

        {
        }
    }
}
