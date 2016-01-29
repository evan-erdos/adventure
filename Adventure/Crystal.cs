/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-09 * Crystal */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PathwaysEngine.Adventure {


    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Light))]
    public class Crystal : Thing {
        bool wait = false, ring_wait = false;
        public bool repeat, first = true;
        float velocity, velocity2;
        public float interval = 10f;
        public AudioClip[] crystalSounds;
        public AudioClip defaultSound;
        RandList<AudioClip> sounds;
        AudioSource _audio;
        Renderer _renderer;
        Light _light;
        Gradient gradient;
        public Color color;
        public Crystal[] crystals;

        public bool ring {get;set;}

        public float Delay {
            get { return delay+Random.Range(-0.01f,0.01f); }
        } public float delay = 5f;

        public override void Awake() {
            _audio = GetComponent<AudioSource>();
            _light = GetComponent<Light>();
            _light.intensity = 0f;
            _renderer = GetComponent<Renderer>();
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            sounds = new RandList<AudioClip>();
            sounds.AddRange(crystalSounds);
        }

        public IEnumerator Resonate(float interval = 10f) {
            if (!wait) {
                if (crystals.Length>0 && first)
                    foreach (var crystal in crystals)
                        crystal.first = false;
                wait = true;
                repeat = true;
                yield return new WaitForSeconds(interval+Delay);
                PlayRing(0.5f);
                if (crystals.Length==0) {
                    wait = false;
                    yield return null;
                }
                foreach (var crystal in crystals) {
                    yield return new WaitForSeconds(crystal.Delay);
                    crystal.StartCoroutine(Resonate());
                }
                wait = false;
                if (repeat) StartCoroutine(Resonate());
            }
        }

        public IEnumerator RingLight() {
            var x = 1f;
            while (x>0) {
                x-=0.01f;
                _light.intensity = Mathf.SmoothDamp(
                    _light.intensity, 1.5f,
                    ref velocity2, Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }

        public IEnumerator Ring() {
            if (!ring_wait) {
                ring_wait = true;
                _audio.PlayOneShot(defaultSound,0.6f);
                StartCoroutine(RingLight());
                yield return new WaitForSeconds(3f);
                ring_wait = false;
            }
        }

        public void Update() {
            _light.intensity = Mathf.SmoothDamp(
                _light.intensity, (repeat)?1f:0f,
                ref velocity, Time.deltaTime, 0.5f);
            if (repeat)
                DynamicGI.SetEmissive(_renderer,color);
            //material.SetColor("_EmissionColor", color);
            //DynamicGI.UpdateMaterials(renderer);
            //DynamicGI.UpdateEnvironment();
        }

        public void OnTriggerEnter(Collider o) {
            if (Player.Is(o) && !repeat) {
                StartCoroutine(Resonate());
                if (!ring_wait) StartCoroutine(Ring());
            }
        }

        void PlayRing(float volume) {
            var sound = sounds.Next();
            if (!sound) return;
            _audio.PlayOneShot(sound,volume);
        }

        //public void OnClick() {
        //  if (!wait) StartCoroutine(Resonate()); }
    }
}