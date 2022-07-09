using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ExternalMergeSorterOptions
    {
        public ExternalMergeSorterOptions()
        {
            Split = new ExternalMergeSortSplitOptions();
            Sort = new ExternalMergeSortSortOptions();
            Merge = new ExternalMergeSortMergeOptions();

            FileLocation = Path.Combine(Directory.GetCurrentDirectory(), ".\\temp");
        }

        public string FileLocation { get; init; }
        public ExternalMergeSortSplitOptions Split { get; init; }
        public ExternalMergeSortSortOptions Sort { get; init; }
        public ExternalMergeSortMergeOptions Merge { get; init; }
    }
}
