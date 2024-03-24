using StaticProgramAnalyzer.Parsing;
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

        string sample = @"procedure First {
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
        string _program =
            @"procedure First {
        x = 2;
        z = 3;
        call Second; }
        procedure Second {
        x = 0;
        i = 5;
        while i {
        x = x + 2*y;
        call Third;
        i = i - 1; }
        if x then {
        x = x+1; }
        wlse {
        z = 1; }
        z = z + x + i;
        y = z + 2;
        x = x * y + z; }
        procedure Third {
        z = 5;
        v = z; }
        ";
        //----------------------------------------------
        //private Parser parser;
        //private string[] lines;
        //private List<ParserToken> tokens;
        //private TreeBuilder treeBuilder;

        [TestInitialize]
        public void Initialize()
        {

            //parser = new Parser();

            //lines = _program.Split(Environment.NewLine);
            //tokens = parser.Parse(lines);
            //treeBuilder = new TreeBuilding.TreeBuilder(parser);
            //var pro = treeBuilder.GetProcedures(tokens);
        }
        [TestMethod]
        public void TestMethod1()
        {
            //Arrange
            var parser = new Parser();
            var lines = sample.Split(Environment.NewLine);
            var tokens = parser.Parse(lines);
            var treeBuilder = new TreeBuilder(parser);
            //Act
            var procedures = treeBuilder.GetProcedures(tokens);
            //Assert
            Assert.AreEqual(3, procedures.Count);
            Assert.AreEqual("First", procedures[0].Name);
            Assert.AreEqual("Second", procedures[1].Name);
            Assert.AreEqual("Third", procedures[2].Name);

        }
    }
}
