namespace GKMemory
{
    [System.Serializable]
    public class GKCircularBuffer<T>
    {
        public GKCircularBuffer(int size)
        {
            _buf = new T[size];
            _start = 0;
            _count = 0;
        }

        public int Count { get { return _count; } }
        public int Start { get { return _start; } }

        public T this[int i]
        {
            get
            {
                if (i < 0 || i >= _count)
                {
                    throw new System.IndexOutOfRangeException();
                }
                return _buf[(_start + i) % _buf.Length];
            }
            set
            {
                if (i < 0 || i >= _count)
                {
                    throw new System.IndexOutOfRangeException();
                }
                _buf[(_start + i) % _buf.Length] = value;
            }
        }

        public T[] ToArray()
        {
            var o = new T[_count];

            for (int i = 0; i < _count; i++)
            {
                o[i] = _buf[(i + _start) % _buf.Length];
            }
            return o;
        }

        public void Enqueue(T v) { PushBack(v); }
        public void PushBack(T v)
        {
            if (_count >= _buf.Length)
            {
                throw new System.IndexOutOfRangeException();
            }
            int i = (_start + _count) % _buf.Length;

            _buf[i] = v;
            _count++;
        }

        public T PopBack()
        {
            if (_count <= 0)
            {
                throw new System.IndexOutOfRangeException();
            }
            int i = (_start + _count - 1) % _buf.Length;

            T v = _buf[i];
            _buf[i] = default(T);
            _count--;

            return v;
        }

        public void PushFront(T v)
        {
            if (_count >= _buf.Length)
            {
                throw new System.IndexOutOfRangeException();
            }
            _start = (_buf.Length + _start + _count) % _buf.Length;
            _count++;
            _buf[_start] = v;
        }

        public T Dequeue() { return PopFront(); }
        public T PopFront()
        {
            if (_count <= 0)
            {
                throw new System.IndexOutOfRangeException();
            }
            T v = _buf[_start];
            _buf[_start] = default(T);

            _start = (_start + 1) % _buf.Length;
            _count--;

            return v;
        }

        public T Peek()
        {
            if (_count <= 0)
            {
                throw new System.IndexOutOfRangeException();
            }
            return _buf[_start];
        }


        T[] _buf;
        int _start;
        int _count;
    }
}