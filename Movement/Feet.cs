/* Ben Scott * bescott@andrew.cmu.edu * 2015-10-27 * Feet */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Movement {


    /** `Feet` : **`class`**
    |*
    |* A `Component` of `Person` and therefore `Player`, this
    |* plays a pretty large variety of foot sounds, which are
    |* triggered by the `PhysicMaterial`s that the `Person` or
    |* `Player` step on.
    |**/
    [RequireComponent(typeof(AudioSource))]
    public class Feet : MonoBehaviour {
        bool wait = false, isLanding = false;
        public const float step = 0.4f;
        public float landVolume = 0.6f;
        Vector3 last;
        AudioSource _audio;
        public AudioClip[] stepSounds;
        public Dictionary<StepTypes,RandomList<AudioClip>> sounds;
        public util::key jump, dash, duck;

        /** `Volume` : **`real`**
        |*
        |* hacky value to make landing louder than walking, etc
        |**/
        public float Volume {
            get { return (dash.input)?0.2f:((duck.input)?0.05f:0.1f); } }

        /** `Rate` : **`real`**
        |*
        |* hacky value to speed up play frequency when running
        |**/
        public float Rate {
            get { return (dash.input)?0.15f:((duck.input)?0.3f:0.2f); } }


        /** `Feet` : **`constructor`**
        |*
        |* Usually frowned upon in `Unity` development, but in
        |* this case, it harmlessly creates input listeners for
        |* the few inputs that this class needs to know about.
        |**/
        public Feet() {
            jump = new util::key((n)=>jump.input=n);
            dash = new util::key((n)=>dash.input=n);
            duck = new util::key((n)=>duck.input=n);
        }


        bool HasMoved(float d = step) {
            return ((Player.Position-last).sqrMagnitude>d*d); }

        public IEnumerator Land(StepTypes stepType, float volume) {
            if (!isLanding) {
                isLanding = true;
                PlayStep(stepType,volume);
                yield return new WaitForSeconds(
                    Rate+Random.Range(-0.005f,0.05f));
                last = Player.Position;
                isLanding = false;
            }
        }

        public IEnumerator Step(StepTypes stepType) {
            if (Player.IsGrounded && !wait) {
                wait = true;
                isLanding = true;
                yield return new WaitForSeconds(
                    Rate+Random.Range(-0.005f,0.005f));
                isLanding = false;
                if (HasMoved() && !Player.IsSliding)
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


        bool PlayerLanded() {
            if (isLanding) return false;
            if (Player.WasGrounded==Player.IsGrounded) return false;
            if (Player.IsJumping==Player.WasJumping) return false;
            if (!HasMoved(0.2f)) return false;
            return true;
        }

        public void OnFootstep(PhysicMaterial physicMaterial) {
            var name = physicMaterial.name.ToLower();
            foreach (var elem in util::Enum.GetValues<StepTypes>()) {
                if (name.Contains(elem.ToString().ToLower())) {
                    if (PlayerLanded())
                        StartCoroutine(Land(elem,landVolume));
                    else StartCoroutine(Step(elem));
                    return;
                }
            } StartCoroutine(Step(StepTypes.Default));
        }

        void Awake() {
            _audio = GetComponent<AudioSource>();
            sounds = new Dictionary<StepTypes,RandomList<AudioClip>>();
            foreach (var elem in util::Enum.GetValues<StepTypes>()) {
                sounds[elem] = new RandomList<AudioClip>();
                foreach (var clip in stepSounds) {
                    var name = clip.name.ToLower();
                    if (name.Contains(elem.ToString().ToLower()))
                        sounds[elem].Add(clip);
                }
            }
        }

        void OnCollisionEnter(Collision collision) {
            OnFootstep(collision.collider.material); }
    }
}