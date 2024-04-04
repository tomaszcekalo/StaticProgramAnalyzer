using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Text;

namespace StaticProgramAnalyzer.KnowledgeBuilding
{
    public class KnowledgeBuilder
    {
        int _statementCounter = 0;
        public KnowledgeBuilder(Parser parser)
        {
            Parser = parser;
        }

        public Parser Parser { get; }

        public ProgramKnowledgeBase GetPKB(List<ParserToken> tokens)
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
            var result = new ProgramKnowledgeBase()
            {
                ProceduresTree = procedures,
                TokenList = procedures.Concat(procedures.SelectMany(p => p.GetDescentands())).OrderBy(x=>x.Source.LineNumber),
                CallsDirectly = procedures.Select(x => new
                {
                    procedureName = x.ProcedureName,
                    calls = x.GetDescentands().OfType<CallToken>().Select(c => c.ProcedureName).ToHashSet()
                }).ToDictionary(x => x.procedureName, x => x.calls),
            };

            var allCalls = GetAllCalls(result.CallsDirectly);
            result.AllCalls = allCalls;
            return result;
        }

        public Dictionary<string, HashSet<string>> GetAllCalls(Dictionary<string, HashSet<string>> callsDirectly)
        {
            Dictionary<string, HashSet<string>> allCalls = callsDirectly.ToDictionary(x => x.Key, x =>
            {
                var list = x.Value.ToHashSet();
                foreach (var item in x.Value)
                {
                    if (callsDirectly.ContainsKey(item))
                    {
                        foreach (var newItem in callsDirectly[item])
                        {
                            list.Add(newItem);
                        }
                    }
                }
                return list;
            });

            bool anythingHasBeenAdded = true;
            while (anythingHasBeenAdded)
            {
                anythingHasBeenAdded = false;
                allCalls = allCalls.ToDictionary(x => x.Key, x =>
                {
                    var list = x.Value.ToHashSet();
                    foreach (var item in x.Value)
                    {
                        if (allCalls.ContainsKey(item))
                        {
                            foreach (var newItem in allCalls[item])
                            {
                                if (list.Add(newItem))
                                {
                                    anythingHasBeenAdded = true;
                                };
                            }
                        }
                    }
                    return list;
                });
            }
            return allCalls;
        }

