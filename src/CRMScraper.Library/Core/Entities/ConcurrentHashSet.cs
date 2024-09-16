using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRMScraper.Library.Core.Entities
{
    public class ConcurrentHashSet<T> : IEnumerable<T>
    {
        private readonly ConcurrentDictionary<T, byte> _dict = new ConcurrentDictionary<T, byte>();

        public bool Add(T item)
        {
            return _dict.TryAdd(item, 0);
        }

        public bool Contains(T item)
        {
            return _dict.ContainsKey(item);
        }

        public int Count => _dict.Count;

        public IEnumerator<T> GetEnumerator() => _dict.Keys.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}