/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-08 * Flashlight */

using UnityEngine;
using System.Collections;
//using static PathwaysEngine.Literature.Terminal;


namespace PathwaysEngine.Inventory {


    public class Flashlight : Lamp {
        public override bool Wear() {
            Literature.Terminal.LogCommand(
                "You turn on your flashlight.");
            return base.Wear();
        }

        public override bool Stow() {
            Literature.Terminal.LogCommand(
                "You put away your flashlight.");
            return base.Stow();
        }
    }
}

