using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    internal interface IHasParentToken : IToken
    {
        public IToken Parent { get; }
    }
}
