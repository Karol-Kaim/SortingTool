﻿// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using SortingTool;

String path = "SampleFiles\\tmp1gb.txt";
String outputPath = "sorted.txt";
//Int64 batchSize = 10485760;
Int64 batchSize = 0;

if (args.Length > 0)
{
    //set configuration
}

SortingEngine engine = new SortingEngine(path, outputPath: outputPath, batchSize : batchSize);

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();
engine.SortToFile();
stopwatch.Stop();
Console.WriteLine("Elapsed sorting time: {0}", stopwatch.Elapsed);