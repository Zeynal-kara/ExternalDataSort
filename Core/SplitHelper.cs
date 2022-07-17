using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class SplitHelper
    {       
        public static int BufferSize { get; set; } = 1000000;
        public static int FileCount = 0;

        public void SplitAndSort(string inputPath)
        {
            using (StreamReader inputFile = new StreamReader(inputPath))
            {
                KeepReading:
                long[] numbers = new long[BufferSize];
                int i = 0;
                string line = null;

                // Read file using StreamReader. Reads file line by line 
                while (i < BufferSize && (line = inputFile.ReadLine()) != null)
                {
                    numbers[i++] = long.Parse(line);
                }

                if(i > 0)
                {
                    //Sort and write to on disk
                    Array.Sort(numbers);
                    WriteFile(numbers, i);
                }

                if (line != null)
                    goto KeepReading;
            }
        }

        private void WriteFile(long[] numbers, int length)
        {
            var filename = $"{++FileCount}{Util.SortedFileExt}";

            using var streamWriter = new StreamWriter(Util.FileLocation + filename);
            for (int i = BufferSize - length; i < BufferSize; i++)
            {
                streamWriter.Write(numbers[i] + Util.NewLineChar);
            }
            streamWriter.Close();
        }
    }
}
