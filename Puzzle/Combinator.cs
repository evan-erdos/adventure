/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-18 * Combinator */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PathwaysEngine.Puzzle {


    /** `Combinator` : **`class`**
     *
     * A Set of `IPiece`s which must be solved in a particular
     * configuration for the system to be considered solved.
     **/
    class Combinator<T> : Piece<T>, ICombinator<T> {

        [SerializeField] IPiece<T>[] externalPieces;
        [SerializeField] T[] solveState;


        public bool IsReadOnly {
            get { return false; } }


        public int Count {
            get { return Pieces.Count; } }


        /** `Pieces` : **`IPiece<T> -> T`**
         *
         * Denotes the "solved" state of the current system.
         **/
        public IDictionary<IPiece<T>,T> Pieces {
            get { return pieces; } }
        protected Dictionary<IPiece<T>,T> pieces
            = new Dictionary<IPiece<T>,T>();


        public override bool IsSolved {
            get {
                foreach (var piece in pieces)
                    if (!EqualityComparer<T>.Default.Equals(
                                    piece.Key.Condition,
                                    piece.Value))
                        return false;
                return true;
            }
        }

//        T this[int n] {
//            get { return Pieces[n]; }
//            set { Pieces.Keys[n] = value; } }

//        bool this[T o] {
//            get { return pieces[o]; }
//            set { pieces[o] = value; } }


        public void Add(IPiece<T> o) {
            pieces[o] = default (T); }

        public void Clear() {
            Pieces.Clear(); }

        public bool Contains(IPiece<T> o) {
            return Pieces.ContainsKey(o); }

        public void CopyTo(IPiece<T>[] a, int n) {
            Pieces.Keys.CopyTo(a,n); }

        public bool Remove(IPiece<T> o) {
            return Pieces.Remove(o); }

        public IEnumerator<IPiece<T>> GetEnumerator() {
            return (IEnumerator<IPiece<T>>) Pieces.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() {
            return Pieces.GetEnumerator(); }


        public override bool Solve(T condition) {
            foreach (var piece in pieces)
                if (!EqualityComparer<T>.Default.Equals(
                                piece.Key.Condition,
                                piece.Value))
                    piece.Key.Solve(piece.Value);
            return IsSolved;
        }


        public override void Awake() {
            var list = new List<IPiece<T>>(externalPieces);
            var bits = new List<T>(solveState);
            foreach (Transform child in transform) {
                var children = child.gameObject.GetComponents<T>();
                if (children==null || children.Length<=0) continue;
                foreach (var elem in children)
                    if (elem.GetType()==typeof(IPiece<T>))
                        list.Add((IPiece<T>) elem);
            } if (list.Count<=0 || bits.Count!=list.Count)
                throw new System.Exception("Bad Pieces!");
            foreach (var elem in list)
                pieces.Add(elem,bits[list.IndexOf(elem)]);
        }
    }
}

