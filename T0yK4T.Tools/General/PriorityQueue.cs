using System;
using System.Linq;
using System.Collections.Generic;

namespace T0yK4T.Tools.General
{
    /// <summary>
    /// Does priority queueing
    /// </summary>
    /// <typeparam name="T">The type of the items that are to be queued</typeparam>
    public class PriorityQueue<T>
    {
        private int priorityDefault;
        private Dictionary<int, Queue<QueueItem<T>>> queues = new Dictionary<int, Queue<QueueItem<T>>>();
        private int currentPriority = (int)QueuePriority.Critical;
        private List<int> priorityValues = new List<int>();

        private int returnedCount = 0;

        private int maxPrio = 0;
        private int minPrio = 0;

        /// <summary>
        /// Gets a value indicating the minium possible priority for this instance
        /// </summary>
        public int MinPrio { get { return this.minPrio; } }

        /// <summary>
        /// Gets a vakye indicating the maximum possible priority for this instance
        /// </summary>
        public int MaxPrio { get { return this.maxPrio; } }

        /// <summary>
        /// Values used to define how many items of each we can return before returning a lower priority item
        /// </summary>
        private QueueTweakValues tweaks;

        /// <summary>
        /// Initializes a new instance of <see cref="PriorityQueue{T}"/> with it's <see cref="PriorityQueue{T}.DefaultPriority"/> set to the specified value - <see cref="QueuePriority"/> contains some default values
        /// <para/>
        /// And it's <see cref="QueueTweakValues"/> set to the specified value
        /// </summary>
        /// <param name="priorityDefault"></param>
        /// <param name="tweakValues"></param>
        public PriorityQueue(int priorityDefault, QueueTweakValues tweakValues)
        {
            this.priorityDefault = priorityDefault;
            this.tweaks = tweakValues ?? new QueueTweakValues();
            this.Init();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PriorityQueue{T}"/> with it's <see cref="PriorityQueue{T}.DefaultPriority"/> set to the specified value
        /// <para/>
        /// <see cref="QueuePriority"/> contains some default values
        /// </summary>
        /// <param name="priorityDefault">The default priority of this queue</param>
        public PriorityQueue(int priorityDefault)
        {
            this.priorityDefault = priorityDefault;
            this.tweaks = new QueueTweakValues();
            this.Init();
        }

        /// <summary>
        /// Inititalizes the <see cref="PriorityQueue{T}.queues"/> variable and the <see cref="PriorityQueue{T}.priorityValues"/> variable
        /// <para/>
        /// Uses the <see cref="QueuePriority"/> enum to define preset priorities
        /// </summary>
        private void Init()
        {
            string[] names = Enum.GetNames(typeof(QueuePriority));
            List<QueuePriority> priorities = new List<QueuePriority>();
            foreach (string name in names)
                priorities.Add((QueuePriority)Enum.Parse(typeof(QueuePriority), name));

            foreach (QueuePriority prio in priorities)
            {
                this.queues.Add((int)prio, new Queue<QueueItem<T>>());
                this.priorityValues.Add((int)prio);
            }
        }

        /// <summary>
        /// Enqueues <paramref name="item"/> in the current queue with the default <see cref="QueuePriority"/>
        /// <para/>
        /// The default priority is set in the constructor
        /// <para/>
        /// Or using the <see cref="PriorityQueue{T}.DefaultPriority"/> property
        /// </summary>
        /// <param name="item">The item to queue</param>
        public void Enqueue(T item)
        {
            this.Enqueue(item, this.priorityDefault);
        }

        /// <summary>
        /// Enqueues <paramref name="item"/> in the current queue and sets it's priority to the specified value
        /// </summary>
        /// <param name="item">The item to queue</param>
        /// <param name="priority">The <see cref="QueuePriority"/> of the item</param>
        public void Enqueue(T item, int priority)
        {
            QueueItem<T> qItem = new QueueItem<T>(item, priority);
            if (qItem.Priority != 0)
                this.queues[priority].Enqueue(qItem);
        }

        /// <summary>
        /// Dequeues the next item in the list
        /// <para/>
        /// This method will take items from the queue in the following order:
        /// <para/>
        /// Critical > High > Medium > Low > Ignore
        /// <para/>
        /// Will return default(T) if there are no items in the queue
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            if (this.queues.Values.Where((q) => { return q.Count > 0; }).Count() > 0)
            {
                int nPrio = this.GetNextPriority();
                if (nPrio != this.currentPriority)
                    returnedCount = 0;

                this.currentPriority = nPrio;
                returnedCount++;
                return this.queues[this.currentPriority].Dequeue().Value;
            }
            else
                return default(T);
        }

