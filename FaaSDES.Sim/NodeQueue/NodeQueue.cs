using FaaSDES.Sim.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaaSDES.Sim.Nodes
{
    /// <summary>
    /// Custom queue implementation that allows access to the items in the queue, whether
    /// they are at the front or not. 
    /// 
    /// This is important since some items can be abandoned if waiting too long in the
    /// queue.
    /// </summary>
    public class NodeQueue: IEnumerable<NodeQueueItem>, IEnumerable
    {
        /// <summary>
        /// Returns how many items are currently in the queue.
        /// </summary>
        public int QueueLength
        {
            get { return _itemsInQueue.Count; }
        }

        /// <summary>
        /// Returns how much space is available in the queue.
        /// </summary>
        public int SpaceInQueue
        {
            get { return _maxQueueLength - _itemsInQueue.Count; }
        }
        
        /// <summary>
        /// Signifies whether there are any items in the queue.
        /// </summary>
        public bool HasItemsInQueue
        {
            get { return _itemsInQueue.Count > 0; }
        }        

        /// <summary>
        /// Adds an <see cref="ISimToken"/> to the back of the queue.
        /// </summary>
        /// <param name="token">The <see cref="ISimToken"/> to add.</param>
        public void AddTokenToQueue(ISimToken token)
        {
            if (_itemsInQueue.Count == 0)
                _itemsInQueue.Add(1, 
                    new NodeQueueItem() { TokenInQueue = token });
            else
            _itemsInQueue.Add(_itemsInQueue.Keys.Max() + 1,
                new NodeQueueItem() { TokenInQueue = token });            
        }

        /// <summary>
        /// Dequeues the next item in the queue.
        /// </summary>
        /// <returns>A single <see cref="ISimToken"/>.</returns>
        /// <exception cref="InvalidOperationException">If there are no items in the queue.
        /// </exception>
        public ISimToken GetNextTokenInQueue()
        {
            if (_itemsInQueue.Count == 0)
                throw new InvalidOperationException("There are no more items in the queue.");

            var token = _itemsInQueue.Single(x => x.Key == _itemsInQueue.Keys.Min());

            _itemsInQueue.Remove(_itemsInQueue.Keys.Min());

            return token.Value.TokenInQueue;
        }

        /// <summary>
        /// Removes all items in the queue older than the supplied age.
        /// </summary>
        /// <param name="cycleAge">Queued items older than this number of simulation cycles 
        /// will be dequeued.</param>
        /// <returns>List of <see cref="ISimToken"/>.</returns>
        public IEnumerable<ISimToken> DequeueAbandoningTokens(TimeSpan timeFactor)
        {
            var abandonedQueueItems = _itemsInQueue.Where(x => (x.Value.CyclesInQueue * timeFactor) >= (x.Value.TokenInQueue as SimToken).MaxWaitTime);
                       
            foreach (var queueItem in abandonedQueueItems)
                _itemsInQueue.Remove(queueItem.Key);

            return abandonedQueueItems.Select(x => x.Value.TokenInQueue);
        }

        /// <summary>
        /// Dequeues the <see cref="ISimToken"/> at the specified queue position.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public ISimToken DequeueToken(NodeQueueItem queueItem)
        {
            if(!_itemsInQueue.Any(x => x.Value == queueItem))
            {
                throw new ArgumentException("This queue does not contain the provided key.");
            }
            else
            {
                var itemToRemove = _itemsInQueue.First(x => x.Value == queueItem);
                _itemsInQueue.Remove(itemToRemove.Key, out NodeQueueItem removedQueueItem);
                return removedQueueItem.TokenInQueue;
            }
        }

        /// <summary>
        /// Dequeues the next ISimToken.
        /// </summary>
        /// <returns></returns>
        public ISimToken DequeueTokenNextInLine()
        {
            var itemToRemove = _itemsInQueue.MinBy(x => x.Key);
            _itemsInQueue.Remove(itemToRemove.Key, out NodeQueueItem removedQueueItem);
            return removedQueueItem.TokenInQueue;
        }

        /// <summary>
        /// Purges the queue (i.e. at the end of a workday).
        /// </summary>
        public void PurgeQueue()
        {
            _itemsInQueue.Clear();
        }

        /// <summary>
        /// Sets the maximum number of items that can be in the queue at one time.
        /// </summary>
        /// <param name="max"></param>
        public void SetMaximumQueueLength(int max)
        {
            _maxQueueLength = max;
        }

        #region IEnumerator Implementation

        public IEnumerator<NodeQueueItem> GetEnumerator()
        {
            foreach(KeyValuePair<int, NodeQueueItem> item in this._itemsInQueue)
            {
                yield return item.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of <see cref="NodeQueue"/>.
        /// </summary>
        /// <param name="maxQueueLength"></param>
        public NodeQueue(int maxQueueLength)
        {
            _itemsInQueue = new();
            _maxQueueLength = maxQueueLength;
        }

        /// <summary>
        /// Creates an instance of <see cref="NodeQueue"/>. Defaults the maximum queue
        /// length to <see cref="int.MaxValue"/>.
        /// </summary>
        public NodeQueue()
            : this(int.MaxValue)
        { }

        #endregion

        #region Fields

        private readonly Dictionary<int, NodeQueueItem> _itemsInQueue;
        private int _maxQueueLength = int.MaxValue;

        #endregion
    }
}