        public ProcedureToken BuildProcedure(Queue<ParserToken> tokenQueue)
        {
            ParserToken token = tokenQueue.Dequeue();
            ProcedureToken procedure = new ProcedureToken()
            {
                Source = token
            };
            CheckIfValidName(token.Content);
            procedure.ProcedureName = token.Content;
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == "{");
            procedure.StatementList = GetStatementList(procedure, tokenQueue);
            //procedure.AssigmentList = procedure.StatementList.OfType<AssignToken>().ToList();

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
                    result.Add(this.BuildAssignmentStatement(parent, token, tokenQueue));
                }
                else
                {
                    throw new Exception("Unexpected token: " + token.Content);
                }
            }
            return result;
        }
        class ExpresionTokenPriority
        {
            public static Dictionary<String, int> operatorPriorityDict = new Dictionary<string, int>(){
                {"+", 0 },
                {"-", 0 },
                {"*", 1 },
                {"/", 1 },
                {"(", 100 },
                {")", -100 },
                {";", -1000 }
            };
            public ExpressionToken expresionToken;
            private ExpressionToken _operatorToken;
            public ExpressionToken operatorToken
            {
                set { operatorPriority = ExpresionTokenPriority.operatorPriorityDict[value.Content]; _operatorToken = value; }
                get { return _operatorToken; }
            }
            public int operatorPriority;
            public ExpresionTokenPriority() { }
            public ExpresionTokenPriority(ExpressionToken expresionToken, ExpressionToken operatorToken)
            {
                this.expresionToken = expresionToken;
                this.operatorToken = operatorToken;
                operatorPriority = ExpresionTokenPriority.operatorPriorityDict[operatorToken.Content];
            }
        }

        public StatementToken BuildAssignmentStatement(IToken parent, ParserToken leftHandToken, Queue<ParserToken> tokenQueue)
        {
                /*
                CheckIfValidName(leftHandToken.Content);
                List<ParserToken> tokens = new();
                ParserToken token = tokenQueue.Dequeue();
                while (tokenQueue.Count > 0 && token.Content != ";")
                {
                    token = tokenQueue.Dequeue();
                    tokens.Add(token);
                }
                // TODO: Build expression
                var fakeExpression = string.Join(" ", tokens.Select(t => t.Content));
                AssignToken assignToken = new(parent, leftHandToken, fakeExpression, ++_statementCounter);
                return assignToken;
                */
            CheckIfValidName(leftHandToken.Content);
            Queue<ParserToken> tokens = new();
            ParserToken token = tokenQueue.Dequeue();
            while (tokenQueue.Count > 0 && token.Content != ";")
            {
                token = tokenQueue.Dequeue();
                tokens.Enqueue(token);
            }
            // TODO: Build expression
            var fakeExpression = string.Join(" ", tokens.Select(t => t.Content));


            AssignToken assignToken = new(parent, leftHandToken, fakeExpression, ++_statementCounter);
            assignToken.Left = new ModifyVariableToken(assignToken, leftHandToken.Content);
            assignToken.Right = BuildExpressionToken(tokens).expresionToken;
            assignToken.Modifies = assignToken.Left.UsesVariables;
            assignToken.UsesVariables = assignToken.Right.UsesVariables;
            assignToken.UsesConstants = assignToken.Right.UsesConstants;
            assignToken.Right.Parent = assignToken;
            assignToken.Left.Parent = assignToken;
            //assignToken.SetVariablesAndConstants();

            //assignToken.FakeExpression = string.Join(" ", tokens.Select(t => t.refToken.Content + " " + t.operatorToken.Content));
            return assignToken;
        }

        private ExpresionTokenPriority BuildExpressionToken(Queue<ParserToken> tokens, ExpresionTokenPriority leftToken = null)
        {
            if (tokens.Count > 0 && leftToken == null)
            {
                leftToken = new ExpresionTokenPriority();
                //variable or expression
                var refToken = tokens.Dequeue();
                if(refToken.Content == "("){
                    leftToken.expresionToken = BuildExpressionToken(tokens).expresionToken;
                }
                else {
                    leftToken.expresionToken = BuildVariableToken(refToken);
                }
                //operator
                leftToken.operatorToken = new ExpressionToken(tokens.Dequeue().Content);
                if (leftToken.operatorPriority > 0)
                {
                    leftToken = BuildExpressionToken(tokens, leftToken);
                }
                if (leftToken.operatorToken.Content == ")" || leftToken.operatorToken.Content == ";")
                {
                    return leftToken;
                }

            }

            while (tokens.Count > 0)
            {
                var rightToken = new ExpresionTokenPriority();
                //variable or expression
                var refToken = tokens.Dequeue();
                if (refToken.Content == "(")
                {
                    rightToken.expresionToken = BuildExpressionToken(tokens).expresionToken;
                }
                else
                {
                    rightToken.expresionToken = BuildVariableToken(refToken);
                }
                //operator
                var operatorToken = tokens.Dequeue();
                rightToken.operatorToken = new ExpressionToken(operatorToken.Content);
                if (operatorToken.Content == ")" || operatorToken.Content == ";")
                {
                    var epo = BuildExpressionByOperatorToken(leftToken.operatorToken.Content, leftToken.expresionToken, rightToken.expresionToken);
                    var ret = new ExpresionTokenPriority(epo, rightToken.operatorToken);
                    return ret;
                }
                if (leftToken.operatorPriority > rightToken.operatorPriority)
                {
                    leftToken.expresionToken = BuildExpressionByOperatorToken(leftToken.operatorToken.Content, leftToken.expresionToken, rightToken.expresionToken);
                    leftToken.operatorToken = rightToken.operatorToken;
                    return leftToken;
                }
                else if (leftToken.operatorPriority < rightToken.operatorPriority)
                {
                    ExpresionTokenPriority ep = BuildExpressionToken(tokens, rightToken);
                    leftToken.expresionToken = BuildExpressionByOperatorToken(leftToken.operatorToken.Content, leftToken.expresionToken, ep.expresionToken);
                    leftToken.operatorToken = ep.operatorToken;
                }
                else
                {
                    leftToken.expresionToken = BuildExpressionByOperatorToken(leftToken.operatorToken.Content, leftToken.expresionToken, rightToken.expresionToken);
                    leftToken.operatorToken = rightToken.operatorToken;
                }
                if (leftToken.operatorToken.Content == ")" || leftToken.operatorToken.Content == ";")
                {
                    return leftToken;
                }
            }
            return leftToken;
        }

        private ExpressionToken BuildExpressionByOperatorToken(String exprOperator, ExpressionToken leftExpr, ExpressionToken rightExpr)
        {
            switch (exprOperator)
            {
                case "+":
                    return BuildPlusToken(leftExpr, rightExpr);
                case "-":
                    return BuildMinusToken(leftExpr, rightExpr);
                case "*":
                    return BuildTimesToken(leftExpr, rightExpr);
                default:
                    throw new Exception("Not supported operator");
            }
        }

        private RefToken BuildVariableToken(ParserToken token)
        {
            if (IsConstant(token.Content)) {
                return new ConstantToken(token.Content);
            } else {
                return new VariableToken(null, token.Content, GetTestValue(token.Content));
            }
        }
        Int64 GetTestValue(String variableName)
        {
            int value = 0;
            //forgive me
            foreach(byte c in variableName)
            {
                value += c;
            }
            return value;
        }
        private ExpressionToken BuildPlusToken(ExpressionToken leftToken, ExpressionToken rightToken)
        {
            var expr = new PlusToken(String.Format("<+,{0},{1}>", leftToken.Content, rightToken.Content));
            expr.Left = leftToken;
            expr.Right = rightToken;
            expr.Left.Parent = expr;
            expr.Right.Parent = expr;
            expr.TestValue = leftToken.TestValue + rightToken.TestValue;
            expr.UsesVariables.UnionWith(leftToken.UsesVariables);
            expr.UsesVariables.UnionWith(rightToken.UsesVariables);
            expr.UsesConstants.UnionWith(leftToken.UsesConstants);
            expr.UsesConstants.UnionWith(rightToken.UsesConstants);
            return expr;
        }
        private ExpressionToken BuildMinusToken(ExpressionToken leftToken, ExpressionToken rightToken)
        {
            var expr = new MinusToken(String.Format("<-,{0},{1}>", leftToken.Content, rightToken.Content));
            expr.Left = leftToken;
            expr.Right = rightToken;
            expr.Left.Parent = expr;
            expr.Right.Parent = expr;
            expr.TestValue = leftToken.TestValue - rightToken.TestValue;
            expr.UsesVariables.UnionWith(leftToken.UsesVariables);
            expr.UsesVariables.UnionWith(rightToken.UsesVariables);
            expr.UsesConstants.UnionWith(leftToken.UsesConstants);
            expr.UsesConstants.UnionWith(rightToken.UsesConstants);
            return expr;
        }
        private ExpressionToken BuildTimesToken(ExpressionToken leftToken, ExpressionToken rightToken)
        {
            var expr = new TimesToken(String.Format("<*,{0},{1}>", leftToken.Content, rightToken.Content));
            expr.Left = leftToken;
            expr.Right = rightToken;
            expr.Left.Parent = expr;
            expr.Right.Parent = expr;
            expr.TestValue = leftToken.TestValue * rightToken.TestValue;
            expr.UsesVariables.UnionWith(leftToken.UsesVariables);
            expr.UsesVariables.UnionWith(rightToken.UsesVariables);
            expr.UsesConstants.UnionWith(leftToken.UsesConstants);
            expr.UsesConstants.UnionWith(rightToken.UsesConstants);
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
            var token = tokenQueue.Dequeue();
            CallToken callToken = new(parent, token, ++_statementCounter);
            CheckIfValidName(token.Content);

            callToken.ProcedureName = token.Content;
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == ";");
            return callToken;
        }

        public StatementToken BuildWhileStatement(ParserToken source, IToken parent, Queue<ParserToken> tokenQueue)
        {
            WhileToken whileToken = new(parent, source, ++_statementCounter);
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
            ParserToken token = tokenQueue.Dequeue();
            IfThenElseToken ifToken = new(parent, token, ++_statementCounter);
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