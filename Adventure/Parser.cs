/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-11 * Parser */

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Buffer=System.Text.StringBuilder;

namespace PathwaysEngine.Adventure {

	/** `Parser` : **`class`**
	 *
	 * Main class for dealing with natural language
	 * processing and user-issued commands.
	 **/
	public static class Parser {
		public enum Kinds { All, Item, Examine, Move, Room }

		public static readonly Dictionary<Kinds,Regex> sentences;
		public static readonly Dictionary<string,command> commands;

		/** `Parser` : **`constructor`**
		 *
		 * Initializes all the commands and their regexes
		 * into a dictionary.
		 *
		 * @TODO: Get this out of here, serialize the list
		 **/
		static Parser() {
			var temp = new List<command> {
				new command("sudo", Pathways.Sudo,@"\bsudo\s+"),
				new command("quit", Pathways.Quit,@"\b(quit|restart)\b"),
				new command("again", Pathways.Redo,@"\b(again|redo)\b"),
				new command("load", Pathways.Load,@"\b(load|restore)\b"),
				new command("save", Pathways.Save,@"\b(save)\b"),
				new command("help", Pathways.Help,
					@"\b(help|info|about|idk)\b"),
				new command("status", Player.View,
					@"\b(c|diagnos(e|tic)|status)\b"),
				new command("invt", Player.View,
					@"\b(i(nvt|nventory|tems)?)\b"),
				new command("look", Player.View,
					@"\b(x|examine|l(ook)?|view)\b"),
				new command("show", Terminal.Activate,
					@"\b(show (terminal|window|ui))\b"),
				new command("hide", Terminal.Deactivate,
					@"\b(hide (terminal|window|ui))\b"),
				/*new command("move", Player.Move,
					@"\b(move|run|walk|jump|go)\b"),*/
				new command("travel|goto", Player.Goto,
					@"\b(e|ne|n|nw|w|sw|s|se|u|d)\b"),
				new command("take", Player.Take,
					@"\b(take|get|pick\s+up|grab)\b"),
				new command("drop", Player.Drop,
					@"\b(drop|put(\s+down)?|throw)\b"),
				new command("wear", Player.Wear,
					@"\b(equip|don|wear|put\s+(on)?|sport)\b"),
				new command("stow", Player.Stow,
					@"\b(stow|put(\s+away)|remove|un(equip|load))\b"),
				new command("read", Player.Read,
					@"\b(read|watch|glance)\b")};
			commands = new Dictionary<string,command>();
			foreach (var elem in temp)
				commands.Add(elem.name,elem);
		}

		/** `eval()` : **`function`**
		 *
		 * Parses the sent `string`, creates a `command`
		 * and dispatches it to its `Parse` function for
		 * processing.
		 *
		 * @TODO: Clean this the fuck up.
		 **/
		public static void eval(string s) {
			foreach (var elem in s.Process()) {
				if (string.IsNullOrEmpty(elem)) continue;
				foreach (var kvp in commands.Values) {
					if (kvp.regex.IsMatch(elem)) {
						Pathways.gameState = GameStates.Game;
						kvp.parse(new command(
							kvp.name,kvp.regex,kvp.parse,elem));
						return;
					}
				} Terminal.Log(" > "+s+": Do what, exactly?\n",Formats.Command);
				Pathways.gameState = GameStates.Game;
			}
		}
	}
}


