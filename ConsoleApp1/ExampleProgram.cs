using System;
using System.ComponentModel;
using System.Diagnostics;
using ab_stromy;

namespace ConsoleApp1
{
    internal class ExampleProgram
    {
        static void Main(string[] args)
        {
            //create tree with (a,b) tuple. Please follow restricitons that b >= 2a - 2 and a >= 2
            ABTree<int> tree = new ABTree<int>(2, 3);

            int[] keys = { 1, 5, 2, 9, 10, 8, 3};

            // |2 3| is example of one node
            // || |1| |2 3| || is example of two children of one node

            foreach (int key in keys)
            {
                tree.Insert(key);
                Console.WriteLine("Inserting " + key + ":");
                tree.PrintNodes();
                Console.WriteLine();
            }

            Console.WriteLine("Min: " + tree.FindMin());
            Console.WriteLine("Max: " + tree.FindMax() + "\n");

            foreach (int key in keys)
            {
                tree.Delete(key);
                Console.WriteLine("Deleting " + key + ":");
                tree.PrintNodes();
                Console.WriteLine();
            }

            // Time comparison with a list

            BenchmarkTest(10000);
        }

        static void BenchmarkTest(int sampleSize)
        {
            int[] timeKeys = new int[sampleSize];

            for (int i = 1; i <= sampleSize; i++)
            {
                timeKeys[i] = i;
            }

            int[] shuffledArray = (int[])timeKeys.Clone();

            Random.Shared.Shuffle(shuffledArray);

            List<int> list = shuffledArray.ToList();
            ABTree<int> tree2 = new ABTree<int>(2, 3);

            var listInsertTime = Measure(() => {
                foreach (int key in shuffledArray) list.Add(key);
            });

            var treeInsertTime = Measure(() => {
                foreach (int key in shuffledArray) tree2.Insert(key);
            });

            var treeFindTime = Measure(() => {
                var tree = new ABTree<int>(2, 3);
                foreach (int key in timeKeys) tree.Find(key);
            });

            var listFindTime = Measure(() => {
                var tree = new ABTree<int>(2, 3);
                foreach (int key in timeKeys) list.IndexOf(key);
            });

            var treeDeleteTime = Measure(() => {
                var tree = new ABTree<int>(2, 3);
                foreach (int key in timeKeys) tree.Delete(key);
            });

            var listDeleteTime = Measure(() => {
                var tree = new ABTree<int>(2, 3);
                foreach (int key in timeKeys) list.Remove(key);
            });


            Console.WriteLine("          |   Insert  |    Find  |   Delete");
            Console.WriteLine("-------------------------------------------");

            PrintBenchmark("TreeTime", treeInsertTime, treeFindTime, treeDeleteTime);
            PrintBenchmark("ListTime", listInsertTime, listFindTime, listDeleteTime);
        }

        static TimeSpan Measure(Action action, int repetitions = 5)
        {
            Stopwatch sw = new Stopwatch();
            TimeSpan total = TimeSpan.Zero;

            for (int i = 0; i < repetitions; i++)
            {
                sw.Restart();
                action();
                sw.Stop();
                total += sw.Elapsed;
            }

            return TimeSpan.FromTicks(total.Ticks / repetitions);
        }

        static void PrintBenchmark(string name, TimeSpan insert, TimeSpan find, TimeSpan delete)
        {
            Console.WriteLine("{0,-9} | {1,9} | {2,9}| {3,9}",name,  insert.TotalMilliseconds, find.TotalMilliseconds, delete.TotalMilliseconds);
        }
    }
}
