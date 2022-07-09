namespace Core
{
    public class Sorter
    {
        private long[] _unsortedRows;
        private int _maxUnsortedRows;
        private ExternalMergeSorterOptions _options = new ExternalMergeSorterOptions();

        public string UnsortedFileExtension = ".unsorted";
        public string SortedFileExtension = ".sorted";
        public string TempFileExtension = ".temp";

        public async Task Sort(string inputPath, string outputPath)
        {
            //  Code here   
            Directory.CreateDirectory("temp");
            File.Create("output.txt").Close();
            Stream streamSource = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
            Stream streamTarget = new FileStream(outputPath, FileMode.Open, FileAccess.ReadWrite);

             await Sort(streamSource, streamTarget);
        }

        public async Task Sort(Stream source, Stream target)
        {           
            var files = await SplitFile(source);
            // Here we create a new array that will hold the unsorted rows used in SortFiles.
            _unsortedRows = new long[_maxUnsortedRows];
            var sortedFiles = await SortFiles(files);
            await MergeFiles(sortedFiles, target);
        }

        private async Task<IReadOnlyCollection<string>> SplitFile( Stream sourceStream)
        {
            var fileSize = _options.Split.FileSize;
            var buffer = new byte[fileSize];
            var extraBuffer = new List<byte>();
            var filenames = new List<string>();

            await using (sourceStream)
            {
                var currentFile = 0L;
                while (sourceStream.Position < sourceStream.Length)
                {
                    var totalRows = 0;
                    var runBytesRead = 0;
                    while (runBytesRead < fileSize)
                    {
                        var value = sourceStream.ReadByte();
                        if (value == -1)
                        {
                            break;
                        }

                        var @byte = (byte)value;
                        buffer[runBytesRead] = @byte;
                        runBytesRead++;
                        if (@byte == _options.Split.NewLineSeparator)
                        {
                            // Count amount of rows, used for allocating a large enough array later on when sorting
                            totalRows++;
                        }
                    }

                    var extraByte = buffer[fileSize - 1];

                    while (extraByte != _options.Split.NewLineSeparator)
                    {
                        var flag = sourceStream.ReadByte();
                        if (flag == -1)
                        {
                            break;
                        }
                        extraByte = (byte)flag;
                        extraBuffer.Add(extraByte);
                    }

                    var filename = $"{++currentFile}.unsorted";
                    await using var unsortedFile = File.Create(Path.Combine(_options.FileLocation, filename));
                    await unsortedFile.WriteAsync(buffer, 0, runBytesRead);

                    if (extraBuffer.Count > 0)
                    {
                        await unsortedFile.WriteAsync(extraBuffer.ToArray(), 0, extraBuffer.Count);
                    }

                    if (totalRows > _maxUnsortedRows)
                    {
                        // Used for allocating a large enough array later on when sorting.
                        _maxUnsortedRows = totalRows+1;
                    }

                    filenames.Add(filename);
                    extraBuffer.Clear();
                }

                return filenames;
            }
        }

        private async Task<IReadOnlyList<string>> SortFiles( IReadOnlyCollection<string> unsortedFiles)
        {
            var sortedFiles = new List<string>(unsortedFiles.Count);
            double totalFiles = unsortedFiles.Count;
            foreach (var unsortedFile in unsortedFiles)
            {
                var sortedFilename = unsortedFile.Replace(UnsortedFileExtension, SortedFileExtension);
                var unsortedFilePath = Path.Combine(_options.FileLocation, unsortedFile);
                var sortedFilePath = Path.Combine(_options.FileLocation, sortedFilename);
                await SortFile(File.OpenRead(unsortedFilePath), File.OpenWrite(sortedFilePath));
                File.Delete(unsortedFilePath);
                sortedFiles.Add(sortedFilename);
            }
            return sortedFiles;
        }

        private async Task SortFile(Stream unsortedFile, Stream target)
        {
            using var streamReader = new StreamReader(unsortedFile, bufferSize: _options.Sort.InputBufferSize);
            var counter = 0;
            while (!streamReader.EndOfStream)
            {
                var val = await streamReader.ReadLineAsync();
                _unsortedRows[counter++] =  long.Parse(val);
            }

            Array.Sort(_unsortedRows, _options.Sort.Comparer);
            await using var streamWriter = new StreamWriter(target, bufferSize: _options.Sort.OutputBufferSize);
            foreach (var row in _unsortedRows.Where(x => x != null && x != 0))
            {
                await streamWriter.WriteLineAsync(row.ToString());
            }

            Array.Clear(_unsortedRows, 0, _unsortedRows.Length);
        }

        private async Task MergeFiles(IReadOnlyList<string> sortedFiles, Stream target)
        {
            var done = false;
            while (!done)
            {
                var runSize = _options.Merge.FilesPerRun;
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
                    var outputFilename = $"{++chunkCounter}{SortedFileExtension}{TempFileExtension}";
                    if (files.Length == 1)
                    {
                        File.Move(GetFullPath(files.First()), GetFullPath(outputFilename.Replace(TempFileExtension, string.Empty)));
                        continue;
                    }

                    var outputStream = File.OpenWrite(GetFullPath(outputFilename));
                    await Merge(files, outputStream);
                    File.Move(GetFullPath(outputFilename), GetFullPath(outputFilename.Replace(TempFileExtension, string.Empty)), true);
                }

                sortedFiles = Directory.GetFiles(_options.FileLocation, $"*{SortedFileExtension}")
                    .OrderBy(x =>
                    {
                        var filename = Path.GetFileNameWithoutExtension(x);
                        return int.Parse(filename);
                    })
                    .ToArray();

                if (sortedFiles.Count > 1)
                {
                    continue;
                }

                done = true;
            }
        }

        private async Task Merge( IReadOnlyList<string> filesToMerge, Stream outputStream)
        {
            var (streamReaders, rows) = await InitializeStreamReaders(filesToMerge);
            var finishedStreamReaders = new List<int>(streamReaders.Length);
            var done = false;
            await using var outputWriter = new StreamWriter(outputStream, bufferSize: _options.Merge.OutputBufferSize);

            while (!done)
            {
                rows.Sort((row1, row2) => _options.Sort.Comparer.Compare(row1.Value, row2.Value));
                var valueToWrite = rows[0].Value;
                var streamReaderIndex = rows[0].StreamReader;
                await outputWriter.WriteLineAsync(valueToWrite.ToString()); //ToString eklendi

                if (streamReaders[streamReaderIndex].EndOfStream)
                {
                    var indexToRemove = rows.FindIndex(x => x.StreamReader == streamReaderIndex);
                    rows.RemoveAt(indexToRemove);
                    finishedStreamReaders.Add(streamReaderIndex);
                    done = finishedStreamReaders.Count == streamReaders.Length;
                    continue;
                }

                var value =  long.Parse(streamReaders[streamReaderIndex].ReadLine());
                rows[0] = new Row { Value = value, StreamReader = streamReaderIndex };
            }

            CleanupRun(streamReaders, filesToMerge);
        }


        private async Task<(StreamReader[] StreamReaders, List<Row> rows)> InitializeStreamReaders( IReadOnlyList<string> sortedFiles)
        {
            var streamReaders = new StreamReader[sortedFiles.Count];
            var rows = new List<Row>(sortedFiles.Count);
            for (var i = 0; i < sortedFiles.Count; i++)
            {
                var sortedFilePath = GetFullPath(sortedFiles[i]);
                var sortedFileStream = File.OpenRead(sortedFilePath);
                streamReaders[i] = new StreamReader(sortedFileStream, bufferSize: _options.Merge.InputBufferSize);
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
            return Path.Combine(_options.FileLocation, filename);
        }
    }
}