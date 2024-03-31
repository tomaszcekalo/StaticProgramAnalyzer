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
        /*
        class AssigmentPair
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
            public ParserToken refToken;
            public ParserToken operatorToken;
            public int operatorPriority;
            public AssigmentPair() { }
            public AssigmentPair(ParserToken refToken, ParserToken operatorToken)
            {
                this.refToken = refToken;
                this.operatorToken = operatorToken;
                operatorPriority = AssigmentPair.operatorPriorityDict[operatorToken.Content];
            }
        }*/
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
        public StatementToken BuildAssignmentStatement(IToken parent, string variableName, Queue<ParserToken> tokenQueue)
        {
            AssignToken assignToken = new(parent);
            CheckIfValidName(variableName);
            assignToken.Left = new VariableToken(variableName);
            tokenQueue.Dequeue(); //deque eqals sign 
            
            assignToken.Right = BuildExpressionToken(tokenQueue).expresionToken;

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
                    return new ExpresionTokenPriority(epo, rightToken.operatorToken);
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
            var expr = new PlusToken(String.Format("{0} + {1}", leftToken.Content, rightToken.Content));
            expr.Left = leftToken;
            expr.Right = rightToken;
            return expr;
        }
        private ExpressionToken BuildMinusToken(ExpressionToken leftToken, ExpressionToken rightToken)
        {
            var expr = new MinusToken(String.Format("{0} - {1}", leftToken.Content, rightToken.Content));
            expr.Left = leftToken;
            expr.Right = rightToken;
            return expr;
        }
        private ExpressionToken BuildTimesToken(ExpressionToken leftToken, ExpressionToken rightToken)
        {
            var expr = new TimesToken(String.Format("{0} * {1}", leftToken.Content, rightToken.Content));
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