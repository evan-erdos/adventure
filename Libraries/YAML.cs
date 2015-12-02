/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-11 * YAML */

using System.IO; // Well, here we are! The other main file!
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Buffer=System.Text.StringBuilder;
using YamlDotNet.Serialization;
using UnityEngine;
using invt=PathwaysEngine.Inventory;
using intf=PathwaysEngine.Adventure;
using maps=PathwaysEngine.Adventure.Setting;
using stat=PathwaysEngine.Statistics;
using util=PathwaysEngine.Utilities;


/** `YAML`
|*
|* These classes act as adapters for the `MonoBehaviour`-
|* derived classes, which `YamlDotNet` cannot instantiate.
|* Instances of these classes are instantiated instead, and
|* then populate the main classes in `Awake()`.
|**/
namespace PathwaysEngine {


    public static partial class Pathways {


        /** `GetYAML<T,U>()` : **`function`**
        |*
        |* GetYAML loads data, serialized from `yml`, into an
        |* instance of `Thing`. Should probably make use of
        |* type contravariance, but hey, what can you do?
        |*
        |* - `T` : **`<T>`**
        |*     real type, usually derives from `Monobehaviour`
        |*
        |* - `U` : **`<T>`**
        |*     nested type, usually named `yml`
        |*
        |* - `o` : **`<T>`**
        |*     object to look for
        |**/
        public static void GetYAML<T,U>(T o)
                        where T : IStorable
                        where U : ISerializable<T> {
            Pathways.yml.GetYAML<U>(o.uuid).Deserialize(o); }


        /** `GetYamlFile<T>()` : **`<T>`**
        |*
        |* Representative method for the nested method of the
        |* same name.
        |*
        |* - `s` : **`string`**
        |*     filename to look for
        |**/
        public static T GetYamlFile<T>(string s) {
            return yml.GetYamlFile<T>(s); }


        /** `yml` : **`class`**
        |*
        |* Nested class that deals with serializing &
        |* deserializing data from `*.yml` files.
        |**/
        public static class yml {
            public static readonly string tag = "tag:yaml.org,2002:";
            public static Dictionary<string,object> data;
            public static Dictionary<string,intf::Thing.yml> things;
            static Deserializer deserializer;
            public static string ext = "*.txt", yaml_ext = ".yml";
            public static string dir =
#if UNITY_EDITOR
                Directory.GetCurrentDirectory()
                    +"/Assets/PathwaysEngine/Resources/";
#else
                Application.dataPath+"/Resources/";
#endif


            /** `yml` : **`constructor`**
            |*
            |* Instantiates a `Deserializer`, registers tags, &
            |* reads data from the specified files.
            |**/
            static yml() {
                data = new Dictionary<string,object>();
                deserializer = new Deserializer();
                deserializer.RegisterTagMapping(
                    tag+"regex", typeof(Regex));
                deserializer.RegisterTagMapping(
                    tag+"thing", typeof(intf::Thing.yml));
                deserializer.RegisterTagMapping(
                    tag+"door", typeof(intf::Door.yml));
                deserializer.RegisterTypeConverter(
                    new RegexYamlConverter());
                foreach (var elem in Directory.GetFiles(dir,ext)) {
                    if (!File.Exists(elem))
                        throw new System.Exception("YAML: 404");

#if DUMB
                    var filename = Path.GetFileName(elem);
                    if (filename=="things.txt") {
                        var buffer0 = new Buffer();
                        foreach (var line0 in File.ReadAllLines(elem))
                            buffer0.AppendLine(line0);
                        foreach (var asdf in deserializer
                                .Deserialize<Dictionary<string,intf::Thing.yml>>(
                                    new StringReader(buffer0.ToString())))

                            things[asdf.Key] = (intf::Thing.yml) asdf.Value;
                    }
#endif
                    var buffer = new Buffer();
                    foreach (var line in File.ReadAllLines(elem))
                        buffer.AppendLine(line);
                    foreach (var kvp in deserializer
                            .Deserialize<Dictionary<string,object>>(
                                new StringReader(buffer.ToString())))
                        data[kvp.Key] = kvp.Value;
                }
            }


