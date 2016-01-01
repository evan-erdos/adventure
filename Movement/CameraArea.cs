/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-23 * Camera Area */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using lit=PathwaysEngine.Literature;


namespace PathwaysEngine.Movement {


	/** `CameraArea` : **`MonoBehaviour`**
	 *
	 * This class provides a way to present static views of an
	 * area such that the player can interact with controls.
	 **/
	[RequireComponent(typeof(Collider))]
	public class CameraArea : MonoBehaviour {
		Camera mainCamera;
		Collider _collider;
		public Transform target;
		public float dist = 1f;
		public float nearClip = 0.1f;
		float time = 1f;
		bool wait = false;

		/** `IsActive` : **`bool`**
		 *
		 * This property keeps a bunch of things in sync, and
		 * represents if the `CameraArea` is currently enabled.
		 **/
		public bool IsActive {
			get { return isActive; }
			set { if (isActive==value) return;
				isActive = value;
				_collider.enabled = !isActive;
				if (isActive) {
					mainCamera.transform.rotation = target.transform.rotation;
					mainCamera.transform.position = target.transform.position;
					Pathways.GameState = GameStates.View;
				} else {
					mainCamera.transform.localPosition = Vector3.zero;
					mainCamera.transform.localRotation = Quaternion.identity;
					Pathways.GameState = GameStates.Game;
				}
			}
		} bool isActive = false;


		/** `Entering` : **`coroutine`**
		 *
		 * This is the usual kind of "guarded" `coroutine` I've
		 * been using to ensure that repeated calls won't start
		 * an enormous number of unique `coroutines` instead of
		 * simply keeping one going. Also provides a buffer to
		 * the `Terminal` via the `yield`ed `WaitForSeconds`.
		 **/
		IEnumerator Entering(bool t) {
			if (!wait) {
				wait = true;
				IsActive = t;
				lit::Terminal.Log(Pathways.messages["view"]);
				yield return new WaitForSeconds(time);
				wait = false;
			}
		}


		void Awake() {
			mainCamera = Camera.main;
			if (!mainCamera)
				throw new System.Exception("No Main Camera!");
			_collider = GetComponent<Collider>();
			_collider.isTrigger = true;
			var camera = GetComponentInChildren<Camera>();
			if (!camera && !target)
				throw new System.Exception("No Target!");
			target = camera.transform;
			camera.enabled = false;
		}

		void Update() {
			if (Input.GetButtonDown("Submit") || Input.GetButtonUp("Menu")) {
				IsActive = false; OnMouseExit();
			}
		}

		void LateUpdate() {
			if (!IsActive) return;
			mainCamera.transform.position = target.transform.position;
			mainCamera.transform.rotation = target.transform.rotation;
			if (Pathways.CursorGraphic==Cursors.None)
				Pathways.CursorGraphic = Cursors.Pick;
		}

		IEnumerator OnMouseEnter() {
            if (Vector3.Distance(transform.position,Player.Position)>dist) {
                Pathways.CursorGraphic = Cursors.None; yield break; }
            Pathways.CursorGraphic = Cursors.Look;
            if (Input.GetButton("Fire1"))
                yield return StartCoroutine(Entering(true));
        }

        void OnMouseExit() {
            Pathways.CursorGraphic = Cursors.None;
            StopAllCoroutines();
            wait = false;
        }
	}
}
