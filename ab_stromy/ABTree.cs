using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.Collections;
using System.Security.Cryptography;

namespace ab_stromy
{
    public class ABTree<TKey> where TKey : IComparable<TKey>
    {
        private int a;
        private int b;

        private ABNode<TKey> root;

        public ABTree(int a, int b) 
        {
            this.a = a;
            this.b = b;
            root = new ABNode<TKey>(a, b); 
            root.leaf = true;
        
        }

        public ABNode<TKey> Find(TKey key)
        {
            ABNode<TKey> node = root;

            while (node != null)
            {
                TKey[] data = node.GetKeys();

                int index = node.FindClosestIndex(key);

                if (data[index].Equals(key) && index < node.KeyCount)
                {
                    return node;
                }

                node = node.GetChild(index);
            }

            return null;
        }

        public void Insert(TKey key)
        {
            if(Find(key) != null) //key already in tree
            {
                return;
            }
            else
            {
                ABNode<TKey> node = FindLeaf(root, key); //find where key would go

                node.InsertKey(key, null, null);

                while (node.Overflowed)
                {
                    node.Overflow();
                    node = node.GetParent();
                }

                if (node.GetParent() == null)
                {
                   this.root = node;
                }
            }
        }

        private ABNode<TKey> FindLeaf(ABNode<TKey> node, TKey key)
        {
            while (!node.IsLeaf)
            {
                int index = node.FindClosestIndex(key);
                node = node.GetChild(index);
            }
            return node;
        }

        public void Delete(TKey key)
        {
            ABNode<TKey> node = Find(key);

            if(node == null) { return; } //key not in tree
            if(node == root && node.IsLeaf) { node.DeleteKey(key, null); } //tree has only one node

            int indexInParent = node.GetParent().FindClosestIndex(key);

            if (!node.IsLeaf) //if key is not in a leaf node, it's swapped with successor
            {
                int indexOfKey = node.FindClosestIndex(key);
                ABNode<TKey> successor = FindSuccessor(node, indexOfKey);

                TKey temp = successor.GetKeyRef(0);
                successor.GetKeys()[0] = key;
                node.GetKeys()[indexOfKey] = temp;

                node = successor;
                indexInParent = node.GetParent().FindClosestIndex(key) + 1;
            }

            node.DeleteKey(key, null);

            while (node.Underflowed && root != node)
            {
                node.Underflow(indexInParent);
                node = node.GetParent();

                if(node.GetParent() != null)
                {
                    indexInParent = node.GetParent().FindClosestIndex(node.GetKeyRef(0));
                }
            }

            if (node.KeyCount == 0)
            {
                root = node.GetChild(0);

                if (root.GetChild(0) == null)
                {
                    root.leaf = true;
                }
            }
        }

        private ABNode<TKey> FindSuccessor(ABNode<TKey> node, int index)
        {
            ABNode<TKey> child = node.GetChild(index + 1);

            while (!child.IsLeaf)
            {
                child = child.GetChild(0);
            }
            return child;
        }

        public TKey FindMin()
        {
            ABNode<TKey> node = root;

            while(node.GetChild(0) != null)
            {
                node = node.GetChild(0);
            }
            
            return node.GetKeyRef(0);
        }
        public TKey FindMax()
        {
            ABNode<TKey> node = root;

            while (node.GetChild(node.KeyCount) != null)
            {
                node = node.GetChild(node.KeyCount);
            }

            return node.GetKeyRef(node.KeyCount - 1);
        }
        
        public void PrintKeys()
        {
            if(root == null || root.KeyCount <= 0) { Console.WriteLine("empty"); return; }

            ABNode<TKey> node = root;

            int depth = 1;

            while(node.GetChild(0) != null)
            {
                depth++;
                node = node.GetChild(0);
            }

            List<ABNode<TKey>>[] levels = new List<ABNode<TKey> >[depth];
            levels[0] = new List<ABNode<TKey>> { root };
            root.Print();
            Console.WriteLine();

            for (int i = 1; i < depth; i++)
            {
                levels[i] = new List<ABNode<TKey>>();

                foreach (var nd in levels[i-1])
                {
                    Console.Write(" || ");
                    for(int j = 0; j <= nd.KeyCount; j++)
                    {
                        ABNode<TKey> kid = nd.GetChild(j);
                        kid.Print();
                        Console.Write(" ");
                        levels[i].Add(kid);
                    }
                    Console.Write(" || ");
                }
                Console.WriteLine();
            }
        }

        public void PrintNodes()
        {
            if (root == null) { Console.WriteLine("empty"); return; }

            Stack<ABNode<TKey>> stack = new Stack<ABNode<TKey>>();

            stack.Push(root);

            while (stack.Count > 0)
            {
                ABNode<TKey> node = stack.Pop();

                if (!node.IsLeaf)
                {
                    for (int i = b - 1; i >= 0; i--)
                    {
                        if (node.GetChild(i) != null)
                        {
                            stack.Push(node.GetChild(i));
                        }
                    }
                }

                Console.Write('|');

                for (int i = 0; i < node.KeyCount; i++)
                {
                    Console.Write(node.GetKeyRef(i));
                    Console.Write(' ');
                }
                Console.Write('|');

            }
        }
    }
}
