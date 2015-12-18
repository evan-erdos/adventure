/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-04 * Description */

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using type=System.Type;
using YamlDotNet.Serialization;

namespace PathwaysEngine.Literature {

    public class Description : ILoggable {
        public bool Seen {
            get { return seen; }
            set { seen = value;
                if (seen) Refresh();
            }
        } bool seen;

        [YamlMember(Alias="name")]
        public string Name {get;set;}

        [YamlMember(Alias="template")]
        public virtual string Template {
            get { return template; }
            set { RefreshTemplate(value); }
        } string template = "### {0} ###\n{1}\n\n{2}";


        [YamlMember(Alias="desc")]
        public string Entry {
            get { return desc; }
            set { Refresh(value); }
        } protected string desc, raw;

        [YamlMember(Alias="initial description")]
        public string init {get;set;}

        //[YamlMember(Alias="help")]
        public string help {get;set;}

        [YamlMember(Alias="styles")]
        public Styles[] styles {get;set;}

        [YamlMember(Alias="nouns")]
        public Regex Nouns {
            get { return nouns; }
            set { nouns = new Regex(string.Format(
                (Name!=null)?"({0})|{1}":"{1}",
                    Name,value)); }
        } Regex nouns;

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


        public void Refresh() { Refresh(raw); }

        void Refresh(string s) {
            raw = s;
            desc = Terminal.Format(string.Format(
                Template,Name,s,
                    (init!=null && !Seen)
                        ? (init)
                        : ("")).md(),styles);
            if (help!=null)
                desc += Terminal.Format(help.md(),
                    Styles.Command);
        }

        void RefreshTemplate(string s) {
            template = s
                .Replace("name","0").Replace("desc","1")
                .Replace("init","2").Replace("help","3")
                .Replace("{").Replace("}");
        }

        public bool Fits(string s) {
            return nouns.IsMatch(s); }

        public bool Fits(Command c) {
            return nouns.IsMatch(c.input); }


        /** `Merge()` : **`Description`**
        |*
        |* A static function to ensure that underspecified
        |* description objects don't remove default values, but
        |* do overwrite anything that they have defined.
        |*
        |* - `d0` : **`Description`**
        |*     description to merge into
        |*
        |* - `d1` : **`Description`**
        |*     description to populate from
        |**/
        public static Description Merge(
                        Description d0,
                        Description d1) {
            if (d0==null && d1!=null) return d1;
            if (d0!=null && d1==null) return d1;
            if (d0==null && d1==null) return null;
            if (d0.template==null) d0.template = d1.template;
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
            if (d1.template!=null) d0.template = d1.template;
            if (d1.Name!=null) d0.Name = d1.Name;
            if (d1.init!=null) d0.init = d1.init;
            if (d1.help!=null) d0.help = d1.help;
            if (d1.nouns!=null) d0.nouns = d1.nouns;
            if (d1.rand!=null) d0.rand = d1.rand;
            if (d1.Entry!=null) d0.Entry = d1.raw;
            d0.Refresh();
            return d0;
        }

        public virtual string Log() { return Entry; }

        public override string ToString() { return desc; }

        public static explicit operator string(Description d) {
            return d.Entry; }
    }

    class Description<T> : Description
                 where T : ILoggable {

        public override string Log() {
            return string.Format(Template,desc); }
    }
}
