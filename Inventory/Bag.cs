/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-24 * Bag */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Inventory {


    public partial class Bag : ItemCollection {

        public void DropAll() {
            foreach (var item in this) item.Drop(); }

#if IMPL
        internal void ItemButton(IUsable item, int m) {
            item.Drop();
            if (m==0) item.Take();
            else if (m==1) item.Use();
            else item.Drop();
        }

        internal void ItemButton(Item item, int m) {
            item.Drop();
            if (m==0) item.Take();
//          else if (m==1) item.Use();
            else item.Drop();
        }
        internal void ItemButton(Weapon item, int m) {
            CycleWeapons(true);
            if (m==0) CycleWeapons(true);
            else if (m==1) item.Use();
            else item.Drop();
        }

        internal void ItemButton(Flashlight item, int m) {
            item.Use();
            if (m==0) item.Use();
            if (m==1) item.Drop();
        }

        internal bool CheckForDuplicates(Item item) {
            if (item.playerPack.!=null && item.playerPack..Length>0) {
                foreach (Item elem in item.playerPack.) {
                    if (item.description == elem.description
                    && item.title==elem.title&&item.icon==elem.icon
                    && item.GetType()==elem.GetType()&&item.sound==elem.sound) return true;
                }
            } return false;
        }

        public void Add(Item item) { items[item.GetType()].Add(item); }
        public void Clear() { items = new Dictionary<type,List<Item>>(); }
        public bool Contains(Item item) {
            foreach (var list in items.Values)
                foreach (var elem in list)
                    if (elem.uuid==item.uuid && elem==item) return true;
            return false;
        }

        public void Remove(Item item) { items[item.GetType()].Remove(item); }

        internal Item[] GetNearbyItems() {
            Collider[] temp = Physics.OverlapSphere(
                transform.position,radiusGet,1<<layerItem);
            ArrayList items = new ArrayList();
            foreach (Collider entity in temp)
                if (entity.gameObject.GetComponent<Item>())
                    items.Add(entity.gameObject.GetComponent<Item>());
            return items.ToArray(typeof(Item)) as Item[];
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator) GetEnumerator(); }

        public BagEnum GetEnumerator() {
            return new BagEnum(_items[typeof (Item)]); }
        public class BagEnum : IEnumerator {
            List<Item> _items;
            int position = -1;

            public Item Current {
                get {
                    try { return items[position]; }
                    catch (IndexOutOfRangeException) {
                        throw new InvalidOperatonException();
                    }
                }
            }

            public BagEnum(Item[] _items) { this._items = new List<Item>(_items); }
            public BagEnum(List<Item> _items) { this._items = _items; }

            public bool MoveNext() {
                position++;
                return (position<items.Length);
            }
        }
#endif
    }
}
