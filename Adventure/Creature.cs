/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-31 * Creature */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using stat=PathwaysEngine.Statistics;

namespace PathwaysEngine.Adventure {
	public partial class Creature : Thing, ILiving {
		public virtual stat::Set stats { get; set; }

		public virtual bool isDead {
			get { return _isDead; }
			set { _isDead = value; }
		} protected bool _isDead = false;

		public virtual void ApplyDamage(float n) { }

		public virtual void Kill() {
			isDead = true;
			Terminal.Log(uuid+" has died.",Formats.Alert); }

		public override void Awake() { this.GetYAML(); }
	}
}