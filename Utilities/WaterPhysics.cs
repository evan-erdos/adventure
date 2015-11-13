/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-08 * Water Physics */

using UnityEngine;
using System.Collections;
using mvmt=PathwaysEngine.Movement;

namespace PathwaysEngine.Utilities {
	public class WaterPhysics : MonoBehaviour {
		public float height = 24f, drag = 2f;
//		float initDrag;
//		int depth = 32;
		public Color underwater = new Color32(12,66,66,216);
		BoxCollider cl;
		public GameObject splash;

		void Awake() {
			cl = (GetComponent<BoxCollider>()) ?? gameObject.AddComponent<BoxCollider>();
			//cl.center = new Vector3(0,(float)depth/2-height,0);
			//cl.size = new Vector3(2048f,(float)depth,2048f);
			cl.isTrigger = true;
		}

		void OnTriggerEnter(Collider other) {
			if (other.transform.parent && other.transform.parent.CompareTag("MainPlayer")) {
				CameraFade.StartAlphaFade(underwater,false,0.25f,0.125f,
					()=>{StartCoroutine(Keep(underwater));});
				var temp = other.gameObject.GetComponentInChildren<mvmt::ThirdPersonController>();
				if (temp) {
					temp.dead = true;
					var rbTemp = temp.GetComponentsInChildren<Rigidbody>();
					if (rbTemp!=null) {
						foreach (var elem in rbTemp) {
							//initDrag = elem.drag;
							elem.drag = drag;
							//elem.angularDrag = drag;
						}
					}
				}
			} else {//if (other.gameObject.CompareTag("Player")) {
				var rbs = other.gameObject.GetComponentsInChildren<Rigidbody>();
				if (rbs!=null) {
					foreach (var elem in rbs) {
						//initDrag = elem.drag;
						elem.drag = drag;
						//elem.angularDrag = drag;
					}
				}
			}
		}


		public static IEnumerator Keep(Color c) {
			yield return 0;
			CameraFade.SetScreenOverlayColor(c);
		}

#if PERMANENT
		void OnTriggerStay(Collider other) {
			if (other.transform.parent.CompareTag("MainPlayer")) StartCoroutine(Keep());
		}

		void OnTriggerExit(Collider other) {
			if (other.gameObject.CompareTag("Player")) {
				CameraFade.StartAlphaFade(underwater,true,0.25f,0.25f);
				var temp = other.gameObject;
				foreach (var elem in temp.GetComponentsInChildren<Rigidbody>()) {
					elem.drag = initDrag;
	//				elem.angularDrag = 64f;
				}
			}
		}

		public IEnumerator Keep() {
			for (;;) CameraFade.StartAlphaFade(underwater,true,1f,1f);
		}
#endif
	}
}