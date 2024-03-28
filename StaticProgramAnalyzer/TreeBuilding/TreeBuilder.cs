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
            ExpressionToken expressionToken = null;
            RefToken factorToken = null;
            while (tokens.Count > 0)
            {
                ParserToken token = tokens.Dequeue();
                if (token.Content == "+")
                {
                    expressionToken = BuildPlusToken(factorToken, tokens);
                    factorToken = null;
                }
                else if (token.Content == "-")
                {
                    //expressionToken = BuildMinusToken(factorToken, tokens);
                    //factorToken = null;
                }/*
                else if(token.Content == "*")
                {
                    new TermToken();
                    factorToken = null;
                } */
                else if (token.Content == ";")
                {
                    //empty
                }
                else
                {
                    if (IsConstant(token.Content))
                    {
                        factorToken = new ConstantToken(token.Content);
                    }
                    else
                    {
                        factorToken = new VariableToken(token.Content);
                    }
                }
            }
            if (factorToken != null)
            {
                return factorToken;
            } else {
                return expressionToken;
            }
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
            plusToken.Left = factorToken;
            plusToken.Right = BuildExpressionToken(tokens);
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