/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-24 * Pathways */

using UnityEngine; // Well, here we are! The main file!
using UnityEngine.SceneManagement; // The big one!
using System.Linq; // It's the Pathways Engine!
using System.Collections; // just like I pictured it!
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DateTime=System.DateTime;
using EventArgs=System.EventArgs;
using inv=PathwaysEngine.Inventory;
using adv=PathwaysEngine.Adventure;
using lit=PathwaysEngine.Literature;
using maps=PathwaysEngine.Adventure.Setting;
using stat=PathwaysEngine.Statistics;
using util=PathwaysEngine.Utilities;


/** `PathwaysEngine` : **`namespace`**
|*
|* Global namespace which contains the most important classes
|* and any class which requires interaction between any nested
|* namespaces. This file contains most of the `interfaces`,
|* `enums`, & other loose bits that don't belong anywhere else.
|* Also contains direct references to the `Player`, `Terminal`,
|* and any other extremely important, global / static class.
|**/
namespace PathwaysEngine {


    /** `GameStates` : **`enum`**
    |*
    |* This enumerates the global states that the engine can
    |* have. Switching between them has a considerable number
    |* of side effects, like hiding / showing the mouse, or
    |* enabling / disabling UI windows & movement / input.
    |*
    |* - `None` : **`GameStates`**
    |*     Represents no state, precludes `Player` motion & all
    |*     inputs. Will possibly be used in cases like loading
    |*     scenes & whatnot.
    |*
    |* - `Game` : **`GameStates`**
    |*     State for usual gameplay, all motion input is
    |*     permitted, & the `Terminal` UI, `Window` UI, &
    |*     obviously the main menu are disabled.
    |*
    |* - `View` : **`GameStates`**
    |*     Used for special areas and encounters that can stop
    |*     `Player` motion and camera movement, and also frees
    |*     the mouse such that the `Player` can make selections
    |*     more easily and interact with small buttons, etc.
    |*
    |* - `Term` : **`GameStates`**
    |*     State represents when the `Terminal` is opened, &
    |*     the user is either entering input or dealing with
    |*     some aspect of its function, e.g., when checking the
    |*     `Player`'s inventory, or when resolving a command.
    |*
    |* - `Msgs` : **`GameStates`**
    |*     State represents that `Window` is opened, & the
    |*     `Player` is reading the message. This state disables
    |*     Player` movement, hides the `Terminal`, & the mouse.
    |*
    |* - `Menu` : **`GameStates`**
    |*     Active when the `Menu` is active. Disables almost
    |*     all components & inputs, and shows the `Menu` UI.
    |**/
    public enum GameStates { None, Game, View, Term, Msgs, Menu }


    /** `Cursors` : **`enum`**
    |*
    |* This enumerates the possible cursor graphics, e.g., the
    |* indexes of each respective image in the array.
    |**/
    public enum Cursors : int {
        None = 0, Pick = 1, Hand = 2, Grab = 3,
        Look = 4, Wait = 5, Back = 6 };


    /** `StateHandler` : **`delegate`**
    |*
    |* This is a global event delegate for dealing with changes
    |* to the overall game state. Subscribers are sent the
    |* `gameState`, and do with it whatever they please.
    |*
    |* - `sender` : **`object`**
    |*     reference to whatever sent the event
    |*
    |* - `e` : **`EventArgs`**
    |*     default event arguments
    |*
    |* - `gameState` : **`GameStates`**
    |*     game state to change to
    |**/
    delegate void StateHandler(
                    object sender,
                    System.EventArgs e,
                    GameStates gameState);