        private int GetNextPriority()
        {
            int currentVal = this.currentPriority;
            int tweak = this.tweaks.PriorityDivisor;
            int maxRet = currentVal / tweak;
            int retCount = this.returnedCount;
            if (this.returnedCount >= (currentVal / tweaks.PriorityDivisor))
            {
                int nextVal = this.priorityValues.Max();
                switch (this.tweaks.Mode)
                {
                    case QueueMode.PreferHigher:
                        return this.queues.Where((kvp) => { return (retCount < maxRet && kvp.Key == currentVal && (currentVal < maxPrio ? kvp.Key >= maxPrio : kvp.Value.Count > 0)) || (retCount >= maxRet && (kvp.Key < (currentVal <= minPrio ? maxPrio + 1 : currentVal) && kvp.Value.Count > 0)); }).First().Key;
                    case QueueMode.RoundRobin:
                        return this.queues.Where((kvp) => { return (retCount < maxRet && kvp.Key == currentVal) || (retCount >= maxRet && (kvp.Key < (currentVal <= minPrio ? maxPrio + 1 : currentVal) && kvp.Value.Count > 0)) || (returnedCount >= maxRet && ((kvp.Key <= minPrio ? maxPrio + 1 : kvp.Key) > currentVal && kvp.Value.Count > 0)); }).First().Key;
                    case QueueMode.ReverseRoundRobin:
                        return this.queues.Where((kvp) => { return (retCount < maxRet && kvp.Key == currentVal) || (retCount >= maxRet && (kvp.Key > (currentVal >= maxPrio ? minPrio - 1 : currentVal) && kvp.Value.Count > 0)) || (returnedCount >= maxRet && ((kvp.Key >= maxPrio ? minPrio - 1 : kvp.Key) > currentVal && kvp.Value.Count > 0)); }).First().Key;
                }
                return nextVal;
            }
            else
                return this.currentPriority;
        }

        /// <summary>
        /// Gets or Sets the default <see cref="QueuePriority"/> of this queue
        /// </summary>
        public int DefaultPriority
        {
            get { return this.priorityDefault; }
            set { this.priorityDefault = value; }
        }

        /// <summary>
        /// Gets or Sets the <see cref="QueueTweakValues"/> used in this instance
        /// </summary>
        public QueueTweakValues Tweak
        {
            get { return this.tweaks; }
            set { this.tweaks = value; }
        }

        /// <summary>
        /// Gets a value indicating the total amount of items currently in this <see cref="PriorityQueue{T}"/>
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;
                foreach (Queue<QueueItem<T>> q in this.queues.Values)
                    count += q.Count;
                return count;
            }
        }
    }

    /// <summary>
    /// Used internally to manage the queue and it's contents
    /// </summary>
    /// <typeparam name="TSource">The type of item - this will be the same as the one specified in <see cref="PriorityQueue{T}"/> type param</typeparam>
    public class QueueItem<TSource>
    {
        /// <summary>
        /// Gets or Sets the queuepriority of this item
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or Sets the actualy item contained in this instance
        /// </summary>
        public TSource Value { get; set; }

        /// <summary>
        /// To be used as the default
        /// If <paramref name="value"/> is null <paramref name="prio"/> will be set to 0
        /// </summary>
        /// <param name="value">The value that is to be contained in this instance</param>
        /// <param name="prio">The <see cref="QueuePriority"/> of this instance</param>
        public QueueItem(TSource value, int prio)
        {
            this.Priority = prio;
            if (value == null)
                this.Priority = 0;
            this.Value = value;
        }
    }

    /// <summary>
    /// Specifies how many items max of each priority we can return before starting to return lower priority items
    /// <para/>
    /// A value of -1 will return specified type until there are no more items of said priority in the queue
    /// </summary>
    public class QueueTweakValues
    {
        /// <summary>
        /// Inidicates the value that each priority will be divided by to set the maximum number of returned elements of said priority
        /// <para/>
        /// Default: 10
        /// </summary>
        public int PriorityDivisor = 100;

        /// <summary>
        /// Gets or Sets the <see cref="QueueMode"/> of the queue
        /// <para/>
        /// Default: <see cref="QueueMode.PreferHigher"/>
        /// </summary>
        public QueueMode Mode = QueueMode.PreferHigher;
    }

    /// <summary>
    /// Specifies queue priorities that are used in <see cref="PriorityQueue{T}"/>
    /// </summary>
    public enum QueuePriority
    {
        /// <summary>
        /// Specifies a <see cref="QueuePriority.Ignore"/> priority
        /// <para/>
        /// This is the priority that will be set to queueitems whose value is null
        /// <para/>
        /// Anything lower than this will be discarded...
        /// </summary>
        Ignore = 100,

        /// <summary>
        /// Specifies a <see cref="QueuePriority.Low"/> priority
        /// </summary>
        Low = 300,

        /// <summary>
        /// Specifies a <see cref="QueuePriority.Normal"/> priority
        /// </summary>
        Normal = 400,

        /// <summary>
        /// Specifies a <see cref="QueuePriority.High"/> priority
        /// </summary>
        High = 500,

        /// <summary>
        /// Specifies a <see cref="QueuePriority.Critical"/> priority
        /// This is the highest priority available
        /// </summary>
        Critical = 1000,
    }

    /// <summary>
    /// Specifies queue modes for <see cref="PriorityQueue{T}"/>
    /// </summary>
    public enum QueueMode
    {
        /// <summary>
        /// When using this mode,
        /// <para/>
        /// Will switch from Critical -> High -> Medium -> Low -> Ignore -> {Repeat}
        /// </summary>
        RoundRobin,

        /// <summary>
        /// When using this mode
        /// <para/>
        /// Will switch from Ignore -> Low -> Medium -> High -> Critical -> {Repeat}
        /// </summary>
        ReverseRoundRobin,

        /// <summary>
        /// When using this mode,
        /// <para/>
        /// Will return {N} amount of <see cref="QueuePriority"/> - where {N} is specified as the value of (<see cref="QueuePriority"/> / <see cref="QueueTweakValues"/>) for the current Priority
        /// </summary>
        PreferHigher,
    }
}