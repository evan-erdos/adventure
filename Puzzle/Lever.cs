/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-23 * Lever */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using adv=PathwaysEngine.Adventure;
using lit=PathwaysEngine.Literature;

namespace PathwaysEngine.Puzzle {


    /** `Lever` : **`IPiece`**
    |*
    |* Represents an instance of a lever, which can either be
    |* `Solve`d or not, depending upon if it's pulled.
    |**/
    partial class Lever : adv::Thing, IPiece {
        bool wait = false;
        public float time = 2f, dist = 2f, delay = 4f;
        float tgt, speed, handle_tgt, handle_speed;

        public Vector2 armRange = new Vector2(-20,20);
        public Vector2 handleRange = new Vector2(0,-15);

        [SerializeField] public AudioClip soundLever;
        [SerializeField] public AudioClip soundHandle;

        AudioSource _audio;
        GameObject arm, handle;

        public event OnSolve SolveEvent {
            add { solveEvent += value; }
            remove { solveEvent -= value; }
        } event OnSolve solveEvent;

        public bool IsSolved {
            get { return isSolved; }
            set { isSolved = value;
                tgt = (value)?(armRange.y):(armRange.x);
            }
        } [SerializeField] bool isSolved;

        public bool IsInitSolved {
            get { return isInitSolved; }
            set { isInitSolved = value; }
        } [SerializeField] bool isInitSolved;

        public bool IsLocked {
            get { return isLocked; }
            set { if (isLocked==value) return;
                isLocked = value;
                if (IsLocked)
                    tgt = arm.transform.rotation.z;
            }
        } [SerializeField] bool isLocked = false;


        public float Theta {
            get { return theta; }
            set {
                theta = (theta<armRange.x)?(armRange.x):theta;
                theta = (theta>armRange.y)?(armRange.y):theta;
            }
        } float theta = -20f;

        public bool OnSolved(
                        object sender,
                        System.EventArgs e,
                        bool wasSolved) {
            return IsSolved;
        }

        public override void Awake() { base.Awake();
            arm = transform.FindChild("arm").gameObject;
            handle = arm.transform.FindChild("handle").gameObject;
            _audio = GetComponent<AudioSource>();
            if (armRange.x>armRange.y)
                armRange = new Vector2(armRange.y,armRange.x);
            SolveEvent += this.OnSolve;
        }

        public override void Start() { base.Start();
            IsSolved = IsInitSolved;
        }

        void FixedUpdate() {
            if (IsLocked) return;
            var target = Quaternion.Euler(0f,0f,tgt);
            arm.transform.localRotation = Quaternion.Slerp(
                arm.transform.localRotation, target, Time.deltaTime*5f);

            var angle = Quaternion.Euler(0f,0f,handle_tgt);
            handle.transform.localRotation = Quaternion.Slerp(
                handle.transform.localRotation, angle, Time.deltaTime*8f);
        }


        void Pull(lit::Command c) {
            if (description.Nouns.IsMatch(c.input))
                StartCoroutine(Pulling(!isSolved)); }

        IEnumerator Solving(bool t) {
            yield break;
            //while (arm.transform.rotation.y!=tgt) {
            //    yield return new WaitForEndOfFrame();
            //}
        }


        /** `Pulling()` : **`coroutine`**
        |*
        |* Called with a boolean argument, specifies if the
        |* lever should be pulled or pushed over a period of
        |* `delay`. Also issues a `Terminal.Log()` message to
        |* inform the player of the action taken. This is here
        |* because once this is called, it is certain that the
        |* event of pulling the lever is going to take place.
        |**/
        IEnumerator Pulling(bool t) {
            if (!wait) {
                wait = true;
                _audio.PlayOneShot(soundLever,0.2f);
                lit::Terminal.LogCommand(
                    "You pull the lever "+
                        ((t)?"back.":"forwards."));
                IsSolved = t;
                Solve();
                yield return new WaitForSeconds(delay);
                wait = false;
            }
        }

        public bool OnSolve(
                        object sender,
                        System.EventArgs e,
                        bool wasSolved) {
            return IsSolved;
        }

        public bool Solve() {
            return solveEvent(this,System.EventArgs.Empty,IsSolved);
        }

        public override IEnumerator OnMouseOver() {
            if (Vector3.Distance(transform.position,Player.Position)>dist) {
                Pathways.CursorGraphic = Cursors.None; yield break; }
            if (Pathways.CursorGraphic==Cursors.Grab) yield break;
            Pathways.CursorGraphic = Cursors.Hand;
            if (Input.GetButton("Fire1"))
                yield return StartCoroutine(Pulling(!isSolved));
            while (Input.GetButton("Fire1")
            && Vector3.Distance(transform.position,Player.Position)<dist) {
                var v0 = Input.mousePosition;
                Pathways.CursorGraphic = Cursors.Grab;
                handle_tgt = handleRange.y;
                var v1 = Pathways.mainCamera.ScreenToWorldPoint(v0);
                v0 = new Vector3(
                    Input.mousePosition.x,
                    Input.mousePosition.y,
                    Vector3.Distance(v1,transform.position));
                //Position = Camera.main.ScreenToWorldPoint(v0);
                yield return new WaitForEndOfFrame();
            } wait = false;
            if (!Input.GetButton("Fire1")) {
                Pathways.CursorGraphic = Cursors.Hand;
                handle_tgt = handleRange.x;
            }
        }

        public override void OnMouseExit() {
            base.OnMouseExit();
            handle_tgt = handleRange.x;
        }
    }
}

