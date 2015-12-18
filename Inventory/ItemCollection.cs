/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-04 * Item Collection */

using UnityEngine;
using type=System.Type;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Inventory {


    /** `ItemCollection` : **`class`**
    |*
    |* Defines a set of `Item`s, which can be used as any other
    |* `ICollection`, but deals with `Group`ing similar `Item`s
    |* and can perform a really fast search on the basis of the
    |* possibly different and usually quite varied subtypes for
    |* easy filtering of specific types of `Item`s.
    |**/
    public abstract class ItemCollection : Item, IItemSet {

        public bool IsSynchronized {
            get { return false; } }

        public bool IsReadOnly {
            get { return items.IsReadOnly; } }

        public int Count {
            get { return items.Count; } }

        public uint maxCount {get;set;}

        public object SyncRoot {
            get { return default (object); } }

        public IItemSet items {
            get { return _items; }
        } IItemSet _items;

        public ItemCollection() {
            this._items = new ItemList(); }

        public ItemCollection(List<Item> items) {
            this._items = new ItemList(items); }

        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator) GetEnumerator(); }

        public void Add(Item item) { items.Add(item); }

        public void Clear() {
            foreach (var item in items) item.Drop();
            _items = new ItemList();
        }

        public IEnumerator<Item> GetEnumerator() {
            return items.GetEnumerator(); }

        public bool Contains(Item item) {
            return items.Contains(item); }

        public void CopyTo(Item[] arr, int n) {
            items.CopyTo(arr,n); }

        public bool Remove(Item item) {
            return items.Remove(item); }

        public T GetItem<T>() where T : Item {
            return items.GetItem<T>(); }

        public List<T> GetItems<T>() where T : Item {
            return items.GetItems<T>(); }
    }
}
