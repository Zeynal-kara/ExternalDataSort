using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ExternalMergeSortSortOptions
    {
        public IComparer<long> Comparer { get; init; } = Comparer<long>.Default;
        public int InputBufferSize { get; init; } = 327680;
        public int OutputBufferSize { get; init; } = 327680;
        public IProgress<double> ProgressHandler { get; init; }
    }
}
