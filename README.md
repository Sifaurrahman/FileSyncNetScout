# FileSyncNetScout

Final C# project — Vilnius University  
A distributed system with two file indexing agents and a central master processor using named pipes, threads, and CPU core affinity.

## Structure

- **AgentA**: Scans .txt files, indexes words, sends to Master via pipe `agent1`
- **AgentB**: Same, sends via pipe `agent2`
- **Master**: Receives from both pipes, merges and prints results

## How to Run

1. Open 3 CMD terminals
2. Run Master first
3. Then run AgentA and AgentB
4. Input directory path like `C:\Files` for each
5. Watch final merged output on Master

## Tech Used

- .NET 7 Console Apps
- Multithreading
- Named pipes (NamedPipeClientStream / ServerStream)
- `Processor.ProcessorAffinity`
