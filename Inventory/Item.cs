/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-22 * Item */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using intf=PathwaysEngine.Adventure;

namespace PathwaysEngine.Inventory {


    /** `Item` : **`class`**
    |*
    |* Represents anything that the `Player` can take, drop, or
    |* use in any other way. Does not apply to any interactive
    |* entities that the `Player` can't take with them.
    |**/
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(SphereCollider))]
    public partial class Item : intf::Thing, IGainful {
        float dist = 3f;
        public AudioClip sound;
        public Texture2D icon;
        AudioSource _audio;
        Rigidbody _rigidbody;
        SphereCollider _collider;


        /** `Held` : **`bool`**
        |*
        |* While the `Item` does not neccesarily know if the
        |* `Player` is holding it, but does need to know, and
        |* will disable its components on this basis.
        |**/
        public bool Held {
            get { return held; }
            set { held = value;
                _collider.enabled = !held;
                _rigidbody.isKinematic = held;
                _rigidbody.useGravity = !held;
                _audio.enabled = !held;
            }
        } bool held = false;


        /** `Cost` : **`int`**
        |*
        |* Clearly, this represents the price of an `Item`s.
        |**/
        public int Cost { get; set; }


        /** `Mass` : **`real`**
        |*
        |* This basically just extends the `mass` field from
        |* `UnityEngine.Rigidbody`.
        |**/
        public float Mass {
            get { return _rigidbody.mass; }
            set { _rigidbody.mass = value; } }


        /** `Take()` : **`function`**
        |*
        |* Local function for the global command for `Take`-ing
        |* `Items`. Currently, this does a bunch of nonsense
        |* specific to `Unity3D`, such as reparenting this
        |* `Item` to the `Player`.
        |**/
        public virtual void Take() {
            if (gameObject.activeInHierarchy && _audio.enabled)
                _audio.PlayOneShot(sound);
            Terminal.Log(string.Format("You take the {0}.", name),
                Formats.Command, Formats.Newline);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            Held = true;
            gameObject.SetActive(false);
        }


        /** `Drop()` : **`function`**
        |*
        |* Inverse of `Take()`, which drops the object, and
        |* reparents it to `root`, or the global transform in
        |* Unity. Also prints a `Terminal` message.
        |**/
        public virtual void Drop() {
            gameObject.SetActive(true);
            Terminal.Log(string.Format("You drop the {0}.", name),
                Formats.Command, Formats.Newline);
            Held = false;
            transform.parent = null;
            transform.position = Player.Position;
            if (_audio.enabled) _audio.PlayOneShot(sound);
            _rigidbody.AddForce(
                Quaternion.identity.eulerAngles*4,
                ForceMode.VelocityChange);
        }

        public virtual void Buy() { }

        public virtual void Sell() { }

        public override void Awake() {
            _collider = GetComponent<SphereCollider>();
            _rigidbody = GetComponent<Rigidbody>();
            _audio = GetComponent<AudioSource>();
            _collider.isTrigger = true;
            base.Awake();
            gameObject.layer = LayerMask.NameToLayer("Item");
            Held = false;
        }

        IEnumerator OnMouseOver() {
            if (Vector3.Distance(transform.position,Player.Position)>dist) {
                Pathways.CursorGraphic = Cursors.None; yield break; }
            Pathways.CursorGraphic = Cursors.Hand;
            if (Input.GetButton("Fire1")) {
                Pathways.CursorGraphic = Cursors.Grab;
                yield return new WaitForSeconds(0.1f);
                OnMouseExit();
                Player.Take(this);
            }
        }

        void OnMouseExit() {
            Pathways.CursorGraphic = Cursors.None;
            StopAllCoroutines();
        }


        public override bool Equals(object obj) { return (base.Equals(obj)); }

        public override int GetHashCode() { return (base.GetHashCode()); }

        public override string ToString() { return uuid; }

        public static bool operator ==(Item a, Item b) {
            return (!(a.GetType()!=b.GetType() || a.uuid!=b.uuid)); }

        public static bool operator !=(Item a, Item b) { return (!(a==b)); }
    }
}
