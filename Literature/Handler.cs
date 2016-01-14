/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-11 * Handler */

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using EventArgs=System.EventArgs;
using Buffer=System.Text.StringBuilder;
using adv=PathwaysEngine.Adventure;


namespace PathwaysEngine.Literature {


    /** `Handler` : **`class`**
     *
     * A global state handler to manage user commands.
     **/
    public static class Handler {


        /** `Handler` : **`constructor`**
         *
         * Initializes all the commands and their regexes
         * into a dictionary.
         **/
        static Handler() {
            //commands = Pathways.commands;
        }


        /** `Sudo()` : **`Parse`**
         *
         * For special user commands. Unused, so far.
         **/
        public static bool Sudo(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) => true;


        /** `Redo()` : **`Parse`**
         *
         * Runs the prior command issued to the `Parser` again.
         **/
        public static bool Redo(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) => true;


        /** `Quit()` : **`Parse`**
         *
         * Prompts user through `Terminal` to quit the game.
         **/
        public static bool Quit(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) {
            Terminal.Alert(Pathways.messages["quit"]);
            return true;
        }


        /** `Load()` : **`Parse`**
         *
         * Loads a game from a `*.yml` file. Currently broken.
         **/
        public static bool Load(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) {
            Terminal.Clear();
            Terminal.LogImmediate(
                "I/O Disabled, restarting level.");
            SceneManager.LoadScene(0);
            return true;
        }


        /** `Save()` : **`Parse`**
         *
         * Saves a game from a `*.yml` file. Currently broken.
         **/
        public static bool Save(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) {
            if (File.Exists(input))
                Terminal.Alert("Overwriting file...");
            using (StreamWriter file = new StreamWriter(input)) {
                file.WriteLine("%YAML 1.1");
                file.WriteLine($"---  # Saved Game\n");
                file.WriteLine("player:");
                var pos = new Vector3(0f,0f,0f);
                //    Player.Position.x,
                //    Player.Position.y,
                //    Player.Position.z);
                file.WriteLine($"  position: {pos}");
                //file.WriteLine($"  area: {Player.area}");
                //file.WriteLine($"  room: {Player.Room}");
                file.WriteLine("  items:\n");
                //foreach (var elem in Player.Items)
                //    file.WriteLine($"  - {elem.Name}");
                file.WriteLine("\n...\n");
            } return true;
        } /*(s) => (s.Length>100)?(s.Substring(0,100)+"&hellip;"):(s); */


        /** `Help()` : **`Parse`**
         *
         * Shows the help menu via `Window`.
         **/
        public static bool Help(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) {
            Window.Display(Pathways.messages["help"]);
            return true;
        }


        public static bool View(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) =>
            Player.Current.View(Player.Current,e,c,input);


        public static bool Look(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) => false;
            //Player.Current.Look(Player.Current,e,c,input);


        public static bool Goto(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) => false;


        public static bool Move(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) => false;


        public static bool Invt(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) => false;


        public static bool Take(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) =>
            Player.Current.Take(Player.Current,e,c,input);


        public static bool Drop(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) =>
            Player.Current.Drop(Player.Current,e,c,input);


        public static bool Use(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) =>
            Player.Current.Use(Player.Current,e,c,input);


        public static bool Wear(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) =>
            Player.Current.Wear(Player.Current,e,c,input);


        public static bool Stow(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) =>
            Player.Current.Stow(Player.Current,e,c,input);


        public static bool Read(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) =>
            Player.Current.Read(Player.Current,e,c,input);


        public static bool Open(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) =>
            Player.Current.Open(Player.Current,e,c,input);


        public static bool Shut(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) =>
            Player.Current.Shut(Player.Current,e,c,input);


        public static bool Push(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) =>
            Player.Current.Push(Player.Current,e,c,input);


        public static bool Pull(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) =>
            Player.Current.Pull(Player.Current,e,c,input);


        public static bool Show(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) => false;


        public static bool Hide(
                        adv::Person sender,
                        EventArgs e,
                        Command c,
                        string input) => false;
    }
}


