/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-07 * Lock Transform */

using UnityEngine;
using System.Collections;


namespace PathwaysEngine.Utilities {


    public class LockTransform : MonoBehaviour {
        public bool isLocked = true, useInit = true;
        //Vector3 initT = Vector3.zero;
        Transform src, initParent;
        public Transform tgt;

        void Start() {
            src = transform;
            initParent = src.parent;
            src.parent = tgt;
            //if (useInit) initT = src.localPosition;
            src.parent = initParent;
        }

        //void FixedUpdate() { FixTransform(); }
        void Update() { FixTransform(); }

        void FixTransform() {
            if (isLocked && src && tgt) {
                src.position = tgt.position;
                src.rotation = tgt.rotation;
            }
        }
    }
}