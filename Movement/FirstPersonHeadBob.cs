/*_( [Ben Scott - bescott@andrew.cmu.edu] | [2014.0601 - THUR 1640] | [CS15.0122 - SF14.TR04] | {[Movement : Bob Control]} )_*/

using UnityEngine;												using System.Collections;
using N = System.NonSerializedAttribute;						using S = UnityEngine.SerializeField;
using mvmt = PathwaysEngine.Movement;

[RequireComponent(typeof(mvmt::CharacterMotor))]
public class FirstPersonHeadBob : MonoBehaviour {				// Head bobbing is controlled by a sine wave and the speed
	[N] private bool groundedFrame;								// whether the character was grounded last frame
	[N] private float nextStepTime, headBobCycle, headBobFade; 	// the current position through the headbob cycle, fade->now
	[N] private float springPos, springSpeed, springElastic, springDampen, springThresh, springPosThresh;
	[S] public float frequency, height, angle, lateral, speedmodifier, strideModifier, jumpLandMove, jumpLandTilt;
	[N] private mvmt::CharacterMotor cr;						// a reference to the First Person Character component
	[S] public Transform head;									// the object to which the head-bob movement should be applied
	[N] private AudioSource au;
	[N] private Vector3 originalLocalPos, prevPos, prevSpeed;
	[S] public AudioClip[] footstepSounds;						// an array of footstep sounds to randomly select from
	[S] public AudioClip jumpSound, landSound;					// the sound played when jumping and landing

	public FirstPersonHeadBob() {
		groundedFrame 		= true;
		frequency 			= 1.5f;								height 				= 0.3f;
		angle 				= 0.5f;								lateral 			= 0.05f;
		speedmodifier 		= 0.3f;								strideModifier 		= 0.3f;
		jumpLandMove 		= 3;								jumpLandTilt		= 60;
		nextStepTime 		= 0.5f;								headBobCycle 		= 0;
		headBobFade 		= 0;								springPos 			= 0;
		springSpeed 		= 0;								springElastic 		= 1.1f;
		springDampen 		= 0.8f;								springThresh 		= 0.05f;
		springPosThresh 	= 0.05f;							prevSpeed 			= Vector3.zero;
	}

	public void Start() {
		originalLocalPos = head.localPosition;
		cr = GetComponent<mvmt::CharacterMotor>();
		au = (GetComponent<AudioSource>()) ?? gameObject.AddComponent<AudioSource>();
		prevPos = GetComponent<Rigidbody>().position;
	}

	public void LateUpdate() {
		Vector3 velocity = (GetComponent<Rigidbody>().position - prevPos) / Time.deltaTime;
		Vector3 velocityChange = velocity - prevSpeed;			// we use the actual distance moved as the velocity since
		prevPos = GetComponent<Rigidbody>().position;			// last frame rather than reading the rigidbody's velocity
		prevSpeed = velocity;
		springSpeed -= velocityChange.y;						// input to spring from change in character Y velocity
		springSpeed -= springPos*springElastic;					// elastic spring force towards zero position
		springSpeed *= springDampen;							// damping towards zero velocity
		springPos += springSpeed * Time.deltaTime;				// output to head Y position
		springPos = Mathf.Clamp(springPos, -.3f, .3f);			// clamp spring distance
		if (Mathf.Abs(springSpeed)<springThresh && Mathf.Abs (springPos)<springPosThresh) { springSpeed = 0; springPos = 0; }
		float flatVelocity = new Vector3(velocity.x,0,velocity.z).magnitude;
		float strideLengthen = 1 + (flatVelocity * strideModifier);
		headBobCycle += (flatVelocity / strideLengthen) * (Time.deltaTime / frequency);
		float bobFactor = Mathf.Sin(headBobCycle*Mathf.PI*2);	// actual bobbing and swaying values calculated using Sine wave
		float bobSwayFactor = Mathf.Sin(headBobCycle*Mathf.PI*2 + Mathf.PI*.5f); // sway along sin curve by 1/4 radians
		bobFactor = 1-(bobFactor*.5f+1); 						// bob value is brought into 0-1 range and inverted
		bobFactor *= bobFactor;									// bob value is biased towards 0
		if (new Vector3(velocity.x,0,velocity.z).magnitude < 0.1f) headBobFade = Mathf.Lerp(headBobFade,0,Time.deltaTime);
		else headBobFade = Mathf.Lerp(headBobFade,1,Time.deltaTime);
		float speedHeightFactor = 1 + (flatVelocity * speedmodifier); // height of bob is exaggerated based on speed
		float xPos = -lateral * bobSwayFactor;					// finally, set the position and rotation values
		float yPos = springPos * jumpLandMove + bobFactor*height*headBobFade*speedHeightFactor;
		float xTilt = -springPos*jumpLandTilt;
		float zTilt = bobSwayFactor*angle*headBobFade;
		head.localPosition = originalLocalPos + new Vector3(xPos, yPos, 0);
		head.localRotation = Quaternion.Euler(xTilt,0,zTilt);
		// Play audio clips based on groundedness and head bob cycle
		if (cr.isGrounded) {
			if (!groundedFrame) {
				au.clip = landSound;
				au.Play();
				nextStepTime = headBobCycle + .5f;
			} else {
				if (headBobCycle > nextStepTime) {
					nextStepTime = headBobCycle + .5f;			// time for next footstep sound:
					if (footstepSounds.Length!=0) {
						int n = Random.Range(1,footstepSounds.Length);
						au.clip = footstepSounds[n];				// pick & play a random footstep sound from the array,
						au.Play();									// excluding sound at index 0
						footstepSounds[n] = footstepSounds[0];		// move picked sound to index 0 so it's not picked next time
						footstepSounds[0] = au.clip;
					}
				}
			} groundedFrame = true;
		} else {
			if (groundedFrame) { au.clip = jumpSound; au.Play();}
			groundedFrame = false;
		}
	}
}
