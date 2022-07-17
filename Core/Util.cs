using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Util
    {
        public static string SortedFileExt { get; set; } = ".sorted";
        public static string TempFileExt { get; set; } = ".temp";
        public static string FileLocation { get; set; }
            = Path.Combine(Directory.GetCurrentDirectory(), ".\\temp\\");

        public static string NewLineChar { get; set; } = "\n";
        public static IComparer<long> Comparer { get; set; } = Comparer<long>.Default;

        
    }
}
