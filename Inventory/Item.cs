/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-22 * Item */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using intf=PathwaysEngine.Adventure;

namespace PathwaysEngine.Inventory {
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(AudioSource))]
	[RequireComponent(typeof(SphereCollider))]
	public partial class Item : intf::Thing, IGainful {
		public AudioClip sound;
		public Texture2D icon;
		public Rigidbody rb;
		public SphereCollider cl;
		public AudioSource au;

		public bool Held {
			get { return held; }
			set { held = value;
				cl.enabled = !held;
				rb.isKinematic = held;
				rb.useGravity = !held;
				au.enabled = !held;
			}
		} bool held = false;

		public int Cost { get; set; }

		public float Mass {
		 	get { return rb.mass; }
			set { rb.mass = value; } }

		public override void Awake() { base.Awake();
			cl = GetComponent<SphereCollider>();
			rb = GetComponent<Rigidbody>();
			au = GetComponent<AudioSource>();
			cl.isTrigger = true;
			held = false;
		}

		public virtual void Take() {
			if (gameObject.activeInHierarchy && au.enabled)
				au.PlayOneShot(sound);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			held = true;
			gameObject.SetActive(false);
		}

		public virtual void Drop() {
			gameObject.SetActive(true);
			held = false;
			transform.parent = null;
			if (au.enabled) au.PlayOneShot(sound);
			rb.AddForce(Quaternion.identity.eulerAngles*4,
						ForceMode.VelocityChange);
		}

		public virtual void Buy() { }
		public virtual void Sell() { }
		public override bool Equals(object obj) { return (base.Equals(obj)); }
		public override int GetHashCode() { return (base.GetHashCode()); }
		public override string ToString() { return uuid; }
		public static bool operator ==(Item a, Item b) {
			return (!(a.GetType()!=b.GetType() || a.uuid!=b.uuid)); }
		public static bool operator !=(Item a, Item b) { return (!(a==b)); }
	}
}
