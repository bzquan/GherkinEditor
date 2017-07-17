using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class PriorityQueue<T> where T: IComparable
    {
        private List<T> list = new List<T>();

        public int IndexOf(T item)
        {
            var index = list.BinarySearch(item);
            return index < 0 ? -1 : index;
        }

        public void Push(T item)
        {
            // 見つからなかった場合は、indexは負の値。
            // これは、item の次に大きい要素のインデックスのビットごとの補数です。
            // ただし、大きい要素が存在しない場合は、Count のビットごとの補数です。
            int index = list.BinarySearch(item);
            if (index < 0)
            {
                index = ~index;
            }
            list.Insert(index, item);
        }

        public bool Contains(T item)
        {
            return list.BinarySearch(item) >= 0;
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsEmpty() => list.Count == 0;
        public bool Remove(T item)
        {
            var index = list.BinarySearch(item);
            if (index < 0) return false;
            list.RemoveAt(index);
            return true;
        }

        public T Top() => list.Last();

        public void Pop()
        {
            if (Count > 0)
            {
                list.Remove(list.Last());
            }
        }

        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list.RemoveAt(index);
                this.Push(value);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}
