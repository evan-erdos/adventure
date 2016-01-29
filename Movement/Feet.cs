/* Ben Scott * bescott@andrew.cmu.edu * 2015-10-27 * Feet */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EventArgs=System.EventArgs;
using adv=PathwaysEngine.Adventure;
using u=PathwaysEngine.Utilities;


namespace PathwaysEngine.Movement {


    /** `Feet` : **`MonoBehaviour`**
     *
     * A `Component` of `Person` and therefore `Player`, this
     * plays a pretty large variety of foot sounds, which are
     * triggered by the `PhysicMaterial`s that the `Person` or
     * `Player` step on.
     **/
    [RequireComponent(typeof(AudioSource))]
    public class Feet : MonoBehaviour {

        bool wait = false, isLanding = false;
        public const float step = 0.4f;
        public float landVolume = 0.6f;
        Vector3 last;
        AudioSource _audio;
        public AudioClip[] stepSounds;
        public Dictionary<StepTypes,RandList<AudioClip>> sounds;
        public u::key jump, dash, duck;


        /** `Volume` : **`real`**
         *
         * hacky value to make landing louder than walking, etc
         **/
        public float Volume =>
            (dash.input)?0.2f:((duck.input)?0.05f:0.1f);


        /** `Rate` : **`real`**
         *
         * hacky value to speed up play frequency when running
         **/
        public float Rate =>
            (dash.input)?0.15f:((duck.input)?0.3f:0.2f);


        /** `Feet` : **`constructor`**
         *
         * Usually frowned upon in `Unity` development, but in
         * this case, it harmlessly creates input listeners for
         * the few inputs that this class needs to know about.
         **/
        public Feet() {
            jump = new u::key((n)=>jump.input=n);
            dash = new u::key((n)=>dash.input=n);
            duck = new u::key((n)=>duck.input=n);
        }


        public void OnMove(adv::Person sender, EventArgs e) {  }


        bool HasMoved(float d = step) =>
            Player.IsNear(last,d*d);


        public IEnumerator Land(
                        StepTypes stepType,
                        float volume) {
            if (!isLanding) {
                isLanding = true;
                PlayStep(stepType,volume);
                yield return new WaitForSeconds(
                    Rate+Random.Range(-0.005f,0.05f));
                last = transform.position;
                isLanding = false;
            }
        }


        public IEnumerator Step(StepTypes stepType) {
            //if (Player.IsGrounded && !wait) {
            if (!wait) {
                wait = true;
                isLanding = true;
                yield return new WaitForSeconds(
                    Rate+Random.Range(-0.005f,0.005f));
                isLanding = false;
                if (HasMoved()) //&& !Player.IsSliding)
                    StartCoroutine(Land(stepType, Volume));
                wait = false;
            }
        }


        void PlayStep(StepTypes stepType, float volume) =>
            _audio.PlayOneShot(
                sounds[stepType].Next() ??
                sounds[StepTypes.Default].Next(), volume);


        bool PlayerLanded() {
            if (isLanding) return false;
            if (!HasMoved(0.2f)) return false;
            return true;
        }

        public void OnFootstep(PhysicMaterial physicMaterial) {
            var name = physicMaterial.name.ToLower();
            foreach (var elem in u::Enum.GetValues<StepTypes>()) {
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
            sounds = new Dictionary<StepTypes,RandList<AudioClip>>();
            foreach (var elem in u::Enum.GetValues<StepTypes>()) {
                sounds[elem] = new RandList<AudioClip>();
                foreach (var clip in stepSounds) {
                    var name = clip.name.ToLower();
                    if (name.Contains(elem.ToString().ToLower()))
                        sounds[elem].Add(clip);
                }
            }
        }

        void OnCollisionEnter(Collision collision) =>
            OnFootstep(collision.collider.material);
    }
}