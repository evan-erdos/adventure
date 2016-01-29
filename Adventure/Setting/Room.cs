/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-24 * Room */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using inv=PathwaysEngine.Inventory;
using lit=PathwaysEngine.Literature;
//using static PathwaysEngine.Literature.Terminal;
using Buffer=System.Text.StringBuilder;


namespace PathwaysEngine.Adventure.Setting {


    /** `Room` : **`Thing`**
     *
     * This class represents a room or an outdoor room, which
     * defines its connections to other rooms, any of the other
     * `Thing`s it might contain, and defines, pretty broadly,
     * where the `Player` is, and what they're doing.
     **/
    public class Room : Thing {
        bool waitViewRoom;
        byte frames;


        /** `Depth` : **`int`**
         *
         * Determines which room gets precedence when the room
         * colliders intersect. Higher values represent more
         * deeply nested rooms.
         **/
        public int Depth {
            get { return depth; }
            set { depth = value; }
        } [SerializeField] int depth = 0;


        public override float Radius => 0f;


        public override string Template => $@"
## {Name} ##
{description.init}{description.raw}

{description.Help}";


        /** `Things` : **`Thing[]`**
         *
         * Collection of `Thing`s that start in the room.
         **/
        public List<Thing> Things {
            get { return new List<Thing>(things); }
            set { things = value; }
        } List<Thing> things;


        public override lit::Description description {get;set;}


        /** `Viewing` : **`coroutine`**
         *
         * Locks the `Terminal` for a bit while the description
         * of this `Room` is `Log`ged. Can be called from all
         * over, even from `Update` functions which are called
         * every frame, because it will take at least `4s` for
         * the next `Description` to be logged.
         **/
        IEnumerator Viewing(Player player) {
            if (!waitViewRoom) {
                waitViewRoom = true;
                if (_collider)
                    _collider.enabled = false;
                yield return new WaitForSeconds(2f);
                if (Seen) lit::Terminal.Log(
                    $"<cmd>Now Entering:</cmd> {Name}");
                else if (frames>0xC) View();
                Seen = true;
                player.Room = this;
            }
        }


        public void FixedUpdate() {
            if (frames>0x02) frames--; }


        public override void Awake() { base.Awake();
            gameObject.layer = LayerMask.NameToLayer("Room"); }


        public void OnTriggerEnter(Collider o) {
            if (Player.Is(o))
                StartCoroutine(Viewing(
                    Player.InstanceFor(o)));
        }

        public void OnTriggerStay(Collider o) {
            if (Player.Is(o) && frames<0xFA) frames+=2; }


        public void OnTriggerExit(Collider o) {
            var player = Player.InstanceFor(o);
            if (player==null) return;
            waitViewRoom = false;
            StopAllCoroutines();
            if (player.Room==this) player.Room = null;
        }


        public override void Deserialize() =>
            Pathways.Deserialize<Room,Room_yml>(this);
    }
}
