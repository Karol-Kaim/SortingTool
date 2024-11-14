using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SampleFileGenerationApp;
class SampleFileGenerationApp
{
    private static String wordDictionaryFilePath = "WordDictionary.txt";
    private Int64 maxFileSize = 256;

    
    private Int64 sizeSeparator = 2;
    private Int64 parallelCount = 2;
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            args.Contains("?");
        }
        Console.WriteLine("Hello, World!");
    }
}