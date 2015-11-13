/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-12 * Player */

using UnityEngine; // The main player class, inherits from person
using type=System.Type; // the holdall auto-assigns / creates a
using System.Collections; // singleton-ish thing when the player
using System.Collections.Generic; // has no backpack, to simulate
using System.Text.RegularExpressions; // coatpockets and hands
using PathwaysEngine.Adventure; // and whatnot. Interfaces with
using invt=PathwaysEngine.Inventory; // the movement controller
using intf=PathwaysEngine.Adventure; // and also deals /w
using maps=PathwaysEngine.Adventure.Setting; // damage, death,
using mvmt=PathwaysEngine.Movement; // and operations done on
using stat=PathwaysEngine.Statistics; // all sorts of other game
using util=PathwaysEngine.Utilities; // things

namespace PathwaysEngine {
	public class Player : Person {
		public bool wait = false;
		static public List<invt::Item> wornItems;
		static public new string uuid = "Amelia Earhart";
		static public mvmt::Hand right, left;
		static public mvmt::Feet feet;
		static public mvmt::IMotor motor;
		static public new stat::Set stats;
		static public util::key menu, term, lamp;
		static public new maps::Room room;
		static public new maps::Area area;

		static public bool isGrounded { get; set; }
		static public bool wasGrounded { get; set; }
		static public bool isJumping { get; set; }
		static public bool wasJumping { get; set; }

		static public bool isSliding {
			get { return motor.isSliding; } }

		static public bool IsDead {
			get { return motor.isDead; }
			set { __isDead = value;
				if (__isDead) Pathways.Log("dead");
			}
		} static bool __isDead = false;

		public uint massLimit { get; set; }

		static public new Vector3 position {
			get { return motor.position; } }

		static public new invt::IItemSet holdall {
			get { if (_holdall==null)
					_holdall = new Player.Holdall();
				return _holdall; }
			set { _holdall = value; }
		} static invt::IItemSet _holdall;

		static public new List<invt::Item> nearbyItems {
			get { return ((Person) Pathways.player).nearbyItems; }
		}

		static public new List<intf::Thing> nearbyThings {
			get { return ((Person) Pathways.player).nearbyThings; }
		}

		static Player() { }

		public Player() {
			menu = new util::key((n)=>menu.input=n);
			term = new util::key((n)=>term.input=n);
			lamp = new util::key((n)=>{lamp.input=n;
				if (n && left.heldItem!=null && left.heldItem.Held)
					left.heldItem.Use(); });
		}

		public override void Awake() { //base.Awake();
			Pathways.player = this;
			feet = GetComponentInChildren<mvmt::Feet>();
			motor = GetComponentInChildren<mvmt::IMotor>();
		}

		public override void Start() {
			Pathways.gameState = GameStates.Game;
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

		public void Update() {
			Player.isGrounded = motor.isGrounded;
			Player.isJumping = motor.isJumping;
		}

		public void LateUpdate() {
			Player.wasGrounded = motor.wasGrounded;
			Player.wasJumping = motor.wasJumping;
		}

		public static void OnCollisionEnter(Collider collider) {
			feet.OnFootstep(collider.material);
		}

		public static void OnCollisionEnter(Collision collision) {
			feet.OnFootstep(collision.collider.material);
		}

		public void ResetPlayerLocalPosition() {
			motor.localPosition = Vector3.zero;
		}

		IEnumerator DelayToggle(float t) {
			wait = true;
			yield return new WaitForSeconds(t);
			wait = false;
		}

		public static void Drop(command cmd) {
			var temp = new List<invt::Item>();
			if ((new Regex(@"\ball\b")).IsMatch(cmd.input))
				Player.Drop();
			else foreach (var item in holdall)
				if (item.description.IsMatch(cmd.input))
					temp.Add(item);
			if (temp.Count==1) Player.Drop(temp[0]);
			else if (temp.Count!=0) Terminal.Resolve(cmd,temp);
		}

		public static void Take(command cmd) {
			if (nearbyItems.Count==0) return;
			var temp = new List<invt::Item>();
			if ((new Regex(@"\b(all)\b")).IsMatch(cmd.input))
				Player.Take();
			else foreach (var item in nearbyItems)
				if (item.description.IsMatch(cmd.input))
					temp.Add(item);
			if (temp.Count==1)
				Player.Take(temp[0]);
			else if (temp.Count!=0)
				Terminal.Resolve(cmd,temp);
		}

		public static void Wear(command cmd) {
			if (holdall.Count==0) {
				Terminal.Log("You have nothing to wear!",
					Formats.Command); return; }
			var temp = new List<invt::IWearable>();
			if ((new Regex(@"\b(all)\b")).IsMatch(cmd.input))
				Player.Wear();
			else foreach (var item in holdall)
				if (item is invt::IWearable
				&& item.description.IsMatch(cmd.input))
					temp.Add((invt::IWearable) item);
			if (temp.Count==1) Player.Wear(temp[0]);
			else if (temp.Count!=0) Terminal.Resolve(cmd,temp);
		}

		public static void Stow(command cmd) {
			if (holdall.Count==0) {
				Terminal.Log("You have nothing to stow!",
					Formats.Command); return; }
			var temp = new List<invt::IWearable>();
			if ((new Regex(@"\b(all)\b")).IsMatch(cmd.input))
				Player.Stow();
			else foreach (var item in holdall)
				if (item is invt::IWearable
				&& item.description.IsMatch(cmd.input))
					temp.Add((invt::IWearable) item);
			if (temp.Count==1)
				Player.Stow(temp[0]);
			else if (temp.Count!=0)
				Terminal.Resolve(cmd,temp);
			else Terminal.Log("You don't have anything you can stow.",
				Formats.Command);
		}

		public static void View(command cmd) {
			foreach (var elem in nearbyThings)
				if (elem.description.nouns.IsMatch(cmd.input)) {
					Terminal.Log(string.Format(
						" > {0}: {1}", cmd.input, elem.description),
						Formats.Command);
					return;
				}
		}

		public static void Read(command cmd) { }

		public static void Read(invt::IReadable item) { item.Read(); }

		//@TODO: fix error when !areas
		public static void Goto(command cmd) {
			foreach (var elem in Pathways.areas)
				if (cmd.regex.IsMatch(elem.name))
					Player.Goto(elem);
		}

		public static new void Kill() {
			Player.IsDead = true;
			motor.Kill();
			//((Person) Pathways.player).Kill();
		}

		public static void Kill(string s) {
			Terminal.Activate(new command());
			Terminal.Clear();
			Terminal.Log(s,Formats.Alert,Formats.Newline);
			Player.Kill();
		}

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

			public void Log() {
				Terminal.Log("You are holding: ");
				foreach (var item in this)
					Terminal.Log(string.Format("\n- {0}",item));
			}
		}
	}
}
