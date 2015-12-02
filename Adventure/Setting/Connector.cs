/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-09 * Connector */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Adventure.Setting {


	/** `Connector` : **`class`**
	|*
	|* Used for explicit connections beween `Rooms` or `Area`s.
	|**/
	public class Connector : Thing {

		public Room src, tgt;

		public override Description description {get;set;}

		public string desc_type {
			get { return string.Format(
					"{0} {1}",_desc_type,(src!=null && tgt!=null)
						? string.Format(
							"It goes between {0} and {1}.",src,tgt)
						: "It doesn't seem to go anywhere."); }
			set { _desc_type = value; }
		} string _desc_type;

		public virtual void OnTriggerEnter(Collider o) {
			if (Player.IsCollider(o)) Terminal.Log(this); }
	}
}
