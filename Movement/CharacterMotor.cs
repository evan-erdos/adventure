/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Character Motor */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Movement {
	[RequireComponent(typeof(CharacterController))]
	public class CharacterMotor : MonoBehaviour, IMotor {
		public enum transfer { None, initial, PermaTransfer, PermaLocked }
		bool newPlatform, wait = false, recentlyLanded = false;
		uint massPlayer; //deltaPitch, deltaStep,
		float maxSpeed, dampingGround, dampingAirborne,
			lastStartTime, lastEndTime, tgtCrouch, tgtCrouchLand;
		public float modSprint, modCrouch, speedAnterior, speedLateral,
			speedPosterior, speedVertical, deltaHeight, weightPerp,
			weightSteep, extraHeight, slidingSpeed, lateralControl,
			speedControl, deltaCrouch, landingDuration;
		public transfer dirTransfer;
		public AnimationCurve responseSlope;
		CollisionFlags hitFlags;
		Vector3 inputMove, jumpDir, platformVelocity,
			groundNormal, lastGroundNormal, hitPoint,
			lastHitPoint, activeLocalPoint, activeGlobalPoint;
		Transform hitPlatform, activePlatform, mCamr, playerGraphics;
		Quaternion activeLocalRotation, activeGlobalRotation;
		Matrix4x4 lastMatrix;
		CharacterController cr;
		ControllerColliderHit lastColl;
		public util::key jump, dash, duck;
		public util::axis axisX, axisY;

		public bool isDead {
			get { return _isDead; }
			set { _isDead = value; }
		} bool _isDead = false;

		public bool isJumping {
			get { return _isJumping; }
			set {
				if (_isJumping!=value) {
					if (value && !wasJumping) OnJump();
					else OnLand();
				}
				wasJumping = _isJumping;
				_isJumping = value;
			}
		} bool _isJumping = false;

		public bool wasJumping { get; set; }

		public bool isGrounded {
			get { return _isGrounded; }
			set {
				wasGrounded = _isGrounded;
				_isGrounded = value;
			}
		} bool _isGrounded = false;

		public bool wasGrounded { get; set; }

		public bool grounded {
			get { return (groundNormal.y>0.01); }
		}

		public bool isSliding {
			get { return (isGrounded && TooSteep()); }
		}

		public bool isSprinting {
			get { return dash.input; }
		}

		public Vector3 position {
			get { return transform.position; }
			set { transform.position = value; }
		}

		public Vector3 localPosition {
			get { return transform.localPosition; }
			set { transform.localPosition = value; }
		}

		public Vector3 velocity {
			get { return _velocity; }
			set {
				_velocity = value;
				lastVelocity = Vector3.zero;
				isGrounded = false;
			}
		} Vector3 _velocity = Vector3.zero;

		public Vector3 lastVelocity { get; set; }

		internal CharacterMotor() {
			maxSpeed		= 57.2f;		massPlayer			= 80;
			dampingGround	= 30.0f;		dampingAirborne		= 20.0f;
			modSprint		= 1.6f;			modCrouch			= 0.8f;
			speedAnterior	= 16.0f;		speedPosterior		= 10.0f;
			speedLateral	= 12.0f;		speedVertical		= 1.0f;
			extraHeight		= 4.1f;			slidingSpeed		= 15.0f;
			weightPerp		= 0.0f;			weightSteep			= 0.5f;
			lastStartTime	= 0.0f;			lastEndTime			= -100f;
			lateralControl	= 1.0f;			speedControl		= 0.4f;
			deltaCrouch 	= 1.0f;			deltaHeight			= 2.0f;
			tgtCrouchLand	= 1.5f; 		landingDuration 	= 0.15f;
			lastVelocity	= Vector3.zero; hitPoint			= Vector3.zero;
			groundNormal	= Vector3.zero;	lastGroundNormal	= Vector3.zero;
			jumpDir			= Vector3.up;	inputMove			= Vector3.zero;
			dirTransfer 	= transfer.PermaTransfer;
			lastHitPoint 	= new Vector3(Mathf.Infinity,0,0);
			jump 			= new util::key((n)=>jump.input=n);
			dash			= new util::key((n)=>dash.input=n);
			duck 			= new util::key((n)=>duck.input=n);
			axisX 			= new util::axis((n)=>axisX.input=n);
			axisY 			= new util::axis((n)=>axisY.input=n);
		}

		/* internal ~CharacterMotor() { GameObject.Destroy(mapFollower); } */

		public void Awake() {
			cr = GetComponent<CharacterController>();
			mCamr = GameObject.FindGameObjectsWithTag("MainCamera")[0].transform;
			responseSlope = new AnimationCurve(
				new Keyframe(-90,1),
				new Keyframe(90,0));
		}

		public void Kill() {
			isDead = true;
			GetComponent<CapsuleCollider>().enabled = true;
			GetComponent<Rigidbody>().isKinematic = false;
			GetComponent<Rigidbody>().useGravity = true;
			GetComponent<Rigidbody>().freezeRotation = false;
			cr.enabled = false;
			this.enabled = false;

		}

		public void OnJump() { }

		public IEnumerator Landed() {
			if (!wait) {
				wait = true;
				recentlyLanded = true;
				yield return new WaitForSeconds(landingDuration);
				recentlyLanded = false;
				wait = false;
			}
		}

		public void OnLand() {
			StartCoroutine(Landed());
		}

		public void Update() {
			if (Mathf.Abs(Time.timeScale)<0.01f) return;
			if (modSprint==0 || modCrouch==0 || speedAnterior==0
			|| speedLateral==0 || speedPosterior==0) return;
			var dirVector = new Vector3(axisX.input, 0, axisY.input);
			if (dirVector != Vector3.zero) {
				var dirLength = dirVector.magnitude;
				dirVector /= dirLength;
				dirLength = Mathf.Min(1f,dirLength);
				dirLength = dirLength * dirLength;
				dirVector = dirVector * dirLength;
			} inputMove = transform.rotation * dirVector;
			if (!isDead) UpdateFunction();
		}

		public void FixedUpdate() {
			if ((Mathf.Abs(Time.timeScale)>0.1f) && activePlatform != null) {
				if (!newPlatform)
					platformVelocity = (activePlatform.localToWorldMatrix.MultiplyPoint3x4(activeLocalPoint) - lastMatrix.MultiplyPoint3x4(activeLocalPoint))/((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
				lastMatrix = activePlatform.localToWorldMatrix;
				newPlatform = false;
			} else platformVelocity = Vector3.zero;
		}

		public void OnCollisionEvent(Collision collision) { }

		void UpdateFunction() {
			var tempVelocity = velocity;
			var moveDistance = Vector3.zero;
			tempVelocity = applyDeltaVelocity(tempVelocity);
			if (MoveWithPlatform()) {
				var newGlobalPoint = activePlatform.TransformPoint(activeLocalPoint);
				moveDistance = (newGlobalPoint - activeGlobalPoint);
				if (moveDistance != Vector3.zero) cr.Move(moveDistance);
				var newGlobalRotation = activePlatform.rotation * activeLocalRotation;
				var rotationDiff = newGlobalRotation * Quaternion.Inverse(activeGlobalRotation);
				var yRotation = rotationDiff.eulerAngles.y;
				if (yRotation!=0) transform.Rotate(0,yRotation,0);
			}
			var lastPosition = transform.position;
			var currentMovementOffset = tempVelocity * ((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
			var pushDownOffset = Mathf.Max(cr.stepOffset, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);
			if (isGrounded) currentMovementOffset -= pushDownOffset*Vector3.up;
			hitPlatform = null;
			groundNormal = Vector3.zero;
			// This one moves the user and returns the direction of the hit
			hitFlags = cr.Move(currentMovementOffset);
			lastHitPoint = hitPoint;
			lastGroundNormal = groundNormal;
			if (activePlatform != hitPlatform && hitPlatform != null) {
				activePlatform = hitPlatform;
				lastMatrix = hitPlatform.localToWorldMatrix;
				newPlatform = true;
			}
			var oldHVelocity = new Vector3(tempVelocity.x,0,tempVelocity.z);
			velocity = (transform.position - lastPosition) / ((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
			var newHVelocity = new Vector3(velocity.x, 0, velocity.z);
			if (oldHVelocity != Vector3.zero) {
				var projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
				velocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + velocity.y * Vector3.up;
			} else velocity = new Vector3(0, velocity.y, 0);
			if (velocity.y < tempVelocity.y - 0.001) {
				if (velocity.y < 0) velocity = new Vector3(
					velocity.x,tempVelocity.y,velocity.z);
				else wasJumping = false;
			} if (isGrounded && !grounded) {
				isGrounded = false;
				if ((dirTransfer==transfer.initial
				|| dirTransfer==transfer.PermaTransfer)) {
					lastVelocity = platformVelocity;
					velocity += platformVelocity;
				} transform.position += pushDownOffset * Vector3.up;
			} else if (!isGrounded && grounded) {
				isGrounded = true;
				isJumping = false;
				SubtractNewPlatformVelocity();
			} if (MoveWithPlatform()) {
				activeGlobalPoint = transform.position
					+Vector3.up*(cr.center.y-cr.height*0.5f+cr.radius);
				activeLocalPoint = activePlatform.InverseTransformPoint(activeGlobalPoint);
				activeGlobalRotation = transform.rotation;
				activeLocalRotation = Quaternion.Inverse(
					activePlatform.rotation) * activeGlobalRotation;
			}
			slidingSpeed = (duck.input)?(4f):(15f);
			tgtCrouch = (duck.input)?1.62f:2f;
			if (recentlyLanded)
				tgtCrouch = tgtCrouchLand;
			if (Mathf.Abs(deltaHeight-tgtCrouch)<0.01f)
				deltaHeight = tgtCrouch;
			deltaHeight = Mathf.SmoothDamp(
				deltaHeight, tgtCrouch,
				ref deltaCrouch, 0.06f,
				64, Time.smoothDeltaTime);
			cr.height = deltaHeight;
			if (mCamr) mCamr.localPosition = Vector3.up*(deltaHeight-0.2f);
			cr.center = Vector3.up*(deltaHeight/2f);
		}

		Vector3 applyDeltaVelocity(Vector3 tempVelocity) {
			// the horizontal to calculate direction from the isJumping event
			Vector3 desiredVelocity;
			if (isGrounded && TooSteep()) {
			// and to support walljumping I need to change horizontal here
				desiredVelocity = new Vector3(groundNormal.x,0,groundNormal.z).normalized;
				var projectedMoveDir = Vector3.Project(
					inputMove, desiredVelocity);
				desiredVelocity = desiredVelocity+projectedMoveDir*speedControl
					+ (inputMove - projectedMoveDir) * lateralControl;
				desiredVelocity *= slidingSpeed;
			} else desiredVelocity = GetDesiredHorizontalVelocity();
			if (dirTransfer==transfer.PermaTransfer) {
				desiredVelocity += lastVelocity;
				desiredVelocity.y = 0;
			} if (isGrounded) desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, groundNormal);
			else tempVelocity.y = 0;
			// Enforce zero on Y because the axes are calculated separately
			var maxSpeedChange = GetMaxAcceleration(isGrounded) * Time.deltaTime;
			var velocityChangeVector = (desiredVelocity - tempVelocity);
			if (velocityChangeVector.sqrMagnitude > maxSpeedChange * maxSpeedChange)
				velocityChangeVector = velocityChangeVector.normalized * maxSpeedChange;
			if (isGrounded) tempVelocity += velocityChangeVector;
			if (isGrounded) tempVelocity.y = Mathf.Min(velocity.y, 0);
			if (!jump.input) { // This second section aplies only the vertical axis motion but
				// the reason I've conjoined these two is because I now have an
				wasJumping = false;
				// interaction between the user's vertical & horizontal vectors
				lastEndTime = -100;
			} if (jump.input && lastEndTime<0)
				lastEndTime = Time.time;
			if (isGrounded) tempVelocity.y = Mathf.Min(0, tempVelocity.y) - -Physics.gravity.y * Time.deltaTime;
			else {
				tempVelocity.y = velocity.y - -Physics.gravity.y*2*Time.deltaTime;
				if (isJumping && wasJumping) {
				   if (Time.time<lastStartTime + extraHeight / CalculateJumpVerticalSpeed(speedVertical))
						tempVelocity += jumpDir * -Physics.gravity.y*2 * Time.deltaTime;
				} tempVelocity.y = Mathf.Max(tempVelocity.y, -maxSpeed);
			} if (isGrounded) {
				if (Time.time-lastEndTime<0.2) {
					isGrounded = false;
					isJumping = true;
					lastStartTime = Time.time;
					lastEndTime = -100;
					wasJumping = true;
					if (TooSteep())
						jumpDir = Vector3.Slerp(
							Vector3.up, groundNormal, weightSteep);
					else jumpDir = Vector3.Slerp(
						Vector3.up, groundNormal, weightPerp);
					tempVelocity.y = 0;
					tempVelocity += jumpDir * CalculateJumpVerticalSpeed(speedVertical);
					if (dirTransfer==transfer.initial
					|| dirTransfer==transfer.PermaTransfer) {
						lastVelocity = platformVelocity;
						tempVelocity += platformVelocity;
					}
				} else wasJumping = false;
			} else if (cr.collisionFlags==CollisionFlags.Sides)
				Vector3.Slerp(Vector3.up,lastColl.normal,lateralControl);
			return tempVelocity;
		}

		void OnCollisionEnter(Collision collision) {
			Player.OnCollisionEnter(collision);
		}

		void OnControllerColliderHit(ControllerColliderHit hit) {
			Player.OnCollisionEnter(hit.collider);
			var other = hit.collider.attachedRigidbody;
			lastColl = hit;
			if (other && hit.moveDirection.y>-0.05)
				other.velocity = new Vector3(
					hit.moveDirection.x,0,hit.moveDirection.z)
					*(massPlayer+other.mass)/(2*-Physics.gravity.y);
			if (hit.normal.y>0
			&& hit.normal.y>groundNormal.y
			&& hit.moveDirection.y<0) {
				if ((hit.point - lastHitPoint).sqrMagnitude>0.001
				|| lastGroundNormal==Vector3.zero)
					groundNormal = hit.normal;
				else groundNormal = lastGroundNormal;
				hitPlatform = hit.collider.transform;
				lastVelocity = Vector3.zero;
				hitPoint = hit.point;
			}
		}

		IEnumerator SubtractNewPlatformVelocity() {
			if (dirTransfer==transfer.initial
			|| dirTransfer==transfer.PermaTransfer) {
				if (newPlatform) {
					Transform platform = activePlatform;
					// Both yields are present as a kind of corruption of von Braun style redundancy as it might be near or have missed the call
					yield return new WaitForFixedUpdate();
					yield return new WaitForFixedUpdate();
					if (isGrounded && platform==activePlatform) yield break;
				} velocity -= platformVelocity;
			}
		}

		Vector3 GetDesiredHorizontalVelocity() {
			var dirDesired = transform.InverseTransformDirection(inputMove);
		   	var maxSpeed = 0.0f;
			if (dirDesired != Vector3.zero) {
				var zAxisEllipseMultiplier = (dirDesired.z>0 ? speedAnterior : speedPosterior) / speedLateral;
				if (dash.input && isGrounded)
					zAxisEllipseMultiplier *= modSprint;
				else if (duck.input && isGrounded)
					zAxisEllipseMultiplier *= modCrouch;
				var temp = new Vector3(dirDesired.x, 0, dirDesired.z / zAxisEllipseMultiplier).normalized;
				maxSpeed = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * speedLateral;
			} if (isGrounded) {
				var movementSlopeAngle = Mathf.Asin(velocity.normalized.y) * Mathf.Rad2Deg;
				maxSpeed *= responseSlope.Evaluate(movementSlopeAngle);
			} return transform.TransformDirection(dirDesired * maxSpeed);
		}

		bool MoveWithPlatform() {
			return (isGrounded || dirTransfer==transfer.PermaLocked)&&(activePlatform);
		}

		Vector3 AdjustGroundVelocityToNormal(Vector3 v,Vector3 normal) {
			return Vector3.Cross(Vector3.Cross(Vector3.up, v), normal).normalized * v.magnitude;
		}

		float GetMaxAcceleration(bool isGrounded) {
			return isGrounded ? dampingGround : dampingAirborne;
		}

		float CalculateJumpVerticalSpeed(float tgtHeight) {
			return Mathf.Sqrt(2*tgtHeight*-Physics.gravity.y*2);
		}

		bool isTouchingCeiling() {
			return (hitFlags&CollisionFlags.CollidedAbove)!=0;
		}

		bool TooSteep() {
			return (groundNormal.y <= Mathf.Cos(cr.slopeLimit*Mathf.Deg2Rad));
		}
	}
}