            /** `GetYamlFile<T>(string)` : **`<T>`**
            |*
            |* Returns the entire deserialized content of a
            |* file, which is usually a `Dictionary` or `List`.
            |*
            |* - `file` : **`string`**
            |*     Filename to look for
            |* - `throws` : **`IOException`**
            |*     404 on filename
            |**/
            public static T GetYamlFile<T>(string s) {
                var buffer = new Buffer();
                if (!File.Exists(dir+s+yaml_ext))
                    throw new System.Exception("YAML: 404");

                foreach (var line in File.ReadAllLines(dir+s+yaml_ext))
                    buffer.AppendLine(line);
                return deserializer.Deserialize<T>(
                    new StringReader(buffer.ToString()));
            }


            /** `GetYAML<T>()` : **`<T>`**
            |*
            |* Returns an object of type `<T>` from the
            |* dictionary if it exists.
            |*
            |* - `s` : **`string`**
            |*     Key to look for.
            |* - `throws` : **`Exception`**
            |*     There is no key at `data[s]`.
            |**/
            public static T GetYAML<T>(string s) {
                object temp; // fix instance issues
                if (!data.TryGetValue(s,out temp))
                    throw new System.Exception("404 : "+s);
                if (!(temp is T))
                    throw new System.Exception(string.Format(
                        "Bad cast: {0} as {1}",typeof(T),s));
                return ((T) temp);
            }
        }
    }



    public class Command_yml : IStorable {
        public string uuid { get; set; }
        public Regex regex { get; set; }
        public ParserEvents parse { get; set; }


        /** `ParserEvents` : **`enum`**
        |*
        |* This local `enum` defines the `Parse` delegates that
        |* the `Parser` needs to call for each verb and command
        |* entered. This is sloppy and should be removed/fixed.
        |*
        |* - `Sudo` : **`ParserEvents`**
        |*     `Pathways.Sudo` deals with overriding commands
        |*
        |* - `Quit` : **`ParserEvents`**
        |*     `Pathways.Quit` begins the quitting routine
        |*
        |* - `Redo` : **`ParserEvents`**
        |*     `Pathways.Redo` repeats the last command
        |*
        |* - `Save` : **`ParserEvents`**
        |*     `Pathways.Save` saves the game
        |*
        |* - `Load` : **`ParserEvents`**
        |*     `Pathways.Load` loads from a `*.yml` file
        |*
        |* - `Help` : **`ParserEvents`**
        |*     `Pathways.Help` displays a simple help text
        |*
        |* - `View` : **`ParserEvents`**
        |*     `Player.View` examines some object
        |*
        |* - `Look` : **`ParserEvents`**
        |*     `Player.Look` looks around a room / examines
        |*
        |* - `Goto` : **`ParserEvents`**
        |*     `Player.Goto` travels the player to a new place
        |*
        |* - `Move` : **`ParserEvents`**
        |*     `Player.Goto` can be called to move objects
        |*
        |* - `Invt` : **`ParserEvents`**
        |*     `Player.Invt` opens the inventory menu
        |*
        |* - `Take` : **`ParserEvents`**
        |*     `Player.Take` takes an item
        |*
        |* - `Drop` : **`ParserEvents`**
        |*     `Player.Drop` drops an item
        |*
        |* - `Wear` : **`ParserEvents`**
        |*     `Player.Wear` has the player put something on
        |*
        |* - `Stow` : **`ParserEvents`**
        |*     `Player.Stow` has the player take something off
        |*
        |* - `Read` : **`ParserEvents`**
        |*     `Player.Read` reads an `IReadable` thing
        |*
        |* - `Open` : **`ParserEvents`**
        |*     `Player.Open` opens something
        |*
        |* - `Shut` : **`ParserEvents`**
        |*     `Player.Shut` closes something
        |*
        |* - `Push` : **`ParserEvents`**
        |*     `Player.Push` pushes something
        |*
        |* - `Pull` : **`ParserEvents`**
        |*     `Player.Pull` pulls something
        |**/
        public enum ParserEvents {
            Sudo, Quit, Redo, Save, Load, Help,
            View, Look, Goto, Move, Invt, Take,
            Drop, Wear, Stow, Read, Open, Shut,
            Push, Pull, Show, Hide }

        public Command Deserialize(string s) {
            this.uuid = s;
            return new Command(
                this.uuid,
                this.regex,
                SelectParse(this.parse));
        }

        public void Deserialize(Command o) {
            o = new Command(
                this.uuid,
                this.regex,
                SelectParse(this.parse));
        }

