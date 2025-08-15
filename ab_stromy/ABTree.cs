using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.Collections;

namespace ab_stromy
{
    class ABTree<TKey> where TKey : IComparable<TKey>
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

                    if (data[index].Equals(key))
                    {
                        return node;
                    }

                node = node.GetChild(index);
            }

            return null;
        }

        public void Insert(TKey key)
        {
            
            if(Find(key) != null)
            {
                return;
            }
            else
            {
                ABNode<TKey> leaf = FindLeaf(root, key);

                leaf.InsertKey(key, null, null);

                if(leaf.Overflowed)
                {
                    leaf.Overflow();

                    ABNode<TKey> parent = leaf.GetParent();

                    while (parent.Overflowed)
                    {
                        parent.Overflow();
                        parent = parent.GetParent();
                    }

                    if (parent.GetParent() == null)
                    {
                        this.root = parent;
                    }
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

            if (node == null) { return; }

            if (!node.IsLeaf)
            {
                int indexOfKey = node.FindClosestIndex(key);
                ABNode<TKey> successor = FindSuccessor(node, indexOfKey);

                TKey temp = successor.GetKeyRef(0);
                successor.GetKeys()[0] = key;
                node.GetKeys()[indexOfKey] = temp;
                
                node = successor;
            }

            node.DeleteKey(key, null);

            while (node.Underflowed && root != node)
            {
                node.Underflow();

                if(node.GetParent().KeyCount == 0)
                {
                    root = node.GetParent().GetChild(0);

                    if(root.GetChild(0) == null)
                    {
                        root.leaf = true;
                    }
                    break;
                }

                node = node.GetParent();
            }
        }

        private ABNode<TKey> FindSuccessor(ABNode<TKey> node, int index)
        {
            ABNode<TKey> child = node.GetChild(index);

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
        
    }
}
