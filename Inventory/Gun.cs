/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-22 * Gun */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
	public class Gun : Weapon {
		public bool isAuto, isBurst, isPrimary;
		public uint countAmmo, sizeClip, countLoaded,
			countBursts, fovDefault, fovScoped, range;
		public float rateSpread, rateCool, rateScope,
			rateMax, spreadEndemic, spreadAimed, spreadMax,
			spreadJam, timeShot, sinceShot, timeReload,
			sinceReload, forceShot, damageShot, deltaShot;
		public string MunitionType;
		public string[] animNames, animFire, animReload;
		public enum WeaponTypes : byte { Projectile, Trajectile, Crystal, Melee };
		public WeaponTypes WeaponType = WeaponTypes.Projectile;
		public Animation am;
		public AudioClip audEmpty;
		public AudioClip[] audImpacts, audReloads;
		public Transform[] trPos; // Default, Attack, Shell, Stowed, Focus, Crossed
		public GameObject fireShell, fireFlash, fireExplosion;
		public GameObject[] ShotParticles;
		public Camera mCAMR;

		internal Gun() {
			isAuto 			= false;	isBurst 		= false;
			countAmmo 		= 24;		isPrimary 		= false;
			countLoaded 	= 7;		countBursts 	= 1;
			fovDefault		= 0;		fovScoped 		= 30;
			range 			= 128;		forceShot 		= 443.0f;
			damageShot 		= 1024.0f;	deltaShot 		= 256.0f;
			rate 			= 0.2f;		timeReload 		= 2.0f;
			rateSpread 		= 0.1f;		rateCool 		= 0.05f;
			rateScope		= 0.06f;	rateMax			= 0.3f;
			spreadEndemic 	= 0.1f;		spreadJam	 	= 1.0f;
			spreadMax 		= 0.1f;		spreadAimed 	= 0.1f;
			timeShot		= 0.05f;	sinceShot		= 0.0f;
			sizeClip 		= 8;		MunitionType 	= "9mm PARA";
		}

		public override void Awake() {
			base.Awake();
			mCAMR = Camera.main;
			am = GetComponent<Animation>();
			fovDefault = (uint)Camera.main.fieldOfView;
			var tempClips = new List<string>();
			foreach (AnimationState elem in am) {
				tempClips.Add(elem.name);
				elem.speed = (rate/elem.length)*1.5f;
			} // animNames = tempClips.toArray(typeof(string));
		}

		internal void Update() {
			if (sinceShot<rate) sinceShot+=Time.deltaTime;
			if (sinceReload<timeReload) sinceReload+=Time.deltaTime;
		//	StartCoroutine(Stow(toHolster));
			if (Time.timeScale>0) {
				StartCoroutine(Focus());
				if (Input.GetButtonDown("Reload")) Reload (true);
				if (Input.GetMouseButtonDown(0)) Use();
			}
		}

		public override void Attack() {
			if (sinceShot>rate && !am.isPlaying) {
#if TODO
				string cAnimation;
				if (animNames.Length>0) cAnimation = (string)animNames[UnityEngine.Random.Range(0, animNames.Length)];
				sinceShot = 0;
				if (attackSounds.Length>0) localAU.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Length)]);
