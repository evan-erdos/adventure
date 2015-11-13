/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-09 * Area */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PathwaysEngine.Adventure.Setting {
	public partial class Area : Thing {
		public List<Room> rooms;
		public List<Area> areas;

		public bool safe;
		public int level;
		public override Description description {get;set;}

		public override void Awake() { base.Awake(); this.GetYAML(); }

		public void OnTriggerEnter(Collider other) {
			if (other.tag=="Player") Player.area = this; }
	}
}