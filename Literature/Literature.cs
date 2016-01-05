/* Ben Scott * bescott@andrew.cmu.edu * 2015-12-03 * Literature */

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using Type=System.Type;
using EventArgs=System.EventArgs;
using Buffer=System.Text.StringBuilder;
using inv=PathwaysEngine.Inventory;
using adv=PathwaysEngine.Adventure;
using maps=PathwaysEngine.Adventure.Setting;
using stat=PathwaysEngine.Statistics;
using util=PathwaysEngine.Utilities;


/** `Literature` : **`namespace`**
 *
 * A somewhat pretentiously monikered namespace which contains
 * any and all things related to text and language. Of note, it
 * holds the `Parser`, the `Terminal`, and `Description`s. Most
 * classes in the engine have some sort of representation here,
 * and most can be written to the `Terminal` via `Log()`, or to
 * `Window` via `Display()`, usually in the form of `struct`s
 * like `Message`or `Description`.
 **/
namespace PathwaysEngine.Literature {


    /** `Styles` : **`enum`**
     *
     * This enumerates the various formatting options that the
     * `Terminal` can use. Most values have some meaning, which
     * are used by the `Terminal.Format` function. They might
     * be `hex` values for colors, sizes of headers, etc.
     *
     * - `Inline` : **`Styles`**
     *     Removes a newline, so the message is logged on the
     *     same line as the previous message.
     *
     * - `Newline` : **`Styles`**
     *     Add a newline before logging the message.
     *
     * - `Paragraph` : **`Styles`**
     *     Add a newline before and after logging the message.
     *
     * - `Refresh` : **`Styles`**
     *     Clears the buffer before printing the message.
     *
     * - `h1` : **`Styles`**
     *     Makes this message a <h1> header, its enum value
     *     being the size for the text.
     *
     * - `h2` : **`Styles`**
     *     Makes this message a <h2> header, its enum value
     *     being the size for the text.
     *
     * - `h3` : **`Styles`**
     *     Makes this message a <h3> header, its enum value
     *     being the size for the text.
     *
     * - `h4` : **`Styles`**
     *     Makes this message a <h4> header, its enum value
     *     being the size for the text.
     *
     * - `Default` : **`Styles`**
     *     The base color for the text, currently `0xFFFFFF`,
     *     for pure white text. This one doesn't need to be
     *     specified when formatting, but its value is used.
     *
     * - `State` : **`Styles`**
     *     Special color to use to represent some change in
     *     state, be it some game event passing, or any number
     *     of other changes the `Player` should be privy to.
     *
     * - `Alert` : **`Styles`**
     *     Usually red, this alerts the `Player` to dangerous
     *     or very important messages in the `Terminal`.
     *
     * - `Command` : **`Styles`**
     *     Color to use when the user is issuing / resolving
     *     commands from the parser.
     **/
    public enum Styles : int {
        Inline=0, Newline=1, Paragraph=2, Refresh=3,
        h1=36, h2=28, h3=24, h4=18,
        Default=0xFFFFFF, State=0x2A98AA, Change=0xFFAE10,
        Alert=0xFC0000, Command=0xBBBBBB}


    public enum Templates : int {
    	Name=0,
    	Description=1,
    	State=4}


    public enum Inputs : byte {
        Trigger, Click, Elapsed, Sight };


    /** `Parse` : **`delegate`**
     *
     * The standard `event`/`delegate` for commands from
     * the `Parser` class.
     *
     * - `sender` : **`Person`**
     *     the instance which is issuing the command.
     *
     * - `e` : **`EventArgs`**
     *     ubiquitous event arguments
     *
     * - `c` : **`Command`**
     *     Default `Command` struct, sometimes unused, but
     *     means that this function is a `Parse` delegate.
     *
     * - `input` : **`string`**
     *     raw input from the user
     **/
    public delegate bool Parse(
        adv::Person sender,
        EventArgs e,
        Command c,
        string input);


#if FAIL
    public interface IDescribable<in T> : IDescribable
    where T : ILoggable {
        Description<Thing> description {get;set;} }
#endif


    /** `ILoggable` : **`interface`**
     *
     * That can be logged by the `Terminal`.
     **/
    public interface ILoggable {

    	/** `Template` : **`string`**
    	 *
    	 * A formatting `string` to specialize the way that the
    	 * `ILoggable` object is displayed.
    	 **/
    	string Template {get;set;}


        /** `Entry` : **`string`**
         *
         * Text to be logged by the `Terminal`. All deriving
         * classes should do any formatting computations when
         * setting this property for optimal performance.
         **/
        string Entry {get;set;}


        /** `styles` : **`Styles[]`**
         *
         * A list of `Styles` to be applied to the `ILoggable`
         * object, (hopefully ahead of time).
         **/
        Styles[] styles {get;set;}

        /** `Log()` : **`string`**
         *
         * Base string to be formatted by the `Terminal` in the
         * process of rendering it into Markdown. It could make
         * the name a header, the text italic, or any other
         * combination of effects.
         **/
        string Log();
    }


    /** `IDescribable` : **`interface`**
     *
     * Shared interface for all sorts of describable things.
     **/
    public interface IDescribable : IStorable {


        /** `description` : **`Description`**
         *
         * This property encapsulates a `Description` `object`
         * that every deriving class (e.g., half of all the
         * `class`es in this engine) interfaces with.
         **/
        Description description {get;set;}
    }


    /** `IReadable` : **`interface`**
     *
     * Interface to anything that can be read.
     **/
    public interface IReadable {

    	/** `Passage` : **`string`**
    	 *
    	 * Represents the body of text to be read
    	 **/
    	string Passage {get;set;}


        /** `Read()` : **`bool`**
         *
         * Function to call when reading something.
         **/
        bool Read();
    }


        /** `proper_name` : **`struct`**
     *
     * Attempt to formalize names for `Actor`s.
     *
     * - `name` : **`string`**
     *     Full name of the `P_ID` to be used when serializing.
     *
     * - `first` : **`string`** - First Name
     *
     * - `last` : **`string`** - Surname
     **/
    public struct proper_name {
        public string first, last;

        public proper_name(string name) {
            var s = name.Split(' ');
            if (s.Length!=2)
                throw new System.Exception("bad name input");
            this.first = s[0];
            this.last = s[1];
        }

        public proper_name(string first, string last) {
            this.first = first; this.last = last; }
    }
}

