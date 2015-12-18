/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-23 * CursorHover */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using lit=PathwaysEngine.Literature;

namespace PathwaysEngine.Puzzle {

    /** `CursorHover` : **`MonoBehaviour`**
    |*
    |* Deals with the cursor, how it displays, and changes it
    |* when mousing over this particular entity. Unless there
    |* is a rigidbody on the parent, this needs to have one for
    |* it to be triggered by clicks.
    |**/
    [RequireComponent(typeof (Collider))]
    public class CursorHover : MonoBehaviour {
        bool first = false;
        public Cursors cursor = Cursors.Pick;
        public float distance = 4f;
        Collider _collider;

        void Awake() {
            _collider = GetComponent<Collider>();
            if (!_collider) throw new System.Exception("No collider!");
        }

        IEnumerator OnMouseOver() {
            var d = Vector3.Distance(transform.position,Player.Position);
            if (d>distance) {
                Pathways.CursorGraphic = Cursors.None; yield break; }
            Pathways.CursorGraphic = cursor;
            if (Input.GetButton("Fire1") && !first) {
                lit::Terminal.LogCommand(
                    "Nothing special happens.");
                first = true;
            }
        }

        void OnMouseExit() {
            Pathways.CursorGraphic = Cursors.None;
            StopAllCoroutines();
        }
    }
}
