/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-18 * Piece */

using UnityEngine;
using System.Collections.Generic;
using EventArgs=System.EventArgs;
using adv=PathwaysEngine.Adventure;


namespace PathwaysEngine.Puzzle {


    /** `Piece<T>` : **`Thing`**
     *
     * Represents an instance of a puzzle piece, which must be
     * `Solve`d to finish a puzzle.
     **/
    abstract partial class Piece<T> : adv::Thing, IPiece<T> {


        /** `SolveEvent` : **`event`**
         *
         * This property wraps the inherited `solveEvent` event
         *
         **/
        public virtual event OnSolve<T> SolveEvent {
            add { solveEvent += value; }
            remove { solveEvent -= value; }
        } event OnSolve<T> solveEvent;

        public virtual bool IsSolved {
            get { return EqualityComparer<T>.Default.Equals(
                    Condition, Solution); } }

        public T Condition {get;set;}

        public T Solution {
            get { return solution; }
            private set { solution = value; }
        } T solution;


        public virtual bool Solve(T condition) {
            var wasSolved = IsSolved;
            Condition = condition;
            if (IsSolved!=wasSolved)
                OnSolve(this,EventArgs.Empty,IsSolved);
            return IsSolved;
        }

        public virtual T OnSolve(
                        IPiece<T> sender,
                        EventArgs e,
                        bool solved) {
            if (solved) Debug.Log("Piece Solved!");
            return Condition;
        }

        public override void Awake() {
            solveEvent += OnSolve; }

        public virtual void OnDestroy() {
            solveEvent -= OnSolve; }
    }
}