#endif
				switch (WeaponType) {
					case WeaponTypes.Projectile :
					case WeaponTypes.Trajectile :
						Vector3 ProjectileDirection = new Vector3();
						ProjectileDirection = transform.forward;
						ProjectileDirection = Spray(ProjectileDirection, spreadEndemic);
#if TODO
						GameObject temp = Weapon.Instantiate(fireFlash,trPos[1].position,trPos[1].rotation) as GameObject;
							temp.transform.parent = null;
						GameObject newShell = Weapon.Instantiate(fireShell,trPos[2].position,trPos[2].rotation) as GameObject;
						newShell.rigidbody.AddRelativeForce(
							Vector3.up+UnityEngine.Random.insideUnitSphere*.2f,ForceMode.Impulse);
						newShell.rigidbody.AddRelativeTorque(
							Vector3.up+UnityEngine.Random.insideUnitSphere*.1f,ForceMode.Impulse);
						am.Play(cAnimation);
#endif
//	if (WeaponType == WeaponTypes.KinematicTrajectile) Weapon.Instantiate(Trajectile, trPos[1].position, trPos[1].rotation);
					if (WeaponType == WeaponTypes.Projectile) {
						RaycastHit ProjectileHit = new RaycastHit();
						Quaternion ProjectileNormal = new Quaternion();
						if (Physics.Raycast(transform.position,ProjectileDirection,out ProjectileHit,(int)range,layerMask)) {
							ProjectileNormal = Quaternion.FromToRotation(Vector3.up, ProjectileHit.normal);
							Weapon.Instantiate (ShotParticles[UnityEngine.Random.Range(0, ShotParticles.Length)], ProjectileHit.point, ProjectileNormal);
							if (ProjectileHit.rigidbody)
								ProjectileHit.rigidbody.AddForceAtPosition(forceShot*ProjectileDirection, ProjectileHit.point);
							//mCAMR.GetComponent("MouseLook").offsetY = DischargeClimbAngle; Anything else?
						}
					} break;
				case (WeaponTypes.Crystal) : break;
	//			case (WeaponTypes.Melee) : am.Play(cAnimation); break;
				}
			}
		}

		internal void Reload(bool canReload) {
			if (countLoaded < sizeClip && sinceReload > timeReload && canReload) {
			//	countAmmo = playerPack.AmmunitionTotal (rMunitionType);
				if (countAmmo>(sizeClip-countLoaded)) countAmmo -= sizeClip-countLoaded;
				else if (countAmmo < (sizeClip-countLoaded)) countLoaded = countAmmo;
				countAmmo = countAmmo-countLoaded;
				sinceReload = 0;
			}
		}

		//public void Stow() { StartCoroutine(Holster(true)); }
		//public void Equip() { StartCoroutine(Holster(false)); }

		internal IEnumerator Focus() {
			float cViewField = mCAMR.fieldOfView;
			float rTime = 0f, oTime = 0f;
			if (Input.GetMouseButtonDown (1)) while (Input.GetMouseButton(1)) {
				rTime+=Time.deltaTime;
				transform.localPosition = Vector3.Lerp(trPos[0].localPosition, trPos[4].localPosition, rTime);
				rTime /= rateScope;
				cViewField = Mathf.Lerp(fovDefault, fovScoped, rTime);
			//	cViewField = MathematicF.Coserp (fovDefault, fovScoped, rTime);
//				foreach (GameObject lCamera in pCameras) lCamera.camera.fieldOfView = cViewField;
				mCAMR.fieldOfView = cViewField;
				yield return null;
			} if (Input.GetMouseButtonUp(1)) while (!Input.GetMouseButton(1)) {
				oTime+=Time.deltaTime;
				transform.localPosition = Vector3.Lerp(trPos[4].localPosition, trPos[0].localPosition, oTime);
				oTime/= rateScope;
				cViewField = Mathf.Lerp (fovScoped, fovDefault, oTime);
			//	cViewField = MathematicF.Sinerp (fovScoped, fovDefault, oTime);
//				foreach (GameObject lCamera in pCameras) lCamera.camera.fieldOfView = cViewField;
				mCAMR.fieldOfView = cViewField;
				yield return null;
			}
		}

		internal IEnumerator Holster(bool stow) {
#if TODO
			float rTime = 0f;
			float oTime = 0f;
			if (stow) while (stow) {
				rTime+=Time.deltaTime;
				transform.localPosition = Vector3.Lerp(trPos[0].localPosition, trPos[3].localPosition, rTime);
				transform.localRotation = Quaternion.Lerp(trPos[0].localRotation, trPos[3].localRotation, rTime);
				if (transform.localPosition == trPos[3].localPosition
				&& transform.localRotation == trPos[3].localRotation)
					gameObject.SetActive(false);
			} else while (!stow) {
				oTime+=Time.deltaTime;
				transform.localPosition = Vector3.Lerp(trPos[3].localPosition, trPos[0].localPosition, oTime);
				transform.localRotation = Quaternion.Lerp(trPos[3].localRotation, trPos[0].localRotation, oTime);
			}
#endif
			yield return null;
		}

		internal static Vector3 Spray(Vector3 ProjectileDirection, float cSpread) {
		//	Vector2 cDirection = new Vector2(ProjectileDirection.x, ProjectileDirection.y);
		//	cDirection += (UnityEngine.Random.insideUnitCircle-new Vector2(.5f,.5f)*cSpread);
		//	ProjectileDirection.x += cDirection.x;
		//	ProjectileDirection.y += cDirection.y;
			return ProjectileDirection;
		}
		/*//
		public override void PickUp (bool PickedUp) {
			playerPack.reUptakeTimer = 0;
			localAU.PlayOneShot (Sound);
			if (isDuplicate && dWeapon) GameObject.Destroy(gameObject);
			else {
				foreach (Transform Child in transform) Child.gameObject.SetActive(!PickedUp);
				Collider[] inCChildren = GetComponentsInChildren<Collider>();
				foreach (Collider childC in inCChildren)
					if (rPlayer.collider.enabled && childC.enabled ) Physics.IgnoreCollision(rPlayer.collider, childC);
				localCD.enabled = !PickedUp;
				localRB.isKinematic = PickedUp;
				localAU.enabled = !PickedUp;
				isHeld = !PickedUp;
				GetComponent<Look>().enabled = !PickedUp;
				GetComponent<SphereCollider>().enabled = !PickedUp;
				am.enabled = !PickedUp;
				transform.rotation = Quaternion.identity;
				transform.parent = PickedUp ? (playerPack.transform) : (null);
				transform.position = PickedUp ? (rPlayer.transform.position) : (Vector3.zero);
				if (!PickedUp) localRB.AddForce (Quaternion.identity.eulerAngles*4, ForceMode.VelocityChange);
				playerPack.invWeapons = PickedUp ?
					(ArrayF.Push(playerPack.invWeapons, cWeapon)) :
					(ArrayF.Remove(playerPack.invWeapons, cWeapon));
			}
		}

	//	public override void Use ( bool useWeapon ) { Enable(useWeapon); }
		//*/
	}
}
