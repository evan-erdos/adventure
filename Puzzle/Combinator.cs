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
    class Combinator<T> : Piece, ICombinator<T>
                where T : IPiece {

        [SerializeField] T[] externalPieces;
        [SerializeField] BitArray solveState;


        public bool IsReadOnly {
            get { return false; } }


        public int Count {
            get { return Pieces.Count; } }


        /** `Pieces` : **`<T> -> bool`**
         *
         * Denotes the "solved" state of the current system.
         **/
        public IDictionary<T,bool> Pieces {
            get { return pieces; } }
        protected Dictionary<T,bool> pieces
            = new Dictionary<T,bool>();


        public override bool IsSolved {
            get {
                foreach (var piece in pieces)
                    if (!piece.Key.IsSolved!=piece.Value)
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


        public void Add(T o) {
            pieces[o] = false; }

        public void Clear() {
            Pieces.Clear(); }

        public bool Contains(T o) {
            return Pieces.ContainsKey(o); }

        public void CopyTo(T[] a, int n) {
            Pieces.Keys.CopyTo(a,n); }

        public bool Remove(T o) {
            return Pieces.Remove(o); }

        IEnumerator IEnumerable.GetEnumerator() {
            return Pieces.GetEnumerator(); }

        public IEnumerator<T> GetEnumerator() {
            return (IEnumerator<T>) Pieces.GetEnumerator(); }


        public override bool Solve() {
            foreach (var piece in pieces)
                if (!piece.Key.IsSolved!=piece.Value)
                    piece.Key.Solve();
            return IsSolved;
        }


        public override void Awake() {
            var list = new List<T>(externalPieces);
            var bits = new BitArray(solveState);
            foreach (Transform child in transform) {
                var children = child.gameObject.GetComponents<T>();
                if (children==null || children.Length<=0) continue;
                foreach (var elem in children)
                    if (elem.GetType()==typeof(T))
                        list.Add((T) elem);
            } if (list.Count<=0 || bits.Count!=list.Count)
                throw new System.Exception("Bad Pieces!");
            foreach (var elem in list)
                pieces.Add(elem,bits[list.IndexOf(elem)]);
        }
    }
}

