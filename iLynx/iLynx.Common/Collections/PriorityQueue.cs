﻿using System.Collections.Generic;
using System.Linq;

namespace iLynx.Common.Collections
{
    public interface IQueue<TQueue>
    {
        TQueue Dequeue();
        void Enqueue(TQueue value);
        int Count { get; }
    }

    public enum Priority
    {
        Lowest = -2,
        Low = -1,
        Normal = 0,
        High = 1,
        Highest = 2,
    }

    public class PriorityItem<T>
    {
        public Priority Priority { get; private set; }
        public T Item { get; private set; }

        public PriorityItem(Priority priority,
                            T item)
        {
            Priority = priority;
            Item = item;
        }
    }

    public interface IPriorityQueue<TQueue> : IQueue<PriorityItem<TQueue>>
    {
        void Enqueue(TQueue item,
                     Priority priority = Priority.Normal);

        TQueue RawDequeue();
    }

    public class PriorityQueue<TQueue> : IPriorityQueue<TQueue>
    {
        private readonly SortedList<Priority, Queue<PriorityItem<TQueue>>> queues = new SortedList<Priority, Queue<PriorityItem<TQueue>>>();

        #region Implementation of IQueue<PriorityItem<TQueue>>

        public PriorityItem<TQueue> Dequeue()
        {
            lock (queues)
            {
                for (var i = Priority.Highest; i > Priority.Lowest - 1; --i)
                {
                    if (!queues.ContainsKey(i)) continue;
                    var q = queues[i];
                    if (q.Count < 1) continue;
                    return q.Dequeue();
                }
            }
            return null;
        }

        public void Enqueue(PriorityItem<TQueue> value)
        {
            value.Guard("value");
            lock (queues)
            {
                if (!queues.ContainsKey(value.Priority))
                    queues.Add(value.Priority, new Queue<PriorityItem<TQueue>>());
                queues[value.Priority].Enqueue(value);
            }
        }

        public void Enqueue(TQueue item,
                            Priority priority = Priority.Normal)
        {
            Enqueue(new PriorityItem<TQueue>(priority, item));
        }

        public TQueue RawDequeue()
        {
            var result = Dequeue();
            return null == result ? default(TQueue) : result.Item;
        }

        public int Count
        {
            get { return queues.Sum(x => x.Value.Count); }
        }

        #endregion
    }
}
