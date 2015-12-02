/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-24 * Command */

using UnityEngine;
using System.Text.RegularExpressions;
using intf=PathwaysEngine.Adventure;

namespace PathwaysEngine {


    /** `Command` : **`struct`**
    |*
    |* Represents any `Player`-issued command.
    |**/
    public struct Command {
        public string uuid, input;
        public Regex regex { get; set; }
        public intf::Parse parse { get; set; }

        public Command(
                        string uuid,
                        Regex regex,
                        intf::Parse parse) {
            this.uuid = uuid;
            this.regex = regex;
            this.parse = parse;
            this.input = "";
        }

        public Command(
                        string uuid,
                        Regex regex,
                        intf::Parse parse,
                        string input)
            : this(uuid,regex,parse) { this.input = input; }

        public Command(
                        string uuid,
                        string regex,
                        intf::Parse parse)
            : this(uuid,new Regex(regex),parse) { }

        public Command(
                        string uuid,
                        string regex,
                        intf::Parse parse,
                        string input)
            : this(uuid,new Regex(regex),parse) { this.input = input; }

        public Command(
                        string uuid,
                        intf::Parse parse,
                        string regex)
            : this(uuid,new Regex(regex),parse) { }

        public Command(
                        string uuid,
                        intf::Parse parse,
                        string regex,
                        string input)
            : this(uuid,new Regex(regex),parse) { this.input = input; }
    }
}

