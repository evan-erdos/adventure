/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-24 * Pathways */

using UnityEngine; // Well, here we are! The main file!
using ui=UnityEngine.UI; // The big one! It's the Pathways
using System.Linq; // Engine, just like I pictured it!
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using type=System.Type;
using Flags=System.FlagsAttribute;
using Buffer=System.Text.StringBuilder;
using DateTime=System.DateTime;
using YamlDotNet.Serialization;
using invt=PathwaysEngine.Inventory;
using intf=PathwaysEngine.Adventure;
using maps=PathwaysEngine.Adventure.Setting;
using stat=PathwaysEngine.Statistics;
using util=PathwaysEngine.Utilities;

/** `PathwaysEngine` : **`namespace`**
 *
 * Global namespace which contains the most important classes
 * and any class which requires interaction between any nested
 * namespaces. This file contains most of the `interfaces`,
 * `enums`, & other loose bits that don't belong anywhere else.
 * Also contains direct references to the `Player`, `Terminal`,
 * and any other extremely important, global / static class.
 **/
namespace PathwaysEngine {

	/** `GameStates` : **`enum`**
	 *
	 * This enumerates the global states that the engine can
	 * have. Switching between them has a considerable number
	 * of side effects, like hiding / showing the mouse, or
	 * enabling / disabling UI windows & movement / input.
	 *
	 * - `None` : **`GameStates`**
	 *     Represents no state, precludes `Player` motion & all
	 *     inputs. Will possibly be used in cases like loading
	 *     scenes & whatnot.
	 *
	 * - `Game` : **`GameStates`**
	 *     State for usual gameplay, all motion input is
	 *     permitted, & the `Terminal` UI, `Window` UI, &
	 *     obviously the main menu are disabled.
	 *
	 * - `Term` : **`GameStates`**
	 *     State represents when the `Terminal` is opened, &
	 *     the user is either entering input or dealing with
	 *     some aspect of its function, e.g., when checking the
	 *     `Player`'s inventory, or when resolving a command.
	 *
	 * - `Msgs` : **`GameStates`**
	 *     State represents that `Window` is opened, & the
	 *     `Player` is reading the message. This state disables
	 *     Player` movement, hides the `Terminal`, & the mouse.
	 *
	 * - `Menu` : **`GameStates`**
	 *     Active when the `Menu` is active. Disables almost
	 *     all components & inputs, and shows the `Menu` UI.
	 *
	 **/
	public enum GameStates { None, Game, Term, Msgs, Menu }


	/** `Formats` : **`enum`**
	 *
	 * This enumerates the various formatting options that the
	 * `Terminal` can use. Most values have some meaning, which
	 * are used by the `Terminal.Format` function. They might
	 * be `hex` values for colors, sizes of headers, etc.
	 *
	 * - `Newline` : **`Formats`**
	 *     Add a newline before logging the message.
	 *
	 * - `Inline` : **`Formats`**
	 *     Removes a newline, so the message is logged on the
	 *     same line as the previous message.
	 *
	 * - `h1` : **`Formats`**
	 *     Makes this message a <h1> header, its enum value
	 *     being the size for the text.
	 *
	 * - `h2` : **`Formats`**
	 *     Makes this message a <h2> header, its enum value
	 *     being the size for the text.
	 *
	 * - `h3` : **`Formats`**
	 *     Makes this message a <h3> header, its enum value
	 *     being the size for the text.
	 *
	 * - `Default` : **`Formats`**
	 *     The base color for the text, currently `0xFFFFFF`,
	 *     for pure white text. This one doesn't need to be
	 *     specified when formatting, but its value is used.
	 *
	 * - `State` : **`Formats`**
	 *     Special color to use to represent some change in
	 *     state, be it some game event passing, or any number
	 *     of other changes the `Player` should be privy to.
	 *
	 * - `Alert` : **`Formats`**
	 *     Usually red, this alerts the `Player` to dangerous
	 *     or very important messages in the `Terminal`.
	 *
	 * - `Command` : **`Formats`**
	 *     Color to use when the user is issuing / resolving
	 *     commands from the parser.
	 **/
	public enum Formats : int {
		Newline=0, Inline=1, Refresh=2,
		h1=36,     h2=28,    h3=24,
		Default=0xFFFFFF, State=0x2A98AA,
		Change=0xFFAE10, Alert=0xFC0000,
		Command=0x999999 };

