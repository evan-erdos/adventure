/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-24 * Room */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using invt=PathwaysEngine.Inventory;
using Buffer=System.Text.StringBuilder;

namespace PathwaysEngine.Adventure.Setting {
	public partial class Room : Thing {
		bool wait = false;

		public override string format {
			get { return "\n# \n## {0} ##\n{{0}}\n\n{1}"; } }
		public List<invt::Item> items;
		public List<Thing> things;
		public List<Room> nearbyRooms;

		public int depth { get; set; }
		public override Description description { get; set; }

		public void OnTriggerEnter(Collider other) {
			if (other.tag=="Player" && IsValidRoom(Player.room))
				StartCoroutine(LogRoom());
			//else if (other.tag=="Item")
			//	items.Add(other.attachedRigidbody.GetComponent<invt::Item>());
		}

		public void OnTriggerExit(Collider other) {
			if (other.tag=="Player" && Player.room==this)
				Player.room = null;
		}

		IEnumerator LogRoom() {
			if (!wait) {
				wait = true;
				yield return new WaitForSeconds(1f);
				seen = true;
				Terminal.Log(this);
				Player.room = this;
				yield return new WaitForSeconds(3f);
				wait = false;
			}
		}

		public string descItems() {
			if (items==null || items.Count<1) return "";
			if (items.Count==1)
				return string.Format("You see a {0} here.",items[0]);
			var buffer = new Buffer("You see a ");
			foreach (var item in items) buffer.Append(item.name+", ");
			return buffer.ToString();
		}

		bool IsValidRoom(Room room) {
			if (!room) return true;
			if (room==this) return false;
			return (room.depth<=this.depth);
		}

		public override void Awake() { base.Awake();
			this.GetYAML();
			FormatDescription();
		}

		public override void FormatDescription() {
			description.SetFormat(string.Format(
				format,uuid,descItems()));
		}
	}
}
