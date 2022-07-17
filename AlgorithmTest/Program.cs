using Core;

public class Program {
    
    static async Task Main(string[] args)
    {
        Console.WriteLine("\nProcess has been started...\n");

        if (args == null || args.Length < 2)
        {
            throw new Exception("Input and Output path arguments have to be provided.");
        }

        Directory.CreateDirectory("temp");
        File.Create(args[1]).Close();

        await new Sorter().ExternalSort(args[0], args[1]);

        new UnitTest().RunAllTest(args[0], args[1]);
    }
}
