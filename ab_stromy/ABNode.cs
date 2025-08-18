using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.Threading.Channels;

namespace ab_stromy
{
    public class ABNode<TKey> where TKey : IComparable<TKey>
    {
        private ABNode<TKey>[] children;
        private ABNode<TKey> parent;
        private TKey[] keys;
        private int n { get; set; }
        private int a;
        private int b;
        internal bool leaf;


        public ABNode(int a, int b)
        {
            this.a = a;
            this.b = b;
            children = new ABNode<TKey>[b + 1];
            keys = new TKey[b];
        }

        internal TKey[] GetKeys() => keys;
        internal ref TKey GetKeyRef(int index) => ref keys[index];
        internal int KeyCount => n;
        internal bool IsLeaf => leaf;
        internal bool Overflowed => n > b - 1;
        internal bool Underflowed => n < a - 1;
        internal ABNode<TKey> GetChild(int index) => children[index];
        internal ABNode<TKey> GetParent() => parent;

        internal ABNode<TKey>[] GetChildren() => children;

        internal void Overflow()
        {
            int half = (b - 1) / 2;
            TKey TraverseUp = keys[half];
            ABNode<TKey> newNode = new ABNode<TKey>(a, b);

            if (IsLeaf)
            {
                newNode.leaf = true;
            }
            else
            {
                newNode.leaf = false;
            }

            int i = half + 1;

            while (i < n)
            {
                newNode.keys[i - half - 1] = keys[i];
                newNode.n++;
                i++;
            }


            if (!IsLeaf)
            {
                i = half + 1;

                while (i < n + 1)
                {
                    newNode.children[i - half - 1] = children[i];
                    children[i].parent = newNode;
                    i++;
                }
            }

            this.n = half;

            if (parent == null)
            {
                parent = new ABNode<TKey>(a, b);
                parent.leaf = false;
            }

            parent.InsertKey(TraverseUp, this, newNode);
        }

        internal void Print()
        {
            Console.Write('|');

            for (int i = 0; i < KeyCount - 1; i++)
            {
                Console.Write(keys[i]);
                Console.Write(' ');
            }
            Console.Write(keys[KeyCount - 1]);

            Console.Write('|');
        }

        

        internal void InsertKey(TKey key, ABNode<TKey> leftChild, ABNode<TKey> rightChild)
        {
            
            if (n == 0)
            {
                n = 1;
                keys[0] = key;

                children[0] = leftChild;
                children[1] = rightChild;

                if(leftChild != null)
                {
                    leftChild.parent = this;
                }

                if(rightChild != null)
                {
                    rightChild.parent = this;
                }

                return;
            }
            

            int index = FindClosestIndex(key);

            for (int i = n; i > index; i--)
            {
                this.keys[i] = keys[i - 1];

                if (!IsLeaf)
                {
                    this.children[i + 1] = this.children[i];
                }

            }

            this.keys[index] = key;

            if (!IsLeaf)
            {
                this.children[index] = leftChild;
                this.children[index + 1] = rightChild;

                if (leftChild != null)
                {
                    leftChild.parent = this;
                }

                if (rightChild != null)
                {
                    rightChild.parent = this;
                }
            }
            
            n++;
        }
        
        internal void DeleteKey(TKey key, ABNode<TKey> newNode)
        {
            int index = FindClosestIndex(key);

            if (!keys[index].Equals(key))
            {
                return; // key not in node
            }

            int i = index;

            while (i < n - 1) //move keys backwards
            {
                keys[i] = keys[i + 1];
                i++;
            }

            if (!IsLeaf) //move children as well, if they exist
            {
                children[index] = newNode;
                newNode.parent = this;

                i = index + 1;

                while (i < n)
                {
                    children[i] = children[i + 1];
                    i++;
                }
            }

            n--;
        }
        internal int FindClosestIndex(TKey key)
        {
            int i = 0;

            while (key.CompareTo(keys[i]) > 0 && i < KeyCount) { i++; }

            return i;
        }

