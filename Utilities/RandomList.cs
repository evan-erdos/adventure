/* Ben Scott * bescott@andrew.cmu.edu * 2015-10-27 * Random List */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine {

    /* `RandomList<T>` : **`List<T>`**
    |*
    |* Encapsulation class for List<T> with added methods for
    |* random choices. Using `Random` is thread safe, and also
    |* potentially inefficient, as this will instantiate a new
    |* instance of `Random` for each list.
    |* - `<T>` **Type**: Type of the `IList` to pick elements from.
    |* - `return` **T**: random element of `list`.
    |**/
    public class RandomList<T> : List<T> {
        System.Random random;

#if DUMB
        public bool IsReadOnly { get { return false; } }

        public int Count { get { return list.Count; } }

        public T this[int index] {
            get { return list[index]; }
            set { list[index] = value; } }
#endif

        public RandomList() : this(new System.Random()) { }

        public RandomList(System.Random random) {
            this.random = random;
        }

        public T Pick() {
            if (this.Count>0)
                return this[random.Next(this.Count)];
            return default (T);
        }
    }
}