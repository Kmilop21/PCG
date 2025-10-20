using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace Container
{
    public class PriorityQueue<TElement, TComparer> : IEnumerable<TElement>
        where TComparer : IComparer<TElement>
    {
        private readonly List<TElement> elements;
        private TComparer comparer;
        public TElement Top { get { return elements[^1]; } }
        public int Count { get { return elements.Count; } }
        public PriorityQueue(TComparer comparer)
        {
            elements = new List<TElement>();
            this.comparer = comparer;
        }

        public void Enqueue(TElement element)
        {
            if(elements.Count == 0 || comparer.Compare(element, elements[^1]) > 0)
            {
                elements.Add(element);
                return;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                if (comparer.Compare(element, elements[i]) <= 0)
                {
                    elements.Insert(i, element);
                    return;
                }
            }
        }

        public void Dequeue()
        {
            elements.RemoveAt(elements.Count - 1);
        }

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return elements.GetEnumerator();
        }
    }

    public class PriorityQueue<TElement> : PriorityQueue<TElement, TElement> 
        where TElement : IComparer<TElement>
    {
        public PriorityQueue() : base((TElement)Activator.CreateInstance(typeof(TElement)))
        {

        }
    }
}
