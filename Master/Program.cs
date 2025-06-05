using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;

class Program
{
    static Dictionary<string, Dictionary<string, int>> sharedData = new();
    static object lockObj = new();

    static void HandlePipe(string pipeName, string agentLabel)
    {
        try
        {
            using var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In);
            Console.WriteLine($"Waiting for connection from {agentLabel}...");
            pipeServer.WaitForConnection();
            Console.WriteLine($"{agentLabel} connected!");

            using var reader = new StreamReader(pipeServer);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 3)
                {
                    string file = parts[0];
                    string word = parts[1];
                    int count = int.Parse(parts[2]);

                    lock (lockObj)
                    {
                        if (!sharedData.ContainsKey(file))
                            sharedData[file] = new Dictionary<string, int>();

                        if (sharedData[file].ContainsKey(word))
                            sharedData[file][word] += count;
                        else
                            sharedData[file][word] = count;
                    }
                }
            }

            Console.WriteLine($"{agentLabel} finished sending data.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling {agentLabel}: {ex.Message}");
        }
    }

    static void Main()
    {
        Console.WriteLine("Master process started.");

        Thread agent1Thread = new(() => HandlePipe("agent1", "AgentA"));
        Thread agent2Thread = new(() => HandlePipe("agent2", "AgentB"));

        agent1Thread.Start();
        agent2Thread.Start();

        agent1Thread.Join();
        agent2Thread.Join();

        Console.WriteLine("\nMerged Word Index:");
        foreach (var file in sharedData)
        {
            foreach (var word in file.Value)
            {
                Console.WriteLine($"{file.Key}:{word.Key}:{word.Value}");
            }
        }
    }
}
