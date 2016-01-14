/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-06 * Door */

using UnityEngine; // “Help, Help, Snark in MTS!”
using System.Collections;
using EventArgs=System.EventArgs;
using lit=PathwaysEngine.Literature;
//using static PathwaysEngine.Literature.Terminal;
using inv=PathwaysEngine.Inventory;


namespace PathwaysEngine.Adventure.Setting {


    /** `Door` : **`Thing`**
     *
     * Represents any portal that can be opened or that usually
     * behaves like a door.
     **/
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Door : Thing, IOpenable, ILockable {
        bool wait, wait_open;
        public bool isSwitching, isStuck, isMoving,
            isReverse, isAutomatic, isInitOnly, autoClose;
        byte frameOpen;
        float time = 4f;
        internal float speedDelta, delay = 0.2f;
        Vector3 dirInit, dirOpen,
                dirFrame, dirTarget, dirDelta;
        protected new AudioSource audio;
        public AudioClip soundClick, soundOpen;
        public GameObject door;
        public Transform tgt;

        event lit::Parse OpenEvent, ShutEvent;


        /** `IsOpen` : **`bool`**
         *
         * The state of the `Door`, denotes if it's opening or
         * closing, rather than its actual state (this can be
         * true while the `Door` is opening, or it can be false
         * when the door is closing but not closed).
         **/
        public bool IsOpen {
            get { return isOpen; }
            set { if (value==isOpen) return;
                isOpen = value;
                if (!value) Shut();
                else Open();
            }
        } bool isOpen;


        /** `IsLocked` : **`bool`**
         *
         * Defines whether or not the door is locked. Setting
         * it changes the state of the door, e.g., unlocks it.
         **/
        public bool IsLocked {
            get { return isLocked; }
            set { isLocked = value; }
        } [SerializeField] bool isLocked;


        /** `IsInitOpen` : **`bool`**
         *
         * Defines whether or not the door begins opened.
         **/
        public bool IsInitOpen {
            get { return isInitOpened; }
            set { isInitOpened = value; }
        } [SerializeField] bool isInitOpened;


        /** `LockMessage` : **`string`**
         *
         * An optional message to print out when the `Player`
         * tries to open a locked door.
         **/
        public string LockMessage {get;set;}


        /** `LocalPosition` : **`<real,real,real>`**
         *
         * Represents the `door` object's local position, and
         * stops it if it takes the `door` out of bounds.
         **/
        public Vector3 LocalPosition {
            get { return door.transform.localPosition; }
            set { if (IsLocked || wait) return;
                door.transform.position = value;
                var x = Mathf.Min(Mathf.Max(
                    door.transform.localPosition.x,
                    tgt.localPosition.x),0f);
                door.transform.localPosition = new Vector3(
                    x,0f,0f);
                if (x>=(3f*tgt.localPosition.x)/4f)
                    if (IsOpen) Shut();
                else if (x<=tgt.localPosition.x/4f)
                    if (!IsOpen) Open();
            }
        }


        /** `LockKey` : **`Key`**
         *
         * The `Key` object which can unlock this `Door`. Can
         * be either a `Key` with a `Keys`value higher than the
         * `Door`'s security, or a unique object.
         **/
        public inv::Key LockKey {
            get { return lockKey; }
            set { lockKey = value; }
        } [SerializeField] inv::Key lockKey;


        /** `Open()` : **`Parse`**
         *
         * This is the callback for the `Open` command, which
         * calls all subscribers when `this` is `Open()`-ed.
         **/
        public bool Open(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            OpenEvent?.Invoke(sender,e,c,input);
            if (IsOpen) {
                lit::Terminal.Log(
                    "<cmd>It's already opened.</cmd>");
                return false;
            } else if (IsLocked)
                return sender.Unlock(this);
            else return this.Open();
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


        public void OnTriggerExit(Collider o) {
            if (isAutomatic && frameOpen>0 && Player.Is(o))
                frameOpen = 0x0;
        }


        /** `Opening()` : **`coroutine`**
         *
         * Called with a boolean argument, specifies if the
         * door should be opened or closed over the period of
         * `time`. Also issues a `Terminal.Log()` message to
         * inform the player of the action taken. This is here
         * because once this is called, it is certain that the
         * event of opening the door is going to take place.
         **/
        IEnumerator Opening(bool t) {
            if (!wait) {
                wait = true;
                collider.enabled = false;
                dirTarget = (t)?(dirOpen):(dirInit);
                Literature.Terminal.Log((t)
                    ? $"<cmd>You open the {Name}.</cmd>"
                    : $"<cmd>The {Name} clicks closed.</cmd>");
                audio.PlayOneShot(soundOpen,0.8f);
                yield return new WaitForSeconds(time);
                if (autoClose && t) {
                    yield return new WaitForSeconds(time);
                    Shut();
                } wait = false;
            }
        }

        public bool Open() {
            StartCoroutine(Opening(true));
            return true;
        }


        /** `Shut()` : **`bool`**
         *
         * Closes the door, the higher-level command.
         **/
        public bool Shut(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            ShutEvent?.Invoke(sender,e,c,input);
            if (dirTarget==dirInit) {
                lit::Terminal.Log(
                    "<cmd>It's already closed.</cmd>");
                return false;
            } return this.Shut();
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
            if (IsOpen || !Player.IsNear(this)) {
                Pathways.CursorGraphic = Cursors.None;
                yield break; }
            Pathways.CursorGraphic = Cursors.Hand;
            if (!IsLocked && !IsOpen
            && Input.GetButton("Fire1"))
                yield return StartCoroutine(Unlocking());
            while (!IsLocked && Player.IsNear(this)
            && Input.GetButton("Fire1")) {
                Pathways.CursorGraphic = Cursors.Grab;
                var v0 = Input.mousePosition;
                var v1 = Camera.main.ScreenToWorldPoint(v0);
                v0 = new Vector3(
                    Input.mousePosition.x,
                    Input.mousePosition.y,
                    Vector3.Distance(v1,transform.position));
                LocalPosition = Camera.main.ScreenToWorldPoint(v0);
                yield return new WaitForEndOfFrame();
            } wait_open = false;
        }


        public override void Deserialize() =>
            Pathways.Deserialize<Door,Door_yml>(this);
    }
}
