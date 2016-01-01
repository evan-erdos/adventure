/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-11 * YAML */

using System.IO; // Well, here we are! The other main file!
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Buffer=System.Text.StringBuilder;
using DateTime=System.DateTime;
using Type=System.Type;
using YamlDotNet.Serialization;
using UnityEngine;
using adv=PathwaysEngine.Adventure;
using map=PathwaysEngine.Adventure.Setting;
using inv=PathwaysEngine.Inventory;
using lit=PathwaysEngine.Literature;
using puzl=PathwaysEngine.Puzzle;
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


    /** `YAML`
    |*
    |* These classes act as adapters for the `MonoBehaviour`-
    |* derived classes, which `YamlDotNet` cannot instantiate.
    |* Instances of these classes are instantiated instead, and
    |* then populate the main classes on `Awake()`.
    |**/
    public static partial class Pathways {

        public static Dictionary<string,object> data =
            new Dictionary<string,object>();

        public static Dictionary<Type,object> defaults =
            new Dictionary<Type,object>();

        public static Dictionary<string,lit::Command> commands =
        new Dictionary<string,lit::Command>();

        public static Dictionary<string,lit::Message> messages =
        new Dictionary<string,lit::Message>();


        /** `Deserialize<T,U>()` : **`function`**
        |*
        |* Deserialize loads data, serialized from `yml`, into
        |* an instance of `Thing`. Should probably make use of
        |* type contravariance, but hey, what can you do?
        |*
        |* - `<T>` : **`Type`**
        |*     real type, usually derives from `Monobehaviour`
        |*
        |* - `<U>` : **`Type`**
        |*     nested type, usually named `yml`
        |*
        |* - `o` : **`<T>`**
        |*     object of type `T` to look for
        |**/
        public static void Deserialize<T,U>(T o)
                        where T : IStorable
                        where U : ISerializable<T> {
            Pathways.yml.Deserialize<U>(o.Name).Deserialize(o); }


        /** `yml` : **`class`**
        |*
        |* Nested class that deals with serializing &
        |* deserializing data from `*.yml` files.
        |**/
        public static class yml {

            static Deserializer deserializer =
                new Deserializer();


            /** `yml` : **`constructor`**
            |*
            |* Instantiates a `Deserializer`, registers tags, &
            |* reads data from the specified files. While the
            |* usage of `static`s *and*  `constructor`s aren't
            |* kosher in `Unity`, but in this case, it's ok, as
            |* this has nothing to do with the `MonoBehaviour`
            |* loading / instantiation process.
            |**/
            static yml() {
                string  pre = "tag:yaml.org,2002:",
                        ext = ".yml",
                        dir =
#if UNITY_EDITOR
                            Directory.GetCurrentDirectory()
                                +"/Assets/PathwaysEngine/Resources/";
#else
                            Application.dataPath+"/Resources/";
#endif

                // mapping of all the tags to their types
                var tags = new Dictionary<string,Type> {
                    { "regex", typeof(Regex) },
                    { "date", typeof(DateTime) },

                    // Adventure Tags
                    { "thing", typeof(adv::Thing_yml) },
                    { "creature", typeof(adv::Creature.yml) },
                    { "person", typeof(adv::Person.yml) },
                    { "player", typeof(Player.yml) },

                    // Setting Tags
                    { "area", typeof(map::Area.yml) },
                    { "room", typeof(map::Room.yml) },
                    { "door", typeof(map::Door.yml) },

                    // Literature Tags
                    { "parse", typeof(lit::Parse) },
                    { "message", typeof(lit::Message) },
                    { "encounter", typeof(lit::Encounter.yml) },

                    // Inventory Tags
                    { "item", typeof(inv::Item.yml) },
                    { "lamp", typeof(inv::Lamp.yml) },
                    { "items", typeof(inv::ItemSet) },
                    { "book", typeof(inv::Book.yml) },
                    { "bag", typeof(inv::Bag.yml) },
                    { "backpack", typeof(inv::Backpack.yml) },
                    { "key", typeof(inv::Key.yml) },
                    { "crystal", typeof(inv::Crystal.yml) },
                    { "weapon", typeof(inv::Weapon.yml) },
                    { "gun", typeof(inv::Gun.yml) },

                    // Puzzle Tags
                    //{ "piece", typeof(puzl::Piece.yml) },
                    { "button", typeof(puzl::Button.yml) },
                    { "lever", typeof(puzl::Lever.yml) }};

                foreach (var tag in tags)
                    deserializer.RegisterTagMapping(
                        pre+tag.Key, tag.Value);

                deserializer.RegisterTypeConverter(
                    new RegexYamlConverter());

                var files = new[] { // special files
                    "commands", // list of commands
                    "defaults", // default data
                    "pathways", // system messages
                    "settings"}; // project settings

                foreach (var file in files) {
                    var r = GetReader(Path.Combine(dir,file)+ext);
                    switch (file) {
                        case "commands" :
                            foreach (var kvp in deserializer.Deserialize<Dictionary<string,lit::Command_yml>>(r))
                                Pathways.commands[kvp.Key] = kvp.Value.Deserialize(kvp.Key);
                            break;
                        case "defaults" :
                            foreach (var elem in deserializer.Deserialize<Dictionary<string,Dictionary<string,object>>>(r))
                                foreach (var kvp in elem.Value)
                                    Pathways.defaults[GetTypeYAML(elem.Key,kvp.Key)] = kvp.Value;
                            break;
                        case "pathways" :
                            foreach (var kvp in deserializer.Deserialize<Dictionary<string,lit::Message>>(r))
                                Pathways.messages[kvp.Key] = kvp.Value;
                            break;
                    }
                }

                foreach (var elem in Directory.GetFiles(dir,'*'+ext)) {
                    foreach (var file in files)
                        if (elem.Contains(file)) continue;
                    Deserialize(elem);
                }
            }


            /** `GetReader()` : **`StringReader`**
            |*
            |* Gets the `*.yml` file in the main directory only
            |* if it exists and has the proper extension.
            |*
            |* - `throw` : **`Exception`**
            |*     if the file does not exist
            |**/
            static StringReader GetReader(string file) {
                if (!File.Exists(file))
                    throw new System.Exception("YAML 404: "+file);
                var buffer = new Buffer();
                foreach (var line in File.ReadAllLines(file))
                    buffer.AppendLine(line);
                return new StringReader(buffer.ToString());
            }


            /** `GetTypeYAML()` : **`Type`**
            |*
            |* Tries to find a `Type` in `PathwaysEngine` which
            |* is a match for the supplied `string`.
            |*
            |* - `ns` : **`string`**
            |*     `string`ified namespace to look in
            |*
            |* - `s` : **`string`**
            |*     `string`ified name of a `Type` to look for
            |**/
            public static Type GetTypeYAML(string ns, string s) {
                //Debug.Log("type: "+type+"  string: "+ns+s+"+yml");
                return Type.GetType("PathwaysEngine."+ns+"."+s+"+yml"); }


            /** `Deserialize()` : **`function`**
            |*
            |* Called without type arguments, this will simply
            |* deserialize into the `data` object. This is used
            |* only by the `static` constructor to get data out
            |* of the rest of the files (skipping the few files
            |* which are specified above).
            |*
            |* - `file` : **`string`**
            |*     filename to look for
            |*
            |* - `throw` : **`IOException`**
            |**/
            static void Deserialize(string file) {
                foreach (var kvp in deserializer.Deserialize<Dictionary<string,object>>(GetReader(file))) data[kvp.Key] = kvp.Value; }


            /** `Deserialize<T>()` : **`<T>`**
            |*
            |* Returns an object of type `<T>` from the
            |* dictionary if it exists.
            |*
            |* - `<T>` : **`Type`**
            |*      type to look for, and then to cast to, when
            |*      deserializing the data from the file.
            |*
            |* - `s` : **`string`**
            |*     key to look for
            |*
            |* - `throw` : **`Exception`**
            |*     There is no key at `data[s]`, or some other
            |*     problem occurs when attempting to cast the
            |*     object to `<T>`.
            |**/
            public static T Deserialize<T>(string s) {
                object o;
                if (!data.TryGetValue(s,out o))
                    o = DeserializeDefault<T>();
                if (!(o is T))
                    throw new System.Exception(string.Format(
                        "Bad cast: {0} as {1}",typeof(T),s));
                return ((T) o);
            }

            /** `DeserializeDefault<T>()` : **`<T>`**
            |*
            |* Returns the default object of type `<T>`.
            |**/
            public static T DeserializeDefault<T>() {
                object o;
                if (!defaults.TryGetValue(typeof(T),out o))
                    throw new System.Exception(string.Format(
                        "Type {0} not found in defaults",typeof (T)));
                return ((T) o);
            }

            public static object DeserializeDefault(Type type) {
                object o;
                if (!defaults.TryGetValue(type,out o))
                    throw new System.Exception(
                        type+" not found.");
                return o;
            }
        }
    }


    /** `IStorable` : **`interface`**
    |*
    |* Interface for anything that needs to be serialized
    |* from the `yml` dictionary.
    |**/
    public interface IStorable {


        /** `Name` : **`string`**
        |*
        |* This should be an unique identifier that the `*.yml`
        |* `Deserializer` should look for in files.
        |**/
        string Name { get; }
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
    public interface ISerializable<T>
                    where T : IStorable {

        void Deserialize(T o);
    }


    public partial class Player {
        public override void Deserialize() {
            Pathways.Deserialize<Player,Player.yml>(this); }

        public new class yml : adv::Person.yml, ISerializable<Player> {
            public List<string> deathMessages {get;set;}

            public void Deserialize(Player o) {
                Deserialize((adv::Person) o);
                o.deathMessages = new RandList<string>();
                o.deathMessages.AddRange(this.deathMessages);
            }
        }
    }

    namespace Adventure {

        public class Thing_yml : ISerializable<Thing> {
            public bool seen {get;set;}
            public string Name {get;set;}
            public lit::Description description {get;set;}

            public void ApplyDefaults<T>(Thing o) {
                var d = Pathways.yml.DeserializeDefault<Thing_yml>();
                o.Seen = d.seen;
                o.description = d.description;
            }

            public void Deserialize(Thing o) {
                ApplyDefaults<Thing>(o);
                o.Seen = this.seen;
                o.description.Name = o.Name;
                o.description = lit::Description.Merge(
                    this.description, o.description);
            }
        }


        public partial class Creature {

            public override void Deserialize() {
                Pathways.Deserialize<Creature,Creature.yml>(this); }

            public class yml : ISerializable<Creature> {//Thing_yml,
                public bool isDead {get;set;}

                public void Deserialize(Creature o) {
                    //Deserialize((Thing) o);
                    o.isDead = isDead;
                }
            }
        }

        public partial class Person {

            public override void Deserialize() {
                Pathways.Deserialize<Person,Person.yml>(this); }

            public new class yml : Creature.yml, ISerializable<Person> {
                public map::Area area {get;set;}
                public map::Room room {get;set;}

                public void Deserialize(Person o) {
                    Deserialize((Creature) o);
                    o.area = this.area;
                    o.room = this.room;
                }
            }
        }

        namespace Setting {
            public partial class Room {

                public override void Deserialize() {
                    Pathways.Deserialize<Room,Room.yml>(this); }

                public class yml : Thing_yml, ISerializable<Room> {
                    public int depth {get;set;}
                    public List<Thing> things {get;set;}
                    public List<Room> nearbyRooms {get;set;}

                    public void Deserialize(Room o) {
                        Deserialize((Thing) o);
                        o.things = this.things;
                        o.nearbyRooms = this.nearbyRooms;
                    }
                }
            }

            public partial class Area {

                public override void Deserialize() {
                    Pathways.Deserialize<Area,Area.yml>(this); }

                public class yml : Thing_yml, ISerializable<Area> {
                    public bool safe {get;set;}
                    public int level {get;set;}
                    public List<Room.yml> rooms {get;set;}
                    public List<Area.yml> areas {get;set;}

                    public void Deserialize(Area o) {
                        Deserialize((Thing) o);
                        o.safe = this.safe;
                        o.level = this.level;
                        //o.rooms = this.rooms;
                        //o.areas = this.areas;
                    }
                }
            }

            public partial class Door {

                public override void Deserialize() {
                    Pathways.Deserialize<Door,Door.yml>(this); }

                public class yml : Thing_yml, ISerializable<Door> {

                    [YamlMember(Alias="opened")]
                    public bool IsOpen {get;set;}

                    [YamlMember(Alias="initially opened")]
                    public bool IsInitOpen {get;set;}

                    [YamlMember(Alias="locked")]
                    public bool IsLocked {get;set;}

                    [YamlMember(Alias="lock message")]
                    public string LockMessage {get;set;}

                    public void ApplyDefaults(Door o) {
                        ApplyDefaults<Door>((Thing) o);
                        var d = Pathways.yml.DeserializeDefault<Door.yml>();
                        o.IsOpen = d.IsOpen;
                        o.IsInitOpen = d.IsInitOpen;
                        o.IsLocked = d.IsLocked;
                        o.LockMessage = d.LockMessage;
                    }

                    public void Deserialize(Door o) {
                        Deserialize((Thing) o);
                        o.IsOpen = this.IsOpen;
                        o.IsLocked = this.IsLocked;
                        o.IsInitOpen = this.IsInitOpen;
                        o.LockMessage = this.LockMessage;
                    }
                }
            }
        }
    }

    namespace Inventory {
        public partial class Item {

            public override void Deserialize() {
                Pathways.Deserialize<Item,Item.yml>(this); }

            public class yml : adv::Thing_yml, ISerializable<Item> {
                public int cost {get;set;}
                public float mass {get;set;}

                public void Deserialize(Item o) {
                    Deserialize((adv::Thing) o);
                    o.Mass = this.mass;
                    o.Cost = this.cost;
                }
            }
        }

        partial class Lamp {

            public override void Deserialize() {
                Pathways.Deserialize<Lamp,Lamp.yml>(this); }

            public new class yml : Item.yml, ISerializable<Lamp> {
                public float time {get;set;}

                public void Deserialize(Lamp o) {
                    Deserialize((Item) o);
                    o.time = time;
                }
            }
        }


        public partial class Key {

            public override void Deserialize() {
                Pathways.Deserialize<Key,Key.yml>(this); }

            public new class yml : Item.yml, ISerializable<Key> {
                [YamlMember(Alias="key type")]
                public Keys Kind {get;set;}

                [YamlMember(Alias="lock number")]
                public int Value {get;set;}

                public void Deserialize(Key o) {
                    Deserialize((Item) o);
                    o.Kind = this.Kind;
                    o.Value = this.Value;
                }
            }
        }

        partial class Book {

            public override void Deserialize() {
                Pathways.Deserialize<Book,Book.yml>(this); }

            public new class yml : adv::Thing_yml, ISerializable<Book> {
                public string passage {get;set;}

                public void ApplyDefaults(Book o) {
                    ApplyDefaults<Book>(o);
                    var d = Pathways.yml.DeserializeDefault<Book.yml>();
                    o.description = lit::Description.Merge(
                        o.description, d.description);

                }



                public void Deserialize(Book o) {
                    ApplyDefaults(o);
                    Deserialize((adv::Thing) o);
                    o.Passage = this.passage;
                }
            }
        }


        partial class Crystal {

            public override void Deserialize() {
                Pathways.Deserialize<Crystal,Crystal.yml>(this); }

            public new class yml : Item.yml, ISerializable<Crystal> {
                public uint shards {get;set;}

                public void Deserialize(Crystal o) {
                    Deserialize((Item) o);
                    o.Shards = this.shards;
                }
            }
        }

        public partial class Weapon {

            public override void Deserialize() {
                Pathways.Deserialize<Weapon,Weapon.yml>(this); }

            public new class yml : Item.yml, ISerializable<Weapon> {
                public float rate {get;set;}

                public void Deserialize(Weapon o) {
                    Deserialize((Item) o);
                    o.rate = rate;
                }
            }
        }
    }

    namespace Literature {

        public class Command_yml : IStorable {
            public string Name {get;set;}
            public Regex regex {get;set;}
            public ParserEvents parse {get;set;}


            /** `ParserEvents` : **`enum`**
            |*
            |* This local `enum` defines the `Parse` delegates
            |* that the `Parser` should call for each verb and
            |* command entered. This is awful.
            |*
            |* - `Sudo` : **`ParserEvents`**
            |*     `Pathways.Sudo` overrides commands
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
            |*     `Player.Look` looks around/examines a room
            |*
            |* - `Goto` : **`ParserEvents`**
            |*     `Player.Goto` sends the `Player` somewhere
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
            |*     `Player.Wear` has the player wear something
            |*
            |* - `Stow` : **`ParserEvents`**
            |*     `Player.Stow` has the player stow something
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
                Push, Pull, Show, Hide, Use }

            public Command Deserialize(string s) {
                this.Name = s;
                return new Command(
                    this.Name,
                    this.regex,
                    SelectParse(this.parse));
            }

            public void Deserialize(Command o) {
                o = new Command(
                    this.Name,
                    this.regex,
                    SelectParse(this.parse));
            }

            Parse SelectParse(ParserEvents e) {
                switch (e) {
                    case ParserEvents.Sudo :
                        return Pathways.Sudo;
                    case ParserEvents.Quit :
                        return Pathways.Quit;
                    case ParserEvents.Redo :
                        return Pathways.Redo;
                    case ParserEvents.Save :
                        return Pathways.Save;
                    case ParserEvents.Load :
                        return Pathways.Load;
                    case ParserEvents.Help :
                        return Pathways.Help;
                    case ParserEvents.View :
                        return Player.View;
                    //case ParserEvents.Look :
                    //    return Player.Look;
                    case ParserEvents.Goto :
                        return Player.Goto;
                    //case ParserEvents.Move :
                    //    return Player.Move;
                    //case ParserEvents.Invt :
                    //    return Player.Invt;
                    case ParserEvents.Take :
                        return Player.Take;
                    case ParserEvents.Drop :
                        return Player.Drop;
                    case ParserEvents.Use  :
                        return Player.Use;
                    case ParserEvents.Wear :
                        return Player.Wear;
                    case ParserEvents.Stow :
                        return Player.Stow;
                    case ParserEvents.Read :
                        return Player.Read;
                    case ParserEvents.Open :
                        return Player.Open;
                    case ParserEvents.Shut :
                        return Player.Shut;
                    case ParserEvents.Push :
                        return Player.Push;
                    case ParserEvents.Pull :
                        return Player.Pull;
                    case ParserEvents.Show :
                        return lit::Terminal.Show;
                    case ParserEvents.Hide :
                        return lit::Terminal.Hide;
                    default : return null;
                }
            }
        }



        public partial class Encounter {

            public override void Deserialize() {
                Pathways.Deserialize<Encounter,Encounter.yml>(this); }

            public class yml : adv::Thing_yml, ISerializable<Encounter> {
                public bool reuse {get;set;}
                public float time {get;set;}
                public Inputs input {get;set;}

                public void Deserialize(Encounter o) {
                    base.Deserialize((adv::Thing) o);
                    o.reuse = this.reuse;
                    o.time = this.time;
                    o.input = this.input;
                }
            }
        }
    }


    namespace Puzzle {

        abstract partial class Piece<T> : adv::Thing, IPiece<T> {

            public override void Deserialize() {
                Pathways.Deserialize<Piece<T>,Piece<T>.yml>(this); }

            public class yml : adv::Thing_yml, ISerializable<Piece<T>> {

                public T condition {get;set;}

                public T solution {get;set;}

                public void Deserialize(Piece<T> o) {
                    Deserialize((adv::Thing) o);
                    o.Condition = condition;
                    o.Solution = solution;
                }
            }
        }

        partial class Button : Piece<bool> {

            public override void Deserialize() {
                Pathways.Deserialize<Button,Button.yml>(this); }

            public new class yml : Piece<bool>.yml, ISerializable<Button> {

                public void Deserialize(Button o) {
                    Deserialize((Piece<bool>) o);
                }
            }
         }

        partial class Lever : adv::Thing, IPiece<int> {

            public override void Deserialize() {
                Pathways.Deserialize<Lever,Lever.yml>(this); }

            public class yml : adv::Thing_yml, ISerializable<Lever> {
                public bool IsSolved {get;set;}

                public void Deserialize(Lever o) {
                    Deserialize((adv::Thing) o);
                }
            }
        }
    }
}