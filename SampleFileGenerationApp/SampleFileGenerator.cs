using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleFileGenerationApp
{
    internal class SampleFileGenerator
    {
        private readonly String wordsListFilePath;
        
        private readonly Int64 minSize = 1024;

        private Dictionary<Int64, String> wordDictionary = new Dictionary<Int64, String>();
        private readonly Int64 kilo = 1024;
        private readonly Int64 mega = 1048576;
        private readonly Int64 giga = 1073741824;

        internal SampleFileGenerator(String wordsListFilePath = "WordsList.txt", Int64 minSize = 1024)
        {
            this.wordsListFilePath = wordsListFilePath;
            this.minSize = minSize;
        }

        public bool GenerateToTextFile(String fileName = "sampleFile.txt")
        {
            try
            {
                if (File.Exists(wordsListFilePath))
                {
                    LoadWordDictionaryFromFile();
                }
                else
                {
                    return false;
                }

                Random random = InitializeRandom();

                StreamWriter sw = new StreamWriter(fileName);
                sw.AutoFlush = true;
                int maxWords = wordDictionary.Count;
                StringBuilder sb = new StringBuilder();
                FileInfo fi = new FileInfo(fileName);

                while (fi.Length < minSize)
                {
                    Int64 wordIndex = random.Next(maxWords) + 1;
                    Int64 numberValue = random.Next(1000);

                    sb.Clear();
                    sb.Append(numberValue);
                    sb.Append(". ");
                    sb.Append(wordDictionary[wordIndex]);

                    sw.WriteLine(sb.ToString());
                    fi.Refresh();
                }

                sw.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                Console.Write("Error while generating file: ");
                Console.WriteLine(ex.Message);
                return false;
            }
            catch
            {
                Console.WriteLine("Error while generating file.");
                return false;
            }
        }

        private static Random InitializeRandom()
        {
            int seed = DateTime.Now.Microsecond;
            Random random = new Random(seed);
            return random;
        }

        private void LoadWordDictionaryFromFile()
        {
            wordDictionary.Clear();
            StreamReader sr = File.OpenText(wordsListFilePath);
            String? line = sr.ReadLine();
            Int64 counter = 1;
            while (line != null)
            {
                wordDictionary.Add(counter++, line);
                line = sr.ReadLine();
            }
            sr.Dispose();
        }
    }
}
