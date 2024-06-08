using StaticProgramAnalyzer.KnowledgeBuilding;
using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.QueryProcessing;
using System.Diagnostics;

namespace StaticProgramAnalyzer.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var parser = new Parser();
            var lines = File.ReadAllLines(args[0]);
            var tokens = parser.Parse(lines);
            var treeBuilder = new KnowledgeBuilder(parser);
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());
            var cfgdisplay = new CFGDisplay();
            if (args.Contains("display"))
            {
                cfgdisplay.Display(pkb.TokenList);
            }
            //var logFilePath = "log.txt";
            //var processor = new SuperProcessor();
            //processor.LoadFromFile(args[0]);
            //using (StreamWriter logFileWriter = new StreamWriter(logFilePath, append: true))
            {
                //logFileWriter.WriteLine(DateTime.Now.ToString());
                //logFileWriter.WriteLine($"{args.Length} arguments");

                foreach (var arg in args)
                {
                    //logFileWriter.WriteLine(arg);
                }
                //logFileWriter.WriteLine(Console.ReadLine());
                System.Console.WriteLine("Ready");
                while (true)
                {
                    var declarations = System.Console.ReadLine();
                    //logFileWriter.WriteLine($"{declarations} ");
                    var select = System.Console.ReadLine();
                    //logFileWriter.WriteLine($"{select} ");
                    //logFileWriter.Flush();
                    try
                    {
                        System.Console.WriteLine(processor.ProcessQuery(declarations, select));
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}