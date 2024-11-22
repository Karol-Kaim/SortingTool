using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SortingTool
{
    internal class DictionarySortingEngine : SortingEngineBase
    {
        private static string _separator = ". ";

        public DictionarySortingEngine(string inputPath, string outputPath, Int64 batchSize)
            : base(inputPath, outputPath: outputPath, batchSize:batchSize)
        {
        }
        private static int CompareLines(string l1, string l2)
        {
            if (l1 == null || l2 == null)
                return 0;

            int splitIndexX = l1.IndexOf(_separator);
            int splitIndexY = l2.IndexOf(_separator);
            Int64 parsedNumberX = 0;
            String parsedKeyX = String.Empty;
            Int64 parsedNumberY = 0;
            String parsedKeyY = String.Empty;

            if (splitIndexX > 0 && splitIndexY > 0)
            {

                parsedKeyX = x.Substring(splitIndexX + _separator.Length, x.Length - splitIndexX - _separator.Length);
                parsedKeyY = y.Substring(splitIndexY + _separator.Length, y.Length - splitIndexY - _separator.Length);

                int initialComparison = parsedKeyX.CompareTo(parsedKeyY);

                if (initialComparison == 0)
                {
                    if (Int64.TryParse(x.Substring(0, splitIndexX), out parsedNumberX) && Int64.TryParse(y.Substring(0, splitIndexY), out parsedNumberY))
                    {
                        return parsedNumberX.CompareTo(parsedNumberY);
                    }
                    else
                        return 0;
                }
                else
                    return initialComparison;
            }
            else
                return 0;
        }
        protected override int CompareEntries(string firstEntry, string secondEntry)
        {
            return CompareLines(firstEntry, secondEntry);
        }

        private bool ExtractValues(string stringToParse, out string  key, out Int64 number)
        {
            int splitIndex = stringToParse.IndexOf(_separator);
            Int64 parsedNumber = 0;
            String parsedKey = String.Empty;
            if (splitIndex > 0)
            {
                if (Int64.TryParse(stringToParse.Substring(0, splitIndex), out parsedNumber))
                {
                    parsedKey = stringToParse.Substring(splitIndex + _separator.Length, stringToParse.Length - splitIndex - _separator.Length);

                }
            }
            number = parsedNumber;
            key = parsedKey;
            return true;
        }

        internal override List<Task<string>> ReadAndDivideInputToTasks(BlockingCollection<String> filesList)
        {
            if (filesList == null)
                throw new ArgumentNullException("filesList");

            List<Task<String>> resultList = new List<Task<String>>();
            StreamReader sr = new StreamReader(inputFilePath, new FileStreamOptions() { Options = FileOptions.SequentialScan, Mode = FileMode.Open });
            Int64 index = 0;
            Int64 sumOfCharLengths = 0;
            String? line;
            Task<String> sortBatchTask;
            Dictionary<String, List<Int64>> list = new Dictionary<String, List<Int64>>(); 
#if DEBUG
            Console.WriteLine("Starting reading file at: {0}", DateTime.UtcNow.ToString());
#endif
            String key = String.Empty;
            Int64 number = 0;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    ExtractValues(line, out key, out number);
                    if (list.ContainsKey(key))
                    {
                        list[key].Add(number);
                    }
                    else
                    {
                        list.Add(key, new List<long>() { number });
                    }
                }
                    sumOfCharLengths += line.Length;

                    if (sumOfCharLengths > batchSize)
                    { //start and reset
                      //start sorting
#if DEBUG
                        Console.WriteLine("Creating new batch at: {0}", DateTime.UtcNow.ToString());
#endif
                        sortBatchTask = StartDictionaryBatchSorting(list).ContinueWith<string>((t) => { filesList.Add(t.Result); return t.Result; });
                        resultList.Add(sortBatchTask);
                        sumOfCharLengths = 0;
                        list = new Dictionary<String, List<Int64>>();
                    }


                
                index++;
            }

            sr.Dispose();

            if (list.Count > 0)
            {
                sortBatchTask = StartDictionaryBatchSorting(list).ContinueWith<string>((t) => { filesList.Add(t.Result); return t.Result; });
                resultList.Add(sortBatchTask);
            }

            return resultList;
        }

internal Task<String> StartDictionaryBatchSorting(Dictionary<string, List<Int64>> list)
{
    Task<String> sortBatchTask = _taskFactory.StartNew(() => { return CreateSortedBatchFile(list); });
    return sortBatchTask;
}

        protected string CreateSortedBatchFile(Dictionary<string, List<Int64>> list)
        {
            String outputPath = String.Format("batch_{0}.tmp", Task.CurrentId);
            //#if DEBUG
            //            Console.WriteLine("Starting sorting file by OrderBy {1} at: {0}", DateTime.UtcNow.ToString(), outputPath);
            //#endif
            //            var newList = list.OrderBy(x => x, _comparer).ToList();
#if DEBUG
            Console.WriteLine("Starting sorting file {1} at: {0}", DateTime.UtcNow.ToString(), outputPath);
#endif
            //list.Sort(_comparer);
#if DEBUG
            Console.WriteLine("Writing to file {1} at: {0}", DateTime.UtcNow.ToString(), outputPath);
#endif
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                var keys = list.Keys.OrderBy(k => k).ToList();
                foreach (var p in keys)
                { 
                List<Int64> lines = list[p].OrderBy(p => p).ToList();
                foreach(Int64 l in lines)
                    {
                        writer.WriteLine(String.Format("{0}{1}{2}", l, _separator, p));
                    }
                }
            }

#if DEBUG
            Console.WriteLine("ending sorting file {1} at: {0}", DateTime.UtcNow.ToString(), outputPath);
#endif
            return outputPath;
        }

        protected override string CreateSortedBatchFile(List<string> list)
        {
            String outputPath = String.Format("batch_{0}.tmp", Task.CurrentId);
            //#if DEBUG
            //            Console.WriteLine("Starting sorting file by OrderBy {1} at: {0}", DateTime.UtcNow.ToString(), outputPath);
            //#endif
            //            var newList = list.OrderBy(x => x, _comparer).ToList();
#if DEBUG
            Console.WriteLine("Starting sorting file {1} at: {0}", DateTime.UtcNow.ToString(), outputPath);
#endif
            //list.Sort(_comparer);
#if DEBUG
            Console.WriteLine("Writing to file {1} at: {0}", DateTime.UtcNow.ToString(), outputPath);
#endif
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                foreach (String p in list)
                { writer.WriteLine(p); }
            }

#if DEBUG
            Console.WriteLine("ending sorting file {1} at: {0}", DateTime.UtcNow.ToString(), outputPath);
#endif
            return outputPath;
        }
    }
}
