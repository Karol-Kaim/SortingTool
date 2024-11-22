using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingTool
{
    internal abstract class SortingEngineBase
    {
        internal String inputFilePath;
        internal String outputFilePath;
        internal Int64 batchSize;
        internal TaskFactory<String> _taskFactory = new TaskFactory<String>();

        public SortingEngineBase(String inputPath, String outputPath = "sorted.txt", Int64 batchSize = 0, Int64 parallelMax = 0)
        {
            this.inputFilePath = inputPath;
            this.outputFilePath = outputPath;
            Int32 processorCount = Environment.ProcessorCount;

            if (batchSize > 0)
            {
                this.batchSize = batchSize;
            }
            else
            {
                PerformanceCounter availableMemory = new PerformanceCounter("Memory", "Available MBytes");
                float availableMB = availableMemory.NextValue();
                Console.WriteLine($"Available Physical Memory: {availableMB} MB");
                float availableMemoryChars = availableMB * 1024 * 1024 / 20 * 9; // about 90% per core, char took 2B
                this.batchSize = (Int64)(availableMemoryChars / processorCount);
            }
        }

        public bool SortToFile()
        {
#if DEBUG
            Console.WriteLine("Starting at: {0}", DateTime.UtcNow.ToString());
#endif

            try
            {
                BlockingCollection<String> filesToMerge = new BlockingCollection<string>();
                List<Task<String>> batchTaskList = new List<Task<String>>();

                if (File.Exists(inputFilePath))
                {
                    batchTaskList = ReadAndDivideInputToTasks(filesToMerge);

                    //Merge tasks
                    Task anySorted = Task.WhenAny(batchTaskList);
                    anySorted.Wait();

#if DEBUG
                    Console.WriteLine("Starting merging at: {0}", DateTime.UtcNow.ToString());
#endif

                    while (batchTaskList.Count > 0) //while there are tasks to run
                    {
                        List<Task<String>> newBatchList = new List<Task<String>>();

                        newBatchList.AddRange(batchTaskList.Where(t => !t.IsCompletedSuccessfully));

                        while (filesToMerge.Count > 1)
                        {
                            String firstFile = filesToMerge.Take();
                            String secondFile = filesToMerge.Take();
                            Task<String> mergingTask = _taskFactory.StartNew(() => { return MergingFiles(firstFile, secondFile); }).ContinueWith((t) => { filesToMerge.Add(t.Result); return t.Result; });
                            newBatchList.Add(mergingTask);
                        }

                        batchTaskList = newBatchList;

                        if (batchTaskList.Count > 0)
                        {
                            anySorted = Task.WhenAny(batchTaskList);
                            anySorted.Wait();
                        }
                    }

                    //Write to File
                    string tmpFile = filesToMerge.Take();
                    if (File.Exists(outputFilePath))
                    {
                        File.Delete(outputFilePath);
                    }
                    File.Move(tmpFile, outputFilePath);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }

        internal virtual List<Task<string>> ReadAndDivideInputToTasks(BlockingCollection<String> filesList)
        {
            if (filesList == null)
                throw new ArgumentNullException("filesList");

            List<Task<String>> resultList = new List<Task<String>>();
            StreamReader sr = new StreamReader(inputFilePath, new FileStreamOptions() { Options = FileOptions.SequentialScan, Mode = FileMode.Open });
            Int64 index = 0;
            Int64 sumOfCharLengths = 0;
            String? line;
            Task<String> sortBatchTask;
            List<String> list = new List<String>();
#if DEBUG
            Console.WriteLine("Starting reading file at: {0}", DateTime.UtcNow.ToString());
#endif

            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    list.Add(line);
                    sumOfCharLengths += line.Length;

                    if (sumOfCharLengths > batchSize)
                    { //start and reset
                      //start sorting
#if DEBUG
                        Console.WriteLine("Creating new batch at: {0}", DateTime.UtcNow.ToString());
#endif
                        sortBatchTask = StartBatchSorting(list).ContinueWith<string>((t) => { filesList.Add(t.Result); return t.Result; });
                        resultList.Add(sortBatchTask);
                        sumOfCharLengths = 0;
                        list = new List<String>();
                    }


                }
                index++;
            }

            sr.Dispose();

            if (list.Count > 0)
            {
                sortBatchTask = StartBatchSorting(list).ContinueWith<string>((t) => { filesList.Add(t.Result); return t.Result; });
                resultList.Add(sortBatchTask);
            }

            return resultList;
        }

        internal String MergingFiles(String firstFile, String secondFile)
        {
            StreamReader first = new StreamReader(firstFile);
            StreamReader second = new StreamReader(secondFile);
            String outputFile = String.Format("mergeTMP_{0}_{1}.tmp", DateTime.UtcNow.ToString("yyyyMMddhhmmss"), Task.CurrentId);
            StreamWriter output = new StreamWriter(outputFile);

            String firstEntry, secondEntry;

            firstEntry = first.ReadLine();
            secondEntry = second.ReadLine();


            while (firstEntry != null && secondEntry != null)
            {
                if (CompareEntries(firstEntry, secondEntry) > 0)
                {
                    output.WriteLine(secondEntry);
                    secondEntry = second.ReadLine();
                }
                else
                {
                    output.WriteLine(firstEntry);
                    firstEntry = first.ReadLine();
                }
            }


            if (!first.EndOfStream)
            {
                output.WriteLine(firstEntry);
                //add remaining items
                while (!first.EndOfStream)
                { output.WriteLine(first.ReadLine()); }
            }

            if (!second.EndOfStream)
            {
                output.WriteLine(secondEntry);
                while (!second.EndOfStream)
                { output.WriteLine(second.ReadLine()); }
            }

            first.Dispose();
            second.Dispose();
            output.Dispose();
            return outputFile;
        }

        protected abstract int CompareEntries(string firstEntry, string secondEntry);

        private Task<String> StartBatchSorting(List<String> list)
        {
            Task<String> sortBatchTask = _taskFactory.StartNew(() => { return CreateSortedBatchFile(list); });
            return sortBatchTask;
        }

        protected abstract String CreateSortedBatchFile(List<String> list);
    }
}
