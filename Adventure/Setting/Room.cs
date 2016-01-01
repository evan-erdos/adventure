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
    public partial class Room : Thing {
        bool wait;
        public byte hack = 0x0;

        public List<Room> nearbyRooms;


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
        IEnumerator LoggingRoom() {
            if (!wait) {
                wait = true;
                collider.enabled = false;
                yield return new WaitForSeconds(1f);
                Seen = true; //if (hack>0x7F) {
                PathwaysEngine.Literature.Terminal.LogCommand(
                    "Now Entering: "+Name);
                Player.Room = this;
                // now rooms are one time use
                //yield return new WaitForSeconds(10f);
                //collider.enabled = true;
                //wait = false;
            }
        }

        public string descThings() {
            if (things==null || things.Count<1) return "";
            if (things.Count==1)
                return string.Format(
                    "You see a {0} here.",things[0]);
            var buffer = new Buffer("You see a ");
            foreach (var thing in things)
                buffer.Append(thing.name+", ");
            return buffer.ToString();
        }


        /** `IsValidRoom()` : **`bool`**
         *
         * Checks if the `Player`'s current room can switch to
         * this room, and if it already is the current room.
         **/
        static bool IsValidRoom(Room room) {
            if (!room || room==Player.Room) return false;
            if (!Player.Room) return true;
            return (room.Depth>Player.Room.Depth);
        }

        void LogRoom() {
            if (Seen) PathwaysEngine.Literature.Terminal.LogCommand($"Now Entering: {Name}");
            else PathwaysEngine.Literature.Terminal.Log(description);
            Seen = true;
            Player.Room = this;
        }


        public override void Awake() { base.Awake();
            gameObject.layer = LayerMask.NameToLayer("Room"); }


        public void OnTriggerEnter(Collider o) {
            if (Player.IsCollider(o) && IsValidRoom(this)) {
                //if (hack>0x7E && !wait)
                StartCoroutine(LoggingRoom());
                if (hack<=0x80)
                    hack = unchecked ((byte)((hack<<0x1)|0x1));
            }
        }

        public void OnTriggerExit(Collider o) {
            if (Player.IsCollider(o)) {
                hack = 0x0; wait = false;
                if (Player.Room==this) Player.Room = null;
            }
        }
    }
}
