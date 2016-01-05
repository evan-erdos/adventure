/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-18 * Combinator */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using adv=PathwaysEngine.Adventure;


namespace PathwaysEngine.Puzzle {


    /** `Combinator` : **`class`**
     *
     * A Set of `IPiece`s which must be solved in a particular
     * configuration for the system to be considered solved.
     **/
    public class Combinator<T> : Piece<T>, ICombinator<T> {

        [SerializeField] protected IPiece<T>[] externalPieces;
        [SerializeField] protected T[] solveState;


        public bool IsReadOnly => false;

        public int Count => Pieces.Count;


        /** `Pieces` : **`IPiece<T> -> T`**
         *
         * Denotes the "solved" state of the current system.
         **/
        public IDictionary<IPiece<T>,T> Pieces => pieces;
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


        public void Add(IPiece<T> o) =>
            pieces[o] = default (T);

        public void Clear() => Pieces.Clear();

        public bool Contains(IPiece<T> o) =>
            Pieces.ContainsKey(o);

        public void CopyTo(IPiece<T>[] a, int n) =>
            Pieces.Keys.CopyTo(a,n);

        public bool Remove(IPiece<T> o) =>
            Pieces.Remove(o);

        public IEnumerator<IPiece<T>> GetEnumerator() =>
            (IEnumerator<IPiece<T>>) Pieces.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            Pieces.GetEnumerator();

        public override bool Solve(T condition) {
            foreach (var piece in pieces)
                if (!EqualityComparer<T>.Default.Equals(
                                piece.Key.Condition,
                                piece.Value))
                    piece.Key.Solve(piece.Value);
            return IsSolved;
        }


        public override void Awake() {
            var list = new List<IPiece<T>>();
            if (externalPieces!=null)
                foreach (var elem in externalPieces)
                    list.Add(elem);
            var bits = new List<T>(solveState);
            foreach (Transform child in transform) {
                var children = child.gameObject.GetComponents<adv::Thing>();
                if (children==null || children.Length<=0) continue;
                foreach (var elem in children)
                    if (elem is IPiece<T>)
                        list.Add((IPiece<T>) elem);
            } if (list.Count<=0 || bits.Count!=list.Count)
                throw new System.Exception(
                    $"Bad Pieces!\n{list.Count},{bits.Count}");
            foreach (var elem in list)
                pieces.Add(elem,bits[list.IndexOf(elem)]);
        }
    }
}

