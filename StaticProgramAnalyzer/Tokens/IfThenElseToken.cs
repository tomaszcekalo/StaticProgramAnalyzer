﻿using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    internal class IfThenElseToken : StatementToken, IDeterminesFollows
    {
        public IfThenElseToken(IToken parent, ParserToken source, int statementNumber) : base(parent, source, statementNumber)
        {
        }

        public string VariableName { get; set; }

        public List<StatementToken> Then { get; internal set; }

        public List<StatementToken> Else { get; internal set; }

        public bool Follows(StatementToken left, StatementToken right)
        {
            if(Then.Contains(left) && Then.Contains(right))
                return Then.IndexOf(left) == Then.IndexOf(right) - 1;
            if(Else.Contains(left) && Else.Contains(right))
                return Else.IndexOf(left) == Else.IndexOf(right) - 1;
            return false;
        }

        public bool Follows(int statementNumber, StatementToken right)
        {
            if (Then.Contains(right))
            {
                var index = Then.IndexOf(right);
                if (index == 0)
                    return false;
                return Then[index - 1].StatementNumber == statementNumber;
            }
            if (Else.Contains(right))
            {
                var index = Else.IndexOf(right);
                if (index == 0)
                    return false;
                return Else[index - 1].StatementNumber == statementNumber;
            }
            return false;

        }

        public bool Follows(StatementToken left, int statementNumber)
        {
            if(Then.Contains(left))
            {
                var index = Then.IndexOf(left);
                if (index == Then.Count - 1)
                    return false;
                return Then[index + 1].StatementNumber == statementNumber;
            }
            if (Else.Contains(left))
            {
                var index = Else.IndexOf(left);
                if (index == Else.Count - 1)
                    return false;
                return Else[index + 1].StatementNumber == statementNumber;
            }
            return false;
        }

        public bool Follows(int leftStatementNumber, int rightStatementNumber)
        {
            StatementToken left = null;
            left = Then.Find(t => t.StatementNumber == leftStatementNumber);
            if (left != null)
            {
                var index = Then.IndexOf(left);
                if (index == Then.Count - 1)
                    return false;
                return Then[index + 1].StatementNumber == rightStatementNumber;
            }
            left = Else.Find(t => t.StatementNumber == leftStatementNumber);
            if (left != null)
            {
                var index = Else.IndexOf(left);
                if (index == Else.Count - 1)
                    return false;
                return Else[index + 1].StatementNumber == rightStatementNumber;
            }
            return false;
        }

        public bool FollowsStar(StatementToken left, StatementToken right)
        {
            if (Then.Contains(left) && Then.Contains(right))
                return Then.IndexOf(left) < Then.IndexOf(right);
            if (Else.Contains(left) && Else.Contains(right))
                return Else.IndexOf(left) < Else.IndexOf(right);
            return false;
        }

        public override IEnumerable<StatementToken> GetChildren()
        {
            return Then.Concat(Else);
        }

        public override IEnumerable<IToken> GetDescentands()
        {
            return Then
                .Concat(Then.SelectMany(t => t.GetDescentands()))
                .Concat(Else)
                .Concat(Else.SelectMany(e => e.GetDescentands()));
        }

    }
}