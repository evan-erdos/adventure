/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-30 * Responder */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using lit=PathwaysEngine.Literature;


namespace PathwaysEngine.Puzzle {


    /** `Responder<T>` : **`class`**
    |*
    |* Represents an instance of a puzzle piece, which must be
    |* `Solve`d to finish a puzzle.
    |**/
    class Responder<T> : Piece<T>, IResponder<T>
               where T : Component {

        [SerializeField] protected T[] list;

        public bool WhenSolved(IPiece<T> piece) {
            if (!piece.IsSolved) return false;
            //IsSolved = true;
            return Solve(default (T));
        }

        public override bool Solve(T condition) {
            foreach (var elem in list)
                elem.gameObject.SetActive(IsSolved);
            return IsSolved;
        }
    }
}


