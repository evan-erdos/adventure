/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-12 * Player */

using UnityEngine;
using EventArgs=System.EventArgs;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PathwaysEngine.Adventure;
using adv=PathwaysEngine.Adventure;
using map=PathwaysEngine.Adventure.Setting;
using inv=PathwaysEngine.Inventory;
using lit=PathwaysEngine.Literature;
using mv=PathwaysEngine.Movement;
using stat=PathwaysEngine.Statistics;
using u=PathwaysEngine.Utilities;

using System.Reflection;


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
        static Regex regex = new Regex(
            @"Player(Gimbal|Graphics)?");
        static public List<inv::Item> wornItems;
        static public new string Name = "Amelia Earhart";
        static public new stat::Set stats;
        static public u::key menu, term, lamp;
        static public new map::Area area;

        static public map::Room Room {
            get { return ((Person) Pathways.player).room; }
            set { ((Person) Pathways.player).room = value; } }

        static public bool IsGrounded {get;set;}
        static public bool WasGrounded {get;set;}
        static public bool IsJumping {get;set;}
        static public bool WasJumping {get;set;}

        static public bool IsSliding {
            get { return ((Person) Pathways.player).motor.IsSliding; } }

        static public new bool IsDead {
            get { return ((Person) Pathways.player).motor.IsDead; }
            set { __isDead = value;
                if (__isDead)
                    lit::Terminal.Log(
                        Pathways.messages["dead"]);
            }
        } static bool __isDead = false;

        public uint massLimit {get;set;}

        static public float Saturation {
            get { return Pathways.mainCamera.GetComponent<ColorCorrectionCurves>().saturation; }
            set { Pathways.mainCamera.GetComponent<ColorCorrectionCurves>().saturation = value; }}

        public RandList<string> deathMessages {get;set;}

        static public new Vector3 Position {
            get { return ((Person) Pathways.player).motor.Position; } }

        static public new inv::IItemSet Items {
            get { return ((Person) Pathways.player).Items; }
            set { ((Person) Pathways.player).Items = value; } }

        static public new List<inv::Item> nearbyItems {
            get { return ((Person) Pathways.player).nearbyItems; } }

        static public new List<adv::Thing> NearbyThings {
            get { return ((Person) Pathways.player).NearbyThings; } }

        public Player() {
            menu = new u::key((n)=>menu.input=n);
            term = new u::key((n)=>term.input=n);
            lamp = new u::key((n)=>{lamp.input=n;
                if (n && left.heldItem!=null && left.heldItem.Held)
                    left.heldItem.Use(); });
        }

        public override void Awake() { base.Awake();
            Pathways.player = this;
            Pathways.mainCamera = Camera.main;
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        public override void Start() {
            //Player.Saturation = 0f;
            Pathways.GameState = GameStates.Game; }

        public void Update() {
            Player.IsGrounded = ((Person) Pathways.player).motor.IsGrounded;
            Player.IsJumping = ((Person) Pathways.player).motor.IsJumping;
        }

        public void LateUpdate() {
            Player.WasGrounded = ((Person) Pathways.player).motor.WasGrounded;
            Player.WasJumping = ((Person) Pathways.player).motor.WasJumping;
            Cursor.visible = (Pathways.CursorGraphic!=Cursors.None);
        }

        public static void OnCollisionEnter(Collider c) {
            ((Person) Pathways.player).feet.OnFootstep(c.material); }

        public static void OnCollisionEnter(Collision c) {
            ((Person) Pathways.player).feet.OnFootstep(c.collider.material); }

        public static bool IsCollider(Collider c) {
            return (regex.IsMatch(c.tag)); }

        public void ResetPlayerLocalPosition() {
            ((Person) Pathways.player).motor.LocalPosition = Vector3.zero; }

        public static bool Wear(lit::Command c) {
            if (Items.Count==0) return false;
            var temp = new List<inv::IWearable>();
            if ((new Regex(@"\b(all)\b")).IsMatch(c.input))
                return Player.Wear();
            else foreach (var item in Items)
                if (item is inv::IWearable
                && item.IsMatch(c.input))
                    temp.Add((inv::IWearable) item);
            if (temp.Count==1)
                return Player.Wear(temp[0]);
            //else if (temp.Count!=0) lit::Terminal.Resolve(c,temp);
            return false;
        }

        public static bool Stow(lit::Command c) {
            if (Items.Count==0) return false;
            var temp = new List<inv::IWearable>();
            if ((new Regex(@"\b(all)\b")).IsMatch(c.input))
                return Player.Stow();
            else foreach (var item in Items)
                if (item is inv::IWearable
                && item.description.IsMatch(c.input))
                    temp.Add((inv::IWearable) item);
            if (temp.Count==1)
                return Player.Stow(temp[0]);
            //else if (temp.Count!=0)
            //    lit::Terminal.Resolve(c,temp);
            return false;
        }

        public static List<T> DoForNearby<T>(lit::Command c)
                        where T : adv::Thing {
            var list = new List<T>();
            foreach (var thing in NearbyThings)
                if (thing.IsMatch(c.input))
                    if (thing is T) list.Add((T) thing);
            return list;
        }

        public static bool View(lit::Command c) {
            var list = DoForNearby<Thing>(c);
            if (list.Count<1)
                return Player.View();
            if (list.Count>1)
                throw new lit::AmbiguityException<Thing>(
                    "Which did you want to view:",list);
            return list[0].View(
                Pathways.player,list[0],EventArgs.Empty,c);
        }

        public static bool Read(lit::Command c) {
            var list = DoForNearby<Thing>(c);
            foreach (var elem in list)
                if (elem is lit::IReadable)
                    ((lit::IReadable) elem).Read();
            return true;
        }

        public static bool Open(lit::Command c) {
            foreach (var thing in NearbyThings) {
                if (thing.IsMatch(c.input)) {
                    if (thing is IOpenable)
                        ((IOpenable) thing).Open();
                }
            } return true;
        }

        public static bool Shut(lit::Command c) {
            foreach (var thing in NearbyThings) {
                if (thing.IsMatch(c.input)) {
                    if (thing is IOpenable)
                        ((IOpenable) thing).Shut();
                }
            }
            return true;
        }

        public static bool Push(lit::Command c) {
            foreach (var thing in NearbyThings) {
                if (thing.IsMatch(c.input)) {
                    if (thing is IOpenable)
                        ((IOpenable) thing).Shut();
                }
            }
            return true;
        }

        public static bool Pull(lit::Command c) { return true; }

        public static bool Read(lit::IReadable o) { return o.Read(); }

        //@TODO: fix error when !areas
        public static bool Goto(lit::Command c) {
            foreach (var elem in Pathways.areas)
                if (c.regex.IsMatch(elem.name))
                    return Player.Goto(elem);
            return false;
        }

        public static new bool Kill() {
            return Player.Kill(Pathways.player.deathMessages.Next()); }

        public static bool Kill(string s) {
            if (Player.IsDead) return false;
            lit::Terminal.Show(new lit::Command());
            lit::Terminal.Clear();
            lit::Terminal.LogImmediate(new lit::Message(s,lit::Styles.Alert));
            Player.IsDead = true;
            return ((Person) Pathways.player).motor.Kill();
        }


        public static new bool Use(lit::Command c) {
            return ((Person) Pathways.player).Use(c); }
        public static new bool Unlock(lit::Command c) {
            return ((Person) Pathways.player).Unlock(c); }
        public static new bool View() {
            return ((Person) Pathways.player).View(); }
        public static new bool Drop(lit::Command c) {
            return ((Person) Pathways.player).Drop(c); }
        public static new bool Drop() {
            return ((Person) Pathways.player).Drop(); }
        public static new bool Drop(inv::Item item) {
            return ((Person) Pathways.player).Drop(item); }
        public static new bool Take(lit::Command c) {
            return ((Person) Pathways.player).Take(c); }
        public static new bool Take() {
            return ((Person) Pathways.player).Take(); }
        public static new bool Take(inv::Item item) {
            return ((Person) Pathways.player).Take(item); }
        public static new bool Wear() {
            return ((Person) Pathways.player).Wear(); }
        public static new bool Wear(inv::IWearable item) {
            return ((Person) Pathways.player).Wear(item); }
        public static new bool Stow() {
            return ((Person) Pathways.player).Stow(); }
        public static new bool Stow(inv::IWearable item) {
            return ((Person) Pathways.player).Stow(item); }
        public static new bool Goto(map::Area o) {
            return ((Person) Pathways.player).Goto(o); }
        public static new bool Unlock(adv::ILockable o) {
            return ((Person) Pathways.player).Unlock(o); }
        public static new bool Lock(adv::ILockable o) {
            return ((Person) Pathways.player).Lock(o); }
        public static new bool Open(adv::IOpenable o) {
            return ((Person) Pathways.player).Open(o); }
        public static new bool Shut(adv::IOpenable o) {
            return ((Person) Pathways.player).Shut(o); }
        public static new bool Push(adv::IPushable o) {
            return ((Person) Pathways.player).Push(o); }
        public static new bool Pull(adv::IPullable o) {
            return ((Person) Pathways.player).Pull(o); }

        public class Holdall : inv::ItemSet {
            public new List<inv::Item> Items;

            public uint lim { get { return 4; } }

            public new void Add(inv::Item item) {
                if (item.GetType().IsSubclassOf(typeof(inv::Backpack)))
                    Player.Items = (inv::IItemSet) item;
                else if (Items.Count>=lim)
                    lit::Terminal.LogCommand(
                        "Your hands are full.");
                else base.Add(item);
            }

            public T GetItem<T>(string s) where T : inv::Item {
                foreach (var elem in GetItems<T>())
                    if (elem.name==s) return (T) elem;
                return default (T);
            }
        }
    }
}
