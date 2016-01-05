
using UnityEngine;
using ui=UnityEngine.UI;
using PathwaysEngine;
using inv=PathwaysEngine.Inventory;
using System.Collections.Generic;


class SimpleInventory : MonoBehaviour {
	List<Slot> slots = new List<Slot>();

	public void Awake() {
		foreach (Transform child in transform) {
			var o = child.gameObject.GetComponent<Slot>();
			if (o!=null) slots.Add(o);
		}
	}

	public void Update() {
		var i = 0;
		foreach (var slot in slots)
			if (!Player.Current.Items.Contains(slot.item))
				slot.item = null;
		foreach (var item in Player.Current.Items) {
			if (i>=slots.Count) break;
			slots[i].item = item;
			i++;
		}
	}
}