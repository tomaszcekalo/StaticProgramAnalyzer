using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace StaticProgramAnalyzer.TreeBuilding
{
    public class TreeBuilder
    {
        public TreeBuilder(Parser parser)
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
                TokenList = procedures.Concat(procedures.SelectMany(p => p.GetDescentands())),
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

        public StatementToken BuildAssignmentStatement(IToken parent, ParserToken leftHandToken, Queue<ParserToken> tokenQueue)
        {
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
            AssignToken assignToken = new(parent, leftHandToken, fakeExpression)
            {
            };
            return assignToken;
        }

        public StatementToken BuildProcedureCall(IToken parent, Queue<ParserToken> tokenQueue)
        {
            var token = tokenQueue.Dequeue();
            CallToken callToken = new(parent, token);
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
            ParserToken token = tokenQueue.Dequeue();
            IfThenElseToken ifToken = new(parent, token);
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