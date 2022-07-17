using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class MergeHelper
    {
        public static int FilesPerRun { get; set; } = 10;

        public async Task MergeFiles(int sortedFilesCount, string outputPath)
        {
            List<string> sortedFiles = new List<string>();
            var target = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            for (int i = 1; i <= sortedFilesCount; i++)
                sortedFiles.Add(i + Util.SortedFileExt);

            var done = false;
            while (!done)
            {
                var runSize = FilesPerRun;
                var finalRun = sortedFiles.Count <= runSize;

                if (finalRun)
                {
                    await Merge(sortedFiles, target);
                    return;
                }

                // TODO better logic when chunking, we don't want to have 1 chunk of 10 and 1 of 1 for example, better to spread it out.
                var runs = sortedFiles.Chunk(runSize);
                var chunkCounter = 0;
                foreach (var files in runs)
                {
                    var outputFilename = $"{++chunkCounter}{Util.SortedFileExt}{Util.TempFileExt}";
                    if (files.Length == 1)
                    {
                        File.Move(GetFullPath(files.First()), GetFullPath(outputFilename.Replace(Util.TempFileExt, string.Empty)));
                        continue;
                    }

                    var outputStream = File.OpenWrite(GetFullPath(outputFilename));
                    await Merge(files, outputStream);
                    File.Move(GetFullPath(outputFilename), GetFullPath(outputFilename.Replace(Util.TempFileExt, string.Empty)), true);
                }

                sortedFiles = Directory.GetFiles(Util.FileLocation, $"*{Util.SortedFileExt}")
                    .OrderBy(x =>
                    {
                        var filename = Path.GetFileNameWithoutExtension(x);
                        return int.Parse(filename);
                    })
                    .ToList();

                if (sortedFiles.Count > 1)
                {
                    continue;
                }

                done = true;
            }
        }

        private async Task Merge(IReadOnlyList<string> filesToMerge, Stream outputStream)
        {
            var (streamReaders, rows) = await InitializeStreamReaders(filesToMerge);
            var finishedStreamReaders = new List<int>(streamReaders.Length);
            var done = false;
            await using var outputWriter = new StreamWriter(outputStream);

            while (!done)
            {
                rows.Sort((row1, row2) => Util.Comparer.Compare(row1.Value, row2.Value));
                var valueToWrite = rows[0].Value;
                var streamReaderIndex = rows[0].StreamReader;
                await outputWriter.WriteAsync(valueToWrite + Util.NewLineChar);

                if (streamReaders[streamReaderIndex].EndOfStream)
                {
                    var indexToRemove = rows.FindIndex(x => x.StreamReader == streamReaderIndex);
                    rows.RemoveAt(indexToRemove);
                    finishedStreamReaders.Add(streamReaderIndex);
                    done = finishedStreamReaders.Count == streamReaders.Length;
                    continue;
                }

                var value = long.Parse(streamReaders[streamReaderIndex].ReadLine());
                rows[0] = new Row { Value = value, StreamReader = streamReaderIndex };
            }

            CleanupRun(streamReaders, filesToMerge);
        }


        private async Task<(StreamReader[] StreamReaders, List<Row> rows)> InitializeStreamReaders(IReadOnlyList<string> sortedFiles)
        {
            var streamReaders = new StreamReader[sortedFiles.Count];
            var rows = new List<Row>(sortedFiles.Count);
            for (var i = 0; i < sortedFiles.Count; i++)
            {
                var sortedFilePath = GetFullPath(sortedFiles[i]);
                var sortedFileStream = File.OpenRead(sortedFilePath);
                streamReaders[i] = new StreamReader(sortedFileStream);
                var value = long.Parse(await streamReaders[i].ReadLineAsync());
                var row = new Row
                {
                    Value = value,
                    StreamReader = i
                };
                rows.Add(row);
            }

            return (streamReaders, rows);
        }

        private void CleanupRun(StreamReader[] streamReaders, IReadOnlyList<string> filesToMerge)
        {
            for (var i = 0; i < streamReaders.Length; i++)
            {
                streamReaders[i].Dispose();
                // RENAME BEFORE DELETION SINCE DELETION OF LARGE FILES CAN TAKE SOME TIME
                // WE DONT WANT TO CLASH WHEN WRITING NEW FILES.
                var temporaryFilename = $"{filesToMerge[i]}.removal";
                File.Move(GetFullPath(filesToMerge[i]), GetFullPath(temporaryFilename));
                File.Delete(GetFullPath(temporaryFilename));
            }
        }

        private string GetFullPath(string filename)
        {
            return Path.Combine(Util.FileLocation, filename);
        }


    }
}