	public delegate void StateHandler(
		object sender,System.EventArgs e,GameStates gameState);

	/** `Pathways` : **`main`**
	 *
	 * This is the main class for the entire engine. While it
	 * does have a lot of global states, some of them are the
	 * only way I can see to deal with the complex interactions
	 * between the many subsystems in the engine. While having
	 * any sort of global state is usually bad, using it in a
	 * limited manner can help the entire engine's cohesion, as
	 * well as the organization of these different subsystems.
	 **/
 	public static class Pathways {
		public static DateTime gameDate, finalDate;
		public static Camera mainCamera;
		public static Player player;
		public static MessageWindow messageWindow;
		public static Terminal terminal;
		public static event StateHandler StateChange;
		public static List<maps::Area> areas;

		/** `gameState` : **`GameStates`**
		 *
		 * Changes the global state of the engine, and has many
		 * side effects, such as changing the mouse visiblility
		 * and movement inputs, UI visibility, etc.
		 **/
		public static GameStates gameState {
			get { return _gameState; }
			set { if (_gameState!=value) _lastState = _gameState;
				_gameState = value;
				if (StateChange!=null)
					StateChange(null,System.EventArgs.Empty,_gameState);
			}
		} static GameStates _gameState = GameStates.Game;

		/** `lastState` : **`GameStates`**
		 *
		 * Records what the state was last frame. Useful when
		 * trying to sort out if / what actions to take when
		 * changing between states.
		 **/
		public static GameStates lastState {
			get { return _lastState; }
		} static GameStates _lastState = GameStates.Game;

		/** `Pathways` : **`constructor`**
		 *
		 * Initializes some important global variables before
		 * most other classes / `MonoBehaviour`s are created,
		 * and well before any call that `UnityEngine` makes to
		 * `MonoBehaviour.Awake` or any other event.
		 **/
		static Pathways() {
			mainCamera = Camera.main;
			gameDate = new DateTime(1994,5,8,2,0,0);
			finalDate = new DateTime(1994,5,13,14,0,0);
		} /* (Application.LoadLevel(1); */

		/** `OnStateChange()` : **`void`**
		 *
		 * This function registers subscribers to the global
		 * state change event delegate.
		 *
		 * - `e` : **`EventArgs`**
		 *     Standard event class, included for consistency.
		 *
		 * - `gameState` : **`GameStates`**
		 *     Gamestate to change to, is sent to `StateChange`
		 *     if it's !null, and creates a new `StateHandler`
		 *     if necessary. Adds the main listener,
		 *     `EventListener`, in the case that it hasn't been
		 *     initialized yet.
		 **/
		public static void OnStateChange(
						System.EventArgs e,
						GameStates gameState) {
			if (StateChange==null)
				StateChange += new StateHandler(EventListener);
			else StateChange(default (object),e,gameState);
		}

		/** `EventListener()` : **`void`**
		 *
		 * This is the main multicast `delegate` for changes
		 * made to the global state of the game. Lots of other
		 * classes subscribe to this event via `EventListener`.
		 *
		 * - `sender` : **`object`**
		 *     Included in case I want any kind of callback.
		 *
		 * - `e` : **`EventArgs`**
		 *     Standard event class, included for consistency.
		 *
		 * - `gameState` : **`GameStates`**
		 *     Gamestate to change to.
		 **/
		public static void EventListener(
						object sender,
						System.EventArgs e,
						GameStates gameState) {
			Cursor.visible = (gameState==GameStates.Msgs
				|| gameState==GameStates.Menu);
		}

		/** `Log()` : **`void`**
		 *
		 * Log loads data, serialized from `yaml`. This is bad
		 * design cohesion. `Log`s the message to the terminal.
		 *
		 * - `s` : **`string`**
		 *     Key to look for in serialized dictionary.
		 **/
		public static void Log(string s) {
			Terminal.Log((intf::Message) yaml.data[s]); }

		/** `Sudo()` : **`Parse`**
		 *
		 * For special user commands. Unused, so far.
		 *
		 * - `cmd` : **`command`**
		 *     Default `command` struct, sometimes unused, but
		 *     means that this function is a `Parse` delegate.
		 **/
		public static void Sudo(command cmd) { }

