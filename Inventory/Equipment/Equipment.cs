/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-11 * Equipment Base */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	public abstract class Equipment : Item, IWearable {

		public uint Uses { get; set; }

		public bool Worn {
			get { return worn; }
			set { worn = value;
				if (worn) Wear();
				else Stow(); }
		} bool worn;

		public void Use() { Worn = !Worn; }

		public override void Take() { base.Take(); Worn = true; }

		public override void Drop() { base.Drop(); Worn = false; }

		public void Wear() {
			if (gameObject) gameObject.SetActive(true); }

		public void Stow() { gameObject.SetActive(false); }
	}
}
