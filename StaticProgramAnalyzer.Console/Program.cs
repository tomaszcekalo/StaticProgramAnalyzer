﻿using StaticProgramAnalyzer.Parsing;
using StaticProgramAnalyzer.QueryProcessing;
using StaticProgramAnalyzer.Tokens;
using StaticProgramAnalyzer.TreeBuilding;
using System.Reflection.Emit;

namespace StaticProgramAnalyzer.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser();
            var lines = File.ReadAllLines(args[0]);
            var tokens = parser.Parse(lines);
            var treeBuilder = new TreeBuilder(parser);
            var pkb = treeBuilder.GetPKB(tokens);
            var processor = new QueryProcessor(pkb, new QueryResultProjector());

            var logFilePath = "log.txt";
            //var processor = new SuperProcessor();
            //processor.LoadFromFile(args[0]);
            using (StreamWriter logFileWriter = new StreamWriter(logFilePath, append: true))
            {
                logFileWriter.WriteLine(DateTime.Now.ToString());
                logFileWriter.WriteLine($"{args.Length} arguments");

                foreach (var arg in args)
                {
                    logFileWriter.WriteLine(arg);

                }
                //logFileWriter.WriteLine(Console.ReadLine());
                System.Console.WriteLine("Ready");
                while (true)
                {
                    var declarations = System.Console.ReadLine();
                    logFileWriter.WriteLine($"{declarations} ");
                    var select = System.Console.ReadLine();
                    logFileWriter.WriteLine($"{select} ");
                    logFileWriter.Flush();
                    System.Console.WriteLine(processor.ProcessQuery(declarations, select));
                }
            }
        }
    }
}
