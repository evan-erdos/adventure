/* Ben Scott * bescott@andrew.cmu.edu * 2015-01-01 * Description */

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using type=System.Type;
using YamlDotNet.Serialization;


namespace PathwaysEngine.Literature {


    /** `Description` : **`ILoggable`**
     *
     * Deals with most of the functions of describable things.
     * It stores a name, some adjectives and nouns which can be
     * used to refer to stuff (by way of a `Regex`), along with
     * some style options, and another string which serves as a
     * formatting template for the other elements.
     **/
    public class Description : ILoggable {


        /** `Seen` : **`bool`**
         *
         * Used to determine if this instance has been seen by
         * the `Player` before, to potentially alter the text
         * it renders when it is `Log()`-ed thereafter.
         **/
        public bool Seen {
            get { return seen; }
            set { seen = value;
                if (seen) Refresh();
            }
        } bool seen;


        /** `Name` : **`string`**
         *
         * Simple UUID such that **`Description`**s can be put
         * into `*.yml` files.
         **/
        [YamlMember(Alias="name")]
        public string Name {get;set;}


        /** `Template` : **`string`**
         *
         * A string to be used to apply `*.md` formatting to
         * the rendered output of `Entry`.
         **/
        [YamlMember(Alias="template")]
        public virtual string Template => $@"
### {Name} ###
{init}{raw}

{Help}";


        /** `Entry` : **`string`**
         *
         * Stores the rendered `*.md` string behind the scenes,
         * and keeps track of the raw input as well.
         **/
        [YamlMember(Alias="desc")]
        public string Entry {
            get { return desc; }
            set { Refresh(value); }
        } protected string desc;

        public string raw;

        /** `init` : **`string`**
         *
         * An optional string to be appended before the main
         * text (or wherever else the `Template` specifies).
         **/
        [YamlMember(Alias="initial description")]
        public string init {get;set;}


        /** `help` : **`string`**
         *
         * A specially formatted help text which can be added
         * to the description if the user appears to be dumb.
         **/
        [YamlMember(Alias="help")]
        public string Help {
            get { return (!Seen)?$"<help>{help}</help>":""; }
            set { help = value; }
        } string help;

        /** `styles` : **`Styles[]`**
         *
         * An array of formatting styles to be applied to the
         * `Entry` before being `Log()`-ed.
         **/
        //[YamlMember(Alias="styles")]
        public Styles[] styles {get;set;}


        /** `Nouns` : **`/regex/`**
         *
         * A `regex` to be matched against user input, which
         * acts as a dynamic identifier for the object.
         **/
        [YamlMember(Alias="nouns")]
        public Regex Nouns {
            get { return nouns; }
            set { nouns = new Regex((Name!=null)
                ?$"({Name})|{value}":$"{value}"); }
        } Regex nouns;


        /** `rand` : **`string[]`**
         *
         * A list of `string`s which will be chosen at random
         * and inserted into the rendered output.
         **/
        [YamlMember(Alias="random descriptions")]
        public RandList<string> rand {get;set;}


        public bool IsMatch(string s) {
            return nouns.IsMatch(s); }


        public Description() : this(" ") { }

        internal Description(string desc) : this(desc," ") { }

        internal Description(string desc,string init)
            : this(desc,init,new Regex(" ")) { }

        internal Description(
                        string desc,
                        string init,
                        string nouns)
            : this(desc,init,new Regex(nouns)) { }

        internal Description(
                        string desc,
                        string init,
                        Regex nouns)
            : this(desc,init,nouns,Styles.Default) { }

        internal Description(
                string desc,
                string init,
                params Styles[] styles)
            : this(desc,init,new Regex(" "),styles) { }

        internal Description(
                string desc,
                string init,
                Regex nouns,
                params Styles[] styles) {
            this.desc = desc;
            this.init = init;
            this.nouns = nouns;
            this.styles = styles;
        }


        /** `Refresh()` : **`function`**
         *
         * Re-applies the formatting to the `Entry`, as some
         * of the variables may have changed since the last
         * time the formatting was applied.
         **/
        public void Refresh() => Refresh(raw);

        void Refresh(string s) {
            raw = s;
            desc = Terminal.Format(Template.md(),styles);
        }


        public bool Fits(string s) => nouns.IsMatch(s);


        /** `Merge()` : **`Description`**
         *
         * A static function to ensure that underspecified
         * description objects don't remove default values, but
         * do overwrite anything that they have defined.
         *
         * - `d0` : **`Description`**
         *     description to merge into
         *
         * - `d1` : **`Description`**
         *     description to populate from
         **/
        public static Description Merge(
                        Description d0,
                        Description d1) {
            if (d0==null && d1!=null) return d1;
            if (d0!=null && d1==null) return d1;
            if (d0==null && d1==null) return null;
            if (d0.Name==null) d0.Name = d1.Name;
            if (d0.init==null) d0.init = d1.init;
            if (d0.help==null) d0.help = d1.help;
            if (d0.nouns==null) d0.nouns = d1.nouns;
            if (d0.rand==null) d0.rand = d1.rand;
            if (d0.Entry==null) d0.Entry = d1.raw;
            d0.Refresh();
            return d0;
        }

        public static Description Onto(
                        Description d0,
                        Description d1) {
            if (d0==null && d1!=null) return d1;
            if (d0!=null && d1==null) return d1;
            if (d0==null && d1==null) return null;
            if (d1.Name!=null) d0.Name = d1.Name;
            if (d1.init!=null) d0.init = d1.init;
            if (d1.help!=null) d0.help = d1.help;
            if (d1.nouns!=null) d0.nouns = d1.nouns;
            if (d1.rand!=null) d0.rand = d1.rand;
            if (d1.Entry!=null) d0.Entry = d1.raw;
            d0.Refresh();
            return d0;
        }

        public virtual string Log() => Entry;

        public override string ToString() => desc;

        public static explicit operator string(Description d) =>
            d.Entry;
    }

    class Description<T> : Description
                 where T : ILoggable {

        public override string Log() => Template;
    }
}
