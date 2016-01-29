/* Ben Scott * bescott@andrew.cmu.edu * 2016-01-17 * Motor */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EventArgs=System.EventArgs;
using adv=PathwaysEngine.Adventure;
using lit=PathwaysEngine.Literature;
using u=PathwaysEngine.Utilities;


namespace PathwaysEngine.Movement {


    public class Motor : CharacterMotor {

        bool waitStep, isLanding;
        public const float step = 0.4f;
        public float landVolume = 0.6f;
        Vector3 last;
        AudioSource _audio;
        public AudioClip[] stepSounds;
        public Dictionary<StepTypes,RandList<AudioClip>> sounds;


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


        public bool OnMove(
                        adv::Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) => false;


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
            if (waitStep) yield break;
            waitStep = true;
            isLanding = true;
            yield return new WaitForSeconds(
                Rate+Random.Range(-0.005f,0.005f));
            isLanding = false;
            if (HasMoved()) //&& !Player.IsSliding)
                StartCoroutine(Land(stepType, Volume));
            waitStep = false;
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

        public override void Awake() { base.Awake();
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

