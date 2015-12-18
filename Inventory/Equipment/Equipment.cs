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

		public override bool Use() {
			Worn = !Worn; return false; }

		public override bool Take() { base.Take();
			Worn = true; return false; }

		public override bool Drop() { base.Drop();
			Worn = false; return false; }

		public bool Wear() {
			if (gameObject) gameObject.SetActive(true);
			return false;
		}

		public bool Stow() {
			gameObject.SetActive(false);
			return false;
		}
	}
}
