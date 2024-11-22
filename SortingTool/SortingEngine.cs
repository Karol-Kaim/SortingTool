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
        public SortingEngine(String inputPath, String outputPath = "sorted.txt", Int64 batchSize = 0, Int64 parallelMax = 0)
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
            list.Sort(_comparer);
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
            //throw new NotImplementedException();
        }

        protected override int CompareEntries(string firstEntry, string secondEntry)
        {
            return firstEntry.CompareTo(secondEntry);
        }
    }
}
