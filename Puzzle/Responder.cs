/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-30 * Responder */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using lit=PathwaysEngine.Literature;

namespace PathwaysEngine.Puzzle {


    /** `Responder<T>` : **`class`**
     *
     * Represents an instance of a puzzle piece, which must be
     * `Solve`d to finish a puzzle.
     **/
    class Responder<T> : Piece, IResponder
               where T : Component {

        [SerializeField] protected T[] list;

        public bool WhenSolved(IPiece piece) {
            if (!piece.IsSolved) return false;
            IsSolved = true;
            return Solve();
        }

        public override bool Solve() {
            foreach (var elem in list)
                elem.gameObject.SetActive(IsSolved);
            return IsSolved;
        }
    }
}


