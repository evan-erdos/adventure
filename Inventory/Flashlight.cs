/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-08 * Flashlight */

using UnityEngine;
using System.Collections;

namespace PathwaysEngine.Inventory {


    public class Flashlight : Lamp {
        public override void Wear() {
            Terminal.Log(" > equip flashlight: You turn on your flashlight.\n",
                Formats.Command);
            base.Wear();
        }

        public override void Stow() {
            Terminal.Log(" > stow flashlight: You put away your flashlight.\n",
                Formats.Command);
            base.Stow();
        }
    }
}

