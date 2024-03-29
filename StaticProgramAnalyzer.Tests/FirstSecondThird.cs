﻿using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.QueryProcessing;
using StaticProgramAnalyzer.TreeBuilding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticProgramAnalyzer.Tests
{
    [TestClass]
    public class FirstSecondThird
    {
        private string _program =
            @"procedure First {
        x = 2;
        z = 3;
        call Second; }
        procedure Second {
        x = 0;
        i = 5;
        while i {
        x = x + 2 * y;
        call Third;
        i = i - 1; }
        if x then {
        x = x + 1; }
        else {
        z = 1; }
        z = z + x + i;
        y = z + 2;
        x = x * y + z; }
        procedure Third {
        z = 5;
        v = z; }";

        //----------------------------------------------
        private Parser parser;

        private string[] lines;
        private List<ParserToken> tokens;
        private TreeBuilder treeBuilder;

        [TestInitialize]
        public void Initialize()
        {
            parser = new Parser();

            lines = _program.Split(Environment.NewLine);
            tokens = parser.Parse(lines);
            treeBuilder = new TreeBuilder(parser);
            //var pro = treeBuilder.GetProcedures(tokens);
        }

        [TestMethod]
        public void Procedures()
        {
            //Arrange
            //Act
            var procedures = treeBuilder.GetPKB(tokens);
            //Assert
            Assert.AreEqual(3, procedures.ProceduresTree.Count);
            Assert.AreEqual("First", procedures.ProceduresTree[0].Name);
            Assert.AreEqual("Second", procedures.ProceduresTree[1].Name);
            Assert.AreEqual("Third", procedures.ProceduresTree[2].Name);
        }

        [TestMethod]
        public void SelectProcedure()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p ");
            //Assert
            Assert.AreEqual("First, Second, Third", result);
        }

        [TestMethod]
        public void SelectAssign()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("assign a;", "Select a ");
            //Assert
            Assert.AreEqual("1, 2, 4, 5, 7, 9, 11, 12, 13, 14, 15, 16, 17", result);
        }

        [TestMethod]
        public void SelectVariable()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("variable v;", "Select v ");
            //Assert
            Assert.AreEqual("x, z, i, y, v", result);
        }

        [TestMethod]
        public void SelectWhile()
        {
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("while w;", "Select w ");
            //Assert
            Assert.AreEqual("8", result);
        }

        [TestMethod]
        public void CallsWithDiscard()
        {
            //Q1. Which procedures call at least one procedure?
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p such that Calls(p, _)");
            //Assert
            Assert.AreEqual("First", result);
        }

        [TestMethod]
        public void Calls()
        {
            //Q1. Which procedures call at least one procedure?
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("procedure p, q;", "Select p such that Calls(p, q)");
            //Assert
            Assert.AreEqual("First, Second", result);
        }

        [TestMethod]
        public void CallsWithDiscard2()
        {
            //Q2. Which procedures are called by at least one other procedure?
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("procedure q;", "Select q such that Calls(_, q)");
            //Assert
            Assert.AreEqual("Second, Third", result);
        }

        [TestMethod]
        public void CallsTouple()
        {
            //Q3. Find all pairs of procedures p and q such that p calls q.
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("procedure p, q;", "Select <p, q> such that Calls(p, q)");
            //Assert
            Assert.AreEqual("First Second, Second Third", result);
        }

        [TestMethod]
        public void FindProcedureFirst()
        {
            //Q4. Find procedure named “First”
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p such that p.procName = \"First\"");
            //Assert
            Assert.AreEqual("First", result);
        }

        [TestMethod]
        public void FindProcedureSecond()
        {
            //Q4. Find procedure named “Second”
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p such that p.procName = \"Second\"");
            //Assert
            Assert.AreEqual("Second", result);
        }

        [TestMethod]
        public void FindProcedureThird()
        {
            //Q4. Find procedure named “Third”
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p such that p.procName = \"Third\"");
            //Assert
            Assert.AreEqual("Third", result);
        }

        [TestMethod]
        public void FindCalledFromSecond()
        {
            //Q5. Find all procedures that are called by “Second”
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("procedure p, q;", "Select q such that Calls(p, q) with p.procName=\"Second\"");
            //Assert
            Assert.AreEqual("Third", result);
        }

        [TestMethod]
        public void FindCalledFromSecondShortForm()
        {
            //Q5. Find all procedures that are called by “Second”
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("procedure q;", "Select q such that Calls(\"Second\", q) ");
            //Assert
            Assert.AreEqual("Third", result);
        }

        [TestMethod]
        public void FindCallingSecond()
        {
            //Q6. Find all procedures that call “Second” and modify the variable named "X"
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p such that Calls(p, \"Second\") and Modifies(p, \"x\") ");
            //Assert
            Assert.AreEqual("First", result);
        }

        [TestMethod]
        public void FindProgLine()
        {
            //Q12. Which statements contain a statement (at stmt#=”n”) that can be executed after line 10?
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("prog_line n; stmt s;", "Select s such that Next* (10, n) and Parent* (s, n) ");
            //Assert
            Assert.AreEqual("10", result);
        }

        [TestMethod]
        public void FindStatementNumberIsEqualTosomeConstant()
        {
            //Q14. Find all statements whose statement number is equal to some constant.
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("stmt s; constant c;", "Select s with s.stmt# = c.value");
            //Assert
            Assert.AreEqual("2, 3, 5, 1", result);
        }

        [TestMethod]
        public void FindFollows10()
        {
            //Q16. Find statements that follow 10:
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("prog_line n; stmt s;", "Select s such that Follows* (n, s) with n = 10");
            //Assert
            Assert.AreEqual("13, 14, 15", result);
        }

        [TestMethod]
        public void FindPatternX()
        {
            //Patterns are specified using relational notation so they look the same as conditions in a such that clause.
            //Think about a node in the AST as a relationship among its children.
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("assign a;", "Select a pattern a (\"x\", _)");
            //Assert
            Assert.AreEqual("1, 4, 7, 11, 15", result);
        }

        [TestMethod]
        public void FindModifiesX()
        {
            //The two queries below yield the same result for SIMPLE programs. Notice also that this might
            //not be the case in other languages. Why?
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("assign a;", "Select a such that Modifies (a, \"x\")");
            //Assert
            Assert.AreEqual("1, 4, 7, 11, 15", result);
        }

        [TestMethod] 
        public void FindBooleanCalls()
        {
            //If there is a procedure that calls some other procedure in the program, the result is TRUE;
            //otherwise, the result is FALSE.:
            //Arrange
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("", "Select BOOLEAN such that Calls (_, _)");
            //Assert
            Assert.AreEqual("true", result);
        }
    }
}