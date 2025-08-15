using System.Collections;

namespace ab_stromy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ABTree<int> tree = new ABTree<int>(2,3);

            int[] keys = [1, 5, 6, 4, 8, 10, 2, 7, 3];
            int[] keys2 = [6, 7, 8, 3, 2, 10];

            foreach (int i in keys)
            {
                tree.Insert(i);
            }

            foreach(int i in keys2)
            {
                tree.Delete(i);
            }

            Console.WriteLine(":)");
           
        }
    }
}
