/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-12 * Player */

using UnityEngine;
using type=System.Type;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PathwaysEngine.Adventure;
using invt=PathwaysEngine.Inventory;
using intf=PathwaysEngine.Adventure;
using maps=PathwaysEngine.Adventure.Setting;
using mvmt=PathwaysEngine.Movement;
using stat=PathwaysEngine.Statistics;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine {


    /** `Player` : **`Person`**
    |*
    |* The main `Player` class inherits from person, and has a
    |* number of interesting features. The `Player`'s holdall
    |* will either create or auto-assigns a temporary & local
    |* `Holdall` when the player has no backpack, to simulate
    |* coatpockets or hands. `Player` also interfaces with the
    |* movement controllers, deals with damage & death, and
    |* almost all other operations in the entire game. It is
    |* one of the few classes which is included in the main
    |* namespace, due to its importance and its involvement in
    |* many of the other namespaces' systems.
    |**/
    public partial class Player : Person {
        public bool wait = false;
        static public List<invt::Item> wornItems;
        static public new string uuid = "Amelia Earhart";
        static public mvmt::Hand right, left;
        static public mvmt::Feet feet;
        static internal mvmt::IMotor motor;
        static public new stat::Set stats;
        static public util::key menu, term, lamp;
        static public new maps::Room room;
        static public new maps::Area area;

        static public bool IsGrounded { get; set; }
        static public bool WasGrounded { get; set; }
        static public bool IsJumping { get; set; }
        static public bool WasJumping { get; set; }

        static public bool IsSliding {
            get { return motor.IsSliding; } }

        static public new bool IsDead {
            get { return motor.IsDead; }
            set { __isDead = value;
                if (__isDead) Pathways.Log("dead");
            }
        } static bool __isDead = false;

        public uint massLimit { get; set; }

        public RandList<string> deathMessages { get; set; }

        static public new Vector3 Position {
            get { return motor.Position; } }

        static public new invt::IItemSet holdall {
            get { if (_holdall==null)
                    _holdall = new Player.Holdall();
                return _holdall; }
            set { _holdall = value; }
        } static invt::IItemSet _holdall;

        static public new List<invt::Item> nearbyItems {
            get { return ((Person) Pathways.player).nearbyItems; } }

        static public new List<intf::Thing> NearbyThings {
            get { return ((Person) Pathways.player).NearbyThings; } }

        public Player() {
            menu = new util::key((n)=>menu.input=n);
            term = new util::key((n)=>term.input=n);
            lamp = new util::key((n)=>{lamp.input=n;
                if (n && left.heldItem!=null && left.heldItem.Held)
                    left.heldItem.Use(); });
        }

        public override void Awake() { base.Awake();
            Pathways.player = this;
            Pathways.mainCamera = Camera.main;
            feet = GetComponentInChildren<mvmt::Feet>();
            motor = GetComponentInChildren<mvmt::IMotor>();
        }

        public override void Start() {
            Pathways.GameState = GameStates.Game; }

        public void Update() {
            Player.IsGrounded = motor.IsGrounded;
            Player.IsJumping = motor.IsJumping;
        }

        public void LateUpdate() {
            Player.WasGrounded = motor.WasGrounded;
            Player.WasJumping = motor.WasJumping;
        }

        public static void OnCollisionEnter(Collider collider) {
            feet.OnFootstep(collider.material); }

        public static void OnCollisionEnter(Collision collision) {
            feet.OnFootstep(collision.collider.material); }

        public static bool IsCollider(Collider c) {
            return (c.tag=="Player" || c.tag=="PlayerGraphics"); }

        public void ResetPlayerLocalPosition() {
            motor.LocalPosition = Vector3.zero; }

        IEnumerator DelayToggle(float t) {
            wait = true;
            yield return new WaitForSeconds(t);
            wait = false;
        }

        public static void Drop(Command c) {
            var temp = new List<invt::Item>();
            if ((new Regex(@"\ball\b")).IsMatch(c.input))
                Player.Drop();
            else foreach (var item in holdall)
                if (item.description.IsMatch(c.input))
                    temp.Add(item);
            if (temp.Count==1) Player.Drop(temp[0]);
            else if (temp.Count!=0) Terminal.Resolve(c,temp);
        }

        public static void Take(Command c) {
            if (nearbyItems.Count==0) return;
            var temp = new List<invt::Item>();
            if ((new Regex(@"\b(all)\b")).IsMatch(c.input))
                Player.Take();
            else foreach (var item in nearbyItems)
                if (item.description.IsMatch(c.input))
                    temp.Add(item);
            if (temp.Count==1)
                Player.Take(temp[0]);
            else if (temp.Count!=0)
                Terminal.Resolve(c,temp);
        }

        public static void Wear(Command c) {
            if (holdall.Count==0) {
                Terminal.Log("You have nothing to wear!",
                    Formats.Command); return; }
            var temp = new List<invt::IWearable>();
            if ((new Regex(@"\b(all)\b")).IsMatch(c.input))
                Player.Wear();
            else foreach (var item in holdall)
                if (item is invt::IWearable
                && item.description.IsMatch(c.input))
                    temp.Add((invt::IWearable) item);
            if (temp.Count==1) Player.Wear(temp[0]);
            else if (temp.Count!=0) Terminal.Resolve(c,temp);
        }

        public static void Stow(Command c) {
            if (holdall.Count==0) {
                Terminal.Log("You have nothing to stow!",
                    Formats.Command); return; }
            var temp = new List<invt::IWearable>();
            if ((new Regex(@"\b(all)\b")).IsMatch(c.input))
                Player.Stow();
            else foreach (var item in holdall)
                if (item is invt::IWearable
                && item.description.IsMatch(c.input))
                    temp.Add((invt::IWearable) item);
            if (temp.Count==1)
                Player.Stow(temp[0]);
            else if (temp.Count!=0)
                Terminal.Resolve(c,temp);
            else Terminal.Log("You don't have anything you can stow.",
                Formats.Command);
        }

        public static List<T> DoForNearby<T>(Command c)
                        where T : intf::Thing {
            var list = new List<T>();
            foreach (var elem in NearbyThings)
                if (elem.description.nouns.IsMatch(c.input))
                    if (elem is T) list.Add((T) elem);
            return list;
        }

        public static void View(Command c) {
            foreach (var elem in NearbyThings)
                if (elem.description.nouns.IsMatch(c.input)) {
                    Terminal.Log(string.Format(
                        " > {0}: ", c.input), Formats.Command);
                    Terminal.Log(elem.description);
                    return;
                }
        }

        public static void Read(Command c) {
            var list = DoForNearby<Thing>(c);
            foreach (var elem in list)
                if (elem is intf::IReadable)
                    ((intf::IReadable) elem).Read();
        }

        public static void Open(Command c) {
            foreach (var elem in NearbyThings) {
                if (elem.description.nouns.IsMatch(c.input)) {
                    if (elem is IOpenable)
                        ((IOpenable) elem).Open();
                }
            }
        }

        public static void Shut(Command c) {
            foreach (var elem in NearbyThings) {
                if (elem.description.nouns.IsMatch(c.input)) {
                    if (elem is IOpenable)
                        ((IOpenable) elem).Shut();
                }
            }
        }

        public static void Push(Command c) {
            foreach (var elem in NearbyThings) {
                if (elem.description.nouns.IsMatch(c.input)) {
                    if (elem is IOpenable)
                        ((IOpenable) elem).Shut();
                }
            }
        }

        public static void Pull(Command c) { }

        public static void Read(IReadable item) { item.Read(); }

        //@TODO: fix error when !areas
        public static void Goto(Command c) {
            foreach (var elem in Pathways.areas)
                if (c.regex.IsMatch(elem.name))
                    Player.Goto(elem);
        }

        public static new void Kill() {
            Player.Kill(Pathways.player.deathMessages.Next()); }

        public static void Kill(string s) {
            if (Player.IsDead) return;
            Terminal.Show(new Command());
            Terminal.Clear();
            Terminal.Alert(s,Formats.Alert,Formats.Newline);
            Player.IsDead = true;
            motor.Kill();
        }

        public static bool Unlock(Command c) {
            return Unlock(null); } //@TODO: UGLY FIX THIS NOW

        public static new void Drop() {
            ((Person) Pathways.player).Drop(); }
        public static new void Drop(invt::Item item) {
            ((Person) Pathways.player).Drop(item); }
        public static new void Take() {
            ((Person) Pathways.player).Take(nearbyItems); }
        public static new void Take(invt::Item item) {
            ((Person) Pathways.player).Take(item); }
        public static new void Wear() {
            ((Person) Pathways.player).Wear(); }
        public static new void Wear(invt::IWearable item) {
            ((Person) Pathways.player).Wear(item); }
        public static new void Stow() {
            ((Person) Pathways.player).Stow(); }
        public static new void Stow(invt::IWearable item) {
            ((Person) Pathways.player).Stow(item); }
        public static new void Goto(maps::Area tgt) {
            ((Person) Pathways.player).Goto(tgt); }
        public static new bool Unlock(intf::Door door) {
            return ((Person) Pathways.player).Unlock(door); }
        public static new void Open(intf::IOpenable tgt) {
            ((Person) Pathways.player).Open(tgt); }
        public static new void Shut(intf::IOpenable tgt) {
            ((Person) Pathways.player).Shut(tgt); }
        public static new void Push(intf::IPushable tgt) {
            ((Person) Pathways.player).Push(tgt); }
        public static new void Pull(intf::IPullable tgt) {
            ((Person) Pathways.player).Pull(tgt); }

        public class Holdall : invt::ItemSet {
            public new List<invt::Item> items;

            public uint lim { get { return 4; } }

            public new void Add(invt::Item item) {
                if (item.GetType().IsSubclassOf(typeof(invt::Backpack)))
                    Player.holdall = (invt::IItemSet) item;
                else if (items.Count>=lim)
                    Terminal.Log("Your hands are full.",Formats.Command);
                else base.Add(item);
            }

            public invt::Item GetItem<T>() where T : invt::Item {
                if (items.Count>1) return items[0];
                return default(invt::Item);
            }

            public invt::Item GetItem<T>(string s) where T : invt::Item {
                foreach (var elem in GetItems<T>())
                    if (elem.name==s) return elem;
                return default(invt::Item);
            }

            public List<invt::Item> GetItems<T>() where T : invt::Item {
                return new List<invt::Item>(items); }
        }
    }
}
