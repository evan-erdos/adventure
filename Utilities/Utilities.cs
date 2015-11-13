/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-13 * Utilities */

/** `PathwaysEngine.Utilities` : **`namespace`**
 * Deals with Everything else. This class contains things such
 * as user input controls, via the `Control` class, various
 * camera scripts, and misfits from other namespaces.
 **/
namespace PathwaysEngine.Utilities {
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class InputKeyAttribute : System.Attribute {
		public string button { get; set; }

		public InputKeyAttribute() { }

		public InputKeyAttribute(string button) {
			this.button = button; }
	}

	public delegate void InputKey(bool value);
	public struct key {
		public bool @get;
		public bool input;
		public InputKey f;

		public key (InputKey _f) {
			@get = true; input = false; f = _f;
		}
	}

	public delegate void InputAxis(float value);
	public struct axis {
		public bool @get;
		public float input;
		public InputAxis f;

		public axis (InputAxis _f) {
			@get = true; input = 0f; f = _f; }
	}
}