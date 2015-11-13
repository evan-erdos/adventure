/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-06 * Door */

using UnityEngine;
using System.Collections;

namespace PathwaysEngine.Adventure { //*// “Help, Help, Snark in MTS!”
	public class Door : MonoBehaviour {
		byte frameOpen;
		internal float speedOpen, speedDelta;
		public bool isLocked, isSwitching, isStuck, isMoving, isReverse, isProximity, isInitOpen, isInitOnly;
		AudioSource au;
		Vector3 dirInit, dirOpen, dirFrame, dirTarget, dirDelta;
		public AudioClip audOpen;

		internal bool isOpen {
			get { return (frameOpen>0xFE); }
			set { dirTarget = (value && !isReverse)?(dirOpen):(dirInit); }
		}

		public Door() {
			isProximity	= true;		isReverse 	= false;
			isLocked 	= false;	isSwitching	= false;
			isInitOpen	= false;	isInitOnly 	= false;
			isStuck		= false;	isMoving 	= false;
			frameOpen 	= 0x0;		speedOpen 	= 0.2f;
		}

		internal void Awake() {
			au = GetComponent<AudioSource>();
			au.clip = audOpen;
			au.loop = true;
		}

		internal void Start() {
			dirInit = transform.position;
			dirOpen	= transform.position+(Vector3.left*4);
			dirTarget = dirOpen;
			if (isInitOpen) {
				transform.position = dirTarget;
				isOpen = true;
			}
		}

		internal void Update() {
			isOpen = (frameOpen>0xFE);
			speedOpen = Mathf.SmoothDamp(speedOpen,(isStuck)?(0.0f):(0.6f), ref speedDelta, 0.1f);
			if (transform.position != dirTarget)
				transform.position = Vector3.SmoothDamp(transform.position,dirTarget,ref dirDelta,0.8f,speedOpen,Time.deltaTime);
		}

		internal void OnTriggerStay(Collider other) {
#if DEBUG
			Debug.Log(System.Convert.ToString(frameOpen,2));
#endif
			if (isProximity && frameOpen<128 && other.gameObject.tag=="Player")
				frameOpen = unchecked ((byte)((frameOpen<<0x1)|0x1));
			if (isOpen && isInitOnly) GetComponent<Collider>().enabled = false;
		}

		internal void OnTriggerExit(Collider other) {
			if (isProximity && frameOpen>0 && other.gameObject.layer==LayerMask.NameToLayer("Player")) frameOpen = 0x0;
		}

#if COMPLEX_ANIM
		internal void Open() {
			if (!isOpen) {
				au.clip = audOpen;
				au.Play();
				animation["Door Open"].time = 0;
				animation["Door Open"].speed = 1;
				animation.Play("Door Open");
				isOpen = true;
			}
		}

		internal void Close() {
			if (isOpen) {
				au.clip = audOpen;
				au.Play();
				animation["Door Open"].time = animation["Door Open"].length;
				animation["Door Open"].speed = -1;
				animation.Play("Door Open");
				isOpen = false;
			}
		}
#endif
	}
}
