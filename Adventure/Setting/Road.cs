/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-13 * Road */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PathwaysEngine.Adventure.Setting {


	/** `Road` : **`class`**
	 *
	 * Acts as a connector between `Area`s, deals with moving
	 * things between `Area`s and loading `Scene`s if needed.
	 **/
	class Road : Connector {

		[SerializeField] Area area_src;
		[SerializeField] Area area_tgt;
	}
}
