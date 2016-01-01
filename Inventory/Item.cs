/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-22 * Item */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using adv=PathwaysEngine.Adventure;
using lit=PathwaysEngine.Literature;

namespace PathwaysEngine.Inventory {


    /** `Item` : **`class`**
     *
     * Represents anything that the `Player` can take, drop, or
     * use in any other way. Does not apply to any interactive
     * entities that the `Player` can't take with them.
     **/
    [RequireComponent(typeof(Rigidbody))]
    public partial class Item : adv::Thing, IGainful {
        float volume = 0.9f, dist = 4f;
        public AudioClip sound;
        public Sprite Icon;


        /** `Held` : **`bool`**
         *
         * While the `Item` does not neccesarily know if the
         * `Player` is holding it, but does need to know, and
         * will disable its components on this basis.
         **/
        public virtual bool Held {
            get { return held; }
            set { held = value;
                foreach (Transform child in transform)
                    child.gameObject.SetActive(!held);
                rigidbody.isKinematic = held;
                rigidbody.useGravity = !held;
            }
        } protected bool held = false;

        //public uint Uses {get;set;}

        /** `Cost` : **`int`**
         *
         * Clearly, this represents the price of an `Item`s.
         **/
        public int Cost { get; set; }


        /** `Mass` : **`real`**
         *
         * This simply extends `Rigidbody.mass`.
         **/
        public float Mass {
            get { return rigidbody.mass; }
            set { rigidbody.mass = value; } }


        public virtual bool Use() { return Drop(); }


        /** `Take()` : **`bool`**
         *
         * Local callback for the global command, `Take()`.
         * Currently, this does a bunch of nonsense specific
         * to `Unity3D`, such as reparenting to the `Player`.
         **/
        public virtual bool Take() {
            lit::Terminal.LogCommand(string.Format(
                "You take the {0}.", name));
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            Held = true;
            gameObject.SetActive(false);
            return true;
        }

        /** `Drop()` : **`bool`**
         *
         * Inverse of `Take()`, which drops the object, and
         * reparents it to `root`, or the global transform in
         * Unity. Also prints a `Terminal` message.
         **/
        public virtual bool Drop() {
            gameObject.SetActive(true);
            if (sound)
                AudioSource.PlayClipAtPoint(
                    sound,transform.position,volume);
            lit::Terminal.LogCommand(string.Format(
                "You drop the {0}.", name));
            Held = false;
            //transform.localPosition = new Vector3(0f,1f,0.5f);
            transform.parent = null;
            transform.position = Player.Position;
            rigidbody.AddForce(
                Quaternion.identity.eulerAngles*4,
                ForceMode.VelocityChange);
            return true;
        }

        public virtual bool Buy() { return false; }

        public virtual bool Sell() { return false; }

        public override void Awake() { base.Awake();
            gameObject.layer = LayerMask.NameToLayer("Item");
            var wasKinematic = rigidbody.isKinematic;
            Held = false;
            if (wasKinematic)
                rigidbody.isKinematic = true;
        }

        public override IEnumerator OnMouseOver() {
            while ((transform.position-Player.Position).sqrMagnitude<dist) {
            //if (Vector3.Distance(transform.position,Player.Position)>dist) {
                //Pathways.CursorGraphic = Cursors.None;
                //yield break;
            //}
            Pathways.CursorGraphic = Cursors.Hand;
            if (Input.GetButton("Fire1")) {
                Pathways.CursorGraphic = Cursors.Grab;
                yield return new WaitForSeconds(0.1f);
                OnMouseExit();
                Player.Take(this);
            } else yield return new WaitForSeconds(0.05f);

            } OnMouseExit();
        }

#if HACK

        public override bool Equals(object obj) { return (base.Equals(obj)); }

        public override int GetHashCode() { return (base.GetHashCode()); }
        public static bool operator ==(Item a, Item b) {
            return ((a==null && b==null) || !(a.GetType()!=b.GetType() || a.Name!=b.Name)); }

        public static bool operator !=(Item a, Item b) { return (!(a==b)); }
#endif
    }
}
