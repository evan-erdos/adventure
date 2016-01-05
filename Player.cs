/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-12 * Player */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EventArgs=System.EventArgs;
using System.Text.RegularExpressions;
using PathwaysEngine.Adventure;
using adv=PathwaysEngine.Adventure;
using map=PathwaysEngine.Adventure.Setting;
using inv=PathwaysEngine.Inventory;
using lit=PathwaysEngine.Literature;
using mv=PathwaysEngine.Movement;
using stat=PathwaysEngine.Statistics;
using u=PathwaysEngine.Utilities;


namespace PathwaysEngine {


    /** `Player` : **`Person`**
     *
     * The main `Player` class inherits from person, and has a
     * number of interesting features. The `Player`'s holdall
     * will either create or auto-assigns a temporary & local
     * `Holdall` when the player has no backpack, to simulate
     * coatpockets or hands. `Player` also interfaces with the
     * movement controllers, deals with damage & death, and
     * almost all other operations in the entire game. It is
     * one of the few classes which is included in the main
     * namespace, due to its importance and its involvement in
     * many of the other namespaces' systems.
     **/
    public class Player : Person {
        bool wait;
        public List<inv::Item> wornItems;
        public string FullName = "Amelia Earhart";
        public new stat::Set stats;
        public u::key menu, term, lamp;

        static Regex regex = new Regex(
            @"Player(Gimbal|Graphics)?");

        public static List<Player> Players => players;
        static List<Player> players = new List<Player>();

        public static Player Current => Players?[0];

        public override bool IsDead {
            set { base.IsDead = value;
                if (value) lit::Terminal.Log(
                    Pathways.messages["dead"]);
            }
        }

        public uint massLimit {get;set;}

        public new Vector3 Position => motor.Position;

        public RandList<string> deathMessages {get;set;}


        public Player() {
            menu = new u::key((n)=>menu.input=n);
            term = new u::key((n)=>term.input=n);
            lamp = new u::key((n)=>{lamp.input=n;
                if (n && left.heldItem!=null
                && left.heldItem.Held)
                    left.heldItem.Use(); });
        }

        public override void Awake() { base.Awake();
            Players.Add(this);
            Pathways.mainCamera = Camera.main;
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        public override void Start() {
            Pathways.GameState = GameStates.Game;
        }

        public void Update() {
//            IsGrounded = motor.IsGrounded;
//            IsJumping = motor.IsJumping;
        }

        public void LateUpdate() {
//            WasGrounded = motor.WasGrounded;
//            WasJumping = motor.WasJumping;
            Cursor.visible = (Pathways.CursorGraphic!=Cursors.None);
        }

        public override void OnDestroy() { base.OnDestroy();
            Players.Remove(this);
        }

        public void OnColliderEnter(Collider c) =>
            feet.OnFootstep(c.material);

        public void OnCollisionEnter(Collision c) =>
            feet.OnFootstep(c.collider.material);


        public static bool Is(Collider c) =>
            regex.IsMatch(c.tag);


        public static bool IsNear(
                        Vector3 position,
                        float radius) {
            var nearest = -1f;
            foreach (var player in Players) {
                var d = (player.Position-position).sqrMagnitude;
                if (d<radius) nearest = d;
            } return (0<nearest && nearest<radius);
        }

        public static bool IsNear(Thing o) =>
            IsNear(o.Position,o.Radius);


        public static Player InstanceFor(Collider c) =>
            c.gameObject.GetComponentInParent<Player>()
                ?? c.gameObject.GetComponent<Player>();


        public static Player NearestTo(Thing o) {
            Player nearest = default (Player);
            var d = -1f;
            foreach (var player in Players) {
                var dist = (player.Position-o.Position).sqrMagnitude;
                if (dist<d && dist<o.Radius) {
                    nearest = player;
                    d = dist;
                }
            } return nearest;
        }


        public void ResetPlayerLocalPosition() =>
            motor.LocalPosition = Vector3.zero;


        public bool Kill(string s) {
            if (IsDead) return false;
            lit::Terminal.Show(new lit::Command());
            lit::Terminal.Clear();
            lit::Terminal.LogImmediate(
                new lit::Message(s,lit::Styles.Alert));
            IsDead = true;
            return motor.Kill();
        }

        public override bool Kill() =>
            Kill(deathMessages.Next());


        public class Holdall : inv::ItemSet {
            public new List<inv::Item> Items;

            public uint lim => 4;

            public T GetItem<T>(string s)
                            where T : inv::Item {
                foreach (var elem in GetItems<T>())
                    if (elem.name==s) return (T) elem;
                return default (T);
            }
        }


        public override void Deserialize() =>
            Pathways.Deserialize<Player,Player_yml>(this);
    }
}
