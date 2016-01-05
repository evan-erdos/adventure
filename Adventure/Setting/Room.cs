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


    /** `Room` : **`class`**
     *
     * This class represents a room or an outdoor room, which
     * defines its connections to other rooms, any of the other
     * `Thing`s it might contain, and defines, pretty broadly,
     * where the `Player` is, and what they're doing.
     **/
    public class Room : Thing {
        bool wait;
        byte hack = 0x0;


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


        /** `Things` : **`Thing[]`**
         *
         * Collection of `Thing`s that start in the room.
         **/
        public List<Thing> Things {
            get { return new List<Thing>(things); }
            set { things = value; }
        } List<Thing> things;


        public override lit::Description description {get;set;}


        /** `LoggingRoom` : **`coroutine`**
         *
         * Locks the `Terminal` for a bit while the description
         * of this `Room` is `Log`ged. Can be called from all
         * over, even from `Update` functions which are called
         * every frame, because it will take at least `4s` for
         * the next `Description` to be logged.
         **/
        IEnumerator LoggingRoom(Player player) {
            if (!wait) {
                wait = true;
                collider.enabled = false;
                yield return new WaitForSeconds(1f);
                Seen = true; //if (hack>0x7F) {
                lit::Terminal.LogCommand(
                    $"Now Entering: {Name}");
                player.Room = this;
            }
        }

        public string descThings() {
            if (things==null || things.Count<1) return "";
            if (things.Count==1)
                return $"You see a {things[0]} here.";
            var buffer = new Buffer("You see a ");
            foreach (var thing in things)
                buffer.Append(thing.name+", ");
            return buffer.ToString();
        }

        void LogRoom() {
            if (Seen) lit::Terminal.LogCommand(
                $"Now Entering: {Name}");
            else lit::Terminal.Log(description);
            Seen = true;
        }


        public override void Awake() { base.Awake();
            gameObject.layer = LayerMask.NameToLayer("Room"); }


        public void OnTriggerEnter(Collider o) {
            if (Player.Is(o)) { //if (hack>0x7E && !wait)
                StartCoroutine(LoggingRoom(
                    Player.InstanceFor(o)));
                if (hack<=0x80)
                    hack = unchecked ((byte)((hack<<0x1)|0x1));
            }
        }

        public void OnTriggerExit(Collider o) {
            var player = Player.InstanceFor(o);
            if (player==null) return;
            hack = 0x0; wait = false;
            if (player.Room==this) player.Room = null;
        }

        public override void Deserialize() =>
            Pathways.Deserialize<Room,Room_yml>(this);
    }
}
