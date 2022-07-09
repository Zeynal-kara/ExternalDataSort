using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ExternalMergeSortSplitOptions
    {
        /// <summary>
        /// Size of unsorted file (chunk) in bytes
        /// </summary>
        public int FileSize { get; init; } = 2 * 1024 * 1024;
        public char NewLineSeparator { get; init; } = '\n';
        public IProgress<double> ProgressHandler { get; init; }
    }
}
