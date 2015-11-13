/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-16 * Markdown */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;

public static class Markdown {
    public static string EmptyElementSuffix {
        get { return _emptyElementSuffix; }
        set { _emptyElementSuffix = value; }
    } static string _emptyElementSuffix = " />";

    public static bool LinkEmails {
        get { return _linkEmails; }
        set { _linkEmails = value; }
    } static bool _linkEmails = false;

    public static bool StrictBoldItalic {
        get { return _strictBoldItalic; }
        set { _strictBoldItalic = value; }
    } static bool _strictBoldItalic = true;

    public static bool AsteriskIntraWordEmphasis {
        get { return _asteriskIntraWordEmphasis; }
        set { _asteriskIntraWordEmphasis = value; }
    } static bool _asteriskIntraWordEmphasis = false;

    public static bool AutoNewLines {
        get { return _autoNewlines; }
        set { _autoNewlines = value; }
    } static bool _autoNewlines = false;

    public static bool AutoHyperlink {
        get { return _autoHyperlink; }
        set { _autoHyperlink = value; }
    } static bool _autoHyperlink = false;

    enum TokenType { Text, Tag }

    struct Token {
        public TokenType Type;
        public string Value;
        public Token(TokenType type, string value) {
            this.Type = type; this.Value = value;
        }
    }

    const int _nestDepth = 6;
    const int _tabWidth = 4;
    const string _markerUL = @"[*+-]";
    const string _markerOL = @"\d+[.]";
    static readonly Dictionary<string, string> _escapeTable;
    static readonly Dictionary<string, string> _invertedEscapeTable;
    static readonly Dictionary<string, string> _backslashEscapeTable;
    static readonly Dictionary<string, string> _urls = new Dictionary<string, string>();
    static readonly Dictionary<string, string> _titles = new Dictionary<string, string>();
    static readonly Dictionary<string, string> _htmlBlocks = new Dictionary<string, string>();

