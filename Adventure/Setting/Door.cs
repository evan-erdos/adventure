/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-06 * Door */

using UnityEngine; // “Help, Help, Snark in MTS!”
using System.Collections;
using EventArgs=System.EventArgs;
using lit=PathwaysEngine.Literature;
//using static PathwaysEngine.Literature.Terminal;
using inv=PathwaysEngine.Inventory;


namespace PathwaysEngine.Adventure.Setting {


    /** `Door` : **`class`**
    |*
    |* Represents any portal that can be opened or that usually
    |* behaves like a door.
    |**/
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    partial class Door : Thing, IOpenable, ILockable {
        bool wait, wait_open;
        byte frameOpen;
        float time = 4f;
        protected float dist = 4f;
        internal float speedDelta, delay = 0.2f;
        public bool isSwitching, isStuck, isMoving,
            isReverse, isAutomatic, isInitOnly, autoClose;
        Vector3 dirInit, dirOpen, dirFrame, dirTarget, dirDelta;
        protected new AudioSource audio;
        public AudioClip soundClick, soundOpen;
        public GameObject door;
        public Transform tgt;

        static event lit::CommandEvent OpenEvent, ShutEvent;


        /** `IsOpen` : **`bool`**
        |*
        |* The state of the `Door`, denotes if it's opening or
        |* closing, rather than its actual state (this can be
        |* true while the `Door` is opening, or it can be false
        |* when the door is closing but not closed).
        |**/
        public bool IsOpen {
            get { return isOpen; }
            set { if (value==isOpen) return;
                isOpen = value;
                if (!value) Shut();
                else Open();
            }
        } bool isOpen;


        /** `IsLocked` : **`bool`**
        |*
        |* Defines whether or not the door is locked. Setting
        |* it changes the state of the door, e.g., unlocks it.
        |**/
        public bool IsLocked {
            get { return isLocked; }
            set { isLocked = value; }
        } [SerializeField] bool isLocked;


        /** `IsInitOpen` : **`bool`**
        |*
        |* Defines whether or not the door begins opened.
        |**/
        public bool IsInitOpen {
            get { return isInitOpened; }
            set { isInitOpened = value; }
        } [SerializeField] bool isInitOpened;

        public bool Near {get;set;}


        /** `LockMessage` : **`string`**
        |*
        |* An optional message to print out when the `Player`
        |* tries to open a locked door.
        |**/
        public string LockMessage {get;set;}


        /** `Position` : **`<real,real,real>`**
        |*
        |* Represents the `door` object's local position, and
        |* stops it if it takes the `door` out of bounds.
        |**/
        public Vector3 Position {
            get { return door.transform.localPosition; }
            set {
                if (IsLocked || wait) return;
                door.transform.position = value;
                var x = Mathf.Min(Mathf.Max(
                    door.transform.localPosition.x,
                    tgt.localPosition.x),0f);
                door.transform.localPosition = new Vector3(x,0f,0f);
                if (x>=(3f*tgt.localPosition.x)/4f && IsOpen) Shut();
                else if (x<=tgt.localPosition.x/4f && !IsOpen)
                    Open(); //only?
            }
        }


        /** `LockKey` : **`Key`**
        |*
        |* The `Key` object which can unlock this `Door`. Can
        |* be either a `Key` with a `Keys`value higher than the
        |* `Door`'s security, or a unique object.
        |**/
        public inv::Key LockKey {
            get { return lockKey; }
            set { lockKey = value; }
        } [SerializeField] inv::Key lockKey;


        /** `Open()` : **`function`**
        |*
        |* This is a `static` callback for the text-commands
        |* that registers any instances of `Door` to the
        |* command multicast delegate, which calls their local
        |* function, `Open()`, when the `Player` or anyone else
        |* enters a command to open doors.
        |**/
        public static void Open(lit::Command c) {
            if (OpenEvent==null) return;
            OpenEvent(null,null,EventArgs.Empty,c);
        }


        /** `Shut()` : **`function`**
        |*
        |* Inverse of the `Open()` command, simply calls the
        |* other event, & does the same thing that `Open()`
        |* does, but instead closes the `Door`.
        |**/
        public static void Shut(lit::Command c) {
            if (ShutEvent==null) return;
            ShutEvent(null,null,EventArgs.Empty,c);
        }

        public override void Awake() { base.Awake();
            audio = GetComponent<AudioSource>();
            audio.clip = soundOpen;
            if (!door) throw new System.Exception("No door!");
        }

        public override void Start() { base.Start();
            dirInit = door.transform.position;
            dirOpen = (tgt)
                ? (tgt.position)
                : (door.transform.position+(Vector3.left*4));
            dirTarget = dirInit;
            if (IsInitOpen) {
                dirTarget = dirOpen;
                door.transform.position = dirTarget;
                IsOpen = true;
            }
        }

        void Update() {
            if (isAutomatic)
                IsOpen = (frameOpen>0xFE);
            delay = Mathf.SmoothDamp(
                delay, (isStuck)?(0f):(0.6f),
                ref speedDelta, 0.1f);
            if (!isStuck && !IsLocked && door.transform.position != dirTarget)
                door.transform.position = Vector3.SmoothDamp(
                    door.transform.position,
                    dirTarget,ref dirDelta,0.8f,
                    delay,Time.deltaTime);
        }
#if SLOW
        public void OnTriggerStay(Collider o) {
            //Debug.Log(System.Convert.ToString(frameOpen,2));
            if (!Near) return;
            if (isAutomatic && frameOpen<128)
                frameOpen = unchecked ((byte)((frameOpen<<0x1)|0x1));
            if (IsOpen && isInitOnly)
                collider.enabled = false;
        }
#endif

        public void OnTriggerExit(Collider o) {
            if (isAutomatic && frameOpen>0
            && o.gameObject.layer==LayerMask.NameToLayer("Player"))
                frameOpen = 0x0;
        }


        /** `Opening()` : **`coroutine`**
        |*
        |* Called with a boolean argument, specifies if the
        |* door should be opened or closed over the period of
        |* `time`. Also issues a `Terminal.Log()` message to
        |* inform the player of the action taken. This is here
        |* because once this is called, it is certain that the
        |* event of opening the door is going to take place.
        |**/
        IEnumerator Opening(bool t) {
            if (!wait) {
                wait = true;
                Near = false;
                collider.enabled = false;
                dirTarget = (t)?(dirOpen):(dirInit);
                PathwaysEngine.Literature.Terminal.LogCommand(
                    (t) ? $"You open the {Name}."
                        : $"The {Name} clicks closed.");
                audio.PlayOneShot(soundOpen,0.8f);
                yield return new WaitForSeconds(time);
                if (autoClose && t) {
                    yield return new WaitForSeconds(time);
                    Shut();
                }
                wait = false;
            }
        }

        public bool Open() {
            StartCoroutine(Opening(true)); return true; }


        /** `Open()` : **`bool`**
        |*
        |* This is the local event handler for the `Parser`'s
        |* command, `Open`.
        |**/
        public bool Open(
                        object source,
                        Thing target,
                        EventArgs e,
                        lit::Command c) {
            if (IsOpen) {
                PathwaysEngine.Literature.Terminal.LogCommand(
                    "It's already opened.");
                return true;
            } else if (IsLocked)
                return Player.Unlock(this);
            else return this.Open();
        }


        /** `OpenOnly()` : **`bool`**
        |*
        |* This does exactly what open does, but locks the door
        |* afterwords.
        |**/
        //public bool OpenOnly() {
        //    StartCoroutine(OpeningOnly());
        //    return true;
        //}


        /** `Shut()` : **`bool`**
        |*
        |* Closes the door, the higher-level command.
        |**/
        public bool Shut(
                        object source,
                        Thing target,
                        EventArgs e,
                        lit::Command c) {
            if (dirTarget!=dirInit)
                return this.Shut();
            PathwaysEngine.Literature.Terminal.LogCommand(
                "It's already closed.");
            return true;
        }

        public bool Shut() {
            StartCoroutine(Opening(false));
            return true;
        }


        public bool Lock(inv::Key key) {
            if (key==LockKey) return true;
            if (!key) return false;
            if (key.Kind!=LockKey.Kind) return false;
            return (key.Value==LockKey.Value);
        }


        public bool Unlock(inv::Key key) {
            if (key==LockKey) return true;
            if (!key) return false;
            if (key.Kind!=LockKey.Kind) return false;
            return (key.Value==LockKey.Value);
        }


        IEnumerator Unlocking() {
            if (!wait_open && !IsLocked) {
                wait_open = true;
                audio.PlayOneShot(soundClick,0.8f);
                yield break;
            }
        }

        public override IEnumerator OnMouseOver() {
            var d = Vector3.Distance(transform.position,Player.Position);
            if (d>dist || IsOpen) {
                Pathways.CursorGraphic = Cursors.None; yield break; }
            Pathways.CursorGraphic = Cursors.Hand;
            if (Input.GetButton("Fire1") && !IsLocked && !IsOpen)
                yield return StartCoroutine(Unlocking());
            while (Input.GetButton("Fire1") && d<dist && !IsLocked) {
                d = Vector3.Distance(transform.position,Player.Position);
                var v0 = Input.mousePosition;
                Pathways.CursorGraphic = Cursors.Grab;
                var v1 = Camera.main.ScreenToWorldPoint(v0);
                v0 = new Vector3(
                    Input.mousePosition.x,
                    Input.mousePosition.y,
                    Vector3.Distance(v1,transform.position));
                Position = Camera.main.ScreenToWorldPoint(v0);
                yield return new WaitForEndOfFrame();
            } wait_open = false;
        }
    }
}
