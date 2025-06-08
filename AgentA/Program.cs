using System;
using System.IO;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

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
        Process current = Process.GetCurrentProcess();
        current.ProcessorAffinity = (IntPtr)(1 << 0); // Core 0

        Console.Write("Enter path to directory with .txt files: ");
        string? input = Console.ReadLine();
        string path = input ?? "";

        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            Console.WriteLine("Invalid directory path.");
            return;
        }

        Thread fileReadAndSendThread = new Thread(() =>
        {
            try
            {
                var indexedData = WordIndexer.IndexWords(path);

                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "agent1", PipeDirection.Out))
                {
                    pipeClient.Connect();
                    using (StreamWriter writer = new StreamWriter(pipeClient))
                    {
                        writer.AutoFlush = true;
                        foreach (var file in indexedData)
                        {
                            foreach (var entry in file.Value)
                            {
                                writer.WriteLine($"{file.Key}:{entry.Key}:{entry.Value}");
                            }
                        }
                    }
                }

                Console.WriteLine("AgentA sent data to master via named pipe.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        });

        fileReadAndSendThread.Start();
        fileReadAndSendThread.Join();
    }
}
