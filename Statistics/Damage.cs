/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-06 * Damage */

using UnityEngine;
using System.Collections;


namespace PathwaysEngine.Statistics {


    /** `Damage` : **`struct`**
    |*
    |* Defines a low-level object to send statistical metadata
    |* between the sorts of things that typically need to do
    |* things like that.
    |**/
#if DUMB
    struct Damage {
        public int crit = 20;

        void Hit(int damage) {
            vitality -= damage;
            if (vitality<=0 && vitality>=(-crit)) Kill(false);
            if (vitality<=crit) Kill(true);
        }

        void Kill(bool fatal) {
            if (deadAnim) Instantiate(deadAnim, transform.position, transform.rotation);
            if (deadReplacement) {
                Instantiate(deadReplacement, transform.position, transform.rotation);
                StartCoroutine(Delay(0.1f));
                if (usePhysics) {
                    var rb = GetComponent<Rigidbody>();
                    deadRoot.velocity = rb.velocity;
                    deadRoot.angularVelocity = rb.angularVelocity;
                }
            } Destroy(gameObject);
        }
    }
#endif
}