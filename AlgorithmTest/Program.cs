using Core;

Console.WriteLine("Process has been started...");

if (args == null || args.Length < 2)
{
    throw new Exception("Input and Output path arguments have to be provided.");
}

Directory.CreateDirectory("temp");
File.Create(args[1]).Close();

//File.Create("output.txt").Close();

await new NSorter().ExternalSort(args[0], args[1]);
//await new NSorter().ExternalSort("input.txt", "output.txt");
