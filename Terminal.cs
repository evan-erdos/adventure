/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-11 * Terminal */

using UnityEngine;
using ui=UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Flags=System.FlagsAttribute;
using Buffer=System.Text.StringBuilder;
using intf=PathwaysEngine.Adventure;
using invt=PathwaysEngine.Inventory;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine {


    /** `Terminal` : **`class`**
    |*
    |* One of the most important classes in the whole engine.
    |* `Terminal` manages player input, and logs `Message`s
    |* from any and all sources through its `static` method
    |* `Log`, and all its overloads. This includes overloads
    |* for `Thing`s, `Message`s, etc.
    |**/
    [RequireComponent(typeof(RectTransform))]
    public class Terminal : MonoBehaviour {
        static bool wait = false, isLocked = false;
        static float initTime = 5f;
        static ui::InputField inputField;
        static ui::Text log;
        static Buffer buffer = new Buffer();
        public util::key term;
        public static RectTransform rect;
        public static Queue<string> logs;
        public ui::Scrollbar scrollbar;
        public ui::ScrollRect scrollRect;
        util::IActivator activator;


        /** `focus` : **`bool`**
        |*
        |* This property represents the state of the `Terminal`
        |* window, i.e., if it's `focus`sed, or not.
        |**/
        public static bool focus {
            get { return _focus; }
            set {
                _focus = value;
                rect.anchorMax = new Vector2(
                    rect.anchorMax.x,
                    (value)?(0.5f):(0.382f)); //@TODO: animate
                var color0 = Pathways.terminal
                    .GetComponent<ui::Image>().color;
                var color1 = new Color(
                    color0.r, color0.g, color0.b,
                    (value)?0.875f:0.618f); //@TODO: animate
                Pathways.terminal
                    .GetComponent<ui::Image>().color = color1;
                inputField.interactable = _focus;
                if (Pathways.terminal.scrollbar)
                    Pathways.terminal.scrollbar
                        .gameObject.SetActive(_focus);
                if (_focus) {
                    inputField.ActivateInputField();
                    inputField.Select();
                } else {
                    inputField.text = "";
                    inputField.DeactivateInputField();
                }
            }
        } static bool _focus = false;


        /** `prompt` : **`string`**
        |*
        |* This property represents the state of the `Terminal`
        |* window, i.e., if it's `focus`sed, or not.
        |**/
        public static string prompt {
            get { return string.Format(
                "{0:yyyy-MM-dd hh:mm} > ",Pathways.gameDate); } }


        /** `EventListener()` : **`event`**
        |*
        |* This property represents the state of the `Terminal`
        |* window, i.e., if it's `focus`sed, or not.
        |**/
        public void EventListener(
                object sender,
                System.EventArgs e,
                GameStates gameState) {
            focus = (gameState==GameStates.Term);
        }


        /** `LockLog()` : **`coroutine`**
        |*
        |* Stops the `Terminal.Log` from printing any new
        |* messages, usually useful in the first few frames of
        |* the game, when the startup information is visible.
        |**/
        IEnumerator LockLog(float t) {
            isLocked = true;
            yield return new WaitForSeconds(t);
            isLocked = false;
        }


        /** `Term()` : **`coroutine`**
        |*
        |* Stops the `Terminal.Log` from printing any new
        |* messages, usually useful in the first few frames of
        |* the game, when the startup information is visible.
        |**/
        IEnumerator Term() {
            wait = true;
            yield return new WaitForSeconds(0.125f);
            if (!activator.IsActivated)
                Terminal.Show(new Command());
            Pathways.GameState = GameStates.Term;
            wait = false;
        }

        void Awake() {
            Pathways.StateChange += new StateHandler(EventListener);
            term = new util::key((n)=> {
                if (!wait && n && Pathways.GameState!=GameStates.Term)
                    StartCoroutine(Term());
                term.input = n;});

            Pathways.terminal = this;
            rect = GetComponent<RectTransform>();
            inputField = GetComponentInChildren<ui::InputField>();
            foreach (var elem in GetComponentsInChildren<ui::Text>())
                if (elem.name=="Prompt") elem.text = prompt;
            log = GetComponentInChildren<ui::Text>();
            logs = new Queue<string>();
            activator = new util::TerminalActivator();
            activator.Initialize();

            scrollbar = GetComponentInChildren<ui::Scrollbar>();
            scrollRect = GetComponentInChildren<ui::ScrollRect>();
        }

        void Start() {
            isLocked = false;
            Clear();
            Pathways.Log("init");
            StartCoroutine(LockLog(initTime));
            //Hide(new Command());
        }

        void LateUpdate() {
            scrollRect.verticalNormalizedPosition = 0f;
        }


        /** `Clear()` : **`function`**
        |*
        |* This clears the entire `Terminal` log, and makes a
        |* new instance of the `buffer`, while `enqueue`-ing
        |* the old text into the `logs`.
        |**/
        public static void Clear() {
            if (isLocked) return;
            logs.Enqueue(log.text);
            buffer = new Buffer();
            log.text = "";
        }

        public static void Display(intf::Message m) {
            MessageWindow.Display(m); }

        public static void Log(string s="\n") {
            if (isLocked) return;
            buffer.Append(s);
            log.text = buffer.ToString();
        }

        public static void Log(params string[] lines) {
            if (isLocked) return;
            buffer.Append("\n");
            foreach (var elem in lines)
                buffer.Append(elem);
            log.text = buffer.ToString();
        }

        public static void Log(string s,params Formats[] f) {
            if (isLocked) return;
            buffer.Append(Format(s,f));
            log.text = buffer.ToString();
        }

        public static void Log(intf::Thing o) {
            if (isLocked) return;
            Log("\n");
            Log(o.description);
        }

        public static void Log(object o) {
            if (isLocked) return;
            Log(o.ToString());
        }

        public static string Format(string s, params Formats[] f) {
            if (s.Length<=0 || f==null || f.Length==0) return s;
            foreach (var elem in f) {
                switch (elem) {
                    case Formats.h1:
                    case Formats.h2:
                    case Formats.h3:
                    case Formats.h4:
                        s = string.Format(
                            "\n<size={0}>{1}</size>",elem,s);
                        goto case Formats.Newline;
                    case Formats.Inline:
                        if (s[s.Length-1]=='\n')
                            s = s.Remove(s.Length-1);
                        break;
                    case Formats.Paragraph:
                        if (s[s.Length-1]!='\n')
                            s = s+"\n";
                        goto case Formats.Newline;
                    case Formats.Newline:
                        if (s[0]!='\n')
                            s = "\n"+s;
                        break;
                    case Formats.State:
                    case Formats.Change:
                    case Formats.Alert:
                    case Formats.Command:
                        s = string.Format(
                            "<color=#{0:X}>{1}</color>",(int) elem,s);
                        break;
                }
            } return s;
        }

        public static void Log(intf::Message m) {
            if (m.formats==null
            || System.Array.IndexOf(m.formats,Formats.Alert)<0)
                Log(m.desc,m.formats);
            else Alert(m.desc,m.formats);
        }

        public static void Alert(string s) {
            Alert(s,Formats.Default); }

        public static void Alert(string s, params Formats[] f) {
            isLocked = false;
            Log(s,f); Pathways.GameState = GameStates.Term;
            isLocked = true;
        }

        public static void Alert(intf::Message m) {
            MessageWindow.Display(m); }

        public void CommandInput() {
            intf::Parser.eval(inputField.text); }

        public void CommandChange() {
            if (inputField.text.Contains("\t"))
                Pathways.GameState = GameStates.Game;
        }

        public static void Show(Command c) {
            Pathways.terminal.activator.Activate(); }

        public static void Hide(Command c) {
            Pathways.terminal.activator.Deactivate(); }

        public static void Resolve<T>(Command c, List<T> list) {
            Log("Which do you mean: ");
            foreach (var elem in list)
                Log("- "+elem,Formats.Command);
            Pathways.GameState = GameStates.Term;
        }
    }
}
