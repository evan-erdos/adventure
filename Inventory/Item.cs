/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-22 * Item */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using adv=PathwaysEngine.Adventure;
//using static PathwaysEngine.Literature.Terminal;
using lit=PathwaysEngine.Literature;


namespace PathwaysEngine.Inventory {


    /** `Item` : **`Thing`**
     *
     * Represents anything that the `Player` can take, drop, or
     * use in any other way. Does not apply to any interactive
     * entities that the `Player` can't take with them.
     **/
    [RequireComponent(typeof(Rigidbody))]
    public class Item : adv::Thing, IGainful {
        static float volume = 0.9f;
        [SerializeField] AudioClip sound;
        [SerializeField] public Sprite Icon;


        public virtual bool Held {
            get { return held; }
            set { held = value;
                foreach (Transform child in transform)
                    child.gameObject.SetActive(!held);
                rigidbody.isKinematic = held;
                rigidbody.useGravity = !held;
            }
        } protected bool held = false;

        public int Cost {get;set;}

        public override float Radius => 8f;

        public float Mass {
            get { return rigidbody.mass; }
            set { rigidbody.mass = value; }
        }


        public override string Template => $@"
**{Name}** : <cmd>|{Mass:N}kg|</cmd>

{description.init}{description.raw}
{(Cost!=0)?"It is worth <cost>"+Cost+"</cost> coins.":""}

{description.Help}";

        public virtual bool Use() => Drop();


        /** `Take()` : **`bool`**
         *
         * Local callback for the global command, `Take()`.
         * Currently, this does a bunch of nonsense specific
         * to `Unity3D`, such as reparenting to the `Player`.
         **/
        public virtual bool Take() {
            lit::Terminal.Log(
                $"<cmd>You take the </cmd>{Name}<cmd>.</cmd>");
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
            AudioSource.PlayClipAtPoint(
                sound,transform.position,volume);
            lit::Terminal.Log(
                $"<cmd>You drop the </cmd>{Name}<cmd>.</cmd>");
            Held = false;
            rigidbody.AddForce(
                Quaternion.identity.eulerAngles*4,
                ForceMode.VelocityChange);
            return true;
        }

        public virtual bool Buy() => false;

        public virtual bool Sell() => false;


        public override void Awake() { base.Awake();
            gameObject.layer = LayerMask.NameToLayer("Item");
            var wasKinematic = rigidbody.isKinematic;
            Held = false;
            if (wasKinematic)
                rigidbody.isKinematic = true;
        }


        public override IEnumerator OnMouseOver() {
            while (Player.IsNear(this)) {
                Pathways.CursorGraphic = Cursors.Hand;
                if (Input.GetButton("Fire1")) {
                    Pathways.CursorGraphic = Cursors.Grab;
                    yield return new WaitForSeconds(0.1f);
                    OnMouseExit();
                    Player.NearestTo(this)?.Take(this);
                } else yield return new WaitForSeconds(0.05f);
            } OnMouseExit();
        }


        public override void Deserialize() =>
            Pathways.Deserialize<Item,Item_yml>(this);
    }
}