    static int _listLevel;
    static string AutoLinkPreventionMarker = "\x1AP";
    static Markdown() {
        _escapeTable = new Dictionary<string, string>();
        _invertedEscapeTable = new Dictionary<string, string>();
        _backslashEscapeTable = new Dictionary<string, string>();
        string backslashPattern = "";
        foreach (char c in @"\`*_{}[]()>#+-.!/:") {
            string key = c.ToString();
            string hash = GetHashKey(key, isHtmlBlock: false);
            _escapeTable.Add(key, hash);
            _invertedEscapeTable.Add(hash, key);
            _backslashEscapeTable.Add(@"\" + key, hash);
            backslashPattern += Regex.Escape(@"\"+key)+"|";
        } _backslashEscapes = new Regex(
            backslashPattern.Substring(0, backslashPattern.Length-1));
    }

    public static string Transform(string text) {
        if (String.IsNullOrEmpty(text)) return "";
        Setup();
        text = Normalize(text);
        text = HashHTMLBlocks(text);
        text = StripLinkDefinitions(text);
        text = RunBlockGamut(text);
        text = Unescape(text);
        Cleanup();
        return text + "\n";
    }

    static string RunBlockGamut(string text, bool unhash = true, bool createParagraphs = false) {
        text = DoHeaders(text);
        text = DoHorizontalRules(text);
        text = DoLists(text);
        text = DoCodeBlocks(text);
        text = DoBlockQuotes(text);
        text = HashHTMLBlocks(text);
        text = FormParagraphs(text, unhash: unhash, createParagraphs: createParagraphs);
        return text;
    }

    static string RunSpanGamut(string text) {
        text = DoCodeSpans(text);
        text = EscapeSpecialCharsWithinTagAttributes(text);
        text = EscapeBackslashes(text);
        text = DoImages(text);
        text = DoAnchors(text);
        text = DoAutoLinks(text);
        text = text.Replace(AutoLinkPreventionMarker, "://");
        text = EncodeAmpsAndAngles(text);
        text = DoItalicsAndBold(text);
        text = DoHardBreaks(text);
        return text;
    }

    static Regex _newlinesLeadingTrailing = new Regex(@"^\n+|\n+\z");
    static Regex _newlinesMultiple = new Regex(@"\n{2,}");
    static Regex _leadingWhitespace = new Regex(@"^[ ]*");
    static Regex _htmlBlockHash = new Regex("\x1AH\\d+H");

    static string FormParagraphs(string text,bool unhash = true,bool createParagraphs = false) {
        string[] grafs = _newlinesMultiple.Split(_newlinesLeadingTrailing.Replace(text,""));
        for (int i=0;i<grafs.Length;i++){
            if (grafs[i].Contains("\x1AH")) {
                if (unhash) { // unhashify HTML blocks
                    int sanityCheck = 50; // just for safety, guard against an infinite loop
                    bool keepGoing = true; // as long as replacements where made, keep going
                    while (keepGoing && sanityCheck>0) {
                        keepGoing = false;
                        grafs[i] = _htmlBlockHash.Replace(grafs[i], match => {
                            keepGoing = true;
                            return _htmlBlocks[match.Value];
                        });
                        sanityCheck--;
                    }
                }
            } else grafs[i] = _leadingWhitespace.Replace(RunSpanGamut(grafs[i]),
                    createParagraphs ?"<p>":"")+(createParagraphs ?"</p>":"");
        } return string.Join("\n\n", grafs);
    }


    static void Setup() {
        _urls.Clear();
        _titles.Clear();
        _htmlBlocks.Clear();
        _listLevel = 0;
    }

    static void Cleanup() { Setup(); }

    static string _nestedBracketsPattern;

    static string GetNestedBracketsPattern() {
        if (_nestedBracketsPattern == null)
            _nestedBracketsPattern =
                RepeatString(@"
                (?>              # Atomic matching
                   [^\[\]]+      # Anything other than brackets
                 |
                   \[
                       ", _nestDepth) + RepeatString(
                @" \]
                )*"
                , _nestDepth);
        return _nestedBracketsPattern;
    }

    static string _nestedParensPattern;

    static string GetNestedParensPattern() {
        if (_nestedParensPattern == null)
            _nestedParensPattern =
                RepeatString(@"
                (?>              # Atomic matching
                   [^()\s]+      # Anything other than parens or whitespace
                 |
                   \(
                       ", _nestDepth) + RepeatString(
                @" \)
                )*"
                , _nestDepth);
        return _nestedParensPattern;
    }

    static Regex _linkDef = new Regex(string.Format(@"
                    ^[ ]{{0,{0}}}\[([^\[\]]+)\]:  # id = $1
                      [ ]*
                      \n?                   # maybe *one* newline
                      [ ]*
                    <?(\S+?)>?              # url = $2
                      [ ]*
                      \n?                   # maybe one newline
                      [ ]*
                    (?:
                        (?<=\s)             # lookbehind for whitespace
                        [""(]
                        (.+?)               # title = $3
                        ["")]
                        [ ]*
                    )?                      # title is optional
                    (?:\n+|\Z)", _tabWidth - 1), RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);//| RegexOptions.Compiled);

    static string StripLinkDefinitions(string text) {
        return _linkDef.Replace(text, new MatchEvaluator(LinkEvaluator));
    }

    static string LinkEvaluator(Match match) {
        string linkID = match.Groups[1].Value.ToLowerInvariant();
        _urls[linkID] = EncodeAmpsAndAngles(match.Groups[2].Value);
        if (match.Groups[3] != null && match.Groups[3].Length > 0)
            _titles[linkID] = match.Groups[3].Value.Replace("\"", "&quot;");
        return "";
    }

    static Regex _blocksHtml = new Regex(GetBlockPattern(), RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);


    static string GetBlockPattern() {
        string blockTagsA = "ins|del";
        string blockTagsB = "p|div|h[1-6]|blockquote|pre|table|dl|ol|ul|address|script|noscript|form|fieldset|iframe|math";
        string attr = @"
        (?>                         # optional tag attributes
          \s                        # starts with whitespace
          (?>
            [^>""/]+                # text outside quotes
          |
            /+(?!>)                 # slash not followed by >
          |
            ""[^""]*""              # text inside double quotes (tolerate >)
          |
            '[^']*'                 # text inside single quotes (tolerate >)
          )*
        )?
        ";

        string content = RepeatString(@"
            (?>
              [^<]+                 # content without tag
            |
              <\2                   # nested opening tag
                " + attr + @"       # attributes
              (?>
                  />
              |
                  >", _nestDepth) +   // end of opening tag
                  ".*?" +             // last level nested tag content
        RepeatString(@"
                  </\2\s*>          # closing nested tag
              )
              |
              <(?!/\2\s*>           # other tags with a different name
              )
            )*", _nestDepth);

        string content2 = content.Replace(@"\2", @"\3");

        string pattern = @"
        (?>
              (?>
                (?<=\n)     # Starting at the beginning of a line
                |           # or
                \A\n?       # the beginning of the doc
              )
              (             # save in $1

                # Match from `\n<tag>` to `</tag>\n`, handling nested tags
                # in between.

                    <($block_tags_b_re)   # start tag = $2
                    $attr>                # attributes followed by > and \n
                    $content              # content, support nesting
                    </\2>                 # the matching end tag
                    [ ]*                  # trailing spaces
                    (?=\n+|\Z)            # followed by a newline or end of document

              | # Special version for tags of group a.

                    <($block_tags_a_re)   # start tag = $3
                    $attr>[ ]*\n          # attributes followed by >
                    $content2             # content, support nesting
                    </\3>                 # the matching end tag
                    [ ]*                  # trailing spaces
                    (?=\n+|\Z)            # followed by a newline or end of document

              | # Special case just for <hr />. It was easier to make a special
                # case than to make the other regex more complicated.

                    [ ]{0,$less_than_tab}
                    <hr
                    $attr                 # attributes
                    /?>                   # the matching end tag
                    [ ]*
                    (?=\n{2,}|\Z)         # followed by a blank line or end of document

              | # Special case for standalone HTML comments:

                  (?<=\n\n|\A)            # preceded by a blank line or start of document
                  [ ]{0,$less_than_tab}
                  (?s:
                    <!--(?:|(?:[^>-]|-[^>])(?:[^-]|-[^-])*)-->
                  )
                  [ ]*
                  (?=\n{2,}|\Z)            # followed by a blank line or end of document

              | # PHP and ASP-style processor instructions (<? and <%)

                  [ ]{0,$less_than_tab}
                  (?s:
                    <([?%])                # $4
                    .*?
                    \4>
                  )
                  [ ]*
                  (?=\n{2,}|\Z)            # followed by a blank line or end of document

              )
        )";

        pattern = pattern.Replace("$less_than_tab", (_tabWidth - 1).ToString());
        pattern = pattern.Replace("$block_tags_b_re", blockTagsB);
        pattern = pattern.Replace("$block_tags_a_re", blockTagsA);
        pattern = pattern.Replace("$attr", attr);
        pattern = pattern.Replace("$content2", content2);
        pattern = pattern.Replace("$content", content);

        return pattern;
    }


    static string HashHTMLBlocks(string text) { return _blocksHtml.Replace(text, new MatchEvaluator(HtmlEvaluator)); }

    static string HtmlEvaluator(Match match) {
        string text = match.Groups[1].Value;
        string key = GetHashKey(text, isHtmlBlock: true);
        _htmlBlocks[key] = text;
        return string.Concat("\n\n", key, "\n\n");
    }

    static string GetHashKey(string s, bool isHtmlBlock) {
        var delim = isHtmlBlock ? 'H' : 'E';
        return "\x1A" + delim +  Math.Abs(s.GetHashCode()).ToString() + delim;
    }

    static Regex _htmlTokens = new Regex(@"
        (<!--(?:|(?:[^>-]|-[^>])(?:[^-]|-[^-])*)-->)|        # match <!-- foo -->
        (<\?.*?\?>)|                 # match <?foo?> " +
        RepeatString(@"
        (<[A-Za-z\/!$](?:[^<>]|", _nestDepth) + RepeatString(@")*>)", _nestDepth) +
                                   " # match <tag> and </tag>",
        RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);// | RegexOptions.Compiled);


    static List<Token> TokenizeHTML(string text) {
        int pos = 0;
        int tagStart = 0;
        var tokens = new List<Token>();
        foreach (Match m in _htmlTokens.Matches(text)) {
            tagStart = m.Index;
            if (pos < tagStart)
                tokens.Add(new Token(TokenType.Text, text.Substring(pos, tagStart - pos)));
            tokens.Add(new Token(TokenType.Tag, m.Value));
            pos = tagStart + m.Length;
        } if (pos < text.Length)
            tokens.Add(new Token(TokenType.Text, text.Substring(pos, text.Length - pos)));
        return tokens;
    }


    static Regex _anchorRef = new Regex(string.Format(@"
        (                               # wrap whole match in $1
            \[
                ({0})                   # link text = $2
            \]

            [ ]?                        # one optional space
            (?:\n[ ]*)?                 # one optional newline followed by spaces

            \[
                (.*?)                   # id = $3
            \]
        )", GetNestedBracketsPattern()), RegexOptions.IgnorePatternWhitespace);

    static Regex _anchorInline = new Regex(string.Format(@"
            (                           # wrap whole match in $1
                \[
                    ({0})               # link text = $2
                \]
                \(                      # literal paren
                    [ ]*
                    ({1})               # href = $3
                    [ ]*
                    (                   # $4
                    (['""])           # quote char = $5
                    (.*?)               # title = $6
                    \5                  # matching quote
                    [ ]*                # ignore any spaces between closing quote and )
                    )?                  # title is optional
                \)
            )", GetNestedBracketsPattern(), GetNestedParensPattern()),
              RegexOptions.IgnorePatternWhitespace);// | RegexOptions.Compiled);

    static Regex _anchorRefShortcut = new Regex(@"
        (                               # wrap whole match in $1
          \[
             ([^\[\]]+)                 # link text = $2; can't contain [ or ]
          \]
        )", RegexOptions.IgnorePatternWhitespace);// | RegexOptions.Compiled);

    static string DoAnchors(string text) {
        if (!text.Contains("[")) return text;
        text = _anchorRef.Replace(text, new MatchEvaluator(AnchorRefEvaluator));
        text = _anchorInline.Replace(text, new MatchEvaluator(AnchorInlineEvaluator));
        text = _anchorRefShortcut.Replace(text, new MatchEvaluator(AnchorRefShortcutEvaluator));
        return text;
    }

    static string SaveFromAutoLinking(string s) { return s.Replace("://", AutoLinkPreventionMarker); }

    static string AnchorRefEvaluator(Match match) {
        string wholeMatch = match.Groups[1].Value;
        string linkText = SaveFromAutoLinking(match.Groups[2].Value);
        string linkID = match.Groups[3].Value.ToLowerInvariant();
        string result;

        if (linkID == "") linkID = linkText.ToLowerInvariant();

        if (_urls.ContainsKey(linkID)) {
            string url = _urls[linkID];
            url = AttributeSafeUrl(url);
            result = "<a href=\"" + url + "\"";
            if (_titles.ContainsKey(linkID)) {
                string title = AttributeEncode(_titles[linkID]);
                title = AttributeEncode(EscapeBoldItalic(title));
                result += " title=\"" + title + "\"";
            } result += ">" + linkText + "</a>";
        } else result = wholeMatch;
        return result;
    }

    static string AnchorRefShortcutEvaluator(Match match) {
        string wholeMatch = match.Groups[1].Value;
        string linkText = SaveFromAutoLinking(match.Groups[2].Value);
        string linkID = Regex.Replace(linkText.ToLowerInvariant(), @"[ ]*\n[ ]*", " ");
        string result;

        if (_urls.ContainsKey(linkID)) {
            string url = _urls[linkID];
            url = AttributeSafeUrl(url);
            result = "<a href=\"" + url + "\"";
            if (_titles.ContainsKey(linkID)) {
                string title = AttributeEncode(_titles[linkID]);
                title = EscapeBoldItalic(title);
                result += " title=\"" + title + "\"";
            } result += ">" + linkText + "</a>";
        } else result = wholeMatch;
        return result;
    }


    static string AnchorInlineEvaluator(Match match) {
        string linkText = SaveFromAutoLinking(match.Groups[2].Value);
        string url = match.Groups[3].Value;
        string title = match.Groups[6].Value;
        string result;

        if (url.StartsWith("<") && url.EndsWith(">"))
            url = url.Substring(1, url.Length - 2);
        url = AttributeSafeUrl(url);
        result = string.Format("<a href=\"{0}\"", url);
        if (!String.IsNullOrEmpty(title)) {
            title = AttributeEncode(title);
            title = EscapeBoldItalic(title);
            result += string.Format(" title=\"{0}\"", title);
        } result += string.Format(">{0}</a>", linkText);
        return result;
    }

    static Regex _imagesRef = new Regex(@"
                (               # wrap whole match in $1
                !\[
                    (.*?)       # alt text = $2
                \]

                [ ]?            # one optional space
                (?:\n[ ]*)?     # one optional newline followed by spaces

                \[
                    (.*?)       # id = $3
                \]

                )", RegexOptions.IgnorePatternWhitespace);// | RegexOptions.Compiled);

    static Regex _imagesInline = new Regex(String.Format(@"
          (                     # wrap whole match in $1
            !\[
                (.*?)           # alt text = $2
            \]
            \s?                 # one optional whitespace character
            \(                  # literal paren
                [ ]*
                ({0})           # href = $3
                [ ]*
                (               # $4
                (['""])       # quote char = $5
                (.*?)           # title = $6
                \5              # matching quote
                [ ]*
                )?              # title is optional
            \)
          )", GetNestedParensPattern()),
              RegexOptions.IgnorePatternWhitespace);// | RegexOptions.Compiled);

    static string DoImages(string text) {
        if (!text.Contains("![")) return text;
        text = _imagesRef.Replace(text, new MatchEvaluator(ImageReferenceEvaluator));
        text = _imagesInline.Replace(text, new MatchEvaluator(ImageInlineEvaluator));
        return text;
    }

    static string EscapeImageAltText(string s) {
        s = EscapeBoldItalic(s);
        s = Regex.Replace(s, @"[\[\]()]", m => _escapeTable[m.ToString()]);
        return s;
    }

    static string ImageReferenceEvaluator(Match match) {
        string wholeMatch = match.Groups[1].Value;
        string altText = match.Groups[2].Value;
        string linkID = match.Groups[3].Value.ToLowerInvariant();
        if (linkID == "") linkID = altText.ToLowerInvariant();

        if (_urls.ContainsKey(linkID)) {
            string url = _urls[linkID];
            string title = null;
            if (_titles.ContainsKey(linkID)) title = _titles[linkID];
            return ImageTag(url, altText, title);
        } else return wholeMatch;
    }

    static string ImageInlineEvaluator(Match match) {
        string alt = match.Groups[2].Value;
        string url = match.Groups[3].Value;
        string title = match.Groups[6].Value;

        if (url.StartsWith("<") && url.EndsWith(">"))
            url = url.Substring(1, url.Length - 2);    // Remove <>'s surrounding URL, if present

        return ImageTag(url, alt, title);
    }

    static string ImageTag(string url, string altText, string title)
    {
        altText = EscapeImageAltText(AttributeEncode(altText));
        url = AttributeSafeUrl(url);
        var result = string.Format("<img src=\"{0}\" alt=\"{1}\"", url, altText);
        if (!String.IsNullOrEmpty(title))
        {
            title = AttributeEncode(EscapeBoldItalic(title));
            result += string.Format(" title=\"{0}\"", title);
        }
        result += _emptyElementSuffix;
        return result;
    }

    static Regex _headerSetext = new Regex(@"
            (.+?)       # removing ^ again?
            [ ]*
            \n
            (=+|-+)     # $1 = string of ='s or -'s
            [ ]*
            \n+",
        RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

    static Regex _headerAtx = new Regex(@"
            (\#{1,6})   # $1 = string of #'s, just removed ^ from front
            [ ]*
            (.+?)       # $2 = Header text
            [ ]*
            \#*         # optional closing #'s (not counted)
            \n+",
        RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);// | RegexOptions.Compiled);

    /// <summary>
    /// Turn Markdown headers into HTML header tags
    /// </summary>
    /// <remarks>
    /// Header 1
    /// ========
    ///
    /// Header 2
    /// --------
    ///
    /// # Header 1
    /// ## Header 2
    /// ## Header 2 with closing hashes ##
    /// ...
    /// ###### Header 6
    /// </remarks>
    static string DoHeaders(string text) {
        text = _headerSetext.Replace(text, new MatchEvaluator(SetextHeaderEvaluator));
        text = _headerAtx.Replace(text, new MatchEvaluator(AtxHeaderEvaluator));
        return text;
    }

    static string SetextHeaderEvaluator(Match match) {
        string header = match.Groups[1].Value;
        int level = match.Groups[2].Value.StartsWith("=") ? 1 : 2;
        return string.Format("<h{1}>{0}</h{1}>\n\n", RunSpanGamut(header), level);
    }

    static string AtxHeaderEvaluator(Match match) {
        string header = match.Groups[2].Value;
        int level = match.Groups[1].Value.Length;
        return string.Format("<h{1}>{0}</h{1}>\n\n", RunSpanGamut(header), level);
    }


    static Regex _horizontalRules = new Regex(@"
        ^[ ]{0,3}         # Leading space
            ([-*_])       # $1: First marker
            (?>           # Repeated marker group
                [ ]{0,2}  # Zero, one, or two spaces.
                \1        # Marker character
            ){2,}         # Group repeated at least twice
            [ ]*          # Trailing spaces
            $             # End of line.
        ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);// | RegexOptions.Compiled);

    /// <summary>
    /// Turn Markdown horizontal rules into HTML hr tags
    /// </summary>
    /// <remarks>
    /// ***
    /// * * *
    /// ---
    /// - - -
    /// </remarks>
    static string DoHorizontalRules(string text) {
        return _horizontalRules.Replace(text, "<hr" + _emptyElementSuffix + "\n");
    }

    static string _wholeList = string.Format(@"
        (                               # $1 = whole list
          (                             # $2
            [ ]{{0,{1}}}
            ({0})                       # $3 = first list item marker
            [ ]+
          )
          (?s:.+?)
          (                             # $4
              \z
            |
              \n{{2,}}
              (?=\S)
              (?!                       # Negative lookahead for another list item marker
                [ ]*
                {0}[ ]+
              )
          )
        )", string.Format("(?:{0}|{1})", _markerUL, _markerOL), _tabWidth - 1);

    static Regex _listNested = new Regex(@"^" + _wholeList,
        RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);// | RegexOptions.Compiled);

    static Regex _listTopLevel = new Regex(@"(?:(?<=\n\n)|\A\n?)" + _wholeList,
        RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace );//| RegexOptions.Compiled);

    /// <summary>
    /// Turn Markdown lists into HTML ul and ol and li tags
    /// </summary>
    static string DoLists(string text) {
        // We use a different prefix before nested lists than top-level lists.
        // See extended comment in _ProcessListItems().
        if (_listLevel > 0)
            text = _listNested.Replace(text, new MatchEvaluator(ListEvaluator));
        else
            text = _listTopLevel.Replace(text, new MatchEvaluator(ListEvaluator));

        return text;
    }

    static string ListEvaluator(Match match) {
        string list = match.Groups[1].Value;
        string marker = match.Groups[3].Value;
        string listType = Regex.IsMatch(marker, _markerUL) ? "ul" : "ol";
        string result;
        string start = "";
        if (listType == "ol") {
            var firstNumber = int.Parse(marker.Substring(0, marker.Length - 1));
            if (firstNumber != 1 && firstNumber != 0)
                start = " start=\"" + firstNumber + "\"";
        }
        result = ProcessListItems(list, listType == "ul" ? _markerUL : _markerOL);
        result = string.Format("<{0}{1}>\n{2}</{0}>\n", listType, start, result);
        return result;
    }

    /// <summary>
    /// Process the contents of a single ordered or unordered list, splitting it
    /// into individual list items.
    /// </summary>
    static string ProcessListItems(string list, string marker) {
        // The listLevel global keeps track of when we're inside a list.
        // Each time we enter a list, we increment it; when we leave a list,
        // we decrement. If it's zero, we're not in a list anymore.

        // We do this because when we're not inside a list, we want to treat
        // something like this:

        //    I recommend upgrading to version
        //    8. Oops, now this line is treated
        //    as a sub-list.

        // As a single paragraph, despite the fact that the second line starts
        // with a digit-period-space sequence.

        // Whereas when we're inside a list (or sub-list), that line will be
        // treated as the start of a sub-list. What a kludge, huh? This is
        // an aspect of Markdown's syntax that's hard to parse perfectly
        // without resorting to mind-reading. Perhaps the solution is to
        // change the syntax rules such that sub-lists must start with a
        // starting cardinal number; e.g. "1." or "a.".

        _listLevel++;

        // Trim trailing blank lines:
        list = Regex.Replace(list, @"\n{2,}\z", "\n");

        string pattern = string.Format(
          @"(^[ ]*)                    # leading whitespace = $1
            ({0}) [ ]+                 # list marker = $2
            ((?s:.+?)                  # list item text = $3
            (\n+))
            (?= (\z | \1 ({0}) [ ]+))", marker);

        //bool lastItemHadADoubleNewline = false;

        // has to be a closure, so subsequent invocations can share the bool
        MatchEvaluator ListItemEvaluator = (Match match) => {
            string item = match.Groups[3].Value;

            //bool endsWithDoubleNewline = item.EndsWith("\n\n");
            //bool containsDoubleNewline = endsWithDoubleNewline || item.Contains("\n\n");

            //var loose = containsDoubleNewline || lastItemHadADoubleNewline;
            // we could correct any bad indentation here..
            item = RunBlockGamut(Outdent(item) + "\n", unhash: false, createParagraphs:false);// loose);

            //lastItemHadADoubleNewline = endsWithDoubleNewline;
            return string.Format("<li>{0}</li>\n", item);
        };

        list = Regex.Replace(list, pattern, new MatchEvaluator(ListItemEvaluator),
                              RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
        _listLevel--;
        return list;
    }

    static Regex _codeBlock = new Regex(string.Format(@"
                (?:\n\n|\A\n?)
                (                        # $1 = the code block -- one or more lines, starting with a space
                (?:
                    (?:[ ]{{{0}}})       # Lines must start with a tab-width of spaces
                    .*\n+
                )+
                )
                ((?=^[ ]{{0,{0}}}[^ \t\n])|\Z) # Lookahead for non-space at line-start, or end of doc",
                _tabWidth), RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);// | RegexOptions.Compiled);

    /// <summary>
    /// /// Turn Markdown 4-space indented code into HTML pre code blocks
    /// </summary>
    static string DoCodeBlocks(string text) {
        text = _codeBlock.Replace(text, new MatchEvaluator(CodeBlockEvaluator));
        return text;
    }

    static string CodeBlockEvaluator(Match match) {
        string codeBlock = match.Groups[1].Value;

        codeBlock = EncodeCode(Outdent(codeBlock));
        codeBlock = _newlinesLeadingTrailing.Replace(codeBlock, "");

        return string.Concat("\n\n<pre><code>", codeBlock, "\n</code></pre>\n\n");
    }

    static Regex _codeSpan = new Regex(@"
                (?<![\\`])   # Character before opening ` can't be a backslash or backtick
                (`+)      # $1 = Opening run of `
                (?!`)     # and no more backticks -- match the full run
                (.+?)     # $2 = The code block
                (?<!`)
                \1
                (?!`)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);// | RegexOptions.Compiled);

    /// <summary>
    /// Turn Markdown `code spans` into HTML code tags
    /// </summary>
    static string DoCodeSpans(string text) {
        //    * You can use multiple backticks as the delimiters if you want to
        //        include literal backticks in the code span. So, this input:
        //
        //        Just type ``foo `bar` baz`` at the prompt.
        //
        //        Will translate to:
        //
        //          <p>Just type <code>foo `bar` baz</code> at the prompt.</p>
        //
        //        There's no arbitrary limit to the number of backticks you
        //        can use as delimters. If you need three consecutive backticks
        //        in your code, use four for delimiters, etc.
        //
        //    * You can use spaces to get literal backticks at the edges:
        //
        //          ... type `` `bar` `` ...
        //
        //        Turns to:
        //
        //          ... type <code>`bar`</code> ...
        //

        return _codeSpan.Replace(text, new MatchEvaluator(CodeSpanEvaluator));
    }

    static string CodeSpanEvaluator(Match match) {
        string span = match.Groups[2].Value;
        span = Regex.Replace(span, @"^[ ]*", ""); // leading whitespace
        span = Regex.Replace(span, @"[ ]*$", ""); // trailing whitespace
        span = EncodeCode(span);
        span = SaveFromAutoLinking(span); // to prevent auto-linking. Not necessary in code *blocks*, but in code spans.
        return string.Concat("<code>", span, "</code>");
    }

    static Regex _bold = new Regex(@"(\*\*|__) (?=\S) (.+?[*_]*) (?<=\S) \1",
        RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);// | RegexOptions.Compiled);
    static Regex _semiStrictBold = new Regex(@"(?=.[*_]|[*_])(^|(?=\W__|(?!\*)[\W_]\*\*|\w\*\*\w).)(\*\*|__)(?!\2)(?=\S)((?:|.*?(?!\2).)(?=\S_|\w|\S\*\*(?:[\W_]|$)).)(?=__(?:\W|$)|\*\*(?:[^*]|$))\2",
        RegexOptions.Singleline);// | RegexOptions.Compiled);
    static Regex _strictBold = new Regex(@"(^|[\W_])(?:(?!\1)|(?=^))(\*|_)\2(?=\S)(.*?\S)\2\2(?!\2)(?=[\W_]|$)",
        RegexOptions.Singleline);// | RegexOptions.Compiled);

    static Regex _italic = new Regex(@"(\*|_) (?=\S) (.+?) (?<=\S) \1",
        RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);// | RegexOptions.Compiled);
    static Regex _semiStrictItalic = new Regex(@"(?=.[*_]|[*_])(^|(?=\W_|(?!\*)(?:[\W_]\*|\D\*(?=\w)\D)).)(\*|_)(?!\2\2\2)(?=\S)((?:(?!\2).)*?(?=[^\s_]_|(?=\w)\D\*\D|[^\s*]\*(?:[\W_]|$)).)(?=_(?:\W|$)|\*(?:[^*]|$))\2",
        RegexOptions.Singleline);// | RegexOptions.Compiled);
    static Regex _strictItalic = new Regex(@"(^|[\W_])(?:(?!\1)|(?=^))(\*|_)(?=\S)((?:(?!\2).)*?\S)\2(?!\2)(?=[\W_]|$)",
        RegexOptions.Singleline);// | RegexOptions.Compiled);

    // Turn Markdown *italics* and **bold** into HTML strong and em tags
    static string DoItalicsAndBold(string text) {
        if (!(text.Contains("*") || text.Contains("_"))) return text;
        if (_strictBoldItalic) {
            if (_asteriskIntraWordEmphasis) {
                text = _semiStrictBold.Replace(text, "$1<strong>$3</strong>");
                text = _semiStrictItalic.Replace(text, "$1<em>$3</em>");
            } else {
                text = _strictBold.Replace(text, "$1<strong>$3</strong>");
                text = _strictItalic.Replace(text, "$1<em>$3</em>");
            }
        } else {
            text = _bold.Replace(text, "<strong>$2</strong>");
            text = _italic.Replace(text, "<em>$2</em>");
        } return text;
    }

    static string DoHardBreaks(string text) {
        if (_autoNewlines) text = Regex.Replace(text, @"\n", string.Format("<br{0}\n", _emptyElementSuffix));
        else text = Regex.Replace(text, @" {2,}\n", string.Format("<br{0}\n", _emptyElementSuffix));
        return text;
    }

    static Regex _blockquote = new Regex(@"
        (                           # Wrap whole match in $1
            (
            ^[ ]*>[ ]?              # '>' at the start of a line
                .+\n                # rest of the first line
            (.+\n)*                 # subsequent consecutive lines
            \n*                     # blanks
            )+
        )", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);// | RegexOptions.Compiled);

    static string DoBlockQuotes(string text) {
        return _blockquote.Replace(text, new MatchEvaluator(BlockQuoteEvaluator));
    }

    static string BlockQuoteEvaluator(Match match) {
        string bq = match.Groups[1].Value;

        bq = Regex.Replace(bq, @"^[ ]*>[ ]?", "", RegexOptions.Multiline);       // trim one level of quoting
        bq = Regex.Replace(bq, @"^[ ]+$", "", RegexOptions.Multiline);           // trim whitespace-only lines
        bq = RunBlockGamut(bq);                                                  // recurse

        bq = Regex.Replace(bq, @"^", "  ", RegexOptions.Multiline);

        // These leading spaces screw with <pre> content, so we need to fix that:
        bq = Regex.Replace(bq, @"(\s*<pre>.+?</pre>)", new MatchEvaluator(BlockQuoteEvaluator2), RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

        bq = string.Format("<blockquote>\n{0}\n</blockquote>", bq);
        string key = GetHashKey(bq, isHtmlBlock: true);
        _htmlBlocks[key] = bq;

        return "\n\n" + key + "\n\n";
    }

    static string BlockQuoteEvaluator2(Match match) {
        return Regex.Replace(match.Groups[1].Value, @"^  ", "", RegexOptions.Multiline);
    }

    const string _charInsideUrl = @"[-A-Z0-9+&@#/%?=~_|\[\]\(\)!:,\.;" + "\x1a]";
    const string _charEndingUrl = "[-A-Z0-9+&@#/%=~_|\\[\\])]";

    static Regex _autolinkBare = new Regex(@"(<|="")?\b(https?|ftp)(://" + _charInsideUrl + "*" + _charEndingUrl + ")(?=$|\\W)",
        RegexOptions.IgnoreCase);// | RegexOptions.Compiled);

    static Regex _endCharRegex = new Regex(_charEndingUrl, RegexOptions.IgnoreCase);// | RegexOptions.Compiled);

    static string handleTrailingParens(Match match) {
        // The first group is essentially a negative lookbehind -- if there's a < or a =", we don't touch this.
        // We're not using a *real* lookbehind, because of links with in links, like <a href="http://web.archive.org/web/20121130000728/http://www.google.com/">
        // With a real lookbehind, the full link would never be matched, and thus the http://www.google.com *would* be matched.
        // With the simulated lookbehind, the full link *is* matched (just not handled, because of this early return), causing
        // the google link to not be matched again.
        if (match.Groups[1].Success)
            return match.Value;

        var protocol = match.Groups[2].Value;
        var link = match.Groups[3].Value;
        if (!link.EndsWith(")")) return "<" + protocol + link + ">";
        var level = 0;
        foreach (Match c in Regex.Matches(link, "[()]")) {
            if (c.Value == "(") {
                if (level <= 0) level = 1;
                else level++;
            } else level--;
        }
        var tail = "";
        if (level < 0) {
            link = Regex.Replace(link, @"\){1," + (-level) + "}$", m => { tail = m.Value; return ""; });
        } if (tail.Length > 0) {
            var lastChar = link[link.Length - 1];
            if (!_endCharRegex.IsMatch(lastChar.ToString())) {
                tail = lastChar + tail;
                link = link.Substring(0, link.Length - 1);
            }
        } return "<" + protocol + link + ">" + tail;
    }

    static string DoAutoLinks(string text) {
        if (_autoHyperlink) {
            text = _autolinkBare.Replace(text, handleTrailingParens);
        } text = Regex.Replace(text, "<((https?|ftp):[^'\">\\s]+)>", new MatchEvaluator(HyperlinkEvaluator));
        if (_linkEmails) {
            string pattern =
                @"<
                  (?:mailto:)?
                  (
                    [-.\w]+
                    \@
                    [-a-z0-9]+(\.[-a-z0-9]+)*\.[a-z]+
                  )
                  >";
            text = Regex.Replace(text, pattern, new MatchEvaluator(EmailEvaluator), RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        } return text;
    }

    static string HyperlinkEvaluator(Match match) {
        string link = match.Groups[1].Value;
        string url = AttributeSafeUrl(link);
        return string.Format("<a href=\"{0}\">{1}</a>", url, link);
    }

    static string EmailEvaluator(Match match) {
        string email = Unescape(match.Groups[1].Value);

        //
        //    Input: an email address, e.g. "foo@example.com"
        //
        //    Output: the email address as a mailto link, with each character
        //            of the address encoded as either a decimal or hex entity, in
        //            the hopes of foiling most address harvesting spam bots. E.g.:
        //
        //      <a href="&#x6D;&#97;&#105;&#108;&#x74;&#111;:&#102;&#111;&#111;&#64;&#101;
        //        x&#x61;&#109;&#x70;&#108;&#x65;&#x2E;&#99;&#111;&#109;">&#102;&#111;&#111;
        //        &#64;&#101;x&#x61;&#109;&#x70;&#108;&#x65;&#x2E;&#99;&#111;&#109;</a>
        //
        //    Based by a filter by Matthew Wickline, posted to the BBEdit-Talk
        //    mailing list: <http://tinyurl.com/yu7ue>
        //
        email = "mailto:" + email;

        // leave ':' alone (to spot mailto: later)
        email = EncodeEmailAddress(email);

        email = string.Format("<a href=\"{0}\">{0}</a>", email);

        // strip the mailto: from the visible part
        email = Regex.Replace(email, "\">.+?:", "\">");
        return email;
    }

    static Regex _outDent = new Regex(@"^[ ]{1,"+_tabWidth+@"}", RegexOptions.Multiline);

    static string Outdent(string block) { return _outDent.Replace(block, ""); }


    static string EncodeEmailAddress(string addr) {
        var sb = new StringBuilder(addr.Length * 5);
        var rand = new Random();
        int r;
        foreach (char c in addr) {
            r = rand.Next(1, 100);
            if ((r > 90 || c == ':') && c != '@') sb.Append(c); // m
            else if (r < 45) sb.AppendFormat("&#x{0:x};", (int)c); // &#x6D
            else sb.AppendFormat("&#{0};", (int)c);    // &#109
        } return sb.ToString();
    }

    static Regex _codeEncoder = new Regex(@"&|<|>|\\|\*|_|\{|\}|\[|\]");

    static string EncodeCode(string code) { return _codeEncoder.Replace(code,EncodeCodeEvaluator);}

    static string EncodeCodeEvaluator(Match match) {
        switch (match.Value) {
            // Encode all ampersands; HTML entities are not
            // entities within a Markdown code span.
#if NOT_UNITY
            case "&": return "&amp;";
#endif
            // Do the angle bracket song and dance
            case "<": return "&lt;";
            case ">": return "&gt;";
            // escape characters that are magic in Markdown
            default: return _escapeTable[match.Value];
        }
    }

#if NOT_UNITY
    static Regex _amps = new Regex(@"&(?!((#[0-9]+)|(#[xX][a-fA-F0-9]+)|([a-zA-Z][a-zA-Z0-9]*));)", RegexOptions.ExplicitCapture);// | RegexOptions.Compiled);
    static Regex _angles = new Regex(@"<(?![A-Za-z/?\$!])", RegexOptions.ExplicitCapture);
#endif

    static string EncodeAmpsAndAngles(string s) {
#if NOT_UNITY
        s = _amps.Replace(s, "&amp;");
        s = _angles.Replace(s, "&lt;");
#endif
        return s;
    }

    static Regex _backslashEscapes;

    static string EscapeBackslashes(string s) { return _backslashEscapes.Replace(s, new MatchEvaluator(EscapeBackslashesEvaluator)); }

    static string EscapeBackslashesEvaluator(Match match) { return _backslashEscapeTable[match.Value]; }

    static Regex _unescapes = new Regex("\x1A" + "E\\d+E");//, RegexOptions.Compiled);

    static string Unescape(string s) { return _unescapes.Replace(s, new MatchEvaluator(UnescapeEvaluator)); }

    static string UnescapeEvaluator(Match match) { return _invertedEscapeTable[match.Value]; }

    static string EscapeBoldItalic(string s) {
        s = s.Replace("*", _escapeTable["*"]);
        s = s.Replace("_", _escapeTable["_"]);
        return s;
    }

    static string AttributeEncode(string s) {
        return s.Replace(">", "&gt;").Replace("<", "&lt;").Replace("\"", "&quot;").Replace("'", "&#39;");
    }

    static string AttributeSafeUrl(string s) {
        s = AttributeEncode(s);
        foreach (var c in "*_:()[]")
            s = s.Replace(c.ToString(), _escapeTable[c.ToString()]);
        return s;
    }

    /// <summary>
    /// Within tags -- meaning between &lt; and &gt; -- encode [\ ` * _] so they
    /// don't conflict with their use in Markdown for code, italics and strong.
    /// We're replacing each such character with its corresponding hash
    /// value; this is likely overkill, but it should prevent us from colliding
    /// with the escape values by accident.
    /// </summary>
    static string EscapeSpecialCharsWithinTagAttributes(string text) {
        var tokens = TokenizeHTML(text);
        // now, rebuild text from the tokens
        var sb = new StringBuilder(text.Length);

        foreach (var token in tokens) {
            string value = token.Value;
            if (token.Type==TokenType.Tag) {
                value = value.Replace(@"\", _escapeTable[@"\"]);

                if (_autoHyperlink && value.StartsWith("<!")) // escape slashes in comments to prevent autolinking there -- http://meta.stackexchange.com/questions/95987/html-comment-containing-url-breaks-if-followed-by-another-html-comment
                    value = value.Replace("/", _escapeTable["/"]);
                value = Regex.Replace(value, "(?<=.)</?code>(?=.)", _escapeTable[@"`"]);
                value = EscapeBoldItalic(value);
            } sb.Append(value);
        } return sb.ToString();
    }

    static string Normalize(string text) {
        var output = new StringBuilder(text.Length);
        var line = new StringBuilder();
        bool valid = false;
        for (int i=0;i<text.Length;i++) {
            switch (text[i]) {
                case '\n':
                    if (valid) output.Append(line);
                    output.Append('\n');
                    line.Length = 0; valid = false;
                    break;
                case '\r':
                    if ((i < text.Length - 1) && (text[i + 1] != '\n')) {
                        if (valid) output.Append(line);
                        output.Append('\n');
                        line.Length = 0; valid = false;
                    } break;
                case '\t':
                    int width = (_tabWidth - line.Length % _tabWidth);
                    for (int k = 0; k < width; k++) line.Append(' ');
                    break;
                case '\x1A':
                    break;
                default:
                    if (!valid && text[i] != ' ') valid = true;
                    line.Append(text[i]);
                    break;
            }
        }
        if (valid) output.Append(line);
        output.Append('\n');
        // add two newlines to the end before return
        return output.Append("\n\n").ToString();
    }

    static string RepeatString(string text, int count) {
        var sb = new StringBuilder(text.Length * count);
        for (int i=0;i<count;i++) sb.Append(text);
        return sb.ToString();
    }
}


#if FAIL

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Buffer=System.Text.StringBuilder;


namespace Markdown { // renders *.md to basic html
public static class MarkdownExtension {
	static string marks = "*_-=#";
	public static string md(this string s) {
		bool e = false, b = false;
		Buffer sb = new Buffer(s);
		for (int i=1;i<s.Length;++i) {
			var t = s[i];
			if (marks.IndexOf(t)<0) continue;
			var r = (string) s[i-1];
			if (r==@"\") continue;
			if (t!='*' || t!='_') continue; // for now
			if ((r=="*" && t=='*')) {
				b = !b;
			} else if (t=='*') e = !e;
		} return sb.ToString();
	}
}
}
/*
sb.Insert(i,"<i>");
sb.Replace('*','_');
sb.Replace("<i>","</i>");

String.Format("{0:yyyy-MM-dd}",System.DateTime.Now);*/

#endif


