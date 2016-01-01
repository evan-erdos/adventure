/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-09 * Area */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using lit=PathwaysEngine.Literature;

namespace PathwaysEngine.Adventure.Setting {


	/** `Area` : **`class`**
	 *
	 * Represents a large space, which could be an entire level
	 * or a significant sub-area. Each `Area` is represented on
	 * the `Map`, and defines what areas it's close to / can be
	 * traveled to from it, either via the `Map`view, or by the
	 * **goto** command. Also defines all contained `Rooms`.
	 **/
	public partial class Area : Thing {


		/** `rooms` : **`Room[]`**
		 *
		 * This is a list of the rooms contained in this`Area`.
		 **/
		public List<Room> rooms;


		/** `areas` : **`Area[]`**
		 *
		 * This is a list of all the adjacent `Area`s that can
		 * be traveled to from here.
		 **/
		public List<Area> areas;


		/** `safe` : **`bool`**
		 *
		 * Defines if this `Area` can be traversed by default,
		 * or if it has to be cleared of assailants first.
		 **/
		public bool safe;


		/** `level` : **`int`**
		 *
		 * Defines a level number, such that the scene can be
		 * switched to by number if the `Player` isn't already
		 * in the specified level. This will keep the engine
		 * from reloading the scene if the target `Area` is in
		 * the same scene as the current `Area`.
		 **/
		public int level;

		public override lit::Description description {get;set;}

		public void OnTriggerEnter(Collider o) {
			if (Player.IsCollider(o)) Player.area = this; }
	}
}

