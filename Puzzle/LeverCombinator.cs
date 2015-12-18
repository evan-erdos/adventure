/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-30 * Lever Combinator */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using lit=PathwaysEngine.Literature;

namespace PathwaysEngine.Puzzle {


    /** `LeverCombinator` : **`class`**
    |*
    |* Represents an instance of a puzzle piece, which must be
    |* `Solve`d to finish a puzzle.
    |**/
    class LeverCombinator : Combinator<Lever> {

        [SerializeField] bool[] flags;
        [SerializeField] LightResponder responder;


        public override bool IsSolved {
            get { if (flags==null || flags.Length<=0) return false;
                for (var i=0;i<pieces.Count;i++)
                    if (flags[i]!=pieces[i].IsSolved)
                        return false;
                return true;
            }
        }

        public override void Awake() { base.Awake();
            if (flags.Length!=pieces.Count)
                throw new System.Exception("Miscounted Levers!");
            foreach (var piece in pieces)
                piece.SolveEvent += this.OnSolve;
        }

        public override bool OnSolve(
                        object sender,
                        System.EventArgs e,
                        bool wasSolved) {
            return Solve();
        }

        public override bool Solve() {
            if (IsSolved) {
                if (responder!=null)
                    responder.WhenSolved(this);
                return true;
            } else return false;
        }
    }
}


