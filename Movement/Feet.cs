/* Ben Scott * bescott@andrew.cmu.edu * 2015-10-27 * Feet */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Movement {

	public enum StepTypes : int {
		Default = 0,   Dirt = 1,
		Gravel = 2,    Puddle = 3,
		Sand = 4,      Swamp = 5,
		Water = 6,     Wood = 7,
		Glass = 8,     Concrete = 9
	}

	[RequireComponent(typeof(AudioSource))]
	public class Feet : MonoBehaviour {
		bool wait = false, isLanding = false;
		public const float step = 0.4f;
		public float landVolume = 0.8f;
		Vector3 last;
		AudioSource _audio;
		public AudioClip[] stepSounds;
		public Dictionary<StepTypes,RandomList<AudioClip>> sounds;
		public util::key jump, dash, duck;

		public Feet() {
			jump = new util::key((n)=>jump.input=n);
			dash = new util::key((n)=>dash.input=n);
			duck = new util::key((n)=>duck.input=n);
		}

		public float Volume {
			get { return (dash.input)?0.3f:((duck.input)?0.05f:0.2f); }
		}

		public float Rate {
			get { return (dash.input)?0.1f:((duck.input)?0.4f:0.2f); }
		}

		void Awake() {
			_audio = GetComponent<AudioSource>();
			sounds = new Dictionary<StepTypes,RandomList<AudioClip>>();
			foreach (var elem in Enum.GetValues<StepTypes>()) {
				sounds[elem] = new RandomList<AudioClip>();
				foreach (var clip in stepSounds) {
					var name = clip.name.ToLower();
					if (name.Contains(elem.ToString().ToLower()))
						sounds[elem].Add(clip);
				}
			}
		}

		bool HasMoved(float d = step) {
			return ((Player.position-last).sqrMagnitude>d*d);
		}

		public IEnumerator Land(StepTypes stepType, float volume) {
			if (!isLanding) {
				isLanding = true;
				PlayStep(stepType,volume);
				yield return new WaitForSeconds(
					Rate+Random.Range(-0.01f,0.01f));
				last = Player.position;
				isLanding = false;
			}
		}

		public IEnumerator Step(StepTypes stepType) {
			if (Player.isGrounded && !wait) {
				wait = true;
				isLanding = true;
				yield return new WaitForSeconds(
					Rate+Random.Range(-0.01f,0.01f));
				isLanding = false;
				if (HasMoved() && !Player.isSliding)
					StartCoroutine(Land(stepType, Volume));
				wait = false;
			}
		}

		void PlayStep(StepTypes stepType, float volume) {
			var sound = sounds[stepType].Pick();
			if (!sound)
				sound = sounds[StepTypes.Default].Pick();
			_audio.PlayOneShot(sound,volume);
		}

		void OnCollisionEnter(Collision collision) {
			OnFootstep(collision.collider.material);
		}

		bool PlayerLanded() {
			if (isLanding) return false;
			if (Player.wasGrounded==Player.isGrounded) return false;
			if (Player.isJumping==Player.wasJumping) return false;
			if (!HasMoved(0.2f)) return false;
			return true;
		}

		public void OnFootstep(PhysicMaterial physicMaterial) {
			var name = physicMaterial.name.ToLower();
			foreach (var elem in Enum.GetValues<StepTypes>()) {
				if (name.Contains(elem.ToString().ToLower())) {
					if (PlayerLanded())
						StartCoroutine(Land(elem,landVolume));
					else StartCoroutine(Step(elem));
					return;
				}
			} StartCoroutine(Step(StepTypes.Default));
		}
	}
}