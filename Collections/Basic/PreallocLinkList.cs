
//#define VERIFY_LIST

using System;
using System.Collections;
using System.Collections.Generic;

namespace Cratesmith.Utils
{
        public class PreallocLinkList<T> : ICollection<T>
    {
	    private static object s_lock = new Object();
	    private static Node s_freeHead = null;
        private Node m_listHead = null;
        private int m_count = 0;

        public class Node
        {
            internal Node m_prev;
            internal Node m_next;

            public Node Prev
            {
                get { return m_prev; }
            }

            public Node Next
            {
                get { return m_next; }
            }

            public T Value;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private PreallocLinkList<T> m_list;
            private Node m_current;

            public Enumerator(PreallocLinkList<T> list)
            {
                m_list = list;
                m_current = null;
            }

            public bool MoveNext()
            {
                if (m_current == m_list.Last)
                {
                    return false;
                }

                m_current = m_current == null ? m_list.First : m_current.Next;
                return true;
            }

            public void Reset()
            {
                m_current = m_list.First;
            }

	        public T Current
            {
                get { return m_current.Value; }
            }
	        
			object IEnumerator.Current
            {
                get { return Current; }
            }

	        public void Dispose()
            {
            }
        }

        public Node First
        {
            get { return m_listHead; }
        }

        public Node Last
        {
            get { return m_listHead != null ? m_listHead.Prev : null; }
        }

        public PreallocLinkList()
        {
            CreateFreeNodes(0);
        }

        public PreallocLinkList(int capacity)
        {
            CreateFreeNodes(capacity);
        }

        private static void CreateFreeNodes(int numNodes)
        {
	        lock (s_lock)
	        {
		        for (int i = 0; i < numNodes; i++)
		        {
			        var prev = s_freeHead;
			        s_freeHead = new Node();
			        if (prev != null)
			        {
				        s_freeHead.m_next = prev;
			        }
		        }		        
	        }
        }

        public void RemoveNode(Node node)
        {
            node.m_prev.m_next = node.m_next;
            node.m_next.m_prev = node.m_prev;

            if (node == m_listHead)
            {
                if (m_listHead.Next == m_listHead)
                {
                    m_listHead = null;
                }
                else
                {
                    m_listHead = m_listHead.Next;
                }
            }
            --m_count;

            node.m_prev = null;
            node.m_next = null;

	        lock (s_lock)
	        {
		        var prevFreeHead = s_freeHead;
		        s_freeHead = node;
		        if (prevFreeHead != null)
		        {
			        s_freeHead.m_next = prevFreeHead;
			        s_freeHead.m_prev = null;
		        }
	        }

	        node.Value = default(T);

            CheckAllNodes();
        }

	    public Enumerator GetEnumerator()
	    {
			return new Enumerator(this);
	    }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            AddLast(item);
        }

        private Node CreateNode(T itemValue)
        {
	        lock (s_lock)
	        {
		        if (s_freeHead != null)
		        {
			        var node = s_freeHead;
			        s_freeHead = s_freeHead.Next;
			        node.m_prev = null;
			        node.m_next = null;
			        node.Value = itemValue;
			        return node;
		        }
		        else
		        {
			        var node = new Node();
			        node.Value = itemValue;
			        node.m_prev = null;
			        node.m_next = null;
			        return node;
		        }
	        }
        }

        public void AddFirst(T item)
        {
            var node = CreateNode(item);
            if (m_listHead == null)
            {
                m_listHead = node;
                m_listHead.m_prev = node;
                m_listHead.m_next = node;
            }
            else
            {
                PlaceNodeAfter(node, m_listHead.m_prev);
                m_listHead = node;
            }
            ++m_count;

            CheckAllNodes();
        }

        public void AddLast(T item)
        {
            var node = CreateNode(item);
            if (m_listHead == null)
            {
                m_listHead = node;
                m_listHead.m_prev = node;
                m_listHead.m_next = node;
            }
            else
            {
                PlaceNodeAfter(node, m_listHead.m_prev);
            }
            ++m_count;

            CheckAllNodes();
        }

        private static void PlaceNodeAfter(Node node, Node afterNode)
        {
            PlaceNode(node, afterNode.Next, afterNode);
        }

        private static void PlaceNodeBefore(Node node, Node beforeNode)
        {
            PlaceNode(node, beforeNode, beforeNode.Prev);
        }

        private static void PlaceNode(Node node, Node beforeNode, Node afterNode)
        {
            afterNode.m_next = node;
            node.m_prev = afterNode;

            beforeNode.m_prev = node;
            node.m_next = beforeNode;
        }

