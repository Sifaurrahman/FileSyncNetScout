using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

class WordIndexer
{
    public static Dictionary<string, Dictionary<string, int>> IndexWords(string directoryPath)
    {
        var result = new Dictionary<string, Dictionary<string, int>>();
        var files = Directory.GetFiles(directoryPath, "*.txt");

        foreach (var file in files)
        {
            var wordCount = new Dictionary<string, int>();
            var text = File.ReadAllText(file);
            var words = text.Split(new char[] { ' ', '\t', '\r', '\n', '.', ',', ';', ':', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                var lower = word.ToLower();
                if (wordCount.ContainsKey(lower))
                    wordCount[lower]++;
                else
                    wordCount[lower] = 1;
            }

            result[Path.GetFileName(file)] = wordCount;
        }

        return result;
    }
}

class Program
{
    static void Main()
    {
        Console.Write("Enter path to directory with .txt files: ");
        string path = Console.ReadLine();

        Thread fileReadThread = new Thread(() =>
        {
            try
            {
                var indexedData = WordIndexer.IndexWords(path);

                Console.WriteLine("\nIndexed Results:");
                foreach (var file in indexedData)
                {
                    foreach (var entry in file.Value)
                    {
                        Console.WriteLine($"{file.Key}:{entry.Key}:{entry.Value}");
                    }
                }

                // We'll later send this data via named pipe to Master.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        });

        fileReadThread.Start();
        fileReadThread.Join();  // Wait for the thread to finish
    }
}
