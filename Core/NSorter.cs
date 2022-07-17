using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class NSorter
    {
        public async Task ExternalSort(string inputPath, string outputPath)
        {
            new SplitHelper()
                .SplitAndSort(inputPath);

            await new MergeHelper()
                .MergeFiles(SplitHelper.FileCount, outputPath);
        }


       
    }
}
