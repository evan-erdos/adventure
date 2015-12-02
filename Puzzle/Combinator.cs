/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-18 * Piece */

using UnityEngine;
using System.Collections.Generic;

namespace PathwaysEngine.Puzzle {


    /** `Combinator` : **`Piece`**
    |*
    |* Represents an instance of a puzzle piece, which must be
    |* `Solve`d to finish a puzzle.
    |**/
    class Combinator<T> : Piece where T : IPiece {

        [SerializeField] T[] externalPieces;

        protected List<T> pieces;

        public override bool IsSolved {
        	get {
        		foreach (var piece in pieces)
        			if (!piece.IsSolved) return false;
        		return true;
        	}
        }

        public override bool Solve() {
        	foreach (var piece in pieces)
        		if (!piece.IsSolved) piece.Solve();
        	return IsSolved;
        }

        public override void Awake() {
            pieces = new List<T>(externalPieces);
            foreach (Transform child in transform) {
                var list = child.gameObject.GetComponents<T>();
                if (list==null || list.Length<=0) continue;
                foreach (var elem in list)
                    if (elem.GetType()==typeof(T))
                        pieces.Add((T) elem);
            } if (pieces==null || pieces.Count<=0)
                throw new System.Exception("No Pieces!");
        }
    }
}

