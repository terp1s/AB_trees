using System;
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

            int[] keys = { 1, 5, 2, 9, 10, 8, 3 , 7, 15, 1, 20, 11, 12};

            // |2 3| is example of one node
            // || |1| |2 3| || is example of two children of one node

            foreach (int key in keys)
            {
                tree.Insert(key);
                Console.WriteLine("Inserting " + key + ":");
                tree.PrintKeys();
                Console.WriteLine();

            }

            Console.WriteLine("Min: " + tree.FindMin());
            Console.WriteLine("Max: " + tree.FindMax());
            Console.WriteLine();

            foreach (int key in keys)
            {
                tree.Delete(key);
                Console.WriteLine("Deleting " + key + ":");
                tree.PrintKeys();
                Console.WriteLine();
            }
        }
    }
}
