/* Ben Scott * bescott@andrew.cmu.edu * 2015-12-20 * TypeTree */

using System.Collections.Generic;


namespace PathwaysEngine.Libraries {

    class TypeTree<T> {//: ITypeTree<T> {

        public bool IsReadOnly { get { return false; } }

        public int Count {
            get { var n = 0;
                //foreach (var elem in this.Values)
                //    n += elem.Count;
                return n;
            }
        }
    }
}