		/** `Redo()` : **`Parse`**
		 *
		 * Runs the prior command issued to the `Parser` again.
		 *
		 * - `cmd` : **`command`**
		 *     Default `command` struct, sometimes unused, but
		 *     means that this function is a `Parse` delegate.
		 **/
		public static void Redo(command cmd) {
			intf::Parser.eval(cmd.input); }

		/** `Quit()` : **`Parse`**
		 *
		 * Prompts user through `Terminal` to quit the game.
		 *
		 * - `cmd` : **`command`**
		 *     Default `command` struct, sometimes unused, but
		 *     means that this function is a `Parse` delegate.
		 **/
		public static void Quit(command cmd) {
			Terminal.Alert((intf::Message) yaml.data["quit"]); }

		/** `Load()` : **`Parse`**
		 *
		 * Loads a game from a `*.yml` file. Currently broken.
		 *
		 * - `cmd` : **`command`**
		 *     Default `command` struct, sometimes unused, but
		 *     means that this function is a `Parse` delegate.
		 **/
		public static void Load(command cmd) {
			Terminal.Clear();
			Terminal.Log("I/O Disabled, restarting level.",
				Formats.Newline);
			Application.LoadLevel(0);
		}

		/** `Save()` : **`Parse`**
		 *
		 * Saves a game from a `*.yml` file. Currently broken.
		 *
		 * - `cmd` : **`command`**
		 *     Default `command` struct, sometimes unused, but
		 *     means that this function is a `Parse` delegate.
		 **/
		public static void Save(command cmd) {
			if (File.Exists(cmd.input))
				Terminal.Alert("Overwriting file...");
			using (StreamWriter file = new StreamWriter(cmd.input)) {
				file.WriteLine("%YAML 1.1");
				file.WriteLine("%TAG !invt! _PathwaysEngine.Inventory.");
				file.WriteLine("%TAG !maps! _PathwaysEngine.Adventure.Setting.");
				file.WriteLine(string.Format("---  # {0}\n",
					new P_ID("Saved_Game_00000",DateTime.Now)));
				file.WriteLine("player:");
				file.WriteLine(string.Format("  position: {0}",
					new Vector3(
						Player.position.x,
						Player.position.y,
						Player.position.z)));
				file.WriteLine(string.Format(
					"  area: {0}",Player.area));
				file.WriteLine(string.Format(
					"  room: {0}",Player.room));
				file.WriteLine("  holdall:\n");
				foreach (var elem in Player.holdall)
					file.WriteLine(string.Format("  - {0}",elem.uuid));
				file.WriteLine("\n...\n");
			}
		} /*(s) => (s.Length>100)?(s.Substring(0,100)+"&hellip;"):(s); */

		/** `Help()` : **`Parse`**
		 *
		 * Shows the help menu via `Window`.
		 *
		 * - `cmd` : **`command`**
		 *     Default `command` struct, sometimes unused, but
		 *     means that this function is a `Parse` delegate.
		 **/
		public static void Help(command cmd) {
			Terminal.Display((intf::Message) yaml.data["help"]); }

		/** `yaml` : **`class`**
		 *
		 * Nested class that deals with serializing &
		 * deserializing data from `*.yml` files.
		 **/
		public static class yaml {
			public static readonly string tag = "tag:yaml.org,2002:";
			public static Dictionary<string,object> data;

			/** `yaml` : **`constructor`**
			 *
			 * Instantiates a `Deserializer`, registers tags, &
			 * reads data from the specified files.
			 **/
			static yaml() {
				data = new Dictionary<string,object>();
				var deserializer = new Deserializer();
				deserializer.RegisterTagMapping(
					tag+"regex", typeof(Regex));
				deserializer.RegisterTypeConverter(
					new RegexYamlConverter());
				foreach (var elem in Directory.GetFiles(
#if UNITY_EDITOR
						Directory.GetCurrentDirectory()
						+"/Assets/PathwaysEngine/Resources/","*.txt")) {
#else
						Application.dataPath+"/Resources/","*.txt")) {
#endif
					if (!File.Exists(elem))
						throw new System.Exception("YAML: 404");
					var buffer = new Buffer();
					foreach (var line in File.ReadAllLines(elem))
						buffer.AppendLine(line);
					foreach (var kvp in deserializer
							.Deserialize<Dictionary<string,object>>(
								new StringReader(buffer.ToString())))
						data[kvp.Key] = kvp.Value;
				}
			}

