/* Ben Scott * bescott@andrew.cmu.edu * 2015-12-03 * Extensions */

using System.Collections.Generic;
using System.Text.RegularExpressions;
//using System.Web.UI; // for apparently illicit DataBinder
using Type=System.Type;
using Buffer=System.Text.StringBuilder;

namespace PathwaysEngine {


    /** `Extension` : **`class`**
    |*
    |* Class to contain all minor extension methods added to
    |* `string` and other `System` types I can't change myself.
    |**/
    static class Extension {


        /** `md()` : **`string`**
        |*
        |* Adds support for `Markdown`, and can be called on
        |* any `string`. Formats the `Markdown` syntax into
        |* `HTML`. Currently removes all `<p>` tags.
        |*
        |* - `s` : **`string`**
        |*    `string` to be formatted.
        |**/
        public static string md(this string s) {
            var buffer = new Buffer(Markdown.Transform(s));
            return buffer
                .Replace("<em>","<i>").Replace("</em>","</i>")
                .Replace("<blockquote>","<i>").Replace("</blockquote>","</i>")
                .Replace("<strong>","<b>").Replace("</strong>","</b>")
                .Replace("<h1>","<size=36>").Replace("</h1>","</size>")
                .Replace("<h2>","<size=24>").Replace("</h2>","</size>")
                .Replace("<h3>","<size=16>").Replace("</h3>","</size>")
                .Replace("<ul>","").Replace("</ul>","")
                .Replace("<li>","").Replace("</li>","")
                .Replace("<p>","").Replace("</p>","").ToString();
        }


        /** `Replace()` : **`string`**
        |*
        |* Adds an overload to the existing `Replace()` that
        |* takes a single argument, for removing things instead
        |* of replacing them.
        |*
        |* - `s` : **`string`**
        |*    `string` to be formatted.
        |*
        |* - `newValue` : **`string`**
        |*    replacement `string` to insert.
        |**/
        public static string Replace(this string s, string newValue) {
            return s.Replace(newValue,""); }


        /** `Strip()` : **`string`**
        |*
        |* @TODO: Dumb name, should be changed.
        |*
        |* - `s` : **`string`**
        |*    `string` to be processed for usage with `Parser`.
        |**/
        public static string Strip(this string s) {
            return s.Trim().ToLower()
                .Replace("\bthe\b").Replace("\ba\b"); }


        /** `DerivesFrom<T>()` : **`bool`**
        |*
        |* Simple extension method to determine if a `Type` is,
        |* or derives from, the type specified.
        |*
        |* - `<T>` : **`Type`**
        |*    type to check against
        |**/
        public static bool DerivesFrom<T>(this Type type) {
            return (type==typeof(T) || type.IsSubclassOf(typeof(T))); }

#if DUMB
        public static string FormatWith(
                        this string format,
                        object source) {
            return FormatWith(format, null, source); }

        public static string FormatWith(
                        this string format,
                        System.IFormatProvider provider,
                        object source) {
            if (format == null)
                throw new System.ArgumentNullException("format");

            Regex r = new Regex(
                @"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            List<object> values = new List<object>();
            string rewrittenFormat = r.Replace(format, delegate(Match m) {
                Group startGroup = m.Groups["start"];
                Group propertyGroup = m.Groups["property"];
                Group formatGroup = m.Groups["format"];
                Group endGroup = m.Groups["end"];

                values.Add((propertyGroup.Value == "0")
                  ? source
                  : DataBinder.Eval(source, propertyGroup.Value));

                return new string('{',startGroup.Captures.Count)+(values.Count-1)+formatGroup.Value+new string('}',endGroup.Captures.Count); });

            return string.Format(provider, rewrittenFormat, values.ToArray());
        }
#endif
    }
}