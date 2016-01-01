/* Ben Scott * bescott@andrew.cmu.edu * 2015-12-02 * Button */

using UnityEngine;
using System.Collections.Generic;
using intf=PathwaysEngine.Adventure;

namespace PathwaysEngine.Puzzle {


    /** `Button` : **`class`**
     *
     * Represents any button which can be pressed, toggled, or
     * or otherwise manipulated with a single click.
     **/
    partial class Button : Piece {


        /** `IsPressed` : **`bool`**
         *
         * Whether or not the button is pressed.
         **/
        public bool IsPressed {get;set;}


        public override bool OnSolve(
                        object sender,
                        System.EventArgs e,
                        bool wasSolved) {
            if (wasSolved) Debug.Log("Button Solved!");
            return IsSolved;
        }


        public override void Awake() {
            SolveEvent += this.OnSolve; }
    }
}

