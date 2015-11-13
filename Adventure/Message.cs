/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-09 * Message */

using UnityEngine;

namespace PathwaysEngine.Adventure {

	/** `Message` : `class`
	 *
	 * Represents any message that can be formatted and sent to
	 * the `Terminal`, and can be serialized via `IStorable`.
	 **/
	public class Message : IStorable {
		public string uuid { get; set; }
		public string desc {
			get { return _desc; }
			set { _desc = Terminal.Format(value.md(),formats); }
		} string _desc;

		public Formats[] formats { get; set; }

		public Message() { uuid = ""; }

		public Message(string uuid) { this.uuid = uuid; }

		public Message(string uuid,string desc)
			: this(uuid) { this.desc = desc; }

		public Message(
				string uuid, string desc,
				params Formats[] format)
			: this(uuid,desc) { this.formats = formats; }
	}
}
