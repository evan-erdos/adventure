/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-22 * Thing */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EventArgs=System.EventArgs;
using lit=PathwaysEngine.Literature;

namespace PathwaysEngine.Adventure {


    /** `Thing` : **`class`**
     *
     * An extremely important class, `Thing` is the base class
     * for anything that has any interface with the `Adventure`
     * namespace, the `Parser`, or the `Statistics`namespace,
     * or the `Terminal` or any deriving/text-based interfaces.
     **/
    partial class Thing : MonoBehaviour, IThing {
        bool waitViewing;
        protected internal new Rigidbody rigidbody;
        protected internal new Collider collider;

        public virtual bool Seen { get; set; }

        public string Name {
            get { return gameObject.name; } }

        public virtual lit::Description description {get;set;}

        static event lit::CommandEvent ViewEvent;


        /** `Thing` : **`constructor`**
         *
         * Currently unused, as `Unity` doesn't play very well
         * with the usual methods of instantiating classes, and
         * prefers to use whatever it uses to create its native
         * script type, `MonoBehaviour`.
         **/
        public Thing() { }


        /** `AddListener()` : **`function`**
         *
         * Every instantiated `Thing` calls if it detects the
         * `Player` is nearby via `OnTriggerEnter()`. It then
         * subscribes itself to or unsubscribes itself from the
         * global/static `Thing`, acts as an event handler to
         * the instances. Subscribers could have any number of
         * functions called iff the `Player` is nearby and the
         * `Player` issues an appropriate command.
         **/
        public static void AddListener(Thing thing) {
            ViewEvent += thing.View;
        }


        /** `RemoveListener()` : **`function`**
         *
         * Corollary to the `AddListener()` function.
         **/
        public static void RemoveListener(Thing thing) {
            ViewEvent -= thing.View;
        }

        public virtual bool Find() { return false; }

        public virtual bool View(
                        object source,
                        Thing target,
                        EventArgs e,
                        lit::Command c) {
            lit::Terminal.Log(description);
            return true;
        }

        public virtual bool View() {
            return View(null,this,EventArgs.Empty,
                new lit::Command()); }


        public virtual bool IsMatch(string s) {
            return description.Nouns.IsMatch(s); }


        IEnumerator Viewing() {
            waitViewing = true;
            View(this,this,EventArgs.Empty,new lit::Command());
            yield return new WaitForSeconds(1f);
            waitViewing = false;
        }


        public virtual void Awake() {
            gameObject.layer = LayerMask.NameToLayer("Thing");
            collider = GetComponent<Collider>();
            rigidbody = GetComponent<Rigidbody>();
            if (rigidbody!=null) {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
            }
            Deserialize();
            AddListener(this);
        }

        public virtual void Start() { }

        //public virtual void OnMouseEnter() {
        //    if (3f>(Player.Position-transform.position).sqrMagnitude)
        //        Pathways.CursorGraphic = Cursors.Pick;
        //}

        public virtual IEnumerator OnMouseOver() {
            while (5f>(Player.Position-transform.position).sqrMagnitude) {
                Pathways.CursorGraphic = Cursors.Pick;
                if (Input.GetButtonUp("Fire1") && !waitViewing)
                    yield return StartCoroutine(Viewing());
                else yield return new WaitForSeconds(0.1f);
            } OnMouseExit();
        }

        public virtual void OnMouseExit() {
            Pathways.CursorGraphic = Cursors.None;
            StopAllCoroutines();
        }


        public override string ToString() { return Name; }

        public static bool operator !(Thing o) {
            return (o==null); }
    }
}
