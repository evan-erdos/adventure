/* Ben Scott * bescott@andrew.cmu.edu * 2015-12-06 * Item List */

using System.Collections.Generic;


namespace PathwaysEngine.Inventory {


    class ItemList : List<Item>, IItemSet {

        internal ItemList() : base() { }

        internal ItemList(List<Item> items) : base() {
            foreach (var item in items) this.Add(item); }


        public void Add<T>(ICollection<T> list)
                        where T : Item {
            foreach (var item in list) this.Add(item); }

        public List<T> GetItems<T>()
                        where T : Item {
            var list = new List<T>();
            foreach (var item in this)
                if (item is T)
                    list.Add((T) item);
            return list;
        }

        public T GetItem<T>()
                        where T : Item {
            foreach (var item in this)
                if (item is T)
                    return (T) item;
            return default (T);
        }
    }
}


