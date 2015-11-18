/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-11 * ItemSet */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Type=System.Type;
using invt=PathwaysEngine.Inventory;
using intf=PathwaysEngine.Adventure;
using maps=PathwaysEngine.Adventure.Setting;
using stat=PathwaysEngine.Statistics;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Inventory {

	/** `ItemSet` : **`class`**
	 *
	 * This class implements the `IItemSet` interface, and
	 * provides a way to deal with collections of items,
	 * iterate over them, & get particular types of `Item`s.
	 **/
	public class ItemSet : IItemSet, ILoggable {

		/** `IsReadOnly` : **`bool`**
		 *
		 * req.d by `ICollection<T>`.
		 **/
		public bool IsReadOnly { get { return false; } }

		/** `Count` : **`int`**
		 *
		 * number of contained elements.
		 **/
		public int Count {
			get { return items[typeof(Item)].Count; } }

		/** `Format` : **`string`**
		 *
		 * Formatting string, specified by `ILoggable`.
		 **/
		public string Format {
			get { return "It contains: \n"; } }

		/** `items` : **`Dictionary<Type,List<Item>>`**
		 *
		 * Mapping between the `Type`s of the `Item` values
		 * and the `Item`s themselves.
		 **/
		public Dictionary<Type,List<Item>> items {
			get { return _items; }
		} Dictionary<Type,List<Item>> _items;

		/** `Keys` : **`ICollection<Type>`**
		 *
		 * Set of keys from the dictionary.
		 **/
		public ICollection<Type> Keys {
			get { return items.Keys; } }

		/** `Values` : **`ICollection<List<Item>>`**
		 *
		 * Set of values from the dictionary.
		 **/
		public ICollection<List<Item>> Values {
			get { return items.Values; } }

		/** `Indexer[Type]` : **`List<Item>`**
		 *
		 * Indexer to get individual `List`s or lists of more
		 * derived `Item`s from the dictionary.
		 *
		 * - `type` : **`Type`**
		 *     Type of the `Item`s to get from the set.
		 **/
		public List<Item> this[Type type] {
			get { return items[type]; }
			set { items[type] = (List<Item>) value; } }

		/** `ItemSet` : **`constructor`**
		 *
		 * Initializes the datastructure with every `Item` in
		 * the scene.
		 **/
		public ItemSet() {
			_items = new Dictionary<Type,List<Item>>() {
				{typeof(Item),new List<Item>()}};}
		public ItemSet(List<Item> items) {
			this._items = new Dictionary<Type,List<Item>>() {
				{typeof(Item), items}};}
		public ItemSet(Dictionary<Type,List<Item>> items) {
			this._items = items;
			if (!_items.ContainsKey(typeof(Item)))
				_items[typeof(Item)] = new List<Item>(); }

		/** `Add()` : **`function`**
		 *
		 * Override of the `ICollection<T>` function.
		 *
		 * - `item` : **`Item`**
		 *     Instance of `Item` to be added.
		 **/
		public void Add(Item item) {
			if (!items.ContainsKey(item.GetType()))
				items[item.GetType()] = new List<Item>();
			items[item.GetType()].Add(item); }

		/** `Add()` : **`function`**
		 *
		 * Override of the `ICollection<T>` function.
		 *
		 * - `type` : **`Type`**
		 *
		 * - `item` : **`Item`**
		 *     Instance of `Item` to be added.
		 **/
		public void Add(Type type,List<Item> list) {
			items[type].AddRange(list); }

		public bool Contains(Item item) {
			List<Item> temp;
			return (TryGetValue<Item>(out temp)
				&& temp!=null && temp.Contains(item));
		}

		public int IndexOf(Item item) {
			return _items[typeof(Item)].IndexOf(item); }

		public bool Remove(Item item) {
			return items[item.GetType()].Remove(item); }

		public void Remove(List<Item> list) {
			if (list==null) return;
			if (ContainsKey(list[0].GetType()))
				items[list[0].GetType()] = null;
		}
		/* end engine specific */

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

		public Item GetItemOfType<T>() where T : Item {
			var temp = GetItemsOfType<T>();
			if (temp!=null) return temp[0];
			else return default(Item);
		}

		/** `Log()` : **`string`**
		 *
		 * Specified by `ILoggable`, `Terminal` calls this
		 * function to get a special `string` to log.
		 **/
		public string Log() {
			var s = Format;
			foreach (var item in itemSet)
				s += string.Format("\n- {0}",item);
			return s;
		}


		public List<Item> GetItemsOfType<T>() where T : Item {
			var temp = new List<Item>();
			if (items.TryGetValue(typeof (T), out temp))
				return items[typeof (T)];
			return default(List<Item>);
		}

		public class ItemSetEnum {
			List<Item> _items;
			int position = -1;

			public Item Current {
				get {
					try { return _items[position]; }
					catch (System.IndexOutOfRangeException) {
						throw new System.Exception();
					}
				}
			}

			public ItemSetEnum(Item[] list) {
				this._items = new List<Item>(list); }
			public ItemSetEnum(List<Item> list) {
				this._items = list; }
			//public ItemSetEnum(List<List<Item>> lists) {
			//	this._items = new List<Item>(lists); }
			public bool MoveNext() {
				position++; return (position<_items.Count); }
			public void Reset() { position = -1; }
		}
	}

}