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
    public class NodeQueue: IEnumerator
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
        public IEnumerable<ISimToken> DequeueAbandoningTokens(int cycleAge)
        {
            var queueItems = _itemsInQueue.Where(x => x.Value.CyclesInQueue >= cycleAge);
            
            foreach(var queueItem in queueItems)
            {
                _itemsInQueue.Remove(queueItem.Key);
            }

            return queueItems.Select(x => x.Value.TokenInQueue);
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

        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }
        public object Current
        {
            get { return _itemsInQueue.Single(x => x.Key == _currentPosition).Value; }
        }

        public bool MoveNext()
        {
            _currentPosition++;
            return _currentPosition <= _itemsInQueue.Count;
        }

        public void Reset()
        {
            _currentPosition = _itemsInQueue.Keys.Min();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of <see cref="NodeQueue"/>.
        /// </summary>
        /// <param name="maxQueueLength"></param>
        public NodeQueue()
        {
            _itemsInQueue = new();
        }

        #endregion

        #region Fields

        private Dictionary<int, NodeQueueItem> _itemsInQueue;
        private int _maxQueueLength = int.MaxValue;
        private int _currentPosition;

        #endregion
    }
}
