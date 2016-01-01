/* Ben Scott * bescott@andrew.cmu.edu * 2015-12-05 * Message */

using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace PathwaysEngine.Literature {


    /** `Message` : `struct`
     *
     * Represents any message that can be formatted and sent to
     * the `Terminal`, and can be serialized via `IStorable`.
     **/
    public struct Message : IStorable, ILoggable {

        public string Name {get;set;}

		[YamlMember(Alias="desc")]
        public string Entry {
            get { return desc; }
            set { desc = Terminal.Format(value.md(),styles); }
        } string desc;

		//[YamlMember(Alias="template")]
        public string Template {get;set;}

		//[YamlMember(Alias="styles")]
        public Styles[] styles {get;set;}

        public Message(
                        string name,
                        string Entry,
                        params Styles[] styles) {
            this.Name = name;
            this.styles = styles;
            this.Entry = Entry;
        }

        public Message(string name)
            : this(name,"",Styles.Default) { }

        public Message(string Entry, params Styles[] styles)
            : this("",Entry,styles) { }

        public Message(string name, string Entry)
            : this(name,Entry,Styles.Default) { }


        public string Log() { return Entry; }

        public override string ToString() { return desc; }
    }
}
