using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace StaticProgramAnalyzer.KnowledgeBuilding
{
    public class KnowledgeBuilder
    {
        private int _statementCounter = 0;

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
                TokenList = procedures.Concat(procedures.SelectMany(p => p.GetDescentands())).OrderBy(x => x.Source.LineNumber),
                CallsDirectly = procedures.Select(x => new
                {
                    procedureName = x.ProcedureName,
                    calls = x.GetDescentands().OfType<CallToken>().Select(c => c.ProcedureName).ToHashSet()
                }).ToDictionary(x => x.procedureName, x => x.calls),
            };

            var allCalls = GetAllCalls(result.CallsDirectly);
            result.AllCalls = allCalls;
            var allUses = GetAllUses(result);
            result.AllUses = allUses;
            var allModifies = GetAllModifies(result);
            result.AllModifies = allModifies;
            return result;
        }

        public Dictionary<string, HashSet<IToken>> GetAllModifies(ProgramKnowledgeBase pkb)
        {
            var directModifies = pkb.ProceduresTree
                .SelectMany(x => x.GetDescentands().OfType<ModifyVariableToken>())
                .Select(x => (x.VariableName, x as IToken));

            var result = directModifies
                .GroupBy(x => x.VariableName)
                .ToDictionary(x => x.Key, x => x.Select(t => t.Item2).ToHashSet());

            if (directModifies.Any())
            {
                List<(string, IToken)> assignments;
                assignments = AddAllParentsOfStatements(result, directModifies);
                while (assignments.Any())
                {
                    assignments = AddAllCallingProcedures(result, assignments, pkb.TokenList);
                    assignments = AddAllParentsOfStatements(result, assignments);
                }
            }
            return result;
        }

        public List<(string, IToken)> AddAllParentsOfStatements(
            Dictionary<string, HashSet<IToken>> result,
            IEnumerable<(string, IToken)> statements)
        {
            List<(string, IToken)> addedParents = new List<(string, IToken)>();
            foreach (var row in statements)
            {
                var allParents = GetAllParents(row.Item2);
                foreach (var parent in allParents)
                {
                    if (result[row.Item1].Add(parent))
                    {
                        addedParents.Add((row.Item1, parent));
                    }
                }
            }
            return addedParents;
        }

        private List<(string, IToken)> AddAllCallingProcedures(
            Dictionary<string, HashSet<IToken>> result,
            List<(string, IToken)> addedParents,
            IEnumerable<IToken> tokenList)
        {
            List<(string, IToken)> addedCalls = new List<(string, IToken)>();
            foreach (var item in addedParents.Where(x => x.Item2 is ProcedureToken))
            {
                var procedure = item.Item2 as ProcedureToken;
                //for each procedure get calls that are calling it
                var calls = tokenList.OfType<CallToken>().Where(x => x.ProcedureName == procedure.ProcedureName);
                foreach (var call in calls)
                {
                    if (result[item.Item1].Add(call))
                    {
                        addedCalls.Add((item.Item1, call));
                    }
                }
            }
            return addedCalls;
        }

        public Dictionary<string, HashSet<IToken>> GetAllUses(ProgramKnowledgeBase pkb)
        {
            var directUses = pkb.ProceduresTree
                .SelectMany(x => x.GetDescentands().OfType<IUseVariableToken>())
                .Select(x => (x.Variable.VariableName, x as IToken)).ToList();

            var result = directUses
                .GroupBy(x => x.VariableName)
                .ToDictionary(x => x.Key, x => x.Select(t => t.Item2).ToHashSet());

            if (directUses.Any())
            {
                List<(string, IToken)> uses;
                uses = AddAllParentsOfStatements(result, directUses);
                while (uses.Any())
                {
                    uses = AddAllCallingProcedures(result, uses, pkb.TokenList);
                    uses = AddAllParentsOfStatements(result, uses);
                }
            }
            return result;
        }

        public IEnumerable<IToken> GetAllParents(IToken x)
        {
            var parent = x.Parent;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
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
                throw new ArgumentException("Procedure name must start with a letter");

            if (!name.All(c => Parser.IsLetter(c) || Parser.IsDigit(c)))
            {
                throw new ArgumentException("Procedure name must contain only letters and digits");
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
                    // TODO: add "next" relations between statements
                    for (int i = 1; i < result.Count; i++)
                    {
                        if (result[i - 1] is IfThenElseToken ifelse)
                        {
                            ifelse.Then.Last().Next.Add(result[i]);
                            ifelse.Else.Last().Next.Add(result[i]);
                            continue;
                        }
                        result[i - 1].Next.Add(result[i]);
                    }
                    // Note To Self - don't do it for IF statements, only for the THEN and ELSE parts - or rather last items in their statement lists

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

        public class ExpresionTokenPriority
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

            public ExpresionTokenPriority()
            { }

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

        public ExpresionTokenPriority BuildExpressionToken(Queue<ParserToken> tokens, ExpresionTokenPriority leftToken = null)
        {
            if (tokens.Count > 0 && leftToken == null)
            {
                leftToken = new ExpresionTokenPriority();
                //variable or expression
                var refToken = tokens.Dequeue();
                if (refToken.Content == "(")
                {
                    leftToken.expresionToken = BuildExpressionToken(tokens).expresionToken;
                }
                else
                {
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
            if (IsConstant(token.Content))
            {
                return new ConstantToken(token.Content);
            }
            else
            {
                return new VariableToken(null, token.Content, GetTestValue(token.Content));
            }
        }

        private Int64 GetTestValue(String variableName)
        {
            int value = 0;
            //forgive me
            foreach (byte c in variableName)
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
            whileToken.Variable = new VariableToken(whileToken, token.Content)
            {
                Source = token
            };
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == "{");
            whileToken.StatementList = this.GetStatementList(whileToken, tokenQueue);
            // TODO: add a "next" relation from while to the first item in it's statement list
            whileToken.Next.Add(whileToken.StatementList.First());
            // TODO: for last item in the while's statement list, add a "next" relation to the while statement
            whileToken.StatementList.Last().Next.Add(whileToken);
            return whileToken;
        }

        public StatementToken BuildIfStatement(IToken parent, Queue<ParserToken> tokenQueue)
        {
            ParserToken token = tokenQueue.Dequeue();
            IfThenElseToken ifToken = new(parent, token, ++_statementCounter);
            CheckIfValidName(token.Content);
            ifToken.Variable = new VariableToken(ifToken, token.Content)
            {
                Source = token
            };
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == "then");
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == "{");
            ifToken.Then = this.GetStatementList(ifToken, tokenQueue);
            // TODO: add a "next" relation from IF to the first item in it's THEN statement list
            ifToken.Next.Add(ifToken.Then.First());
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == "else");
            token = tokenQueue.Dequeue();
            Contract.Assert(token.Content == "{");
            ifToken.Else = this.GetStatementList(ifToken, tokenQueue);
            // TODO: add a "next" relation from IF to the first item in it's ELSE statement list
            ifToken.Next.Add(ifToken.Else.First());
            return ifToken;
        }

        public AssignToken BuildAssignTokenFromString(string right)
        {
            AssignToken assignToken = new AssignToken();
            if (right.EndsWith(";") == false)
            {
                right += ";";
            }
            // x here is OK, there's need to full expresion to parse it corectly
            right = "x=" + right;
            var parsed = Parser.Parse(new String[] { right });
            var queue = new Queue<ParserToken>(parsed);
            //and now we have to drop "x=" ^>^
            queue.Dequeue();
            queue.Dequeue();
            assignToken.Right = BuildExpressionToken(queue).expresionToken;
            return assignToken;
        }
    }
}