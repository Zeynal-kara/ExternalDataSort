using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ExternalMergeSortMergeOptions
    {
        /// <summary>
        /// How many files we will process per run
        /// </summary>
        public int FilesPerRun { get; init; } = 10;
        /// <summary>
        /// Buffer size (in bytes) for input StreamReaders
        /// </summary>
        public int InputBufferSize { get; init; } = 327680;
        /// <summary>
        /// Buffer size (in bytes) for output StreamWriter
        /// </summary>
        public int OutputBufferSize { get; init; } = 327680;
        public IProgress<double> ProgressHandler { get; init; }
    }
}
