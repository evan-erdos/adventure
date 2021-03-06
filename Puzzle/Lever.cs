/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-23 * Lever */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EventArgs=System.EventArgs;
using adv=PathwaysEngine.Adventure;
using lit=PathwaysEngine.Literature;


namespace PathwaysEngine.Puzzle {


    /** `Lever` : **`Thing`**
     *
     * Represents an instance of a lever, which can either be
     * `Solve`d or not, depending upon if it's pulled.
     **/
    public class Lever : adv::Thing, IPiece<int>, adv::IPushable {
        bool wait = false;
        public float time = 2f, dist = 2f, delay = 4f;
        float tgt, speed, handle_tgt, handle_speed;

        public Vector2 armRange = new Vector2(-20,20);
        public Vector2 handleRange = new Vector2(0,-15);

        [SerializeField] public AudioClip soundLever;
        [SerializeField] public AudioClip soundHandle;

        AudioSource _audio;
        GameObject arm, handle;


        public bool IsSolved {
            get { return (Condition==Solution); } }


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


        public int Condition {
            get { return condition; }
            set { condition = value; }
        } int condition;


        public int Solution {
            get { return solution; }
            set { solution = value; }
        } int solution;


        public int Selections => 6;


        public float Theta {
            get { return theta; }
            set {
                theta = (theta<armRange.x)?(armRange.x):theta;
                theta = (theta>armRange.y)?(armRange.y):theta;
            }
        } float theta = -20f;


        public event OnSolve<int> SolveEvent {
            add { solveEvent += value; }
            remove { solveEvent -= value; }
        } event OnSolve<int> solveEvent;


        public override void Awake() { base.Awake();
            arm = transform.FindChild("arm").gameObject;
            handle = arm.transform.FindChild("handle").gameObject;
            _audio = GetComponent<AudioSource>();
            if (armRange.x>armRange.y)
                armRange = new Vector2(armRange.y,armRange.x);
            SolveEvent += this.OnSolve;
        }

        //public override void Start() { base.Start(); }

        void FixedUpdate() {
            if (IsLocked) return;
            var target = Quaternion.Euler(0f,0f,tgt);
            arm.transform.localRotation = Quaternion.Slerp(
                arm.transform.localRotation,
                target, Time.deltaTime*5f);

            var angle = Quaternion.Euler(0f,0f,handle_tgt);
            handle.transform.localRotation = Quaternion.Slerp(
                handle.transform.localRotation,
                angle, Time.deltaTime*8f);
        }


        public virtual bool Push() {
            _audio.PlayOneShot(soundLever,0.2f);
            lit::Terminal.Log(
                $"<cmd>You pull the</cmd> {Name} <cmd>back.</cmd>");
            return Solve(Condition+1);
        }


        public bool Pull(
                        adv::Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            if (!description.Fits(input)) return false;
            StartCoroutine(Pulling(!IsSolved));
            return true;
        }

        public virtual bool Pull() {
            _audio.PlayOneShot(soundLever,0.2f);
            lit::Terminal.Log(
                $"<cmd>You pull the</cmd> {Name} <cmd>forwards.</cmd>");
            return Solve(Condition-1);
        }

        IEnumerator Solving(bool t) {
            yield break;
            //while (arm.transform.rotation.y!=tgt) {
            //    yield return new WaitForEndOfFrame();
            //}
        }


        /** `Pulling` : **`coroutine`**
         *
         * Called with a boolean argument, specifies if the
         * lever should be pulled or pushed over a period of
         * `delay`. Also issues a `Terminal.Log()` message to
         * inform the player of the action taken. This is here
         * because once this is called, it is certain that the
         * event of pulling the lever is going to take place.
         **/
        IEnumerator Pulling(bool t) {
            if (wait) yield break;
            wait = true;
            if (t) Pull();
            else Push();
            yield return new WaitForSeconds(delay);
            wait = false;
        }


        public int OnSolve(
                        IPiece<int> sender,
                        EventArgs e,
                        bool solved) {
            if (!IsSolved) return 0;
            return Condition;
        }

        public bool Solve(int condition) {
            var wasSolved = IsSolved;
            Condition = condition;
            if (IsSolved!=wasSolved)
                solveEvent(this,EventArgs.Empty,IsSolved);
            return IsSolved;
        }

        public override IEnumerator OnMouseOver() {
            if (!Player.IsNear(this)) {
                Pathways.CursorGraphic = Cursors.None;
                yield break; }
            if (Pathways.CursorGraphic==Cursors.Grab)
                yield break;
            Pathways.CursorGraphic = Cursors.Hand;
            if (Input.GetButton("Fire1"))
                yield return StartCoroutine(Pulling(!IsSolved));
            while (Input.GetButton("Fire1")
            && Player.IsNear(this)) {
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

        public static float NonsenseFunction(float x) =>
            Mathf.Max(Mathf.Min((Mathf.Cos(2f*x)-Mathf.Cos(4f*x)+0.3f*Mathf.PI),0),1);

        public override void OnMouseExit() {
            base.OnMouseExit();
            handle_tgt = handleRange.x;
        }

        public override void Deserialize() =>
            Pathways.Deserialize<Lever,Lever_yml>(this);
    }
}

