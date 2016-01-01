/* Ben Scott * bescott@andrew.cmu.edu * 2015-12-05 * Command */

using System.Text.RegularExpressions;

namespace PathwaysEngine.Literature {


    /** `Command` : **`struct`**
     *
     * Represents any `Player`-issued command.
     **/
    public struct Command {
        public string uuid, input;
        public Regex regex {get;set;}
        public Parse parse {get;set;}

        public Command(
                        string uuid,
                        Regex regex,
                        Parse parse,
                        string input) {
            this.uuid = uuid;
            this.regex = regex;
            this.parse = parse;
            this.input = input;
        }

        public Command(
                        string uuid,
                        Regex regex,
                        Parse parse)
            : this(uuid,regex,parse,"") { }

        public Command(
                        string uuid,
                        string regex,
                        Parse parse)
            : this(uuid,new Regex(regex),parse,"") { }

        public Command(
                        string uuid,
                        string regex,
                        Parse parse,
                        string input)
            : this(uuid,new Regex(regex),parse,input) { }

        public Command(
                        string uuid,
                        Parse parse,
                        string regex)
            : this(uuid,new Regex(regex),parse) { }

        public Command(
                        string uuid,
                        Parse parse,
                        string regex,
                        string input)
            : this(uuid,new Regex(regex),parse,input) { }


        public bool Fits(Description d) {
            return d.Fits(this); }

        public bool Fits(IDescribable d) {
            return Fits(d.description); }
    }
}