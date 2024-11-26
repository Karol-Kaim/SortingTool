// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using SortingTool;

String path = "SampleFiles\\tmp.txt";
String outputPath = "sorted.txt";
//Int64 batchSize = 104857600;
Int32 batchSize = 0;

if (args.Length > 0)
{
    //set configuration
}

//SortingEngineBase engine = new DictionarySortingEngine(path, outputPath: outputPath, batchSize : batchSize);
SortingEngineBase engine = new SortingEngine(path, outputPath: outputPath, batchSize: batchSize);

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();
engine.SortToFile();    
stopwatch.Stop();
Console.WriteLine("Elapsed sorting time: {0}", stopwatch.Elapsed);