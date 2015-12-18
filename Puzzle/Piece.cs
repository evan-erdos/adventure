/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-18 * Piece */

using UnityEngine;
using System.Collections.Generic;
using adv=PathwaysEngine.Adventure;

namespace PathwaysEngine.Puzzle {


    /** `Piece` : **`Thing`**
    |*
    |* Represents an instance of a puzzle piece, which must be
    |* `Solve`d to finish a puzzle.
    |**/
    partial class Piece : adv::Thing, IPiece {


        /** `SolveEvent` : **`event`**
        |*
        |* This property wraps the inherited `solveEvent` event
        |*
        |**/
        public virtual event OnSolve SolveEvent {
            add { solveEvent += value; }
            remove { solveEvent -= value; }
        } event OnSolve solveEvent;

        public virtual bool IsSolved {
            get { return isSolved; }
            set { isSolved = value; }
        } protected bool isSolved;

        public virtual bool Solve() {
            return OnSolve(this,System.EventArgs.Empty,IsSolved);
        }

        public virtual bool OnSolve(
                        object sender,
                        System.EventArgs e,
                        bool wasSolved) {
            if (wasSolved) Debug.Log("Piece Solved!");
            return IsSolved;
        }

        public override void Awake() {
            SolveEvent += this.OnSolve; }
    }
}

