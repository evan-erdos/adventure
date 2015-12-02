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
        Collider _collider;

        public override void Awake() { base.Awake();
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }

        public string Desc_Style(Description desc) {
            return string.Format("{1}{0}",desc,
                "## Special Encounter ##\n"); }

        public override void Start() { base.Start();
            if (input==Inputs.Elapsed)
                StartCoroutine(TimedEncounter(time));
        }

        void OnTriggerEnter(Collider o) {
            if (input==Inputs.Trigger && Player.IsCollider(o))
                StartCoroutine(BeginEncounter());
        }

        IEnumerator BeginEncounter() {
            Terminal.Log(Desc_Style(description));
            if (reuse) {
                if (_collider) _collider.enabled = false;
                yield return new WaitForSeconds(1f);
                if (_collider) _collider.enabled = true;
            } else gameObject.SetActive(false);
        }

        IEnumerator TimedEncounter(float t) {
            yield return new WaitForSeconds(t);
            StartCoroutine(BeginEncounter());
        }
    }
}

