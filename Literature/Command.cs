/* Ben Scott * bescott@andrew.cmu.edu * 2015-12-05 * Command */

using System.Text.RegularExpressions;


namespace PathwaysEngine.Literature {


    /** `Command` : **`struct`**
     *
     * Represents any `Player`-issued command.
     **/
    public struct Command {
        public string uuid;
        public Regex regex {get;set;}
        public Parse parse {get;set;}

        public Command(
                        string uuid,
                        Regex regex,
                        Parse parse) {
            this.uuid = uuid;
            this.regex = regex;
            this.parse = parse;
        }

        public Command(
                        string uuid,
                        string regex,
                        Parse parse)
            : this(uuid,new Regex(regex),parse) { }

        public Command(
                        string uuid,
                        Parse parse,
                        string regex)
            : this(uuid,new Regex(regex),parse) { }
    }
}