/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-04 * Item Collection */

using UnityEngine;
using type=System.Type;
using System.Collections;
using System.Collections.Generic;


namespace PathwaysEngine.Inventory {


    /** `ItemCollection` : **`class`**
     *
     * Defines a set of `Item`s, which can be used as any other
     * `ICollection`, but deals with `Group`ing similar `Item`s
     * and can perform a really fast search on the basis of the
     * possibly different and usually quite varied subtypes for
     * easy filtering of specific types of `Item`s.
     **/
    abstract class ItemCollection : Item, IItemSet {

        public bool IsSynchronized => false;

        public bool IsReadOnly => items.IsReadOnly;

        public int Count => items.Count;

        public uint maxCount {get;set;}

        public object SyncRoot => default (object);

        public IItemSet items => new ItemList();

        public ItemCollection()
            : this(new ItemList()) { }

        public ItemCollection(List<Item> items) {
            this.items.Add(items); }

        IEnumerator IEnumerable.GetEnumerator() =>
            (IEnumerator) GetEnumerator();

        public void Add(Item item) =>
            items.Add(item);

        public void Add<T>(ICollection<T> list)
                        where T : Item =>
            items.Add(list);

        public void Clear() {
            foreach (var item in items) item.Drop();
            items.Clear();
        }

        public IEnumerator<Item> GetEnumerator() =>
            items.GetEnumerator();

        public bool Contains(Item item) =>
            items.Contains(item);

        public void CopyTo(Item[] arr, int n) =>
            items.CopyTo(arr,n);

        public bool Remove(Item item) =>
            items.Remove(item);

        public T GetItem<T>() where T : Item =>
            items.GetItem<T>();

        public List<T> GetItems<T>() where T : Item =>
            items.GetItems<T>();
    }
}
