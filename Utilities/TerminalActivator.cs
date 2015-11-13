/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-11 * Terminal Activator */

using UnityEngine;
using ui=UnityEngine.UI;
using System.Collections.Generic;

namespace PathwaysEngine.Utilities {

	/** `TerminalActivator` : **`class`**
	 *
	 * Specific implementation of a `Activator<T>`, used by the
	 * `Terminal` class to enable / disable its UI effects via
	 * a parser command.
	 **/
	public class TerminalActivator : Activator<Behaviour> {

		public override void Initialize() {
			var terminal = Pathways.terminal;
			list.Add(terminal.GetComponent<ui::Image>());
			foreach (var elem in terminal
					.GetComponentsInChildren<ui::Image>())
				list.Add(elem);
			foreach (var elem in terminal
					.GetComponentsInChildren<ui::Text>())
				list.Add(elem);
			foreach (Transform child in terminal.transform) {
				foreach(var elem in child
						.GetComponentsInChildren<ui::Image>())
					list.Add(elem);
				foreach(var elem in child
						.GetComponentsInChildren<ui::Text>())
					list.Add(elem);
			}
		}
	}
}