			/** `GetYAML<T>()` : **`T`**
			 *
			 * Returns an object of type `<T>` from the
			 * dictionary if it exists.
			 *
			 * - `s` : **`string`**
			 *     Key to look for.
			 * - `throws` : **`Exception`**
			 *     There is no key at `data[s]`.
			 **/
			public static T GetYAML<T>(string s) {
				object temp;
				if (data.TryGetValue(s,out temp)) return ((T) temp);
				else throw new System.Exception("404 : "+s);
			}
		}
	}

	/** `RandList<T>` : **`class`**
	 *
	 * Extremely simple wrapper class for `List<T>`, which
	 * adds the ability to return a random element from the
	 * list.
	 *
	 * - `<T>` : **`Type`**
	 **/
	public class RandList<T> : List<T> {
		System.Random random = new System.Random();

		/** `GetRandom` : **`<T>`**
		 *
		 * Returns a random element from the list.
		 **/
		public T GetRandom() {
			return this[random.Next(this.Count())]; }
	}

	/** `Extension` : **`class`**
	 *
	 * Class to contain all minor extension methods added to
	 * `string` and other `System` types I can't change myself.
	 **/
	public static class Extension {

		/** `md()` : **`string`**
		 *
		 * Adds support for `Markdown`, and can be called on
		 * any `string`. Formats the `Markdown` syntax into
		 * `HTML`. Currently removes all `<p>` tags.
		 *
		 * - `s` : **`string`**
		 *    `string` to be formatted.
		 **/
		public static string md(this string s) {
			var buffer = new Buffer(Markdown.Transform(s));
			return buffer
				.Replace("<em>","<i>").Replace("</em>","</i>")
				.Replace("<strong>","<b>").Replace("</strong>","</b>")
				.Replace("<h1>","<size=36>").Replace("</h1>","</size>")
				.Replace("<h2>","<size=24>").Replace("</h2>","</size>")
				.Replace("<h3>","<size=16>").Replace("</h3>","</size>")
				.Replace("<ul>","").Replace("</ul>","")
				.Replace("<li>","").Replace("</li>","")
				.Replace("<p>","").Replace("</p>","").ToString();
		}

		/** `Log()` : **`void`**
		 *
		 * Deprecated, should be part of `IItemSet`.
		 **/
		public static void Log(this invt::IItemSet itemSet) {
			Terminal.Log(string.Format("It contains: ",itemSet));
			foreach (var item in itemSet)
				Terminal.Log(string.Format("\n- {0}",item));
		}

		/** `Replace()` : **`string`**
		 *
		 * Adds an overload to the existing `Replace()` that
		 * takes a single argument, for removing things instead
		 * of replacing them.
		 *
		 * - `s` : **`string`**
		 *    `string` to be formatted.
		 *
		 * - `newValue` : **`string`**
		 *    replacement `string` to insert.
		 **/
		public static string Replace(this string s, string newValue) {
			return s.Replace(newValue,""); }

		/** `Strip()` : **`string`**
		 *
		 * @TODO: Dumb name, should be changed.
		 *
		 * - `s` : **`string`**
		 *    `string` to be processed for usage with `Parser`.
		 **/
		public static string Strip(this string s) {
			return s.Trim().ToLower().Replace("\bthe\b").Replace("\ba\b"); }

		/** `Process()` : **`List<string>`**
		 *
		 * - `s` : **`string`**
		 *    `string` to be split into sentences for the
		 *    `Parser` to do its job.
		 **/
		public static List<string> Process(this string s) {
			return new List<string>(s.Strip().Split('.')); }

		/** `GetYAML()` : **`void`**
		 *
		 * - `o` : **`Encounter`**
		 *     Get's the data for the specified object.
		 **/
		public static void GetYAML(this intf::Encounter o) {
			Pathways.yaml.GetYAML<intf::Encounter.yml>(o.uuid).Deserialize(o); }

		/** `GetYAML()` : **`void`**
		 *
		 * - `o` : **`Lamp`**
		 *     Get's the data for the specified object.
		 **/
		public static void GetYAML(this invt::Lamp o) {
			Pathways.yaml.GetYAML<invt::Lamp.yml>(o.uuid).Deserialize(o); }

		/** `GetYAML()` : **`void`**
		 *
		 * - `o` : **`Area`**
		 *     Get's the data for the specified object.
		 **/
		public static void GetYAML(this maps::Area o) {
			Pathways.yaml.GetYAML<maps::Area.yml>(o.uuid).Deserialize(o); }

		/** `GetYAML()` : **`void`**
		 *
		 * - `o` : **`Room`**
		 *     Get's the data for the specified object.
		 **/
		public static void GetYAML(this maps::Room o) {
			Pathways.yaml.GetYAML<maps::Room.yml>(o.uuid).Deserialize(o); }

		/** `GetYAML()` : **`void`**
		 *
		 * Should be the only overload here.
		 *
		 * - `o` : **`Thing`**
		 *     Get's the data for the specified object.
		 **/
		public static void GetYAML(this intf::Thing o) {
			Pathways.yaml.GetYAML<intf::Thing.yml>(o.uuid).Deserialize(o); }
	}

	/** `P_ID` : **`struct`**
	 *
	 * **Deprecated**
	 *
	 * Attempt to formalize the `uuid`s from the serialization
	 * process.
	 *
	 * - `name` : **`string`**
	 *     Name of the `P_ID` to be used when serializing.
	 *
	 * - `date` : **`DateTime`**
	 *     Date to use, to keep all `P_ID`s unique when
	 *     serializing a saved game.
	 **/
	public struct P_ID { // hehe, get it? PiD!
		public DateTime date;
		public string @name { // ! enforce length, not sure if I care
			get { return string.Format(
				"pathways-{1:yyyy-mm-dd}-{0}",_name,date); }
			private set { _name = value; }
		} private string _name;

		public P_ID(string name) {
			this.@name = name; date = new DateTime(1994,5,8); }

		public P_ID(string name, DateTime date)
			: this(name) { this.date = date; }

		public override string ToString() { return @name; }
	}

	/** `proper_name` : **`struct`**
	 *
	 * Attempt to formalize names for `Actor`s.
	 *
	 * - `name` : **`string`**
	 *     Full name of the `P_ID` to be used when serializing.
	 *
	 * - `first` : **`string`** - First Name
	 *
	 * - `last` : **`string`** - Surname
	 **/
	public struct proper_name {
		public string first, last;

		public proper_name(string name) {
			var s = name.Split(' ');
			if (s.Count()!=2)
				throw new System.Exception("bad name input");
			this.first = s[0];
			this.last = s[1];
		}

		public proper_name(string first, string last) {
			this.first = first; this.last = last; }
	}

	/** `command` : **`struct`**
	 *
	 * Represents any `Player`-issued command.
	 **/
	public struct command {
		public string name,input;
		public Regex regex;
		public intf::Parse parse;

		public command(string name,Regex regex,intf::Parse parse) {
			this.name = name; this.regex = regex;
			this.parse = parse; this.input = "";
		}

		public command(
				string name, Regex regex,
				intf::Parse parse, string input)
			: this(name,regex,parse) { this.input = input; }

		public command(string name,string regex,intf::Parse parse)
			: this(name,new Regex(regex),parse) { }

		public command(
				string name, string regex,
				intf::Parse parse, string input)
			: this(name,new Regex(regex),parse) { this.input = input; }

		public command(
				string name,intf::Parse parse,string regex)
			: this(name,new Regex(regex),parse) { }

		public command(
				string name, intf::Parse parse,
				string regex, string input)
			: this(name,new Regex(regex),parse) { this.input = input; }
	}

	/** `Damages` : **`enum`**
	 *
	 * Represents the many types of damage.
	 *
	 * - `Default` : **`Damages`**
	 *     Default damage is direct, and factors into damage
	 *     calculations against default resistances only.
	 *
	 * - `Pierce` : **`Damages`**
	 *     Penetrative damage, applies to sharp & very fast
	 *     kinds of weapons / missiles.
	 *
	 * - `Crush` : **`Damages`**
	 *     Brute force damage, usually as a result of very
	 *     heavy impacts and very strong people.
	 *
	 * - `Fire` : **`Damages`**
	 *     Burning damage.
	 *
	 * - `Magic` : **`Damages`**
	 *     Magical damage.
	 **/
	public enum Damages { Default, Pierce, Crush, Fire, Magic }

	/** `Affinities` : **`enum`**
	 *
	 * Represents the many types of hits.
	 *
	 * - `Miss` : **`Affinities`**
	 *     No hit takes place.
	 *
	 * - `Graze` : **`Affinities`**
	 *     Glancing blow, or extremely ineffective hit.
	 *
	 * - `Hit` : **`Affinities`**
	 *     Default hit, normal effectiveness.
	 *
	 * - `Crit` : **`Affinities`**
	 *     Critical hit, very damaging / extremely effective
	 *     against the receiver's resistances.
	 **/
	public enum Affinities : byte { Miss, Graze, Hit, Crit }

}




