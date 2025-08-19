

namespace ab_stromy
{
    /// <summary>
    /// node of an (a,b)-tree
    /// </summary>
    /// <typeparam name="TKey">data type of keys</typeparam>
    public class ABNode<TKey> where TKey : IComparable<TKey>
    {
        private readonly ABNode<TKey>[] _children;
        private readonly TKey[] _keys;
        private int _keyCount;
        private readonly int a;
        private readonly int b;

        internal ABNode<TKey> Parent { get; set; }
        internal bool IsLeaf { get; set; }


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="minChildren">minimum amount of children</param>
        /// <param name="maxChildren">maximum amount of children</param>
        /// <param name="isLeaf"></param>
        internal ABNode(int minChildren, int maxChildren, bool isLeaf)
        {
            a = minChildren;
            b = maxChildren;
            _children = new ABNode<TKey>[maxChildren + 1];
            _keys = new TKey[maxChildren];
            IsLeaf = isLeaf;
        }

        internal int KeyCount => _keyCount;
        internal bool Overflowed => _keyCount > b - 1;
        internal bool Underflowed => _keyCount < a - 1;

        internal TKey[] Keys => _keys;
        internal ref TKey GetKeyRef(int index) => ref _keys[index];
        internal ABNode<TKey> GetChild(int index) => _children[index];
        internal ABNode<TKey>[] Children => _children;

        /// <summary>
        /// handles node overflowing - amount of children exceeds b
        /// splits node in half and propagates key upwards, until nodes stop overflowing or a new root is created
        /// </summary>
        internal void Overflow()
        {
            int half = (b - 1) / 2;
            TKey promotedKey = _keys[half];
            ABNode<TKey> rightNode = SplitRightHalf(half);

         
            if (Parent == null)
            {
                Parent = new ABNode<TKey>(a, b, false);
            }

            Parent.InsertKey(promotedKey, this, rightNode);
        }

        /// <summary>
        /// splits node in half, including children, if it has any
        /// </summary>
        /// <param name="splitIndex">index where node is to be split</param>
        /// <returns>node that contains the second "half" of the given node</returns>
        private ABNode<TKey> SplitRightHalf(int splitIndex)
        {
            var newNode = new ABNode<TKey>(a, b, IsLeaf);

            for (int i = splitIndex + 1; i < _keyCount; i++)
            {
                newNode._keys[i - splitIndex - 1] = _keys[i];
                newNode._keyCount++;
            }

            if (!IsLeaf)
            {
                for (int i = splitIndex + 1; i < _keyCount + 1; i++)
                {
                    newNode._children[i - splitIndex - 1] = _children[i];
                    if (_children[i] != null) _children[i].Parent = newNode;
                }
            }

            _keyCount = splitIndex;
            return newNode;
        }

        /// <summary>
        /// prints keys of a node, seperated by a space
        /// example of a printed key: |5 10|
        /// </summary>
        internal void Print()
        {
            Console.Write('|');

            for (int i = 0; i < KeyCount - 1; i++)
            {
                Console.Write(_keys[i]);
                Console.Write(' ');
            }

            Console.Write(_keys[KeyCount - 1]);

            Console.Write('|');
        }

        /// <summary>
        /// inserts key into a node
        /// </summary>
        /// <param name="key">key to be inserted</param>
        /// <param name="leftChild">new left child of the key</param>
        /// <param name="rightChild">new right child of the key</param>
        internal void InsertKey(TKey key, ABNode<TKey> leftChild, ABNode<TKey> rightChild)
        {
            if(_keyCount == 0)
            {
                _keys[0] = key;
                _children[0] = leftChild;
                _children[1] = rightChild;
                _keyCount = 1;

                if (leftChild != null) leftChild.Parent = this;
                if (rightChild != null) rightChild.Parent = this;
                IsLeaf = leftChild == null && rightChild == null;
                return;
            }

            int index = FindClosestIndex(key);

            for (int i = _keyCount; i > index; i--)
            {
                _keys[i] = _keys[i - 1];

                if (!IsLeaf)
                {
                    _children[i + 1] = _children[i];
                }

            }

            _keys[index] = key;

            if (!IsLeaf)
            {
                _children[index] = leftChild;
                _children[index + 1] = rightChild;
                if (leftChild != null) leftChild.Parent = this;
                if (rightChild != null) rightChild.Parent = this;
            }

            _keyCount++;
        }
        
        /// <summary>
        /// deletes key from node
        /// </summary>
        /// <param name="key">key to delete</param>
        /// <param name="newNode">new child to replace two children associated with the deleted node</param>
        internal void DeleteKey(TKey key, ABNode<TKey> newNode)
        {
            int index = FindClosestIndex(key);
            if (index >= _keyCount || !_keys[index].Equals(key)) return; //key not in node

            for (int i = index; i < _keyCount - 1; i++)
            {
                _keys[i] = _keys[i + 1];
            }

            if (!IsLeaf)
            {
                _children[index] = newNode;
                if (newNode != null) newNode.Parent = this;

                for (int i = index + 1; i < _keyCount; i++)
                {
                    _children[i] = _children[i + 1];
                }
            }

            _keyCount--;
        }

