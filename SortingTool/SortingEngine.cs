using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SortingToolClass;

namespace SortingTool
{
    internal class SortingEngine
    {
        private String inputFilePath;
        private String outputFilePath;
        private Int64 batchSize;
        private Int64 parallelMax = Environment.ProcessorCount;
        public SortingEngine(String inputPath, String outputPath = "sorted.txt", Int64 batchSize = 0, Int64 parallelMax = 0)
        {
            this.inputFilePath = inputPath;
            this.outputFilePath = outputPath;
            Int32 processorCount = Environment.ProcessorCount;
            
            if (parallelMax > 0)
            {
                this.parallelMax = parallelMax;
            }
            else {
                this.parallelMax = processorCount;
            }

            if (batchSize > 0)
            {
                this.batchSize = batchSize;
            }
            else
            { 
                PerformanceCounter availableMemory = new PerformanceCounter("Memory", "Available MBytes");
                float availableMB = availableMemory.NextValue();
                Console.WriteLine($"Available Physical Memory: {availableMB} MB");
                float availableMemoryChars = availableMB * 1024 * 1024 / 2;
                this.batchSize = (Int64)(availableMemoryChars / processorCount);
            }
        }

        public bool SortToFile()
        {
            try
            {
                if (File.Exists(inputFilePath))
                {
                    StreamReader sr = new StreamReader(inputFilePath);
                    Int64 index = 0;
                    Int64 sumOfCharLengths = 0;
                    String? line;
                    List<HappyPanda> list = new List<HappyPanda>();
                    HappyPanda panda = null;
                    TaskFactory<String> taskFactory = new TaskFactory<String>();
                    List<Task<String>> batchTaskList = new List<Task<String>>();
                    Task<String> sortBatchTask;

                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (line != null)
                        {
                            if (HappyPanda.TryParse(line, out panda))
                            {
                                if (panda != null)
                                {
                                    list.Add(panda);
                                    sumOfCharLengths += line.Length;

                                    if (sumOfCharLengths > batchSize)
                                    { //start and reset
                                      //start sorting
                                        sortBatchTask = StartBatchSorting(list, taskFactory, batchTaskList);
                                        sumOfCharLengths = 0;
                                        list = new List<HappyPanda>();
                                    }
                                }
                            }
                        }
                        index++;
                    }

                    sr.Dispose();

                    if (list.Count > 0)
                    {
                        sortBatchTask = StartBatchSorting(list, taskFactory, batchTaskList);
                    }
                    //Merge tasks
                    Task anySorted = Task.WhenAny(batchTaskList);
                    anySorted.Wait();
                    while (batchTaskList.Count > 0)
                    {
                        List<Task<String>> toMergeTasks = new List<Task<String>>();
                        List<Task<String>> newBatchList = new List<Task<String>>();
                        foreach (var item in batchTaskList)
                        {
                            if (item.Status == TaskStatus.RanToCompletion)
                            {
                                toMergeTasks.Add(item);
                            }
                            else
                            {
                                newBatchList.Add(item);
                            }
                        }

                        while (toMergeTasks.Count > 1)
                        {
                            String firstFile = toMergeTasks[0].Result;
                            String secondFile = toMergeTasks[1].Result;

                            Task<String> mergingTask = taskFactory.StartNew(() => { return MergingFiles(firstFile, secondFile); });
                            toMergeTasks.RemoveRange(0, 2);
                            newBatchList.Add(mergingTask);
                        }
                        if (toMergeTasks.Count > 1)
                        {
                            batchTaskList.AddRange(toMergeTasks);
                        }

                        batchTaskList = newBatchList;

                        anySorted = Task.WhenAny(batchTaskList);
                        anySorted.Wait();
                    }

                    //Write to File
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }

        private String MergingFiles(String firstFile, String secondFile)
        {
            StreamReader first = new StreamReader(firstFile);
            StreamReader second = new StreamReader(secondFile);
            String outputFile = String.Format("mergeTMP_{0}_{1}.tmp", DateTime.UtcNow.ToString("yyyyMMddhhmmss"), Task.CurrentId);
            StreamWriter output = new StreamWriter(outputFile);

            HappyPanda firstPanda = null;
            HappyPanda secondPanda = null;
            String firstEntry, secondEntry;

            firstEntry = first.ReadLine();
            secondEntry = second.ReadLine();
            HappyPanda.TryParse(firstEntry, out firstPanda);
            HappyPanda.TryParse(secondEntry, out secondPanda);

            while (!first.EndOfStream && !second.EndOfStream)
            {

                if (firstPanda != null && secondPanda != null)
                {
                    if (firstPanda.CompareTo(secondPanda) > 0)
                    {
                        output.WriteLine(secondPanda.ToString());
                        secondEntry = second.ReadLine();
                        HappyPanda.TryParse(secondEntry, out secondPanda);
                    }
                    else
                    {
                        output.WriteLine(firstPanda.ToString());
                        firstEntry = first.ReadLine();
                        HappyPanda.TryParse(firstEntry, out firstPanda);
                    }
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

            //while(!first.EndOfStream)
            //{
            //    string? firstEntry = first.ReadLine();
            //    if (firstEntry != null)
            //    {
            //        HappyPanda.TryParse(firstEntry, out firstPanda);
                        
            //    }
            //    while (!second.EndOfStream)
            //    {
            //        string? secondEntry = second.ReadLine();

            //        if (secondEntry != null)
            //        {
            //            HappyPanda.TryParse(secondEntry, out secondPanda);
            //            if (firstPanda != null && secondPanda != null)
            //            {
            //                if (firstPanda.CompareTo(secondPanda) > 0)
            //                {
            //                    output.WriteLine(secondPanda.ToString());
            //                    continue;
            //                }
            //                else
            //                {
            //                    output.WriteLine(firstPanda.ToString());
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}

            first.Dispose();
            second.Dispose();
            output.Dispose();
            return outputFile;
        }

        private Task<String> StartBatchSorting(List<HappyPanda> list, TaskFactory<String> taskFactory, List<Task<String>> batchTaskList)
        {
            Task<String> sortBatchTask = taskFactory.StartNew(() => { return CreateSortedBatchFile(list); });
            batchTaskList.Add(sortBatchTask);
            return sortBatchTask;
        }

        private String CreateSortedBatchFile(List<HappyPanda> list)
        {
            String outputPath = String.Format("batch_{0}.tmp",Task.CurrentId);
            list.Sort();
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                foreach(HappyPanda p in list) 
                { writer.WriteLine(p.ToString()); }                
            }
            return outputPath;
            //throw new NotImplementedException();
        }
    }
}
