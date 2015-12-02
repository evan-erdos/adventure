/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-18 * PieceIterator */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Puzzle {

    /** `PieceIterator` : **`Piece`**
    |*
    |* Represents an instance of a puzzle piece, which must be
    |* `Solve`d to finish a puzzle.
    |**/
    class PieceIterator<T> : Piece, IPieceIterator<T>
                   where T : IPiece {

        List<T> list;

        public uint Index {
            get { return index; }
        } uint index = 0;

        public bool IsReadOnly {
            get { return true; } }

        public PieceIterator() {
            list = new List<T>();
        }

        public int Count {
            get { return (int) count; }
        } uint count = 0;

        public T Current {
            get { return list[(int) Index]; } }

        public T Next {
            get { return list[(int) Index+1]; } }

        public T Advance() {
            var o = list[(int) Index];
            index++;
            return o;
        }

        public IEnumerator<T> GetEnumerator() {
            return list.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() {
            return list.GetEnumerator(); }

        public void Clear() { list.Clear(); }

        public void CopyTo(T[] arr, int i) {
            list.CopyTo(arr, i); }

        public bool Contains(T piece) {
            return list.Contains(piece); }

        public void Add(T piece) {
            list.Add(piece); }

        public bool Remove(T piece) {
            return list.Remove(piece); }
    }
}

