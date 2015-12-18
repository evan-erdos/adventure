/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-08 * Flashlight */

using UnityEngine;
using System.Collections;
using lit=PathwaysEngine.Literature;

namespace PathwaysEngine.Inventory {


    public class Flashlight : Lamp {
        public override bool Wear() {
            lit::Terminal.LogCommand(
                "You turn on your flashlight.");
            return base.Wear();
        }

        public override bool Stow() {
            lit::Terminal.LogCommand(
                "You put away your flashlight.");
            return base.Stow();
        }
    }
}

