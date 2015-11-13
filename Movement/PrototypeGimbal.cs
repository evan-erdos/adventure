/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-01 * Player Gimbal */

#define DEBUG
#define KINEMATICS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using invt=PathwaysEngine.Inventory;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Movement {
	public class PrototypeGimbal : MonoBehaviour {
		bool isRestoring, isDead, wasHit;
		uint index;
		int iter, maskPlayer;
		float inv, angle, curL, maxL, timeT;
		public float modSprint,modCrouch,thetaL,thetaT;
		double sigma, cutoff;
		double[] rotArray;
	 	public IMotor motor;
		RaycastHit hitSphere;
		public GameObject childPlayer;
		Transform mCamera;
		Vector3 motorRotation,initCam;
		public Transform mapPlayer,deadtemp;
		public util::axis roll;

		internal PrototypeGimbal() {
			isRestoring = true;		isDead 		= false;
			index 		= 0;		sigma		= 0;
			iter 		= 16;		inv			= 1.0f/iter;
			modSprint 	= 2.1f;		modCrouch 	= 3.5f;
			thetaT 		= 50.0f;	timeT 		= 0.8f;
			angle 		= 50.0f;	rotArray 	= new double[iter];
			thetaL 		= 0.0f;		maxL 		= 0.5f;
			cutoff 		= 0.001;
			roll 		= new util::axis((n)=>roll.input=n);
		}

		public void Start() {
			childPlayer = GameObject.FindWithTag("Player");
			maskPlayer = LayerMask.NameToLayer("Player");
			//motor = childPlayer.GetComponent<IMotor>();
			mCamera = Pathways.mainCamera.transform;
			gameObject.GetComponentInChildren<Camera>().backgroundColor = RenderSettings.fogColor;
			if (mapPlayer) mapPlayer = Instantiate((mapPlayer as Object), transform.position, transform.rotation) as Transform;
			util::CameraFade.StartAlphaFade(RenderSettings.fogColor, true, 0.5f, 0.5f);
		}

		public void FixedUpdate() {
			if (Mathf.Abs(Time.deltaTime)<0.01f) return;
			childPlayer.transform.parent = null;
			transform.position = childPlayer.transform.position;
			childPlayer.transform.parent = transform;
			rotArray[(int)index%iter] = roll.input*angle*Time.smoothDeltaTime;
			foreach (float entry in rotArray) sigma += entry;
			sigma *= inv*((motor.isSprinting)?modSprint:1.0f);
			index = (uint)(index%iter)+1;
			if (motor.isGrounded && motor.wasGrounded) {
				initCam = transform.position+Vector3.up*1.8f;
				maxL = ((motor.isSprinting)?modSprint*0.5f:0.5f);
				if (roll.input != 0) {
					wasHit = Physics.SphereCast(initCam,0.4f,initCam+mCamera.TransformDirection(Vector3.right)*maxL*roll.input,out hitSphere, 1f, ~maskPlayer);
					maxL = Mathf.Min(maxL,(wasHit)?hitSphere.distance:maxL);
					if (Mathf.Abs(thetaL-maxL)>0.1f)
						thetaL = Mathf.SmoothDampAngle(thetaL, roll.input*maxL,ref curL,0.1f,5f,Time.deltaTime);
				} else thetaL = Mathf.SmoothDampAngle(thetaL, 0, ref curL, 0.1f, 5f, Time.deltaTime);
				motorRotation.Set(thetaL, mCamera.localPosition.y, 0);
				mCamera.localPosition = motorRotation;
				isRestoring = true;
#if KINEMATICS
			} else if (motor.isGrounded && !motor.wasGrounded) {
				if (Mathf.Abs(Mathf.DeltaAngle(transform.transform.eulerAngles.x, 0))>thetaT
				|| Mathf.Abs(Mathf.DeltaAngle(transform.transform.eulerAngles.y, 0))>thetaT
				|| Mathf.Abs(Mathf.DeltaAngle(transform.transform.eulerAngles.z, 0))>thetaT) PlayerReplace();
#endif
			} if (isRestoring || isDead) { // isDead should be used to get up, after delay or when stopped
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, timeT*Time.smoothDeltaTime*8);
				if (transform.rotation == Quaternion.identity) isRestoring = false;
				else isRestoring = true;
				if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.x,0))<cutoff
				|| Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y,0))<cutoff
				|| Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z,0))<cutoff)
					transform.rotation = Quaternion.identity;
			} if (!motor.isGrounded) transform.RotateAround(
				childPlayer.transform.position,childPlayer.transform.forward,(float)sigma);
		}

		public void PlayerReplace() {
			var playerPack = gameObject.GetComponentInChildren<invt::Backpack>();
			if (playerPack) playerPack.DropAll();
#if OLD
			GameObject oldPlayer = gameObject.GetComponentInChildren<CharacterController>().gameObject;
			Transform temp = Instantiate(deadPlayer,oldPlayer.transform.position,oldPlayer.transform.rotation) as Transform;
			temp.GetComponent<Rigidbody>().velocity = gameObject.GetComponentInChildren<IMotor>().velocity;
			temp.parent = null;
			isDead = true;
			Destroy(gameObject);
#endif
			deadtemp.gameObject.SetActive(true);
			deadtemp.localPosition = childPlayer.GetComponent<Transform>().localPosition;
			deadtemp.GetComponent<Rigidbody>().velocity = childPlayer.GetComponent<IMotor>().velocity;
			childPlayer.gameObject.SetActive(false);
			deadtemp.parent = null;
			Destroy(gameObject);
		}
	}
}
