using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace StaticProgramAnalyzer.TreeBuilding
{
    public class TreeBuilder
    {
        public TreeBuilder(Parser parser)
        {
            Parser = parser;
        }

        public Parser Parser { get; }

        public ProgramKnowledgeBase GetProcedures(List<ParserToken> tokens)
        {
            Queue<ParserToken> tokenQueue = new Queue<ParserToken>(tokens);
            List<ProcedureToken> procedures = new List<ProcedureToken>();

            while (tokenQueue.Count > 0)
            {
                ParserToken token = tokenQueue.Dequeue();
                if (token.Content.ToLower() == "procedure")
                {
                    var procedure = this.BuildProcedure(tokenQueue);
                    procedures.Add(procedure);
                }
            }
            return new ProgramKnowledgeBase()
            {
                ProceduresTree = procedures,
                TokenList = procedures.SelectMany(p => p.GetChildren())
            };
        }

        public ProcedureToken BuildProcedure(Queue<ParserToken> tokenQueue)
        {
            ProcedureToken procedure = new ProcedureToken();
            ParserToken token = tokenQueue.Dequeue();
            CheckIfValidName(token.Content);
            procedure.Name = token.Content;
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == "{");
            procedure.StatementList = GetStatementList(procedure, tokenQueue);

            return procedure;
        }

        public void CheckIfValidName(string name)
        {
            if (!Parser.IsLetter(name[0]))
                throw new Exception("Procedure name must start with a letter");

            if (!name.All(c => Parser.IsLetter(c) || Parser.IsDigit(c)))
            {
                throw new Exception("Procedure name must contain only letters and digits");
            }
        }

        public List<StatementToken> GetStatementList(IToken parent, Queue<ParserToken> tokenQueue)
        {
            List<StatementToken> result = new List<StatementToken>();
            while (tokenQueue.Count > 0)
            {
                ParserToken token = tokenQueue.Dequeue();
                if (token.Content == "}")
                {
                    return result;
                }
                else if (token.Content == "if")
                {
                    result.Add(this.BuildIfStatement(parent, tokenQueue));
                }
                else if (token.Content == "while")
                {
                    result.Add(this.BuildWhileStatement(token, parent, tokenQueue));
                }
                else if (token.Content == "call")
                {
                    result.Add(this.BuildProcedureCall(parent, tokenQueue));
                }
                else if (tokenQueue.Peek().Content == "=")
                {
                    result.Add(this.BuildAssignmentStatement(parent, token.Content, tokenQueue));
                }
                else
                {
                    throw new Exception("Unexpected token: " + token.Content);
                }
            }
            return result;
        }

        public StatementToken BuildAssignmentStatement(IToken parent, string variableName, Queue<ParserToken> tokenQueue)
        {
            AssignToken assignToken = new(parent);
            CheckIfValidName(variableName);
            assignToken.Left = new VariableToken(variableName);
            Queue<ParserToken> tokens = new();
            ParserToken token = tokenQueue.Dequeue();
            while (tokenQueue.Count > 0 && token.Content != ";")
            {
                token = tokenQueue.Dequeue();
                tokens.Enqueue(token);
            }

            assignToken.Right = BuildExpressionToken(tokens);

            assignToken.FakeExpression = string.Join(" ", tokens.Select(t => t.Content));
            return assignToken;
        }

        private ExpressionToken BuildExpressionToken(Queue<ParserToken> tokens)
        {
            ExpressionToken leftToken = null;
            ExpressionToken rightToken = null;
            ExpressionToken expressionToken = null;
            bool wasPlus = false;
            bool wasMinus = false;
            bool wasTimes = false;
            while (tokens.Count > 0)
            {
                ParserToken token = tokens.Dequeue();
                if(rightToken != null)
                {
                    if (wasPlus){
                        leftToken = BuildPlusToken(leftToken, rightToken);
                    } else if (wasMinus) {
                        leftToken = BuildMinusToken(leftToken, rightToken);
                    } else if (wasTimes) {
                        //leftToken = BuildMinusToken(leftToken, rightToken);
                    }

                    if(wasPlus || wasMinus || wasTimes)
                    {
                        rightToken = null;
                        wasPlus = false;
                        wasMinus = false;
                        wasTimes = false;
                    }
                }
                if (token.Content == "+")
                {
                    wasPlus = true;
                }
                else if (token.Content == "-")
                {
                    wasMinus = true;
                }
                else if(token.Content == "*")
                {
                    //expressionToken = BuildTimesToken(factorToken, tokens);
                }
                else if (token.Content == ";")
                {
                    //empty
                }
                else
                {
                    if (IsConstant(token.Content))
                    {
                        if (leftToken == null) {
                            leftToken = new ConstantToken(token.Content);
                        } else {
                            rightToken = new ConstantToken(token.Content);
                        }
                    }
                    else
                    {
                        if (leftToken == null) {
                            leftToken = new VariableToken(token.Content);
                        } else {
                            rightToken = new VariableToken(token.Content);
                        }
                    }
                }
            }

            if(rightToken != null)
            {
                return rightToken;
            } else
            {
                return leftToken;
            }
        }

        private ExpressionToken BuildPlusToken(ExpressionToken leftToken, ExpressionToken rightToken)
        {
            var expr = new PlusToken("+");
            expr.Left = leftToken;
            expr.Right = rightToken;
            return expr;
        }
        private ExpressionToken BuildMinusToken(ExpressionToken leftToken, ExpressionToken rightToken)
        {
            var expr = new MinusToken("-");
            expr.Left = leftToken;
            expr.Right = rightToken;
            return expr;
        }
        private ExpressionToken BuildTimesToken(RefToken factorToken, Queue<ParserToken> tokens)
        {
            TimesToken timesToken = new TimesToken("");
            timesToken.Left = factorToken;
            timesToken.Right = BuildExpressionToken(tokens);
            return null;
        }

        private bool IsConstant(string content)
        {
            Int16 dummy;
            int minSize = content.Length > 1 ? 2 : 1;
            return Int16.TryParse(content.Substring(0, minSize), out dummy);
        }

        private ExpressionToken BuildMinusToken(ExpressionToken factorToken, Queue<ParserToken> tokens)
        {
            MinusToken minusToken = new MinusToken("");
            minusToken.Left = factorToken;
            minusToken.Right = BuildExpressionToken(tokens);
            return null;
        }

        private ExpressionToken BuildPlusToken(ExpressionToken factorToken, Queue<ParserToken> tokens)
        {
            PlusToken plusToken = new PlusToken("");
            ExpressionToken expToken = BuildExpressionToken(tokens);
            plusToken.Left = null;
            plusToken.Right = null;
            return plusToken;
        }

        public StatementToken BuildProcedureCall(IToken parent, Queue<ParserToken> tokenQueue)
        {
            CallToken callToken = new(parent);
            var token = tokenQueue.Dequeue();
            CheckIfValidName(token.Content);

            callToken.ProcedureName = token.Content;
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == ";");
            return callToken;
        }

        public StatementToken BuildWhileStatement(ParserToken source, IToken parent, Queue<ParserToken> tokenQueue)
        {
            WhileToken whileToken = new(parent, source);
            ParserToken token = tokenQueue.Dequeue();
            CheckIfValidName(token.Content);
            whileToken.VariableName = token.Content;
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == "{");
            whileToken.StatementList = this.GetStatementList(whileToken, tokenQueue);
            return whileToken;
        }

        public StatementToken BuildIfStatement(IToken parent, Queue<ParserToken> tokenQueue)
        {
            IfThenElseToken ifToken = new(parent);
            ParserToken token = tokenQueue.Dequeue();
            CheckIfValidName(token.Content);
            ifToken.VariableName = token.Content;
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == "then");
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == "{");
            ifToken.Then = this.GetStatementList(ifToken, tokenQueue);
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == "else");
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == "{");
            ifToken.Else = this.GetStatementList(ifToken, tokenQueue);
            return ifToken;
        }
    }
}