namespace dev {
	public class Point {
		public float x { get; set; }
		public float y { get; set; }
		public float z { get; set; }

		public Point() { x = 0; y = 0; z = 0; }

		public Point(float x,float y,float z) {
			this.x = x; this.y = y; this.z = z; }

		public void draw() {
			Draw.Line(
				new Vector3(x-0.1f,y+0.1f,z-0.1f),
				new Vector3(x+0.1f,y-0.1f,z+0.1f));
			Draw.Line(
				new Vector3(x-0.1f,y-0.1f,z-0.1f),
				new Vector3(x+0.1f,y+0.1f,z+0.1f));
		}
	}

	public static class Draw {
		static Draw() { }

		public static void Line(Vector3 x,Vector3 y) {
			Line(x,y,Color.white); }

		public static void Line(Vector3 x,Vector3 y,Color c) {
			Debug.DrawLine(x,y,c); }

		//public static void Point(Vector3 p) { new Point(p); }

		public static void Circle(Vector3 c, float r) {
			for (int i=0;i<36;++i) {
				var s0 = new Vector3(
					Mathf.Cos((i*10*r)/Mathf.Rad2Deg)+c.x,c.y,
					Mathf.Sin((i*10*r)/Mathf.Rad2Deg)+c.z);
				var s1 = new Vector3(
					Mathf.Cos(((i+1)*10*r)/Mathf.Rad2Deg)+c.x,c.y,
					Mathf.Sin(((i+1)*10*r)/Mathf.Rad2Deg)+c.z);
				Line(s1,s0,Color.white);
			}
		}

