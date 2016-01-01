/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-25 * Item Group */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PathwaysEngine.Inventory {


    class ItemGroup<T> : Item, IItemGroup <T> {
        int radius = 4, layerItem = 16;

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

        internal Item[] GetNearbyItems() {
            Collider[] temp = Physics.OverlapSphere(
                transform.position, radius, 1<<layerItem);
            ArrayList items = new ArrayList();
            foreach (Collider entity in temp)
                if (entity.gameObject.GetComponent<Item>())
                    items.Add(entity.gameObject.GetComponent<Item>());
            return items.ToArray(typeof(Item)) as Item[];
        }

        public void Add(Item elem) {
            if (elem.GetType()==typeof(T)) {
                Count++;
                Destroy(elem.gameObject);
            } else throw new System.Exception("fool");
        }

        public void Add(ItemGroup<T> elem) {
            if (elem.GetType()==typeof (T)) {
                Count+=elem.Count;
                Destroy(elem.gameObject);
            } else throw new System.Exception("silly");
        }
    }
}