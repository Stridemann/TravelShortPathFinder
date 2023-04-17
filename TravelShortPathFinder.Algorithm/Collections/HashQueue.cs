namespace TravelShortPathFinder.Algorithm.Collections
{
    public class HashQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly HashSet<T> _hashSet = new HashSet<T>();

        public void Enqueue(T item)
        {
            if (_hashSet.Contains(item))
                return;
            _hashSet.Add(item);
            _queue.Enqueue(item);
        }

        public T Dequeue()
        {
            var item = _queue.Dequeue();
            _hashSet.Remove(item);

            return item;
        }

        public bool TryDequeue(out T? item)
        {
            if (_queue.TryDequeue(out item))
            {
                _hashSet.Remove(item);

                return true;
            }

            return false;
        }
    }
}