		public static void Cylinder(Vector3 c, float r, float h) {
#if Advanced
		int i;
		for(i=0;i<vertP;i++) {
			fv[i]=newVertex[i];fn[i]=newNormal[i];
			fuv[i]=tada2[tadac2++];
			Vector3 fuvt=transform.TransformPoint(fn[i]).normalized;
			fuv[i].x=(fuvt.x+1f)*.5f;fuv[i].y=(fuvt.y+1f)*.5f;}
//			fuv[i].x=fn[i].x;fuv[i].y=fn[i].y;}

		for(i=vertP;i<fv.Length;i++) {
			fv[i][0]=0;fn[i][0]=0;fuv[i][0]=0;
			fv[i][1]=0;fn[i][1]=0;fuv[i][1]=0;
			fv[i][2]=0;}

		for(i=0;i<triP;i++) {ft[i]=newTri[i];}
		for(i=triP;i<ft.Length;i++) {ft[i]=0;}

		Mesh mesh=((MeshFilter) GetComponent("MeshFilter")).mesh;
	    mesh.vertices = fv ;
	    mesh.uv = fuv;
		mesh.triangles = ft;
	    mesh.normals = fn;

	    /*For Disco Ball Effect*/
	    //mesh.RecalculateNormals();\
#endif
			Circle(c,r); Circle(new Vector3(c.x,c.y+h,c.z), r);
			Line(new Vector3(c.x+r,c.y,c.z),new Vector3(c.x+r,c.y+h,c.z));
			Line(new Vector3(c.x-r,c.y,c.z),new Vector3(c.x-r,c.y+h,c.z));
			Line(new Vector3(c.x,c.y,c.z+r),new Vector3(c.x,c.y+h,c.z+r));
			Line(new Vector3(c.x,c.y,c.z-r),new Vector3(c.x,c.y+h,c.z-r));
		}
	}
}


