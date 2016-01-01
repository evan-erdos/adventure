/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-29 * SmoothLookAt */

using UnityEngine;


namespace PathwaysEngine.Utilities {


	public class SmoothLookAt : MonoBehaviour {
		public bool smooth = true;
		public float damping = 6f;
		public Transform tgt;

		void Start() {
			var _rigidbody = GetComponent<Rigidbody>();
		   	if (_rigidbody) _rigidbody.freezeRotation = true;
		}

		void LateUpdate() {
			if (tgt) {
				if (smooth) {
					var rotation = Quaternion.LookRotation(
						tgt.position-transform.position);
					transform.rotation = Quaternion.Slerp(
						transform.rotation, rotation,
						Time.deltaTime * damping);
				} else transform.LookAt(tgt);
			}
		}
	}
}