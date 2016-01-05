/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-11 * Terminal */

using UnityEngine;
using ui=UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Flags=System.FlagsAttribute;
using Buffer=System.Text.StringBuilder;
using adv=PathwaysEngine.Adventure;
using inv=PathwaysEngine.Inventory;
using util=PathwaysEngine.Utilities;


namespace PathwaysEngine.Literature {


    /** `Terminal` : **`class`**
     *
     * One of the most important classes in the whole engine.
     * `Terminal` manages player input, and logs `Message`s
     * from any and all sources through its `static` method
     * `Log`, and all its overloads. This includes overloads
     * for `Thing`s, `Message`s, etc.
     **/
    [RequireComponent(typeof(RectTransform))]
    public class Terminal : MonoBehaviour {
        static bool wait = false, isLocked = false;
        static float time = 0.5f, initTime = 5f;
        static ui::InputField inputField;
        static ui::Text log;
        static Buffer buffer = new Buffer();
        public util::key term;
        public static RectTransform rect;
        public static Queue<string> logs = new Queue<string>();
        public static Queue<ILoggable> queue = new Queue<ILoggable>();
        public static ILoggable last;
        internal ui::Scrollbar scrollbar;
        internal ui::ScrollRect scrollRect;
        util::IActivator activator;


        /** `focus` : **`bool`**
         *
         * This property represents the state of the `Terminal`
         * window, i.e., if it's `focus`sed, or not.
         **/
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
        } static bool _focus;


        /** `prompt` : **`string`**
         *
         * This property represents the state of the `Terminal`
         * window, i.e., if it's `focus`sed, or not.
         **/
        public static string prompt =>
            $"{Pathways.gameDate:yyyy-MM-dd hh:mm} > ";


        /** `EventListener()` : **`event`**
         *
         * This property represents the state of the `Terminal`
         * window, i.e., if it's `focus`sed, or not.
         **/
        public void EventListener(
                object sender,
                System.EventArgs e,
                GameStates gameState) {
            focus = (gameState==GameStates.Term);
        }


        /** `LockLog` : **`coroutine`**
         *
         * Stops the `Terminal.Log` from printing any new
         * messages, usually useful in the first few frames of
         * the game, when the startup information is visible.
         **/
        IEnumerator LockLog(float t) {
            isLocked = true;
            yield return new WaitForSeconds(t);
            isLocked = false;
        }


        /** `Term` : **`coroutine`**
         *
         * Provides a time buffer when switching between states
         * using the terminal input key, usually **TAB**.
         **/
        IEnumerator Term() {
            wait = true;
            yield return new WaitForSeconds(0.125f);
            if (!activator.IsActivated)
                Terminal.Show(new Command());
            Pathways.GameState = GameStates.Term;
            wait = false;
        }


        /** `Logging` : **`coroutine`**
         *
         * Waits for `time` seconds between logging any objects
         * that were sent as `ILoggable`. Simple `string`s will
         * not be logged if `isLocked` is `true`.
         **/
        IEnumerator Logging() {
            for (;;) {
                if (queue.Count>0 && !isLocked)
                    LogImmediate(queue.Dequeue());
                yield return new WaitForSeconds(time);
            }
        }


        /** `Clear()` : **`function`**
         *
         * This clears the entire `Terminal` log, and makes a
         * new instance of the `buffer`, while `enqueue`-ing
         * the old text into the `logs`.
         **/
        public static void Clear() {
            logs.Enqueue(log.text);
            buffer = new Buffer();
            log.text = "";
        }

        public static void Log(string s="\n") =>
            Log(new Message(s,Styles.Default));

        public static void Log(params string[] lines) {
            var b = new Buffer();
            b.Append("\n");
            foreach (var elem in lines) b.Append(elem);
            Log(new Message(b.ToString(), Styles.Default));
        }

        public static void Log(string s,params Styles[] f) =>
            Log(new Message(s,f));

        public static void Log(ILoggable o) {
            if (o==null || o==last) return;
            //if (last!=null && o.Entry==last.Entry) return;
            queue.Enqueue(o);
            o = last;
        }

        public static void Log(
                        ICollection<ILoggable> list,
                        params Styles[] f) {
            if (list==null || list.Count<1) return;
            var sb = new Buffer();
            foreach(var elem in list)
                sb.AppendLine($" - {elem.Entry} ");
            LogImmediate(Terminal.Format(sb.ToString(),f));
        }

        public static void Log(ICollection<ILoggable> list) =>
            Log(list,Styles.Default);

