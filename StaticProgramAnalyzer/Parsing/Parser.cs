using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace StaticProgramAnalyzer.Parsing
{
    public class Parser
    {
        char[] _curlyBraces = new char[] { '{', '}' };
        char[] _whiteSpace = new char[] { ' ', '\t' };
        char semicolon = ';';
        char[] _specialCharacters = new char[] { '{', '}', ';', '=', '+', '*', '/', '-', ';' };
        char[] _mathOperator = new char[] { '=', '+', '*', '/', '-' };

        public List<ParserToken> Parse(string[] lines)
        {
            int ln = 0;
            int pos=0;
            var result = new List<ParserToken>();
            foreach (var line in lines)
            {
                ln++;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < line.Length; i++)
                {
                    if (IsLetter(line[i]) || IsDigit(line[i]))
                    {
                        sb.Append(line[i]);
                        if(pos==0)
                        {
                            pos = i;
                        }
                    }
                    else
                    {
                        var isWhitespace = IsWhitespace(line[i]);
                        var isSpecialCharacter = IsSpecialCharacter(line[i]);
                        if (sb.Length > 0)
                        {
                            result.Add(new ParserToken()
                            {
                                Content = sb.ToString(),
                                LineNumber = ln,
                                Position = pos
                            });
                            sb = new StringBuilder();
                            pos = 0;
                        }
                        if (isSpecialCharacter )
                        {
                            result.Add(new ParserToken()
                            {
                                Content = line[i].ToString(),
                                LineNumber = ln,
                                Position = i
                            });
                        }
                        else if (!isWhitespace)
                        {
                            throw new Exception($"Unexpected character found at line: {ln} character: {i}");
                        }
                    }
                }
            }
            return result;
        }
        public bool IsWhitespace(char symbol) => _whiteSpace.Contains(symbol);
        public bool IsDigit(char symbol) => symbol >= '0' && symbol <= '9';
        public bool IsSpecialCharacter(char symbol) => _specialCharacters.Contains(symbol);
        public bool IsLetter(char symbol)
        {
            if (symbol >= 'a' && symbol <= 'z')
                return true;
            if (symbol >= 'A' && symbol <= 'Z')
                return true;
            return false;
        }
        

    }
}