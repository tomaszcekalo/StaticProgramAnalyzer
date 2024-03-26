using StaticProgramAnalyzer.Parsing;
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

        string _program = 
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
        public void TestProcedures()
        {
            //Arrange
            //Act
            var procedures = treeBuilder.GetProcedures(tokens);
            //Assert
            Assert.AreEqual(3, procedures.ProceduresTree.Count);
            Assert.AreEqual("First", procedures.ProceduresTree[0].Name);
            Assert.AreEqual("Second", procedures.ProceduresTree[1].Name);
            Assert.AreEqual("Third", procedures.ProceduresTree[2].Name);

        }

        [TestMethod]
        public void TestSelectProcedure()
        {
            //Arrange
            var pkb = treeBuilder.GetProcedures(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("procedure p;", "Select p ");
            //Assert
            Assert.AreEqual("First, Second, Third", result);

        }

        [TestMethod]
        public void TestSelectAssign()
        {
            //Arrange
            var pkb = treeBuilder.GetProcedures(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("assign a;", "Select a ");
            //Assert
            Assert.AreEqual("1, 2, 4, 5, 7, 9, 11, 12, 13, 14, 15, 16, 17", result);

        }

        [TestMethod]
        public void TestSelectVariable()
        {
            //Arrange
            var pkb = treeBuilder.GetProcedures(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("variable v;", "Select v ");
            //Assert
            Assert.AreEqual("x, z, i, y, v", result);

        }

        [TestMethod]
        public void TestSelectWhile()
        {
            //Arrange
            var pkb = treeBuilder.GetProcedures(tokens);
            var processor = new QueryProcessor(pkb);
            //Act
            var result = processor.ProcessQuery("while w;", "Select w ");
            //Assert
            Assert.AreEqual("6", result);

        }

    }
}