        intf::Parse SelectParse(ParserEvents e) {
            switch (e) {
                case ParserEvents.Sudo : return Pathways.Sudo;
                case ParserEvents.Quit : return Pathways.Quit;
                case ParserEvents.Redo : return Pathways.Redo;
                case ParserEvents.Save : return Pathways.Save;
                case ParserEvents.Load : return Pathways.Load;
                case ParserEvents.Help : return Pathways.Help;
                case ParserEvents.View : return Player.View;
                //case ParserEvents.Look : return Player.Look;
                case ParserEvents.Goto : return Player.Goto;
                //case ParserEvents.Move : return Player.Move;
                //case ParserEvents.Invt : return Player.Invt;
                case ParserEvents.Take : return Player.Take;
                case ParserEvents.Drop : return Player.Drop;
                case ParserEvents.Wear : return Player.Wear;
                case ParserEvents.Stow : return Player.Stow;
                case ParserEvents.Read : return Player.Read;
                case ParserEvents.Open : return Player.Open;
                case ParserEvents.Shut : return Player.Shut;
                case ParserEvents.Push : return Player.Push;
                case ParserEvents.Pull : return Player.Pull;
                case ParserEvents.Show : return Terminal.Show;
                case ParserEvents.Hide : return Terminal.Hide;
                default : return null;
            }
        }
    }

    public partial class Player : intf::Person {
        public override void GetYAML() {
            Pathways.GetYAML<Player,Player.yml>(this); }

        public new class yml : intf::Person.yml, ISerializable<Player> {
            public List<string> deathMessages { get; set; }

            public virtual void Deserialize(Player o) {
                Deserialize((intf::Person) o);
                o.deathMessages = new RandList<string>();
                o.deathMessages.AddRange(this.deathMessages);
            }
        }
    }


    /** `IStorable` : **`interface`**
    |*
    |* Interface for anything that needs to be serialized
    |* from the `yml` dictionary.
    |**/
    public interface IStorable {


        /** `uuid` : **`string`**
        |*
        |* Unique ID for any serialized value in the `yml`
        |* dictionary.
        |*
        |* @TODO: Make them only need to be unique for the
        |* local file, and only for particular types.
        |**/
        string uuid { get; }
    }


    /** `ISerializeable<T>` : **`interface`**
    |*
    |* Typically implemented by nested classes named `yml`, the
    |* main function of this interface is to ensure that there
    |* is a method called `Deserialize` which gets the correct
    |* class to deserialize to at startup.
    |*
    |* - `<T>` : **`type`**
    |*    the type of object to deserialize to, usually just
    |*    the class that this is nested in.
    |**/
    public interface ISerializable<T> where T : IStorable {
        void Deserialize(T o);
    }


    namespace Adventure {

        public partial class Thing : MonoBehaviour, IDescribable {

            public virtual void GetYAML() {
                Pathways.GetYAML<Thing,Thing.yml>(this); }

            public class yml : ISerializable<Thing> {
                public string uuid { get; set; }
                public Description description { get; set; }

                public virtual void Deserialize(Thing o) {
                    o.description = this.description;
                }
            }
        }

        public partial class Door : Thing, IOpenable {

            public override void GetYAML() {
                Pathways.GetYAML<Door,Door.yml>(this); }

            public new class yml : Thing.yml, ISerializable<Door> {
                public bool IsOpen { get; set; }
                public bool IsInitOpen { get; set; }
                public bool IsLocked { get; set; }

                public virtual void Deserialize(Door o) {
                    Deserialize((Thing) o);
                    o.IsOpen = this.IsOpen;
                    o.IsLocked = this.IsLocked;
                    o.IsInitOpen = this.IsInitOpen;
                }
            }
        }

        public partial class Creature : Thing, ILiving {

            public override void GetYAML() {
                Pathways.GetYAML<Creature,Creature.yml>(this); }

            public new class yml : Thing.yml, ISerializable<Creature> {
                public bool isDead { get; set; }

                public virtual void Deserialize(Creature o) {
                    Deserialize((Thing) o);
                    o.isDead = isDead;
                }
            }
        }

        public partial class Person : Creature {

            public override void GetYAML() {
                Pathways.GetYAML<Person,Person.yml>(this); }

            public new class yml : Creature.yml, ISerializable<Person> {
                public maps::Area area { get; set; }
                public maps::Room room { get; set; }

                public virtual void Deserialize(Person o) {
                    Deserialize((Creature) o);
                    o.area = this.area;
                    o.room = this.room;
                }
            }
        }