        public void Clear()
        {
            while (m_listHead != null)
            {
                RemoveNode(m_listHead);
            }
        }

        public bool Contains(T item)
        {
            if (m_listHead == null)
            {
                return false;
            }

            var current = First;
            do
            {
                if (current.Value.Equals(item))
                {
                    return true;
                }

                current = current.m_next;
            } while (current != m_listHead);

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (Count + arrayIndex < array.Length)
            {
                throw new System.ArgumentException("insufficient room in array");
            }


            var current = First;
            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = current.Value;
                current = current.m_next;
            }
        }

        public bool Remove(T item)
        {
            if (m_listHead == null)
            {
                return false;
            }

            var current = First;
            do
            {
                if (current.Value.Equals(item))
                {
                    RemoveNode(current);
                    return true;
                }

                current = current.m_next;
            } while (current != m_listHead);

            return false;
        }

        public int Count
        {
            get { return m_count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

	    public void RemoveFirst()
        {
            if (m_listHead == null)
            {
                return;
            }
            RemoveNode(First);
        }

        public void RemoveLast()
        {
            if (m_listHead == null)
            {
                return;
            }
            RemoveNode(Last);
        }

        public void AddAfter(Node afterNode, T item)
        {
            var node = CreateNode(item);
            PlaceNodeAfter(node, afterNode);
            ++m_count;

            CheckAllNodes();
        }

        public void AddBefore(Node beforeNode, T item)
        {
            var node = CreateNode(item);
            PlaceNodeBefore(node, beforeNode);
	        if (beforeNode == m_listHead)
	        {
		        m_listHead = node;
	        }
            ++m_count;

            CheckAllNodes();
        }

        public void AddSorted(T item, Comparison<T> comparison=null)
        {
	        if (comparison == null)
	        {
		        comparison = Comparer<T>.Default.Compare;
	        }

            if (m_listHead == null)
            {
                Add(item);
                return;
            }        
            
            var current = First;
            do
            {
                if (comparison(item, current.Value) < 0)
                {                
                    AddBefore(current, item);
                    return;
                }
	            current = current.Next;
            } while (current!=m_listHead);

            AddLast(item);
        }

        private void CheckAllNodes()
        {
#if VERIFY_LIST
        var nodeCount = 0;
        if (m_listHead == null)
        {
            return;
        }

        var current = First;
        do
        {
            CheckNode(current);
            current = current.m_next;
            ++nodeCount;
        } while (current!=First);   

        Assert.AreEqual(nodeCount, m_count, "node count is incorrect");
#endif
        }

        private void CheckNode(Node node)
        {
#if VERIFY_LIST
        if (node == null)
        {
            Assert.IsNull(m_listHead, "null should only allowed in list is if list is empty");
            return;
        }
        
        if (m_count > 2)
        {
            Assert.AreNotEqual(node.Prev,node.Next, "with more than 2 items, prev!=next");                           
        }

        if (m_listHead != node)
        {
            Assert.AreNotEqual(node,node.Next, "Only single node lists should have node==next");           
            Assert.AreNotEqual(node,node.Prev, "Only single node lists should have node==prev");           
        }          
        
        Assert.IsNotNull(node.Next, "node links must never be null");
        Assert.IsNotNull(node.Prev, "node links must never be null");
        Assert.AreEqual(node.Next.Prev, node, "node next link must match");
        Assert.AreEqual(node.Prev.Next, node, "node prev link must match");
#endif
        }

        public Node FindFirstNode(Func<T, bool> func)
        {
            var current = m_listHead;
            if (current == null)
            {
                return null;
            }

            do
            {
                var next = current.Next;
                if (func(current.Value))
                {
                    return current;
                }
                current = next;
            } while (current != m_listHead);
            return null;
        }

        public T FindFirst(Func<T, bool> func)
        {
            var node = FindFirstNode(func);
            return node != null ? node.Value : default(T);
        }

        public void RemoveFirst(Func<T, bool> func)
        {
            var node = FindFirstNode(func);
            if (node != null)
            {
                RemoveNode(node);
            }
        }

        public void RemoveAll(Func<T, bool> func)
        {
            var current = m_listHead;
            Node marker = null;
            if (current == null)
            {
                return;
            }

            do
            {
                if (func(current.Value))
                {
                    do
                    {
                        var next = current.m_next;
                        RemoveNode(current);
                        current = next;
                    } while (m_count > 0 && func(current.Value));
                }
                else
                {
                    if (marker == null)
                    {
                        marker = current;
                    }
                    current = current.m_next;
                }
            } while (m_count > 0 && current != marker);
        }
    }
}