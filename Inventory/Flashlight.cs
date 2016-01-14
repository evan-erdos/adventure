/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-08 * Flashlight */

using UnityEngine;
using System.Collections;
//using static PathwaysEngine.Literature.Terminal;
using lit=PathwaysEngine.Literature;


namespace PathwaysEngine.Inventory {


    public class Flashlight : Lamp {
        public override bool Wear() {
            lit::Terminal.Log(
                $"<cmd>You turn on the</cmd> {Name}<cmd>.</cmd>");
            return base.Wear();
        }

        public override bool Stow() {
            lit::Terminal.Log(
                $"<cmd>You put away the</cmd> {Name}<cmd>.</cmd>");
            return base.Stow();
        }
    }
}

