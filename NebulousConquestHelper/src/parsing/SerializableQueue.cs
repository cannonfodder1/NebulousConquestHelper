using System;
using System.Collections.Generic;

namespace NebulousConquestHelper
{
    [Serializable]
    public class SerializableQueue<T> : Queue<T>
    {
        public T this[int index]
        {
            get
            {
                int count = 0;

                foreach (T obj in this)
                {
                    if (count == index) return obj;
                    count++;
                }

                return default;
            }
        }

        public void Add(T obj)
        {
            Enqueue(obj);
        }
    }
}
