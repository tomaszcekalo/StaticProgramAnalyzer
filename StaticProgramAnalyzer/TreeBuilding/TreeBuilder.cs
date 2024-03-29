using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Headers;
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
        class AssigmentPair
        {
            private static Dictionary<String, int> operatorPriorityDict = new Dictionary<string, int>(){
                {"+", 0 },
                {"-", 0 },
                {"*", 1 },
                {"/", 1 },
                {"(", 2 },
                {")", -1 },
                {";", -2 }
            };
            public ParserToken refToken;
            public ParserToken operatorToken;
            public int operatorPriority;
            public AssigmentPair(ParserToken refToken, ParserToken operatorToken)
            {
                this.refToken = refToken;
                this.operatorToken = operatorToken;
                operatorPriority = AssigmentPair.operatorPriorityDict[operatorToken.Content];
            }
        }
        public StatementToken BuildAssignmentStatement(IToken parent, string variableName, Queue<ParserToken> tokenQueue)
        {
            AssignToken assignToken = new(parent);
            CheckIfValidName(variableName);
            assignToken.Left = new VariableToken(variableName);
            Queue<AssigmentPair> tokens = new();
            ParserToken token = tokenQueue.Dequeue();
            while (tokenQueue.Count > 0 && token.Content != ";")
            {
                tokens.Enqueue(new AssigmentPair(tokenQueue.Dequeue(), token=tokenQueue.Dequeue()));
            }

            assignToken.Right = BuildExpressionToken(tokens, 0);

            assignToken.FakeExpression = string.Join(" ", tokens.Select(t => t.refToken.Content + " " + t.operatorToken.Content));
            return assignToken;
        }

        private ExpressionToken BuildExpressionToken(Queue<AssigmentPair> tokens, int priority, AssigmentPair deq = null)
        {
            AssigmentPair leftToken = null;
            AssigmentPair rightToken = null;
            ExpressionToken leftExpr = null;
            ExpressionToken rightExpr = null;
            while (tokens.Count > 0)
            {
                if (leftToken == null)
                {
                    leftToken = tokens.Dequeue();
                    leftExpr = BuildVariableToken(leftToken.refToken);
                }
                rightToken = tokens.Count > 0 ? tokens.Peek() : null;
                if(rightToken == null)
                {
                    return BuildVariableToken(leftToken.refToken);
                } else
                {
                    rightExpr = BuildVariableToken(rightToken.refToken);
                }
                if (leftToken.operatorPriority > rightToken.operatorPriority)
                {
                    var tdeq = tokens.Dequeue();
                    switch (leftToken.operatorToken.Content)
                    {
                        case "+":
                            return BuildPlusToken(leftExpr, rightExpr);
                            break;
                        case "-":
                            return BuildMinusToken(leftExpr, rightExpr);
                            break;
                        case "*":
                            return BuildTimesToken(leftExpr, rightExpr);
                            break;
                        default:
                            throw new Exception("Not supported operator");
                    }
                }
                else if (leftToken.operatorPriority < rightToken.operatorPriority)
                {
                    ExpressionToken ep = BuildExpressionToken(tokens, 0);
                    int a = 1;
                    int b = 2;
                    switch (leftToken.operatorToken.Content)
                    {
                        case "+":
                            leftExpr = BuildPlusToken(leftExpr, ep);
                            break;
                        case "-":
                            leftExpr = BuildMinusToken(leftExpr, ep);
                            break;
                        case "*":
                            leftExpr = BuildTimesToken(leftExpr, ep);
                            break;
                        default:
                            throw new Exception("Not supported operator");
                    }
                }
                else
                {
                    var tdeq = tokens.Dequeue();
                    switch (leftToken.operatorToken.Content)
                    {
                        case "+":
                            leftExpr = BuildPlusToken(leftExpr, rightExpr);
                            break;
                        case "-":
                            leftExpr = BuildMinusToken(leftExpr, rightExpr);
                            break;
                        case "*":
                            leftExpr = BuildTimesToken(leftExpr, rightExpr);
                            break;
                        default:
                            throw new Exception("Not supported operator");
                    }
                }
            }
            return leftExpr;
        }
        private RefToken BuildVariableToken(ParserToken token)
        {
            if (IsConstant(token.Content)) {
                return new ConstantToken(token.Content);
            } else {
                return new VariableToken(token.Content);
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
        private ExpressionToken BuildTimesToken(ExpressionToken leftToken, ExpressionToken rightToken)
        {
            var expr = new MinusToken("*");
            expr.Left = leftToken;
            expr.Right = rightToken;
            return expr;
        }

        private bool IsConstant(string content)
        {
            Int16 dummy;
            int minSize = content.Length > 1 ? 2 : 1;
            return Int16.TryParse(content.Substring(0, minSize), out dummy);
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