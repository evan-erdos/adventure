/* Ben Scott * bescott@andrew.cmu.edu * 2014-08-11 * Anchor */

using UnityEngine;
using adv=PathwaysEngine.Adventure;


namespace PathwaysEngine.Utilities {


    public class Anchor : MonoBehaviour {
        public adv::Corpus bodyPart;
        public Transform src;

        void Start() {
            var temp = transform.parent;
            transform.parent = src;
            transform.parent = temp;
        }

        void Update() {
            if (transform && src) {
                transform.position = src.position;
                transform.rotation = src.rotation;
            }
        }
    }
}