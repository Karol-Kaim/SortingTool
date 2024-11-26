using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SampleFileGenerationApp;
class SampleFileGenerationApp
{
    private static String wordsListFilePath = "SampleFiles\\WordsList.txt";
    private static Int64 minFileSize = 1 * 512; // 1GB = 1073741824; 1MB = 1048576
    static void Main(string[] args)
    {
        DateTime start = DateTime.Now;
        DateTime end = DateTime.Now;

        Stopwatch stopwatch = Stopwatch.StartNew();

        if (args.Length > 0)
        {
            for (int i = 0; i < args.Length; i++)
            {

                if (args[i]==("-h") || args[i] == ("-?"))
                    Console.WriteLine("help to be added");
                if (args[i]==("-d"))
                {
                    wordsListFilePath = args[++i];
                }
            }
        }

        Console.Write("Starting... ");
        stopwatch.Start();

        SampleFileGenerator fileGenerator = new SampleFileGenerator(wordsListFilePath, minFileSize);

        fileGenerator.GenerateToTextFile("tmp.txt");
        //fileGenerator.MultipleFile("tmp1gb.txt", 40);

        stopwatch.Stop();
        Console.Write("Elapsed time: {0}", stopwatch.Elapsed);
        //Console.WriteLine(end.TimeOfDay);
        //Console.Write(String.Format("Total time: {0} hours, {1} minutes, {2} seconds", runLength.Hours, runLength.Minutes, runLength.Seconds));
        
    }
} 