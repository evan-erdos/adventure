/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-11 * Parser */

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EventArgs=System.EventArgs;
using Buffer=System.Text.StringBuilder;
using adv=PathwaysEngine.Adventure;


namespace PathwaysEngine.Literature {


    /** `Parser` : **`class`**
     *
     * Main class for dealing with natural language commands,
     * the verbs that can be used on their particular "nouns",
     * and all manner of other user-issued commands.
     **/
    public static class Parser {

        static bool interceptNext;
        static Command lastCommand;
        static IList<adv::Thing> options;

        static RandList<string> confused = new RandList<string> {
            "I don't get it. ",
            "Do what, exactly? ",
            "That's just a bunch of nonsense. ",
            "I didn't understand that. ",
            "You will have to tell me just how you intend to pull that off. " };

        static readonly Dictionary<string,Command> commands =
            new Dictionary<string,Command>();


        /** `Parser` : **`constructor`**
         *
         * Initializes all the commands and their regexes
         * into a dictionary.
         **/
        static Parser() {
            commands = Pathways.commands;
        }


        /** `Process()` : **`string[]`**
         *
         * Input taken directly from the user cannot be used to
         * issue `Command`s without being organized first. This
         * function takes the raw input `string`, and returns a
         * more rigorously organized structure.
         *
         * - `s` : **`string`**
         *     raw user input from the `Terminal`
         **/
        static List<string> Process(string s) {
            var raw = new List<string>(s.Strip().Split('.'));
            var list = new List<string>();
            foreach (var elem in raw)
                if (!string.IsNullOrEmpty(elem))
                    list.Add(elem);
            return list;
        }

        public static bool Failure(string input, string s) {
            if (input.Length>0) {
                Terminal.Log(
                    $@"<cmd> \> **{input}**:</cmd> <warn>{s}</warn>");
                interceptNext = false;
                Pathways.GameState = Pathways.LastState;
                return true;
            } return false;
        }

        public static bool Failure(string s) =>
            Failure(s,confused.Next());

        public static bool Intercept(
                        Command c,
                        IList<adv::Thing> list) {
            options = list;
            lastCommand = c;
            interceptNext = true;
            return true;
        }


        public static bool Intercept(Command c, string s) {
            var input = Process(s);
            if (input.Count<1)
                return Failure(s,"You don't see that here.");
            int n;
            if (!System.Int32.TryParse(input[0], out n))
                return Failure(s,
                    "I couldn't figure out what you meant.");
            if (0<n && n<options.Count)
                return Execute(c,options[n].Name);
            return Failure(s);
        }


        public static bool Intercept(string s) =>
            Intercept(lastCommand,s);

        //public static bool Report(Command c) { }


        /** `Execute()` : **`function`**
         *
         * When a command is parsed in and `Evaluate()`d, it is
         * sent here, and a `Command` is created, dispatched to
         * its `Parse` function for processing, and in the case
         * of a `TextException`, it is `Resolve()`d, such that
         * an appropriate action might be taken. Any kind of
         * text command `Exception` ends here, as they are used
         * only for indicating errors in game logic, not errors
         * relating to anything actually wrong with the code.
         *
         * - `c` : **`Command`**
         *     the command struct without input        *
         *
         * - `s` : **`string`**
         *     the raw, user-issued command
         *
         * - `throw` : **`TextException`**
         *     thrown when command is incoherent. It is caught
         *     locally, and the default behaviour is taken.
         **/
        public static bool Execute(
                        Command c,
                        string input) {
            try {
                if (!string.IsNullOrEmpty(input.Trim()))
                    Terminal.LogImmediate(
                        new Message(
                            $@" \> **{input}**: ",
                            Styles.Command));
                var result = c.parse(
                    null,EventArgs.Empty,c,input);
                interceptNext = false;
                Pathways.GameState = Pathways.LastState;
                return result;
            } catch (AmbiguityException<adv::Thing> e) {
                var sb = new Buffer();
                sb.AppendLine(e.Message);
                foreach (var elem in e.options)
                    sb.AppendLine($"- {elem.Name} ");
                Terminal.LogImmediate(Terminal.Format(
                    sb.ToString(),Styles.Command));
                if (interceptNext)
                    return Failure(input,"I couldn't determine what you meant.");
                    //throw e.InnerException;
                Pathways.GameState = GameStates.Term;
                Terminal.focus = true;
                return Intercept(c,e.options);
            } catch (TextException e) {
                return Failure(input,e.Message); }
        }


        /** `Evaluate()` : **`function`**
         *
         * Parses the sent `string`, creates a `Command`
         * and dispatches it to its `Parse` function for
         * processing.
         **/
        public static bool Evaluate(string s) {
            if (interceptNext) return Intercept(s);
            foreach (var elem in Process(s))
                foreach (var kvp in commands.Values)
                    if (kvp.regex.IsMatch(elem))
                        return Execute(kvp,elem);
            return Failure(s);
        }


        /** `Resolve<T>()` : **`function`**
         *
         * When a `Command` is ambiguous or doesn't make any
         * sense, this prompts the user for some explanation.
         **/
        public static void Resolve<T>(Command c,List<T> list) {
            Terminal.Log("<cmd>Which do you mean:</cmd>");
            foreach (var elem in list)
                Terminal.Log($"<cmd>-</cmd> {elem}");
            Pathways.GameState = GameStates.Term;
        }
    }
}


