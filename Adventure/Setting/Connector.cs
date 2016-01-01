/* Ben Scott * bescott@andrew.cmu.edu * 2016-01-01 * Connector */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using lit=PathwaysEngine.Literature;
//using static PathwaysEngine.Literature.Terminal;


namespace PathwaysEngine.Adventure.Setting {


	/** `Connector` : **`class`**
	|*
	|* Used for explicit connections beween `Rooms` or `Area`s.
	|**/
	class Connector : Thing {

		[SerializeField] Room src;
		[SerializeField] Room tgt;

		public override lit::Description description {get;set;}

		public string desc_type {
			get { return string.Format(
					"{0} {1}",_desc_type,(src!=null && tgt!=null)
						? string.Format(
							"It goes between {0} and {1}.",src,tgt)
						: "It doesn't seem to go anywhere."); }
			set { _desc_type = value; }
		} string _desc_type;

		void OnTriggerEnter(Collider o) {
			if (Player.IsCollider(o)) PathwaysEngine.Literature.Terminal.Log(this); }
	}
}
