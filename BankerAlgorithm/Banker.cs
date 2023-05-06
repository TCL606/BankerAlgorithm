using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BankerAlgorithm
{
    public enum AllocateStatus
    {
        Fail = 0,
        Success = 1,
        Waiting = 2
    }

    public class Banker
    {
        private int[] availableResource;
        public int[] AvailableResource => availableResource;

        private int[,] maxDemand;
        public int[,] MaxDemand => maxDemand;

        private int[,] allocation;
        public int[,] Allocation => allocation;

        private int[,] need;
        public int[,] Need => need;

        public int ProcessNum => maxDemand.GetLength(0);

        private Queue<Tuple<int, int[]>> processQueue;

        public AllocateStatus AllocateRequest(int processID, int[] requestVec) => AllocateAfterFree(processID, requestVec, true);

        private bool TryAllocate(int processID, int[] requestVec)
        {
            for (int i = 0; i < requestVec.Length; i++)
            {
                availableResource[i] -= requestVec[i];
                allocation[processID, i] += requestVec[i];
                need[processID, i] -= requestVec[i];
            }
            int[] work = new int[availableResource.Length];
            Array.Copy(availableResource, work, availableResource.Length);
            bool[] finish = Enumerable.Repeat(false, ProcessNum).ToArray();
            bool findi;
            do
            {
                findi = false;
                for (int i = 0; i < ProcessNum; i++)
                {
                    if (!finish[i])
                    {
                        bool flag = true;
                        for (int j = 0; j < work.Length; j++)
                        {
                            if (need[i, j] > work[j])
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            findi = true;
                            for (int j = 0; j < work.Length; j++)
                            {
                                work[j] += allocation[i, j];
                            }
                            finish[i] = true;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            while (findi);
            bool res = finish.All(b => b);
            if (!res)
            {
                for (int i = 0; i < requestVec.Length; i++)
                {
                    availableResource[i] += requestVec[i];
                    allocation[processID, i] -= requestVec[i];
                    need[processID, i] += requestVec[i];
                }
            }
            return res;
        }

        private AllocateStatus AllocateAfterFree(int processID, int[] requestVec, bool joinQueue = false)
        {
            Console.Write($"Request pid: {processID}; Allocate: ");
            foreach (int r in requestVec)
                Console.Write($"{r}, ");
            Console.WriteLine();
            if (requestVec.Length != availableResource.Length)
                return AllocateStatus.Fail;
            if (processID < 0 || processID >= ProcessNum)
                return AllocateStatus.Fail;
            for (int i = 0; i < requestVec.Length; i++)
            {
                if (requestVec[i] > need[processID, i])
                    return AllocateStatus.Fail;
            }
            for (int i = 0; i < requestVec.Length; i++)
            {
                if (requestVec[i] > availableResource[i])
                {
                    if (joinQueue)
                        processQueue.Enqueue(Tuple.Create(processID, requestVec));
                    return AllocateStatus.Waiting;
                }
            }
            if (TryAllocate(processID, requestVec))
                return AllocateStatus.Success;
            else
            {
                if (joinQueue)
                    processQueue.Enqueue(Tuple.Create(processID, requestVec));
                return AllocateStatus.Waiting;
            }
        }

        public bool FreeRequest(int processID, int[]? requestVec = null)
        {
            Console.Write($"Request pid: {processID}; Free: ");
            if (requestVec != null)
                foreach (int r in requestVec)
                    Console.Write($"{r}, ");
            else
                Console.Write("all.");
            Console.WriteLine();
            if (requestVec != null)
            {
                if (requestVec.Length != availableResource.Length)
                    return false;
                if (processID < 0 || processID >= ProcessNum)
                    return false;
                for (int i = 0; i < requestVec.Length; i++)
                {
                    if (requestVec[i] > allocation[processID, i])
                        return false;
                }
                for (int i = 0; i < requestVec.Length; i++)
                {
                    availableResource[i] += allocation[processID, i];
                    allocation[processID, i] -= requestVec[i];
                    need[processID, i] += requestVec[i];
                }
            }
            else
            {
                for (int i = 0; i < availableResource.Length; i++)
                {
                    availableResource[i] += allocation[processID, i];
                    allocation[processID, i] = 0;
                    need[processID, i] = maxDemand[processID, i];
                }
            }
            AllocateStatus res;
            do
            {
                res = AllocateStatus.Fail;
                processQueue.TryPeek(out var req);
                if (req != null)
                {
                    res = AllocateAfterFree(req.Item1, req.Item2);
                    if (res == AllocateStatus.Success)
                        processQueue.Dequeue();
                }
            } while (res == AllocateStatus.Success);

            return true;
        }

        public void PrintStatus()
        {
            int maxLength = maxDemand.GetLength(1);
            Console.Write("          | ");
            Console.Write("{0, " + $"{-5 * maxLength}}}", "MaxDemand");
            Console.Write("|  ");
            Console.Write("{0, " + $"{-5 * maxLength}}}", "Allocation");
            Console.Write("|  ");
            Console.Write("{0, " + $"{-5 * maxLength}}}", "Need");
            Console.WriteLine();
            for (int i = 0; i < maxDemand.GetLength(0); i++)
            {
                Console.Write("pid: {0, -5}| ", i);
                for (int j = 0; j < maxDemand.GetLength(1); j++)
                {
                    Console.Write("{0, -5}", maxDemand[i, j]);
                }
                Console.Write("|  "); 
                for (int j = 0; j < allocation.GetLength(1); j++)
                {
                    Console.Write("{0, -5}", allocation[i, j]);
                }
                Console.Write("|  ");
                for (int j = 0; j < need.GetLength(1); j++)
                {
                    Console.Write("{0, -5}", need[i, j]);
                }
                Console.WriteLine();
            }

            Console.WriteLine("Available Resource: ");
            for (int j = 0; j < availableResource.GetLength(0); j++)
            {
                Console.Write($"{availableResource[j]}  ");
            }
            Console.WriteLine();
            Console.WriteLine("Waiting Queue: ");
            int k = 0;
            foreach ((var pid, var reqVec) in processQueue)
            {
                Console.Write($"(PID {pid}: ");
                foreach (var v in reqVec)
                {
                    Console.Write("{0}, ", v);
                }
                Console.Write(")");
                if (++k < processQueue.Count)
                    Console.Write("   <---   ");
            }
            Console.WriteLine("\n");
        }

        public Banker(int[] availableResource, int[,] maxDemand)
        {
            if (availableResource.Length != maxDemand.GetLength(1))
                throw new Exception("Inconsistent number of resources!");
            this.availableResource = availableResource;
            this.maxDemand = maxDemand;
            this.allocation = new int[maxDemand.GetLength(0), maxDemand.GetLength(1)];
            this.need = new int[maxDemand.GetLength(0), maxDemand.GetLength(1)];
            Array.Copy(maxDemand, need, maxDemand.Length);
            this.processQueue = new(ProcessNum);
        }
    }
}
