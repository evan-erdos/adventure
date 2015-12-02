/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-04 * Regex Type Converter */

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Type=System.Type;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

public class RegexYamlConverter : IYamlTypeConverter {

    public bool Accepts(Type type) {
        return (type==typeof(Regex)); }

    public object ReadYaml(IParser parser, Type type) {
        var current = parser.Current;
        parser.MoveNext();
        if (type!=typeof(Regex)) return null;
        if (current.Type!=EventType.Scalar) return null;
        var scalar = (Scalar) current;
        if (!isRegexLiteral(scalar.Value)) return null;
        return new Regex(@"\b"+scalar.Value.Trim('/')+@"\b");
    }

    public void WriteYaml(IEmitter emitter, object value, Type type) {
        throw new System.NotImplementedException("Emitter."); }

    bool isRegexLiteral(string s) {
        if (string.IsNullOrEmpty(s)) return false;
        return (s.StartsWith("/") && s.EndsWith("/"));
    }
}
