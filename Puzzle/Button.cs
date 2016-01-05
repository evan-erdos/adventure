/* Ben Scott * bescott@andrew.cmu.edu * 2015-12-02 * Button */

using UnityEngine;
using System.Collections.Generic;
using EventArgs=System.EventArgs;
using intf=PathwaysEngine.Adventure;


namespace PathwaysEngine.Puzzle {


    /** `Button` : **`Piece<bool>`**
     *
     * Represents any button which can be pressed, toggled, or
     * or otherwise manipulated with a single click.
     **/
    public class Button : Piece<bool> {


        public override bool OnSolve(
                        IPiece<bool> sender,
                        EventArgs e,
                        bool wasSolved) {
            if (wasSolved)
                Debug.Log("Button Solved!");
            return IsSolved;
        }


        public override void Awake() {
            SolveEvent += this.OnSolve; }

        public override void Deserialize() =>
            Pathways.Deserialize<Button,Button_yml>(this);
    }
}

