using CommandLine;
using System;

namespace BankerAlgorithm
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Arguments? options = null;
            _ = Parser.Default.ParseArguments<Arguments>(args).WithParsed(o => { options = o; });
            if (options == null)
            {
                Console.WriteLine("Argument parsing failed!");
                return;
            }
            ReadFromFile(options.FileName, out var availRes, out var maxDemand, out var requsetType ,out var requestProcess, out var requestContent);
            Banker banker = new Banker(availRes, maxDemand);
            banker.PrintStatus();
            for (int i = 0; i < requestProcess.Length; i++)
            {
                if (requsetType[i] != "-")
                    banker.AllocateRequest(requestProcess[i], requestContent[i]!);
                else
                    banker.FreeRequest(requestProcess[i], requestContent[i]);
                banker.PrintStatus();
            }
        }

        public static void ReadFromFile(string fileName, out int[] availRes, out int[,] maxDemand, out string[] requestType, out int[] requestProcess, out int[]?[] requestContent)
        {
            string[] lines = File.ReadAllLines(fileName);

            int m = lines[0].Split().Length;
            int n = lines.Skip(2).TakeWhile(line => line != "").Count();
            int l = lines.Skip(n + 3).TakeWhile(line => line != "").Count();

            availRes = new int[m];
            maxDemand = new int[n, m];
            requestType = new string[l];
            requestProcess = new int[l];
            requestContent = new int[l][];

            string[] items = lines[0].Split();
            for (int j = 0; j < m; j++)
            {
                availRes[j] = int.Parse(items[j]);
            }

            for (int i = 0; i < n; i++)
            {
                items = lines[i + 2].Split();
                for (int j = 0; j < m; j++)
                {
                    maxDemand[i, j] = int.Parse(items[j]);
                }
            }

            for (int i = 0; i < l; i++)
            {
                items = lines[i + n + 3].Split();
                requestContent[i] = new int[m];
                for (int j = 0; j < m + 2; j++)
                {
                    if (j == 0)
                        requestType[i] = items[j];
                    else if (j == 1)
                        requestProcess[i] = int.Parse(items[j]);
                    else
                    {
                        if (j < items.Length)
                            requestContent[i]![j - 2] = int.Parse(items[j]);
                        else if (requestType[i] == "-")
                        {
                            requestContent[i] = null;
                            break;
                        }
                    }
                }
            }
        }
    }
}