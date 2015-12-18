/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-11 * ItemSet */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Type=System.Type;
using inv=PathwaysEngine.Inventory;
using adv=PathwaysEngine.Adventure;
using maps=PathwaysEngine.Adventure.Setting;
using lit=PathwaysEngine.Literature;
using stat=PathwaysEngine.Statistics;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Inventory {


    /** `ItemSet` : **`class`**
    |*
    |* This class implements the `IItemSet` interface, and
    |* provides a way to deal with collections of items,
    |* iterate over them, & get particular types of `Item`s.
    |**/
    public class ItemSet : IItemSet, lit::IDescribable {

        public bool IsReadOnly { get { return false; } }

        public int Count {
            get { var n = 0;
                foreach (var elem in Items.Values)
                    n += elem.Count;
                return n;
            }
        }

        public string Name {
            get { return "Jeez! An ItemSEt!/"; } }

        public lit::Description description {get;set;}

        /** `Items` : **`Dictionary<Type,List<Item>>`**
        |*
        |* Mapping between the `Type`s of the `Item` values
        |* and the `Item`s themselves.
        |**/
        public Dictionary<Type,List<Item>> Items {
            get { return items; }
        } Dictionary<Type,List<Item>> items;


        public ICollection<Type> Keys {
            get { return Items.Keys; } }

        public ICollection<List<Item>> Values {
            get { return Items.Values; } }


        /** `Indexer[Type]` : **`List<Item>`**
        |*
        |* Indexer to get individual `List`s or lists of more
        |* derived `Item`s from the dictionary.
        |*
        |* - `type` : **`Type`**
        |*     Type of the `Item`s to get from the set.
        |**/
        public List<Item> this[Type type] {
            get { return Items[type]; }
            set { Items[type] = (List<Item>) value; } }


        /** `ItemSet` : **`constructor`**
        |*
        |* Initializes the datastructure with every `Item` in
        |* the scene.
        |**/
        public ItemSet()
            : this(new Dictionary<Type,List<Item>>()
                {{typeof(Item),new List<Item>()}}) { }

        public ItemSet(List<Item> items) {
            this.items = new Dictionary<Type,List<Item>>() {
                {typeof(Item), items}};}

        public ItemSet(Dictionary<Type,List<Item>> items) {
            this.items = items;
            if (!this.items.ContainsKey(typeof(Item)))
                this.items[typeof(Item)] = new List<Item>(); }


        /** `Add()` : **`function`**
        |*
        |* Override of the `ICollection<T>` function.
        |*
        |* - `item` : **`Item`**
        |*     Instance of `Item` to be added.
        |**/
        public void Add(Item item) {
            if (!Items.ContainsKey(item.GetType()))
                Items[item.GetType()] = new List<Item>();
            Items[item.GetType()].Add(item); }


        /** `Add()` : **`function`**
        |*
        |* Override of the `ICollection<T>` function.
        |*
        |* - `type` : **`Type`**
        |*
        |* - `item` : **`Item`**
        |*     Instance of `Item` to be added.
        |**/
        public void Add(Type type,List<Item> list) {
            Items[type].AddRange(list); }


        /** `Contains()` : **`bool`**
        |*
        |* Membership test for a particular `Item`.
        |*
        |* - `item` : **`Item`**
        |*     Instance of `Item` to be added.
        |**/
        public bool Contains(Item item) {
            List<Item> temp;
            return (TryGetValue<Item>(out temp)
                && temp!=null && temp.Contains(item));
        }


        /** `IndexOf()` : **`int`**
        |*
        |* Finds the first instance of `item` and returns its
        |* position in the set.
        |*
        |* - `item` : **`Item`**
        |*     Instance of `Item` to be added.
        |**/
        public int IndexOf(Item item) {
            return items[typeof(Item)].IndexOf(item); }


        /** `Remove()` : **`bool`**
        |*
        |* Removes the specified
        |*
        |* - `item` : **`Item`**
        |*     Instance of `Item` to be added.
        |**/
        public bool Remove(Item item) {
            return Items[item.GetType()].Remove(item); }


        /** `Remove()` : **`function`**
        |*
        |* Removes the specified
        |*
        |* - `item` : **`Item`**
        |*     Instance of `Item` to be added.
        |**/
        public bool Remove(List<Item> list) {
            if (list==null) return false;
            if (ContainsKey(list[0].GetType())) {
                items[list[0].GetType()] = null;
                return true;
            } else return false;
        }


        public void Add(KeyValuePair<Type,List<Item>> kvp) {
            items.Add(kvp.Key,kvp.Value); }

        public void Clear() { items.Clear(); }

        public bool Contains(KeyValuePair<Type,List<Item>> kvp) {
            foreach (var elem in items)
                if (elem.Key==kvp.Key && elem.Value==kvp.Value)
                    return true;
            return false;
        }

        public bool ContainsKey(Type type) {
            return items.ContainsKey(type); }

        public bool ContainsValue(List<Item> list) {
            return items.ContainsValue(list); }

        public void CopyTo(Item[] list, int n) {
            items[typeof(Item)].CopyTo(list,n); }

        public bool Remove<T>() {
            return Remove(typeof (T)); }

        public bool Remove(Type type) {
            return items.Remove(type); }

        public bool Remove(KeyValuePair<Type,List<Item>> kvp) {
            return items.Remove(kvp.Key); }

        public bool TryGetValue<T>(out List<Item> list)
                where T : Item {
            foreach (var elem in items.Keys) {
                if (elem==typeof (T)) {
                    list = items[typeof (T)];
                    return true;
                }
            } list = default(List<Item>); return false;
        }

        public bool TryGetValue(Type type, out List<Item> list) {
            if (items.ContainsKey(type)) {
                list = items[type]; return true; }
            else { list = default(List<Item>); return false; }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator) GetEnumerator(); }

        public IEnumerator<Item> GetEnumerator() {
            return items[typeof(Item)].GetEnumerator(); }

        public T GetItem<T>() where T : Item {
            var temp = GetItems<T>();
            foreach (var item in temp)
                if (item is T)
                    return (T) item;
            return default (T);
        }


        /** `Log()` : **`string`**
        |*
        |* Specified by `ILoggable`, `Terminal` calls this
        |* function to get a special `string` to log.
        |**/
        public string Log() {
            var s = description.Template;
            foreach (var item in this)
                s += string.Format("\n- {0}",item);
            return s;
        }

        public List<T> GetItems<T>() where T : Item {
            var list = new List<T>();
            List<Item> temp;
            if (items.TryGetValue(typeof (T),out temp)) {
                foreach (var elem in temp)
                    list.Add((T) elem);
                return list;
            } return default (List<T>);
        }

        public class ItemSetEnum {
            List<Item> items;
            int position = -1;

            public Item Current {
                get {
                    try { return items[position]; }
                    catch (System.IndexOutOfRangeException) {
                        throw new System.Exception();
                    }
                }
            }

            public ItemSetEnum(Item[] list) {
                this.items = new List<Item>(list); }
            public ItemSetEnum(List<Item> list) {
                this.items = list; }
            //public ItemSetEnum(List<List<Item>> lists) {
            //  this.items = new List<Item>(lists); }
            public bool MoveNext() {
                position++; return (position<items.Count); }
            public void Reset() { position = -1; }
        }
    }
}

