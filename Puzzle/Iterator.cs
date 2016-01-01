/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-18 * Iterator */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PathwaysEngine.Puzzle {


    /** `Iterator` : **`Piece`**
    |*
    |* Represents an instance of a puzzle piece, which must be
    |* `Solve`d to finish a puzzle.
    |**/
    class Iterator<T> : Piece<T>, IIterator<T> {

        List<IPiece<T>> list = new List<IPiece<T>>();

        public uint Index {
            get { return index; }
        } uint index = 0;

        public bool IsReadOnly {
            get { return true; } }

        public int Count {
            get { return (int) count; }
        } uint count = 0;

        public IPiece<T> Current {
            get { return list[(int) Index]; } }

        public IPiece<T> Next {
            get { return list[(int) Index+1]; } }

        public IPiece<T> Advance() {
            var o = list[(int) Index];
            index++;
            return o;
        }

        public IEnumerator<IPiece<T>> GetEnumerator() {
            return list.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() {
            return list.GetEnumerator(); }

        public void Clear() { list.Clear(); }

        public void CopyTo(IPiece<T>[] arr, int i) {
            list.CopyTo(arr, i); }

        public bool Contains(IPiece<T> piece) {
            return list.Contains(piece); }

        public void Add(IPiece<T> piece) {
            list.Add(piece); }

        public bool Remove(IPiece<T> piece) {
            return list.Remove(piece); }
    }
}

