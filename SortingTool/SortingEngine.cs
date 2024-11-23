using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SortingToolClass;

namespace SortingTool
{
    internal class SortingEngine : SortingEngineBase
    {
        
        private IComparer<string> _comparer = new HappyStringComparer();
        internal TaskFactory _taskDictFactory = new TaskFactory();
        public SortingEngine(String inputPath, String outputPath = "sorted.txt", Int32 batchSize = 0, Int32 parallelMax = 0)
            : base(inputPath, outputPath, batchSize, parallelMax)
        {
        }

        protected override String CreateSortedBatchFile(List<String> list)
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
            //var sortedlist = list.OrderBy(s => s.Split(". ").Last());
            List<List<String>> partionedList = new List<List<String>>();
            List<Dictionary<String, List<Int32>>> dictionariesList = new List<Dictionary<string, List<int>>>();

            int index = 0;
            int count = list.Count;
            int oneSlice = count / Environment.ProcessorCount;
            //int oneSlice = 4000;
            while (index < count)
            {
                if (index + oneSlice > count)
                {
                    oneSlice = count - index;
                }
                partionedList.Add(list.Slice(index, oneSlice));
                dictionariesList.Add(new Dictionary<string, List<int>>(oneSlice));
                index += oneSlice;                
            }
#if DEBUG
            Console.WriteLine("Finished slicing list - file {1} at: {0}", DateTime.UtcNow.ToString(), outputPath);
#endif
            //list.Sort().AsParallel();
            //Parallel.Invoke(() => { list.Sort(_comparer); });
            //Parallel.ForEach(partionedList, plist => { 
            //    plist.Sort(_comparer);
            //});
            Parallel.For(0, partionedList.Count, (i) => { 
            var currentList = partionedList[i];
                var currentDictionary = dictionariesList[i];
                foreach (string item in currentList)
                {
                    var values = item.Split(". ");
                    if (values.Length == 2)
                    {
                        string key = values[1];
                        int number = int.Parse(values[0]);
                        if (currentDictionary.ContainsKey(key))
                        {
                            currentDictionary[key].Add(number);
                        }
                        else
                        {
                            currentDictionary.Add(key, new List<int>());
                            currentDictionary[key].Add(number);
                        }
                    }
                }
            });
            int partitionsCount     = partionedList.Count;
            Dictionary<string,List<Int32>> dict = new Dictionary<string,List<Int32>>();
#if DEBUG
            Console.WriteLine("Starting merging partial results - file {1} at: {0}", DateTime.UtcNow.ToString(), outputPath);
#endif
            if (partitionsCount > 1)
            {
                int firstHalf = partitionsCount / 2;
                //list = MergeSortedLists(partionedList.Slice(0, firstHalf), partionedList.Slice(firstHalf, partitionsCount - firstHalf));
                dict = MergeSortedDictionaries(dictionariesList.Slice(0, firstHalf), dictionariesList.Slice(firstHalf, partitionsCount - firstHalf));
            }
            else
            { list = partionedList.First(); }
#if DEBUG
            Console.WriteLine("Writing to file {1} at: {0}", DateTime.UtcNow.ToString(), outputPath);
#endif
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                //foreach (String p in list)
                //{ writer.WriteLine(p); }
                foreach(string key in dict.Keys)
                {
                    var sortedDict = dict[key].Order();
                    foreach (Int32 number in sortedDict)
                    {
                        writer.WriteLine(String.Format("{0}{1}{2}", number, ". ", key));
                    }
                }
            }

#if DEBUG
            Console.WriteLine("ending sorting file {1} at: {0}", DateTime.UtcNow.ToString(), outputPath);
#endif
            return outputPath;
        }

        internal List<string> MergeSortedLists(List<List<String>> list1, List<List<string>> list2)
        {
            List<string> result = new List<string>();
            List<string> mergedList1 = null;
            List<string> mergedList2 = null;

            if (list1.Count > 1)
            {
                int firstHalf = list1.Count / 2;
                mergedList1 = MergeSortedLists(list1.Slice(0, firstHalf), list1.Slice(firstHalf, list1.Count - firstHalf));
            }
            else
            {
                mergedList1 = list1.First();
            }

            if (list2.Count > 1)
            {
                int firstHalf = list2.Count / 2;
                mergedList2 = MergeSortedLists(list2.Slice(0, firstHalf), list2.Slice(firstHalf, list2.Count - firstHalf));
            }
            else
            {
                mergedList2 = list2.First();
            }

            result = MergeLists(mergedList1, mergedList2);

            return result;
        }

        internal Dictionary<String, List<Int32>> MergeSortedDictionaries(List<Dictionary<String, List<Int32>>> listA, List<Dictionary<String, List<Int32>>> listB)
        {
            Dictionary<String, List<Int32>> result = new Dictionary<String, List<Int32>>();
            Dictionary<String, List<Int32>> resultPartA = null;
            Dictionary<String, List<Int32>> resultPartB = null;
            List<Task<Dictionary<String, List<Int32>>>> mergingTasks = new List<Task<Dictionary<String, List<Int32>>>>();
            //List<Task> mergingTasks = new List<Task>();

            if (listA.Count > 1)
            {
                Int32 half = listA.Count / 2;
                var taskA = _taskDictFactory.StartNew(() => { return MergeSortedDictionaries(listA.Slice(0, half), listA.Slice(half, listA.Count - half)); });
                mergingTasks.Add(taskA);
                //Dictionary<String, List<Int32>> dictA = MergeSortedDictionaries(listA.Slice(0, half), listA.Slice(half, listA.Count - half));
            }
            else
            {
                resultPartA = listA.First();
            }

            if (listB.Count > 1)
            {
                Int32 half = listB.Count / 2;
                var taskA = _taskDictFactory.StartNew(() => { return MergeSortedDictionaries(listB.Slice(0, half), listB.Slice(half, listA.Count - half)); });
                mergingTasks.Add(taskA);
                //Dictionary<String, List<Int32>> dictB = MergeSortedDictionaries(listB.Slice(0, half), listB.Slice(half, listB.Count - half));
            }
            else
            {
                resultPartB = listB.First();
            }

            if (mergingTasks.Count > 0)
            {
                Task.WhenAll(mergingTasks).Wait();
                if (mergingTasks.Count == 1)
                {
                    if (resultPartB == null)
                    { resultPartB = mergingTasks.First().Result; }
                    else
                    { resultPartA = mergingTasks.First().Result; }
                }
                else
                {
                    resultPartA = mergingTasks[0].Result;
                    resultPartB = mergingTasks[1].Result;
                }
            }

            result = MergeSortedDictionary(resultPartA, resultPartB);

            return result;
        }

        internal Dictionary<String, List<Int32>> MergeSortedDictionary(Dictionary<String, List<Int32>> listA, Dictionary<String, List<Int32>> listB)
        {
            Dictionary<String, List<Int32>> result = new Dictionary<String, List<Int32>>();
            var keyList = listA.Keys.Union(listB.Keys).ToList();
            keyList.Sort();
            foreach (var key in keyList)
            {
                result.Add(key, new List<int>());
                if (listA.ContainsKey(key))
                { result[key].AddRange(listA[key]); }

                if (listB.ContainsKey(key))
                {
                    result[key].AddRange(listB[key]);
                }
            }

            return result;
        }

        private List<string> MergeLists(List<string> mergedListA, List<string> mergedListB)
        {
            List<string> result = new List<string>(mergedListA.Count + mergedListB.Count);

            int indexA = 0;
            int indexB = 0;
            String entryA = mergedListA[indexA];
            String entryB = mergedListB[indexB];

            while (indexA < mergedListA.Count && indexB < mergedListB.Count)
            {
                if (_comparer.Compare(entryA, entryB) < 0)
                //if (string.Compare(entryA, entryB) < 0)
                {
                    result.Add(entryA);
                    indexA++;
                    if (indexA < mergedListA.Count)
                    {
                        entryA = mergedListA[indexA];
                    }
                }
                else
                {
                    result.Add(entryB);
                    indexB++;
                    if (indexB < mergedListB.Count)
                    {
                        entryB = mergedListB[indexB];
                    }
                }
            }

            if (indexA < mergedListA.Count)
            {
                result.AddRange(mergedListA.TakeLast(mergedListA.Count - indexA));
            }
            else if (indexB < mergedListB.Count)
            {
                result.AddRange(mergedListB.TakeLast(mergedListB.Count - indexB));
            }

            return result;
        }

        protected override int CompareEntries(string firstEntry, string secondEntry)
        {
            return firstEntry.CompareTo(secondEntry);
        }
    }
}
