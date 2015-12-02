/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-11 * Parser */

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Buffer=System.Text.StringBuilder;

namespace PathwaysEngine.Adventure {

    /** `Parser` : **`class`**
    |*
    |* Main class for dealing with natural language commands,
    |* the verbs that can be used on their particular "nouns",
    |* and all manner of other user-issued commands.
    |**/
    public static class Parser {

        public static readonly Dictionary<string,Command> commands;

        /** `Parser` : **`constructor`**
        |*
        |* Initializes all the commands and their regexes
        |* into a dictionary.
        |*
        |* @TODO: Get this out of here, serialize the list
        |**/
        static Parser() {
            var ymlCommands = Pathways.GetYamlFile<Dictionary<string,Command_yml>>("commands");
            var _commands = new Dictionary<string,Command>();
            foreach (var kvp in ymlCommands)
                _commands[kvp.Key] = kvp.Value.Deserialize(kvp.Key);
            commands = _commands;
        }

        /** `eval()` : **`function`**
        |*
        |* Parses the sent `string`, creates a `Command`
        |* and dispatches it to its `Parse` function for
        |* processing.
        |**/
        public static void eval(string s) {
            foreach (var elem in s.Process()) {
                if (string.IsNullOrEmpty(elem)) continue;
                foreach (var kvp in commands.Values) {
                    if (kvp.regex.IsMatch(elem)) {
                        Pathways.GameState = GameStates.Game;
                        //Debug.Log(kvp.uuid+" : "+kvp.parse);
                        kvp.parse(new Command(
                            kvp.uuid,kvp.regex,kvp.parse,elem));
                        return;
                    }
                } Terminal.Log(string.Format(
                    " > {0}: Do what, exactly?\n",s),
                    Formats.Command);
                Pathways.GameState = GameStates.Game;
            }
        }
    }
}


