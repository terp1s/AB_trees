using System.Collections.Generic;
using System.Collections;

namespace ab_stromy
{
    /// <summary>
    /// (a,b)-tree
    /// </summary>
    /// <typeparam name="TKey">data type of keys</typeparam>
    public class ABTree<TKey> where TKey : IComparable<TKey>
    {
        private readonly int a;
        private readonly int b;

        private ABNode<TKey> _root;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="a">minimum amount of children per node</param>
        /// <param name="b">maximum amount of children per node</param>
        public ABTree(int a, int b) 
        {
            this.a = a;
            this.b = b;
            _root = new ABNode<TKey>(a, b, true);         
        }

        /// <summary>
        /// finds node with given key
        /// </summary>
        /// <param name="key">key to find</param>
        /// <returns>node with the key, if it is in tree, null otherwise</returns>
        public ABNode<TKey> Find(TKey key)
        {
            ABNode<TKey> node = _root;

            while (node != null)
            {
                int index = node.FindClosestIndex(key);

                if (index < node.KeyCount && node.Keys[index].Equals(key))
                {
                    return node;
                }

                node = node.GetChild(index);
            }
            return null;
        }

        /// <summary>
        /// inserts key into the tree
        /// </summary>
        /// <param name="key">key to insert</param>
        public void Insert(TKey key)
        {
            if (Find(key) != null) return; // already exists

            ABNode<TKey> node = FindLeaf(_root, key);
            node.InsertKey(key, null, null);

            while (node.Overflowed)
            {
                node.Overflow();
                node = node.Parent;
            }

            if (node.Parent == null)
            {
                _root = node;
            }
        }

        /// <summary>
        /// finds leaf where key is supposed to go
        /// </summary>
        /// <param name="node">root of subtree that key is to be found</param>
        /// <param name="key">key to be found</param>
        /// <returns>leaf node containing the key</returns>
        private ABNode<TKey> FindLeaf(ABNode<TKey> node, TKey key)
        {
            while (!node.IsLeaf)
            {
                int index = node.FindClosestIndex(key);
                node = node.GetChild(index);
            }
            return node;
        }
        
        /// <summary>
        /// deletes given key from tree
        /// </summary>
        /// <param name="key">key for deletion</param>
        public void Delete(TKey key)
        {
            var node = Find(key);
            if (node == null) return;

            if (node == _root && node.IsLeaf)
            {
                node.DeleteKey(key, null);
                return;
            }

            DeleteInternal(node, key);
        }

        /// <summary>
        /// handles deletion if tree has more than 1 level
        /// </summary>
        /// <param name="node">node containing the key</param>
        /// <param name="key">key for deletion</param>
        private void DeleteInternal(ABNode<TKey> node, TKey key)
        {
            int indexInParent;

            if (!node.IsLeaf)
            {
                int keyIndex = node.FindClosestIndex(key);
                var successor = FindSuccessor(node, keyIndex);

                TKey temp = successor.GetKeyRef(0);
                successor.Keys[0] = key;
                node.Keys[keyIndex] = temp;

                node = successor;
                indexInParent = node.Parent.FindClosestIndex(key) + 1;
            }
            else
            {
                indexInParent = node.Parent.FindClosestIndex(key);
            }

            node.DeleteKey(key, null);
            FixUnderflow(node, indexInParent);
        }

        /// <summary>
        /// checks if node has underflown and fixes it
        /// </summary>
        /// <param name="node">node suspicious of underflowing</param>
        /// <param name="indexInParent">index in Parent.Children that refers to the node</param>
        private void FixUnderflow(ABNode<TKey> node, int indexInParent)
        {
            while (node.Underflowed && node != _root)
            {
                node.HandleUnderflow(indexInParent);
                node = node.Parent;

                if (node.Parent != null)
                    indexInParent = node.Parent.FindClosestIndex(node.GetKeyRef(0));
            }

            if (node.KeyCount == 0)
            {
                if (_root.GetChild(0) == null)
                    _root = new ABNode<TKey>(a, b, true); // strom prázdný

                _root = node.GetChild(0);
                _root.Parent = null;
            }
        }

        /// <summary>
        /// Finds node that contains the successor of a key at _keys[index]
        /// </summary>
        /// <param name="node">node with the key</param>
        /// <param name="index">index of the key</param>
        /// <returns>node with the key successor</returns>
        private ABNode<TKey> FindSuccessor(ABNode<TKey> node, int index)
        {
            ABNode<TKey> child = node.GetChild(index + 1);

            while (!child.IsLeaf)
            {
                child = child.GetChild(0);
            }
            return child;
        }

        /// <summary>
        /// Finds key of minimal value
        /// </summary>
        /// <returns>minimum of keys</returns>
        public TKey FindMin()
        {
            ABNode<TKey> node = _root;

            while(node.GetChild(0) != null)
            {
                node = node.GetChild(0);
            }
            
            return node.GetKeyRef(0);
        }

        /// <summary>
        /// Finds maximum of keys
        /// </summary>
        /// <returns>maximum of keys</returns>
        public TKey FindMax()
        {
            ABNode<TKey> node = _root;

            while (node.GetChild(node.KeyCount) != null)
            {
                node = node.GetChild(node.KeyCount);
            }

            return node.GetKeyRef(node.KeyCount - 1);
        }
        
        /// <summary>
        /// prints tree. Each line contains nodes on the same level
        /// one node is represented for example as |2 5 10|
        /// children of one node are represented for example as || |2| |5 10| ||
        /// </summary>
        public void PrintNodes()
        {
            if(_root == null || _root.KeyCount <= 0) { Console.WriteLine("empty"); return; }

            ABNode<TKey> node = _root;

            int depth = 1;

            while(node.GetChild(0) != null)
            {
                depth++;
                node = node.GetChild(0);
            }

            List<ABNode<TKey>>[] levels = new List<ABNode<TKey> >[depth];
            levels[0] = new List<ABNode<TKey>> { _root };
            _root.Print();
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
    }
}
