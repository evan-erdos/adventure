/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-25 * Item Group */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PathwaysEngine.Inventory {


    class ItemGroup<T> : Item, IItemGroup<T>
               where T : Item {

        public uint Count {
            get { return count; }
            set { count = (value==0)?(1):(value); }
        } uint count = 1;

        public void Group() {
            // search my container and merge
            // maybe take another stack arg
        }

        public IItemGroup<T> Split(uint n) {
            Count -= n;
            return default (ItemGroup<T>);
            // instantiate new Stack _c n
        }

        public void Add(T elem) {
            Count++;
            Destroy(elem.gameObject);
        }

        public void Add(ItemGroup<T> elem) {
            if (elem.GetType()==typeof (T)) {
                Count += elem.Count;
                Destroy(elem.gameObject);
            }
        }
    }
}