    /** `Pathways` : **`main`**
    |*
    |* This is the main class for the entire engine. While it
    |* does have a lot of global states, some of them are the
    |* only way I can see to deal with the complex interactions
    |* between the many subsystems in the engine. While having
    |* any sort of global state is usually bad, using it in a
    |* limited manner can help the entire engine's cohesion, as
    |* well as the organization of these different subsystems.
    |**/
    public static partial class Pathways {
        public static DateTime gameDate, finalDate;
        public static Camera mainCamera;
        public static Player player;
        public static lit::Terminal terminal;
        public static lit::Window window;
        public static CursorMode cursorMode = CursorMode.ForceSoftware;
        public static List<maps::Area> areas;
        public static Texture2D[] cursors;
        public static Vector2 hotSpot = new Vector2(32,32);

        internal static event StateHandler StateChange;


        /** `GameState` : **`GameStates`**
        |*
        |* Changes the global state of the engine, and has many
        |* side effects, such as changing the mouse visiblility
        |* and movement inputs, UI visibility, etc.
        |**/
        public static GameStates GameState {
            get { return gameState; }
            set { if (gameState==value) return;
                lastState = gameState;
                gameState = value;
                StateChange(null,System.EventArgs.Empty,gameState);
            }
        } static GameStates gameState = GameStates.None;


        /** `LastState` : **`GameStates`**
        |*
        |* Records what the state was last frame. Useful when
        |* trying to sort out if / what actions to take when
        |* changing between states.
        |**/
        public static GameStates LastState {
            get { return lastState; }
        } static GameStates lastState = GameStates.Game;


        /** `CursorGraphic` : **`Cursors`**
        |*
        |* This property changes the currently displayed cursor
        |* graphic only if the `enum` assigned is different.
        |**/
        public static Cursors CursorGraphic {
            get { return cursorGraphic; }
            set { if (cursorGraphic==value) return;
                cursorGraphic = value;
                var cursor = cursors[(int) cursorGraphic];
                Cursor.visible = (cursor!=null);
                Cursor.SetCursor(cursor, Vector2.zero, cursorMode);
            }
        } static Cursors cursorGraphic;


        /** `Pathways` : **`constructor`**
        |*
        |* Initializes some important global variables before
        |* most other classes / `MonoBehaviour`s are created,
        |* and well before any call that `UnityEngine` makes to
        |* `MonoBehaviour.Awake` or any other event.
        |**/
        static Pathways() {
            gameDate = new DateTime(1994,5,8,2,0,0);
            finalDate = new DateTime(1994,5,13,14,0,0);

            StateChange += new StateHandler(EventListener);

            cursors = new Texture2D[7];
            cursors[(int) Cursors.Pick] =
                Resources.Load("pick") as Texture2D;
            cursors[(int) Cursors.Hand] =
                Resources.Load("hand") as Texture2D;
            cursors[(int) Cursors.Grab] =
                Resources.Load("grab") as Texture2D;
            cursors[(int) Cursors.Look] =
                Resources.Load("look") as Texture2D;
            cursors[(int) Cursors.Back] =
                Resources.Load("back") as Texture2D;
        }

        /** `EventListener()` : **`function`**
        |*
        |* This is the main multicast `delegate` for changes
        |* made to the global state of the game. Lots of other
        |* classes subscribe to this event via `EventListener`.
        |*
        |* - `sender` : **`object`**
        |*     Included in case I want any kind of callback.
        |*
        |* - `e` : **`EventArgs`**
        |*     Standard event class, included for consistency.
        |*
        |* - `state` : **`GameStates`**
        |*     Gamestate to change to.
        |**/
        public static void EventListener(
                        object sender,
                        System.EventArgs e,
                        GameStates state) {
            Cursor.visible = (state==GameStates.Msgs
                || state==GameStates.Menu
                || state==GameStates.View);

            Cursor.lockState = (state==GameStates.Game)
                ? (CursorLockMode.Locked)
                : (CursorLockMode.Confined);
        }


        public static void Reset() {

        }


        /** `Sudo()` : **`Parse`**
        |*
        |* For special user commands. Unused, so far.
        |*
        |* - `c` : **`Command`**
        |*     Default `Command` struct, sometimes unused, but
        |*     means that this function is a `Parse` delegate.
        |**/
        public static bool Sudo(lit::Command c) { return true; }


