/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-13 * Lamp */

using UnityEngine;
using System.Collections;
using mvmt=PathwaysEngine.Movement;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Inventory {
	[RequireComponent(typeof(Animator))]
	public partial class Lamp : Item, IWieldable {
		bool wait = false;
		public AudioClip auSwitch;
		Animator animator;
		mvmt::Hand hand;
		public util::key dash;

		bool sprint {
			get { return _sprint; }
			set { _sprint = value;
				if (gameObject.activeInHierarchy)
					StartCoroutine(LateSetBool("sprint",_sprint)); }
		} bool _sprint = false;

		public bool on {
			get { return _on; }
			set { if (!Held) return;
				_on = value;
				if (gameObject.activeInHierarchy) {
					StartCoroutine(LateSetBool("on",_on));
					StartCoroutine(On());
				}
			}
		} bool _on;

		public bool Worn {
			get { return worn; }
			set { if (worn==value) return;
				worn=value;
				if (hand) hand.ikActive = worn;
			}
		} bool worn = false;

		public bool Used {get;set;}

		public uint Uses {get;set;}
		public float time {get;set;}

		public Lamp() {
			dash = new util::key((n)=>sprint=n);
		}

		public override void Awake() {
			base.Awake();
			this.GetYAML();
			animator = GetComponent<Animator>();
			foreach (Light elem in GetComponentsInChildren<Light>())
				elem.enabled = false;
			GetComponent<AudioSource>().clip = auSwitch;
		}

		public override void Start() { hand = Player.left; }

		public void Use() { if (Worn && !wait) StartCoroutine(On()); }
		public void Attack() { Use(); }

		IEnumerator On() {
			wait = true;
			yield return new WaitForSeconds(0.125f);
			if (time>0) {
				var l = GetComponentsInChildren<Light>()[0];
				if (l) l.GetComponent<Light>().enabled = on;
			} yield return new WaitForSeconds(0.25f);
			wait = false;
		}

		IEnumerator LateSetBool(string s, bool t) {
			yield return new WaitForEndOfFrame();
			if (animator.enabled) animator.SetBool(s,t);
		}

		public virtual void Wear() {
			gameObject.SetActive(true);
			Worn = true;
			StartCoroutine(LateSetBool("worn",worn));
			on = true;
		}

		public virtual void Stow() {
			Worn = false;
			StartCoroutine(LateSetBool("worn",worn));
			gameObject.SetActive(false);
		}
	}
}

