using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class UnitTest
    {
        public void RunAllTest(string inputPath, string outputPath)
        {
            Console.WriteLine("UnitTest has been started...\n");

            var result = HasSameSize(inputPath, outputPath);
            Console.WriteLine("\nInput and Output File Has Same Sizes: " + result);

            (result, var dataCount) = IsSortingValid(outputPath);
            Console.WriteLine("\nTotal Data Count: " + dataCount);
            Console.WriteLine("\nSorting Is Valid: " + result + "\n");
        }

        public bool HasSameSize(string inputPath, string outputPath)
        {
            FileInfo inputFile = new FileInfo(inputPath);
            FileInfo outputFile = new FileInfo(outputPath);

            Console.WriteLine("InputFile ByteSize: " + inputFile.Length);
            Console.WriteLine("OutputFile ByteSize: " + outputFile.Length);

            return outputFile.Length == inputFile.Length;
        }        

        public (bool, long) IsSortingValid(string outputPath)
        {
            using (StreamReader outputFile = new StreamReader(outputPath))
            {
                long i1 = 0, i2 = 0, dataCount = 1;
                string line = outputFile.ReadLine();

                if (line != null)
                    i1 = long.Parse(line);

                // Read file using StreamReader. Reads file line by line 
                while ((line = outputFile.ReadLine()) != null)
                {
                    dataCount++;

                    i2 = long.Parse(line);
                    if (i1 < i2)
                        i1 = i2;
                    else
                        break;
                }

                return (line == null, dataCount);
            }
        }
    }
}
