// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using SortingTool;

String path = "SampleFiles\\tmp1MB.txt";
String outputPath = "sorted.txt";

if (args.Length > 0)
{
    //set configuration
}

SortingEngine engine = new SortingEngine(path, batchSize : 104857);

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();
engine.SortToFile();
stopwatch.Stop();
Console.WriteLine("Elapsed sorting time: {0}", stopwatch.Elapsed);