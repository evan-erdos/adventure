/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-23 * Encounter */

using UnityEngine;
using System.Collections;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Adventure {
	[RequireComponent(typeof(Collider))]
	public partial class Encounter : Thing, IStorable {
		public enum Inputs : byte { Trigger, Click, Elapsed, Sight };
		Inputs input;
		bool reuse;
		float time;
		Collider cl;

		public override void Awake() { base.Awake();
			cl = GetComponent<Collider>();
			cl.isTrigger = true;
		}

		public string Desc_Style(Description desc) {
			return string.Format("{1}{0}",desc,
				"## Special Encounter ##\n"); }

		public override void Start() {
			if (input==Inputs.Elapsed)
				StartCoroutine(TimedEncounter(time));
		}

		void OnTriggerEnter(Collider other) {
			if (input==Inputs.Trigger && other.gameObject.tag=="Player")
				StartCoroutine(BeginEncounter());
		}

		IEnumerator BeginEncounter() {
			Terminal.Log(Desc_Style(description));
			if (reuse) {
				if (cl) cl.enabled = false;
				yield return new WaitForSeconds(1f);
				if (cl) cl.enabled = true;
			} else gameObject.SetActive(false);
		}

		IEnumerator TimedEncounter(float t) {
			yield return new WaitForSeconds(t);
			StartCoroutine(BeginEncounter());
		}
	}
}

