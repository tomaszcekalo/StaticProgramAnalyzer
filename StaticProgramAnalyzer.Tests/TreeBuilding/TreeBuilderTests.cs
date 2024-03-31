using Microsoft.VisualStudio.TestTools.UnitTesting;
using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.Tokens;
using StaticProgramAnalyzer.TreeBuilding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace StaticProgramAnalyzer.TreeBuilding.Tests
{
    [TestClass()]
    public class TreeBuilderTests
    {
        [TestMethod()]
        public void BuildAssignmentStatementTest()
        {
            string testProgram = @"procedure Test {
y = a+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+(((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k-((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g))+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k+((b+c*g)+d*e)*f+k;
}";
            var parser = new Parser();
            var tokens = parser.Parse(testProgram.Split("\r\n"));
            var tb = new TreeBuilder(parser);
            var pkb = tb.GetProcedures(tokens);

            var at = (pkb.ProceduresTree[0].StatementList[0] as AssignToken);
            Assert.AreEqual(at.Right.TestValue, 11972706660);
        }
    }
}