        internal void Underflow(int indexInParent)
        {
            if(indexInParent > 0) //a left brother exists
            {
                ABNode<TKey> leftBrother = parent.GetChild(indexInParent - 1);

                if(leftBrother.KeyCount == a - 1) //cannot borrow key from brother, as it will underflow
                {
                    MergeWithLeftBrother(indexInParent, leftBrother);
                }
                else
                {
                    BorrowKeyFromLeft(leftBrother, indexInParent);
                }
            } 
            else if (indexInParent <= n) //a right brother exists
            {
                ABNode<TKey> rightBrother = parent.GetChild(indexInParent + 1);

                if (rightBrother.KeyCount == a - 1) //cannot borrow key from brother, as it will underflow
                {
                    MergeWithRightBrother(indexInParent, rightBrother);
                }
                else
                {
                    BorrowKeyFromRight(rightBrother, indexInParent);
                }
            }
        }

        internal void BorrowKeyFromLeft(ABNode<TKey> leftBrother, int indexInParent)
        {
            TKey brotherKey = leftBrother.GetKeyRef(leftBrother.KeyCount - 1);
            TKey parentKey = parent.GetKeyRef(indexInParent - 1);

            parent.GetKeys()[indexInParent - 1] = brotherKey;
            leftBrother.n--;

            for (int i = KeyCount; i < n; i++)
            {
                keys[i+1] = keys[i];
            }

            keys[0] = parentKey;
            n++;
        }

        internal void BorrowKeyFromRight(ABNode<TKey> rightBrother, int indexInParent)
        {
            TKey brotherKey = rightBrother.GetKeyRef(0);
            TKey parentKey = parent.GetKeyRef(indexInParent);

            parent.GetKeys()[indexInParent] = brotherKey;
            keys[n] = parentKey;

            for (int i = rightBrother.KeyCount; i < n; i++)
            {
                rightBrother.keys[i + 1] = rightBrother.keys[i];
            }

            rightBrother.n--;
            n++;
        }

        internal void MergeWithLeftBrother(int indexInParent, ABNode<TKey> leftBrother)
        {
            TKey parentKey = parent.GetKeyRef(indexInParent - 1);

            ABNode<TKey> newNode = MergeNodes(leftBrother, this, parentKey);
           
            parent.DeleteKey(parentKey, newNode);
        }

        internal void MergeWithRightBrother(int indexInParent, ABNode<TKey> rightBrother)
        {
            TKey parentKey = parent.GetKeyRef(indexInParent);

            ABNode<TKey> newNode = MergeNodes(this, rightBrother, parentKey);

            parent.DeleteKey(parentKey, newNode);
        }

        private ABNode<TKey> MergeNodes(ABNode<TKey> firstNode, ABNode<TKey> secondNode, TKey middleValue)
        {
            ABNode<TKey> merged = new ABNode<TKey>(a, b);

            if (firstNode.IsLeaf) { merged.leaf = true;}

            for (int i = 0; i < firstNode.KeyCount; i++)
            {
                merged.keys[i] = firstNode.keys[i];
                merged.children[i] = firstNode.children[i];
            }

            merged.children[firstNode.KeyCount] = firstNode.children[firstNode.KeyCount];
            merged.keys[firstNode.KeyCount] = middleValue;

            for (int i = 0; i < secondNode.KeyCount; i++)
            {
                merged.keys[i + firstNode.KeyCount + 1] = secondNode.keys[i];
                merged.children[i + firstNode.KeyCount + 1] = secondNode.children[i];
            }

            merged.children[secondNode.KeyCount + firstNode.KeyCount + 1] = secondNode.children[secondNode.KeyCount];

            merged.n = firstNode.KeyCount + secondNode.KeyCount + 1;

            for(int i = 0; i <= merged.n; i++)
            {
                if(merged.children[i] != null)
                {
                    merged.children[i].parent = merged;
                }
            }

            return merged;
        }
    }
}
