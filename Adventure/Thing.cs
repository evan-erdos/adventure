/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-22 * Thing */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Adventure {
	public partial class Thing : MonoBehaviour, IDescribable {
		public virtual bool seen { get; set; }
		public string uuid { get { return gameObject.name; } }

		public virtual string format {
			get { return _format; }
		} string _format = "{0}";

		public virtual Description description { get; set; }

		public Thing() {
			description = new Description<Thing>();
		}

		public virtual void Awake() { this.GetYAML();
			FormatDescription(); }

		public virtual void Start() { }

		public virtual void Find() { }

		public virtual void View() {
			Terminal.Log(description); }

		public virtual void FormatDescription() {
			description.SetFormat(format); }

		public virtual string Log() { return description; }

		public override string ToString() { return uuid; }

		public static bool operator !(Thing i) { return (i==null); }
	}
}