        /** `Redo()` : **`Parse`**
        |*
        |* Runs the prior command issued to the `Parser` again.
        |*
        |* - `c` : **`Command`**
        |*     Default `Command` struct, sometimes unused, but
        |*     means that this function is a `Parse` delegate.
        |**/
        public static bool Redo(lit::Command c) { return true; }
            //adv::Parser.eval(c.input); } infinite loop!


        /** `Quit()` : **`Parse`**
        |*
        |* Prompts user through `Terminal` to quit the game.
        |*
        |* - `c` : **`command`**
        |*     Default `command` struct, sometimes unused, but
        |*     means that this function is a `Parse` delegate.
        |**/
        public static bool Quit(lit::Command c) {
            lit::Terminal.Alert(messages["quit"]);
            return true;
        }


        /** `Load()` : **`Parse`**
        |*
        |* Loads a game from a `*.yml` file. Currently broken.
        |*
        |* - `c` : **`Command`**
        |*     Default `Command` struct, sometimes unused, but
        |*     means that this function is a `Parse` delegate.
        |**/
        public static bool Load(lit::Command c) {
            lit::Terminal.Clear();
            lit::Terminal.LogImmediate("I/O Disabled, restarting level.");
            SceneManager.LoadScene(0);
            return true;
        }


        /** `Save()` : **`Parse`**
        |*
        |* Saves a game from a `*.yml` file. Currently broken.
        |*
        |* - `c` : **`Command`**
        |*     Default `Command` struct, sometimes unused, but
        |*     means that this function is a `Parse` delegate.
        |**/
        public static bool Save(lit::Command c) {
            if (File.Exists(c.input))
                lit::Terminal.Alert("Overwriting file...");
            using (StreamWriter file = new StreamWriter(c.input)) {
                file.WriteLine("%YAML 1.1");
                file.WriteLine("%TAG !invt! _PathwaysEngine.Inventory.");
                file.WriteLine("%TAG !maps! _PathwaysEngine.Adventure.Setting.");
                file.WriteLine(string.Format("---  # {0}\n",
                    new P_ID("Saved_Game_00000",DateTime.Now)));
                file.WriteLine("player:");
                file.WriteLine(string.Format("  position: {0}",
                    new Vector3(
                        Player.Position.x,
                        Player.Position.y,
                        Player.Position.z)));
                file.WriteLine(string.Format(
                    "  area: {0}",Player.area));
                file.WriteLine(string.Format(
                    "  room: {0}",Player.Room));
                file.WriteLine("  items:\n");
                foreach (var elem in Player.Items)
                    file.WriteLine(string.Format("  - {0}",elem.Name));
                file.WriteLine("\n...\n");
            } return true;
        } /*(s) => (s.Length>100)?(s.Substring(0,100)+"&hellip;"):(s); */


        /** `Help()` : **`Parse`**
        |*
        |* Shows the help menu via `Window`.
        |*
        |* - `c` : **`Command`**
        |*     Default `Command` struct, sometimes unused, but
        |*     means that this function is a `Parse` delegate.
        |**/
        public static bool Help(lit::Command c) {
            lit::Window.Display(messages["help"]);
            return true;
        }
    }

    /** `P_ID` : **`struct`**
    |*
    |* **Deprecated**
    |*
    |* Attempt to formalize the `uuid`s from the serialization
    |* process.
    |*
    |* - `name` : **`string`**
    |*     Name of the `P_ID` to be used when serializing.
    |*
    |* - `date` : **`DateTime`**
    |*     Date to use, to keep all `P_ID`s unique when
    |*     serializing a saved game.
    |**/
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
}




namespace dev {

/* quick test
    System.Type type = typeof(adv::Door.yml);
    Debug.Log("Full: "+type.Assembly.FullName.ToString());
    Debug.Log("Qual: "+type.AssemblyQualifiedName.ToString());
*/

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
//          fuv[i].x=fn[i].x;fuv[i].y=fn[i].y;}

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


