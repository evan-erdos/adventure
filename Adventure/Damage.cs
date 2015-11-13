/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-06 * Damage */

using UnityEngine;
using System.Collections;

namespace PathwaysEngine.Adventure {
	public class Damage : MonoBehaviour {
		public bool usePhysics = true;
		public int vitality = 64;
		public int crit = 20;
		public Transform deadAnim, deadReplacement;
		public Rigidbody deadRoot;

		void Hit(int damage) {
			vitality -=damage;
			if (vitality<=0&&vitality>=(-crit)) Kill(false);
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

		IEnumerator Delay(float t) { yield return new WaitForSeconds(t); }
	}
}