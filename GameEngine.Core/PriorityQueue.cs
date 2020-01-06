using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GameEngine.Core
{
    /// <summary>
    /// A node in the queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct QueueNode<T>
    {
        public T Item;
        public int Priority;
        public int QueueIndex;
    }

    /// <summary>
    /// A priority queue that can enqueue/dequeue based on the specified priority
    /// Based on https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/blob/master/Priority%20Queue/FastPriorityQueue.cs
    /// </summary>
    /// <typeparam name="T">The type of item to queue</typeparam>
    public class PriorityQueue<T> : IEnumerable<T> where T : IEquatable<T>
    {
        public int Count => numNodes;

        private QueueNode<T>[] nodes;
        private int numNodes;

        public PriorityQueue(int initialCapacity = 4)
        {
            nodes = new QueueNode<T>[initialCapacity];
            numNodes = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item, int priority)
        {
            var node = new QueueNode<T>()
            {
                Item = item,
                Priority = priority,
                QueueIndex = numNodes
            };

            if (numNodes == nodes.Length)
                Array.Resize(ref nodes, nodes.Length * 2);

            nodes[numNodes] = node;
            numNodes++;

            CascadeUp(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
            var returnItem = nodes[0];
            if (numNodes == 1)
            {
                numNodes--;
                return returnItem.Item;
            }

            numNodes--;
            var formerLastNode = nodes[numNodes];
            nodes[0] = formerLastNode;
            formerLastNode.QueueIndex = 0;

            CascadeDown(formerLastNode);

            return returnItem.Item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek()
        {
            return nodes[numNodes - 1].Item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            for (var i = 0; i < numNodes; i++)
            {
                if (nodes[i].Item.Equals(item))
                    return true;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < numNodes; i++)
                yield return nodes[i].Item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CascadeUp(QueueNode<T> node)
        {
            int parent;
            if (node.QueueIndex > 1)
            {
                parent = node.QueueIndex >> 1;
                var parentNode = nodes[parent];
                if (HasHigherOrEqualPriority(parentNode, node))
                    return;

                //Node has lower priority value, so move parent down the heap to make room
                nodes[node.QueueIndex] = parentNode;
                parentNode.QueueIndex = node.QueueIndex;

                node.QueueIndex = parent;
            }
            else
            {
                return;
            }

            while (parent > 1)
            {
                parent >>= 1;
                var parentNode = nodes[parent];
                if (HasHigherOrEqualPriority(parentNode, node))
                    break;

                //Node has lower priority value, so move parent down the heap to make room
                nodes[node.QueueIndex] = parentNode;
                parentNode.QueueIndex = node.QueueIndex;

                node.QueueIndex = parent;
            }
            nodes[node.QueueIndex] = node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CascadeDown(QueueNode<T> node)
        {
            //aka Heapify-down
            int finalQueueIndex = node.QueueIndex;
            int childLeftIndex = 2 * finalQueueIndex;

            // If leaf node, we're done
            if (childLeftIndex > numNodes)
            {
                return;
            }

            // Check if the left-child is higher-priority than the current node
            int childRightIndex = childLeftIndex + 1;
            var childLeft = nodes[childLeftIndex];
            if (HasHigherPriority(childLeft, node))
            {
                // Check if there is a right child. If not, swap and finish.
                if (childRightIndex > numNodes)
                {
                    node.QueueIndex = childLeftIndex;
                    childLeft.QueueIndex = finalQueueIndex;
                    nodes[finalQueueIndex] = childLeft;
                    nodes[childLeftIndex] = node;
                    return;
                }
                // Check if the left-child is higher-priority than the right-child
                var childRight = nodes[childRightIndex];
                if (HasHigherPriority(childLeft, childRight))
                {
                    // left is highest, move it up and continue
                    childLeft.QueueIndex = finalQueueIndex;
                    nodes[finalQueueIndex] = childLeft;
                    finalQueueIndex = childLeftIndex;
                }
                else
                {
                    // right is even higher, move it up and continue
                    childRight.QueueIndex = finalQueueIndex;
                    nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
            }
            // Not swapping with left-child, does right-child exist?
            else if (childRightIndex > numNodes)
            {
                return;
            }
            else
            {
                // Check if the right-child is higher-priority than the current node
                var childRight = nodes[childRightIndex];
                if (HasHigherPriority(childRight, node))
                {
                    childRight.QueueIndex = finalQueueIndex;
                    nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
                // Neither child is higher-priority than current, so finish and stop.
                else
                {
                    return;
                }
            }

            while (true)
            {
                childLeftIndex = 2 * finalQueueIndex;

                // If leaf node, we're done
                if (childLeftIndex > numNodes)
                {
                    node.QueueIndex = finalQueueIndex;
                    nodes[finalQueueIndex] = node;
                    break;
                }

                // Check if the left-child is higher-priority than the current node
                childRightIndex = childLeftIndex + 1;
                childLeft = nodes[childLeftIndex];
                if (HasHigherPriority(childLeft, node))
                {
                    // Check if there is a right child. If not, swap and finish.
                    if (childRightIndex > numNodes)
                    {
                        node.QueueIndex = childLeftIndex;
                        childLeft.QueueIndex = finalQueueIndex;
                        nodes[finalQueueIndex] = childLeft;
                        nodes[childLeftIndex] = node;
                        break;
                    }
                    // Check if the left-child is higher-priority than the right-child
                    var childRight = nodes[childRightIndex];
                    if (HasHigherPriority(childLeft, childRight))
                    {
                        // left is highest, move it up and continue
                        childLeft.QueueIndex = finalQueueIndex;
                        nodes[finalQueueIndex] = childLeft;
                        finalQueueIndex = childLeftIndex;
                    }
                    else
                    {
                        // right is even higher, move it up and continue
                        childRight.QueueIndex = finalQueueIndex;
                        nodes[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }
                }
                // Not swapping with left-child, does right-child exist?
                else if (childRightIndex > numNodes)
                {
                    node.QueueIndex = finalQueueIndex;
                    nodes[finalQueueIndex] = node;
                    break;
                }
                else
                {
                    // Check if the right-child is higher-priority than the current node
                    var childRight = nodes[childRightIndex];
                    if (HasHigherPriority(childRight, node))
                    {
                        childRight.QueueIndex = finalQueueIndex;
                        nodes[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }
                    // Neither child is higher-priority than current, so finish and stop.
                    else
                    {
                        node.QueueIndex = finalQueueIndex;
                        nodes[finalQueueIndex] = node;
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasHigherOrEqualPriority(QueueNode<T> higher, QueueNode<T> lower)
        {
            return higher.Priority <= lower.Priority;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasHigherPriority(QueueNode<T> higher, QueueNode<T> lower)
        {
            return higher.Priority < lower.Priority;
        }
    }
}
