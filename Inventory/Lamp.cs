/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-13 * Lamp */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using mvmt=PathwaysEngine.Movement;
using util=PathwaysEngine.Utilities;


namespace PathwaysEngine.Inventory {


    /** `Lamp` : **`class`**
     *
     * A kind of `Item` which the `Player` can hold and use as
     * a source of light. An instant classic! (remember Zork?)
     **/
    public partial class Lamp : Item, IWieldable {
        bool wait = false;
        List<Light> lights;
        public AudioClip auSwitch;
        Animator animator;
        public util::key dash;


        public override bool Held {
            get { return held; }
            set { held = value;
                foreach (Transform child in transform)
                    child.gameObject.SetActive(!held || Worn);
                rigidbody.isKinematic = held;
                rigidbody.useGravity = !held;
            }
        }


        bool sprint {
            get { return _sprint; }
            set { _sprint = value;
                if (gameObject.activeInHierarchy && animator)
                    StartCoroutine(LateSetBool("sprint",_sprint)); }
        } bool _sprint = false;


        public bool on {
            get { return _on; }
            set { if (!Held) return;
                _on = value;
                if (gameObject.activeInHierarchy && animator) {
                    StartCoroutine(LateSetBool("on",_on));
                    StartCoroutine(On());
                }
            }
        } bool _on;


        public bool Worn {
            get { return worn; }
            set { if (worn==value) return;
                worn = value;
                transform.parent = (worn)
                    ? Pathways.player.left.transform
                    : Pathways.player.transform;
                foreach (Transform child in transform)
                    child.gameObject.SetActive(worn);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        } bool worn = false;

        public bool Used {get;set;}
        public uint Uses {get;set;}
        public float time {get;set;}


        public Lamp() {
            dash = new util::key((n)=>sprint=n);
        }


        public override bool Use() {
            if (Worn && !wait && gameObject.activeInHierarchy)
                StartCoroutine(On());
            return false;
        }


        public bool Attack() => Use();


        IEnumerator On() {
            wait = true;
            yield return new WaitForSeconds(0.125f);
            if (time>0)
                foreach (var light in lights)
                    light.enabled = on;
            yield return new WaitForSeconds(0.25f);
            wait = false;
        }


        IEnumerator LateSetBool(string s, bool t) {
            yield return new WaitForEndOfFrame();
            if (animator && animator.enabled)
                animator.SetBool(s,t);
        }


        public virtual bool Wear() {
            gameObject.SetActive(true);
            Worn = true;
            if (gameObject.activeInHierarchy)
                StartCoroutine(LateSetBool("worn",worn));
            on = true;
            return false;
        }


        public virtual bool Stow() {
            Worn = false;
            if (gameObject.activeInHierarchy)
                StartCoroutine(LateSetBool("worn",worn));
            gameObject.SetActive(false);
            return false;
        }


        public override void Awake() { base.Awake();
            animator = GetComponent<Animator>();
            GetComponent<AudioSource>().clip = auSwitch;
        }


        public override void Start() { base.Start();
            on = true;
            lights = new List<Light>(GetComponentsInChildren<Light>());
        }

    }
}

