/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-06 * Door */

using UnityEngine;
using System.Collections;

namespace PathwaysEngine.Adventure { // “Help, Help, Snark in MTS!”

    /** `Door` : **`class`**
    |*
    |* Represents any portal that can be opened or that usually
    |* behaves like a door.
    |**/
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public partial class Door : Thing, IOpenable {
        bool wait = false, wait_open = false;
        bool isNearToPlayer = false;
        byte frameOpen;
        float time = 4f, dist = 4f, after_delay = 16f;
        internal float delay, speedDelta;
        public bool isSwitching, isStuck, isMoving,
            isReverse, isAutomatic, isInitOnly;
        AudioSource _audio;
        Collider _collider;
        Vector3 dirInit, dirOpen, dirFrame, dirTarget, dirDelta;
        public AudioClip soundClick;
        public AudioClip soundOpen;
        public GameObject door;
        public Transform tgt;

        static event CommandEvent OpenEvent, ShutEvent;


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
        } bool isOpen = false;


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
                    OpenOnly();
            }
        }


        /** `Door` : **`constructor`**
        |*
        |* Defaults for the local fields.
        |**/
        public Door() {
            isAutomatic = false;    isReverse   = false;
            isSwitching = false;    isInitOnly  = false;
            isStuck     = false;    isMoving    = false;
            frameOpen   = 0x0;      delay       = 0.2f;
        }


        /** `Open()` : **`function`**
        |*
        |* This is a `static` callback for the text-commands
        |* that registers any instances of `Door` to the
        |* command multicast delegate, which calls their local
        |* function, `Open()`, when the `Player` or anyone else
        |* enters a command to open doors.
        |**/
        public static void Open(Command c) {
            if (OpenEvent==null) return;
            OpenEvent(null,System.EventArgs.Empty,c);
        }


        /** `Shut()` : **`function`**
        |*
        |* Inverse of the `Open()` command, simply calls the
        |* other event, & does the same thing that `Open()`
        |* does, but instead closes the `Door`.
        |**/
        public static void Shut(Command c) {
            if (ShutEvent==null) return;
            ShutEvent(null,System.EventArgs.Empty,c);
        }


        /** `AddListener()` : **`function`**
        |*
        |* Every "instantiated" `Door` calls this on the static
        |* class to register themselves to the global `Door`,
        |* which acts as an event handler to the instances.
        |* Subscribers have their `Open` & `Shut` functions
        |* called when the `Player` issues such a command.
        |**/
        public static void AddListener(Door door) {
            OpenEvent += door.Open;
            ShutEvent += door.Shut;
        }

        public override void Awake() { base.Awake();
            Door.AddListener(this);
            _audio = GetComponent<AudioSource>();
            _audio.clip = soundOpen;
            _collider = GetComponent<Collider>();
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

        internal void Update() {
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

        internal void OnTriggerStay(Collider o) {
            //Debug.Log(System.Convert.ToString(frameOpen,2));
            if (Player.IsCollider(o)) isNearToPlayer = true;
            if (!isNearToPlayer) return;
            if (isAutomatic && frameOpen<128)
                frameOpen = unchecked ((byte)((frameOpen<<0x1)|0x1));
            if (IsOpen && isInitOnly)
                _collider.enabled = false;
        }

        public void OnTriggerExit(Collider o) {
            if (Player.IsCollider(o)) isNearToPlayer = false;
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
                isNearToPlayer = false;
                dirTarget = (t)?(dirOpen):(dirInit);
                Terminal.Log(
                    (t) ? "You open the door."
                        : "You close the door.",
                    Formats.Command, Formats.Paragraph);
                _audio.PlayOneShot(soundOpen,0.8f);
                yield return new WaitForSeconds(time);
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
                        object sender,
                        System.EventArgs e,
                        Command c) {
            if (!isNearToPlayer) return false;
            if (IsOpen) {
                Terminal.Log("It's already opened.",
                    Formats.Command, Formats.Newline);
                return true;
            } else if (IsLocked && !Player.Unlock(this)) {
                Terminal.Log("You try to open it, but it's locked.",
                    Formats.Command, Formats.Newline);
                return false;
            } else return this.Open();
        }


        IEnumerator OpeningOnly() {
            if (!wait) {
                yield return StartCoroutine(Opening(true));
                _collider.enabled = false;
                yield return new WaitForSeconds(after_delay);
                yield return StartCoroutine(Opening(false));
                _collider.enabled = true;
            }
        }


        /** `OpenOnly()` : **`bool`**
        |*
        |* This does exactly what open does, but locks the door
        |* afterwords.
        |**/
        public bool OpenOnly() {
            StartCoroutine(OpeningOnly());
            return true;
        }


        /** `Shut()` : **`bool`**
        |*
        |* Closes the door, the higher-level command.
        |**/
        public bool Shut(
                        object sender,
                        System.EventArgs e,
                        Command c) {
            if (!isNearToPlayer) return false;
            if (dirTarget==dirInit) {
                Terminal.Log("It's already closed.",
                    Formats.Command, Formats.Newline);
                return true;
            }
#if TEMP
            else if (!description.nouns.IsMatch(c.input))
                Terminal.Log("I only understood you insofar as wanting to close something.",
                    Formats.Command, Formats.Newline);
#endif
            else return this.Shut();
        }

        public bool Shut() { StartCoroutine(Opening(false)); return true; }


        IEnumerator Unlocking() {
            if (!wait_open) {
                wait_open = true;
                _audio.PlayOneShot(soundClick,0.8f);
                yield break;
            }
        }

        IEnumerator OnMouseOver() {
            var d = Vector3.Distance(transform.position,Player.Position);
            if (d>dist) {
                Pathways.CursorGraphic = Cursors.None; yield break; }
            Pathways.CursorGraphic = Cursors.Hand;
            if (Input.GetButton("Fire1") && !IsLocked)
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

        void OnMouseExit() {
            Pathways.CursorGraphic = Cursors.None;
            StopAllCoroutines();
        }
    }
}
