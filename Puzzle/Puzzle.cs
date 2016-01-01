/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-18 * Puzzle */

using UnityEngine;
using System.Collections.Generic;
using EventArgs=System.EventArgs;


/** `PathwaysEngine.Puzzle` : **`namespace`**
 *
 * This namespace of the engine deals with `Puzzle`s, their
 * `Piece`s (which, in this engine's somewhat idiosyncratic
 * vocabulary, simply refers to a component / constituent part
 * of a larger puzzle structure), and all manner of other bits
 * which relate to having interactive puzzles in a game.
 **/
namespace PathwaysEngine.Puzzle {


    /** `OnSolve<T>` : **`event`**
     *
     * Allows for inversion of control, from the lowest piece
     * to the most complex puzzle. When an `IPiece` is solved,
     * the parent should be notified via this `event`.
     *
     * - `sender` : **`T`**
     *     the `IPiece<T>` sending this event
     *
     * - `e` : **`EventArgs`**
     *     typical `event` arguments
     *
     * - `solved` : **`bool`**
     *     was the `sender` solved?
     **/
    delegate T OnSolve<T>(
                    IPiece<T> sender,
                    EventArgs e,
                    bool solved);


    /** `IPiece<T>` : **`interface`**
     *
     * An `IPiece` is an element of a larger `Puzzle`, and can
     * change the state of said `Puzzle` on the basis of its
     * own configuration. It could be solved, unsolved, or in
     * the case of more complicated base types, a piece could
     * represent a digit on a combination lock. In that case,
     * a given piece might not have its own solution, but could
     * represent a solved puzzle when considered in aggregate.
     **/
    interface IPiece<T> {


        /** `SolveEvent` : **`event`**
         *
         * Event callback for inversion of control. Inheritors
         * must at least notify subscribers in the event that
         * they are solved, and when they become unsolved.
         **/
        event OnSolve<T> SolveEvent;


        /** `IsSolved` : **`bool`**
         *
         * Whether or not the current state is the solution.
         * Inheritors which use value types as their generic
         * arguments should enforce the below contract. For any
         * inheritors which need to represent more complicated
         * states, implementation should maintain some parity
         * between `IsSolved` and their actual configuration.
         *
         * - `ensure` : `IsSolved==(Condition==Solution)`
         **/
        bool IsSolved { get; }


        /** `Condition` : **`T`**
         *
         * An instance's present configuration.
         **/
        T Condition {get;set;}


        /** `Solution` : **`T`**
         *
         * When the configuration of an instance is equal to
         * its `Solution`, it's considered solved.
         **/
        T Solution { get; }


        /** `Solve()` : **`T`**
         *
         * Generic approach to solving / resolving aspects of a
         * larger puzzle, or perhaps just one piece. The action
         * of solving might represent the pull of a lever, or
         * the placement of a piece in an actual jigsaw puzzle.
         *
         * - `condition` : **`T`**
         *   value to attempt to solve with
         **/
        bool Solve(T condition);
    }


    /** `IResponder<T>` : **`interface`**
     *
     * For any given state change, an `IPiece` or any deriving
     * class may want to take some action as a direct result of
     * their being solved or unsolved. Such an action might be
     * to activate some components, check if there's some other
     * precondition to then solve some higher state, etc.
     **/
    interface IResponder<T> : IPiece<T> {


        /** `WhenSolved` : **`bool`**
         *
         * When any attached `IPiece` is solved, it can
         * optionally check against this object's solved state,
         * and act accordingly, (ideally with side effects.)
         *
         * - `piece` : **`IPiece<T>`**
         *     whose `IsSolved` should be checked against
         **/
        bool WhenSolved(IPiece<T> piece);
    }


    /** `IIterator` : **`interface`**
     *
     * For puzzles where there are sub-groups of `IPiece`s who
     * need to have their state changed on the basis of higher-
     * level components, this allows other classes to deal with
     * this in an abstract way. If declared on a value type, it
     * will simly operate on those values. An example of this
     * usage would be if it were declared on `int`: this would
     * likely be representative of a single breaker in a
     * combination lock. If declared on a reference type (any
     * deriving type of `IPiece` should be the type constraint)
     * then this iterates through those `IPiece`s.
     **/
    interface IIterator<T> : IPiece<T>, ICollection<IPiece<T>> {


        /** `Current` : **`IPiece<T>`**
         *
         * Represents the next `IPiece` in this collection.
         * has no setter, as this state shouldn't be changed
         * externally.
         **/
        IPiece<T> Current { get; }


        /** `Next` : **`IPiece<T>`**
         *
         * Returns the next `T` in the collection.
         **/
        IPiece<T> Next { get; }


        /** `Advance()` : **`IPiece<T>`**
         *
         * This changes the state of the iterator, and returns
         * the next element.
         **/
        IPiece<T> Advance();
    }


    /** `ICombinator` : **`interface`**
     *
     * This interface provides a common way to have individual
     * `IPiece`s behave together in aggregate, rather than as
     * individuals. This could be used to define the complex
     * relationship between different breakers in a lock, or
     * the behavior between some external forces and a group
     * of `IPiece`s that all need to be solved in unison.
     **/
    interface ICombinator<T> : IPiece<T>, ICollection<IPiece<T>> {


        /** `Pieces` : **`IPiece<T> -> T`**
         *
         * Denotes the "solved" state of the current system.
         **/
        IDictionary<IPiece<T>,T> Pieces { get; }
    }
}
