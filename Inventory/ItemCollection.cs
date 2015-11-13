/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-04 * Item Collection */

using UnityEngine;			using type=System.Type;
using System.Collections;   using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
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

		public ItemSet items {
			get { return _items; }
		} ItemSet _items;

		public ItemCollection() {
			this._items = new ItemSet(); }

		public ItemCollection(List<Item> items) {
			this._items = new ItemSet(items); }

		IEnumerator IEnumerable.GetEnumerator() {
			return (IEnumerator) GetEnumerator(); }

		public void Add(Item item) { items.Add(item); }

		public void Clear() {
			foreach (var item in items) item.Drop();
			_items = new ItemSet();
		}

		public IEnumerator<Item> GetEnumerator() {
			return items.GetEnumerator(); }

		public bool Contains(Item item) {
			return items.Contains(item); }

	 	public void CopyTo(Item[] arr, int n) { items.CopyTo(arr,n); }

	 	public bool Remove(Item item) {
	 		return items.Remove(item); }

	 	public Item GetItemOfType<T>() where T : Item {
	 		return items.GetItemOfType<T>(); }

	 	public List<Item> GetItemsOfType<T>() where T : Item {
	 		return items.GetItemsOfType<T>(); }
	}
}
