/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-24 * Room */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using invt=PathwaysEngine.Inventory;
using Buffer=System.Text.StringBuilder;

namespace PathwaysEngine.Adventure.Setting {


    /** `Room` : **`class`**
    |*
    |* This class represents a room or an outdoor room, which
    |* defines its connections to other rooms, any of the other
    |* `Thing`s it might contain, and defines, pretty broadly,
    |* where the `Player` is, and what they're doing.
    |**/
    public partial class Room : Thing {
        bool wait = false;
        public List<Room> nearbyRooms;

        public override string Format {
            get { return "## {0} ##\n{{0}}\n\n{1}"; } }


        /** `Depth` : **`int`**
        |*
        |* Determines which room gets precedence when the room
        |* colliders intersect. Higher values represent more
        |* deeply nested rooms.
        |**/
        public int Depth { get; set; }



        /** `Things` : **`Thing[]`**
        |*
        |* Collection of `Thing`s that start in the room.
        |**/
        public List<Thing> Things {
            get { return new List<Thing>(things); }
        } List<Thing> things;


        public override Description description { get; set; }


        public override void FormatDescription() {
            description.SetFormat(string.Format(
                Format,uuid,descThings()));
        }


        /** `LogRoom` : **`coroutine`**
        |*
        |* Locks the `Terminal` for a bit while the description
        |* of this `Room` is `Log`ged. Can be called from all
        |* over, even from `Update` functions which are called
        |* every frame, because it will take at least `4s` for
        |* the next `Description` to be logged.
        |**/
        IEnumerator LogRoom() {
            if (!wait) {
                wait = true;
                yield return new WaitForSeconds(1f);
                seen = true;
                Terminal.Log(this);
                Player.room = this;
                yield return new WaitForSeconds(3f);
                wait = false;
            }
        }

        public string descThings() {
            if (things==null || things.Count<1) return "";
            if (things.Count==1)
                return string.Format("You see a {0} here.",things[0]);
            var buffer = new Buffer("You see a ");
            foreach (var thing in things)
                buffer.Append(thing.name+", ");
            return buffer.ToString();
        }


        /** `IsValidRoom()` : **`bool`**
        |*
        |* Checks if the `Player`'s current room can switch to
        |* this room, and if it already is the current room.
        |**/
        bool IsValidRoom(Room room) {
            if (!room) return true;
            if (wait || room==this) return false;
            if (Player.room && Player.room==this) return false;
            return (room.Depth<=this.Depth);
        }

        public override void Awake() { base.Awake();
            FormatDescription();
        }

        public void OnTriggerEnter(Collider o) {
            if (Player.IsCollider(o) && IsValidRoom(Player.room))
                StartCoroutine(LogRoom());
        }

        public void OnTriggerExit(Collider o) {
            if (Player.IsCollider(o) && Player.room==this)
                Player.room = null;
        }
    }
}