        public partial class Encounter : Thing {

            public override void GetYAML() {
                Pathways.GetYAML<Encounter,Encounter.yml>(this); }

            public new class yml : Thing.yml, ISerializable<Encounter> {
                public bool reuse { get; set; }
                public float time { get; set; }
                public Inputs input { get; set; }

                public virtual void Deserialize(Encounter o) {
                    Deserialize((Thing) o);
                    o.reuse = this.reuse;
                    o.time = this.time;
                    o.input = this.input;
                }
            }
        }

        namespace Setting {
            public partial class Room : Thing {

                public override void GetYAML() {
                    Pathways.GetYAML<Room,Room.yml>(this); }

                public new class yml : Thing.yml, ISerializable<Room> {
                    public int depth { get; set; }
                    public List<Thing> things { get; set; }
                    public List<Room> nearbyRooms { get; set; }

                    public virtual void Deserialize(Room o) {
                        Deserialize((Thing) o);
                        o.things = this.things;
                        o.nearbyRooms = this.nearbyRooms;
                    }
                }
            }

            public partial class Area : Thing {

                public override void GetYAML() {
                    Pathways.GetYAML<Area,Area.yml>(this); }

                public new class yml : Thing.yml, ISerializable<Area> {
                    public bool safe { get; set; }
                    public int level { get; set; }
                    public List<Room.yml> rooms { get; set; }
                    public List<Area.yml> areas { get; set; }

                    public virtual void Deserialize(Area o) {
                        Deserialize((Thing) o);
                        o.safe = this.safe;
                        o.level = this.level;
                        //o.rooms = this.rooms;
                        //o.areas = this.areas;
                    }
                }
            }
        }
    }

    namespace Inventory {
        public partial class Item : intf::Thing, IGainful {

            public override void GetYAML() {
                Pathways.GetYAML<Item,Item.yml>(this); }

            public new class yml : intf::Thing.yml, ISerializable<Item> {
                public int cost { get; set; }
                public float mass { get; set; }

                public virtual void Deserialize(Item o) {
                    Deserialize((intf::Thing) o);
                    o.Mass = this.mass;  o.Cost = this.cost;
                }
            }
        }

        public partial class Lamp : Item, IWearable {

            public override void GetYAML() {
                Pathways.GetYAML<Lamp,Lamp.yml>(this); }

            public new class yml : Item.yml, ISerializable<Lamp> {
                public float time { get; set; }

                public virtual void Deserialize(Lamp o) {
                    Deserialize((Item) o);
                    o.time = time;
                }
            }
        }

        public partial class Crystal : Item, IWieldable {

            public override void GetYAML() {
                Pathways.GetYAML<Crystal,Crystal.yml>(this); }

            public new class yml : Item.yml, ISerializable<Crystal> {
                public uint shards { get; set; }

                public virtual void Deserialize(Crystal o) {
                    Deserialize((Item) o);
                    o.Shards = this.shards;
                }
            }
        }

        public partial class Weapon : Item, IWieldable {

            public override void GetYAML() {
                Pathways.GetYAML<Weapon,Weapon.yml>(this); }

            public new class yml : Item.yml, ISerializable<Weapon> {
                public float rate { get; set; }

                public virtual void Deserialize(Weapon o) {
                    Deserialize((Item) o);
                    o.rate = rate;
                }
            }
        }
    }


    namespace Puzzle {

        public partial class Piece : intf::Thing, IPiece {

            public override void GetYAML() {
                Pathways.GetYAML<Piece,Piece.yml>(this); }

            public new class yml : intf::Thing.yml, ISerializable<Piece> {
                public bool IsSolved { get; set; }

                public void Deserialize(Piece o) {
                    Deserialize((intf::Thing) o);
                    o.IsSolved = this.IsSolved;
                }
            }
        }

        partial class Lever : intf::Thing, IPiece {

            public override void GetYAML() {
                Pathways.GetYAML<Lever,Lever.yml>(this); }

            public new class yml : intf::Thing.yml, ISerializable<Lever> {
                public bool IsSolved { get; set; }

                public void Deserialize(Lever o) {
                    Deserialize((intf::Thing) o);
                    o.IsSolved = this.IsSolved;
                }
            }
        }
    }
}