        /// <summary>
        /// finds either index where given key is, or should be, depending if it is in the node or not.
        /// </summary>
        /// <param name="key">key to be found</param>
        /// <returns>index of the supposed place of the key</returns>
        internal int FindClosestIndex(TKey key)
        {
            int i = 0;
            while (i < _keyCount && key.CompareTo(_keys[i]) > 0)
                i++;
            return i;
        }

        /// <summary>
        /// handles underflowing of a key - node has less children than a
        /// </summary>
        /// <param name="indexInParent">index in Parent.Children that leads to this node</param>
        internal void HandleUnderflow(int indexInParent)
        {
            if(indexInParent > 0) //a left brother exists
            {
                HandleUnderflowWithLeftSibling(indexInParent);
            } 
            else if (indexInParent <= _keyCount) //a right brother exists
            {
                HandleUnderflowWithRightSibling(indexInParent);
            }
        }

        /// <summary>
        /// underflown node has a left sibling to borrow a key form or merge with.
        /// </summary>
        /// <param name="indexInParent">index in Parent.Children that leads to this node</param>
        private void HandleUnderflowWithLeftSibling(int indexInParent)
        {
            var leftSibling = Parent.GetChild(indexInParent - 1);
            if (leftSibling.KeyCount == a - 1)
            {
                MergeNodes(leftSibling, this, Parent.GetKeyRef(indexInParent - 1));
            }
            else
            {
                BorrowKeyFromLeft(leftSibling, indexInParent);
            }
        }

        /// <summary>
        /// underflown node has a right sibling to borrow a key form or merge with.
        /// </summary>
        /// <param name="indexInParent">index in Parent.Children that leads to this node</param>
        private void HandleUnderflowWithRightSibling(int indexInParent)
        {
            var rightSibling = Parent.GetChild(indexInParent + 1);
            if (rightSibling.KeyCount == a - 1)
            {
                MergeNodes(this, rightSibling, Parent.GetKeyRef(indexInParent));
            }
            else
            {
                BorrowKeyFromRight(rightSibling, indexInParent);
            }
        }

        /// <summary>
        /// rotation of keys to handle underflowing of a node.
        /// this node gets parent's node and parent gets left sibling's key
        /// </summary>
        /// <param name="leftBrother">left sibling</param>
        /// <param name="indexInParent">index in Parent.Children that leads to this node</param>
        internal void BorrowKeyFromLeft(ABNode<TKey> leftBrother, int indexInParent)
        {
            TKey brotherKey = leftBrother.GetKeyRef(leftBrother.KeyCount - 1);
            TKey ParentKey = Parent.GetKeyRef(indexInParent - 1);
            ABNode<TKey> AdoptedKid = leftBrother.GetChild(leftBrother.KeyCount);

            Parent.Keys[indexInParent - 1] = brotherKey;
            leftBrother._keyCount--;

            if(a > 2)
            {
                _children[KeyCount] = _children[KeyCount - 1];
            }

            for (int i = KeyCount - 1; i > 0; i++)
            {
                _keys[i] = _keys[i - 1];
                _children[i] = Children[i - 1];
            }

            _children[0] = AdoptedKid;
            _keys[0] = ParentKey;
            _keyCount++;
        }

        /// <summary>
        /// rotation of keys to handle underflowing of a node.
        /// this node gets parent's node and parent gets right sibling's key
        /// </summary>
        /// <param name="rightBrother">right sibling</param>
        /// <param name="indexInParent">index in Parent.Children that leads to this node</param>
        internal void BorrowKeyFromRight(ABNode<TKey> rightBrother, int indexInParent)
        {
            TKey brotherKey = rightBrother.GetKeyRef(0);
            TKey ParentKey = Parent.GetKeyRef(indexInParent);
            ABNode<TKey> AdoptedKid = rightBrother.GetChild(0);

            Parent.Keys[indexInParent] = brotherKey;
            _keys[_keyCount] = ParentKey;

            for (int i = 0; i < rightBrother.KeyCount - 1; i++)
            {
                rightBrother._keys[i] = rightBrother._keys[i + 1];
                rightBrother._children[i] = rightBrother._children[i + 1];
            }

            rightBrother.Children[rightBrother.KeyCount - 1] = rightBrother.Children[rightBrother.KeyCount];
            _children[_keyCount + 1] = AdoptedKid;
            rightBrother._keyCount--;
            _keyCount++;
        }

        /// <summary>
        /// merges two nodes
        /// </summary>
        /// <param name="left">left node</param>
        /// <param name="right">right node</param>
        /// <param name="parentKey">key in parent connecting them</param>
        private void MergeNodes(ABNode<TKey> left, ABNode<TKey> right, TKey parentKey)
        {
            int offset = left.KeyCount;

            left._keys[offset] = parentKey;
            for (int i = 0; i < right.KeyCount; i++)
                left._keys[offset + 1 + i] = right._keys[i];

            for (int i = 0; i <= right.KeyCount; i++)
            {
                left._children[offset + 1 + i] = right._children[i];
                if (left._children[offset + 1 + i] != null)
                    left._children[offset + 1 + i].Parent = left;
            }

            left._keyCount += right.KeyCount + 1;
            Parent.DeleteKey(parentKey, left);
        }
    }
}