        public static void Log(
                        ICollection<IDescribable> list,
                        params Styles[] f) {
            var temp = new List<ILoggable>();
            foreach (var elem in list)
                temp.Add((ILoggable) elem.description);
            Log(temp,f);
        }

        public static void Log(
                        ICollection<IDescribable> list) =>
            Log(list,Styles.Default);


        public static void Log(IDescribable o) =>
            Log((ILoggable) o.description);

        public static void LogCommand(string s) =>
            Log(new Message(s,Styles.Command));

        public static void LogImmediate(string s) {
            buffer.Append((TrailingNewline(buffer))
                ? "\n":"\n\n");
            buffer.Append(s);
            log.text = buffer.ToString();
        }

        public static void LogImmediate(ILoggable o) =>
            LogImmediate(o.Entry);

        public static void LogImmediate(
                        ICollection<ILoggable> list) {
            foreach(var elem in list)
                LogImmediate(elem);
        }

        public static void LogImmediate(
                        ICollection<IDescribable> list) {
            foreach(var elem in list)
                LogImmediate(elem.description.Entry);
        }

        static bool TrailingNewline(Buffer b) =>
            (b.Length>0 && b[b.Length-1]=='\n');

        public static string Format(string s, params Styles[] f) {
            if (s==null || s.Length<1) return s;
            if (f==null || f.Length<1) return s;
            foreach (var elem in f)
                if (elem==Styles.Command
                || elem==Styles.State
                || elem==Styles.Change
                || elem==Styles.Alert) {
                    s = $"<color=#{(int) elem:X}>{s.Trim()}</color>";
                    break; }

            foreach (var elem in f) {
                switch (elem) {
                    case Styles.h1:
                    case Styles.h2:
                    case Styles.h3:
                    case Styles.h4:
                        s = $"\n\n<size={elem}>{s.Trim()}</size>\n";
                        break;
                    case Styles.Inline:
                        s = s.Trim(); break;
                    case Styles.Paragraph:
                        s = $"\n\n{s.Trim()}";
                        break;
                    case Styles.Newline:
                        s = $"\n{s.Trim()}";
                        break;
                }
            } return s;
        }

        public static void Alert(string s) =>
            Alert(s,Styles.Alert);

        public static void Alert(string s, params Styles[] f) {
            isLocked = false;
            Log(s,f);
            Pathways.GameState = GameStates.Term;
            isLocked = true;
        }

        public static void Alert(Message m) {
            Pathways.GameState = GameStates.Msgs;
            Window.Display(m);
        }

        public static bool Show(Command c) {
            Pathways.terminal.activator.Activate();
            return true;
        }

        public static bool Hide(Command c) {
            Pathways.terminal.activator.Deactivate();
            return true;
        }

        // Unity Events

        void Awake() {
            Pathways.StateChange += EventListener;
            term = new util::key((n)=> {
                if (!wait && n && !focus
                && Pathways.GameState!=GameStates.Term)
                    StartCoroutine(Term());
                term.input = n;});

            Pathways.terminal = this;
            rect = GetComponent<RectTransform>();
            inputField = GetComponentInChildren<ui::InputField>();
            foreach (var elem in GetComponentsInChildren<ui::Text>())
                if (elem.name=="Prompt") elem.text = prompt;
            log = GetComponentInChildren<ui::Text>();
            activator = new util::TerminalActivator();
            activator.Initialize();
            scrollbar = GetComponentInChildren<ui::Scrollbar>();
            scrollRect = GetComponentInChildren<ui::ScrollRect>();
        }

        void Start() {
            isLocked = false;
            Clear();
            StopAllCoroutines();
            LogImmediate($@"
## {Pathways.Config.Title} v{Pathways.Config.Version} ##
© {Pathways.Config.Date}, {Pathways.Config.Author} <{Pathways.Config.Email}>
{Pathways.Config.Link}");
            LogImmediate(Pathways.messages["init"]);
            StartCoroutine(Logging());
            StartCoroutine(LockLog(initTime));
            Hide(new Command());
        }

        void LateUpdate() {
            if (Pathways.GameState!=GameStates.Term)
                scrollRect.verticalNormalizedPosition = 0f; }

        public void CommandInput() =>
            Parser.Evaluate(inputField.text);

        public void CommandChange() {
            if (inputField.text.Contains("\t")) {
                Pathways.GameState = GameStates.Game;
                focus = false;
            }
        }
    }
}
