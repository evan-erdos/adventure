/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-18 * Puzzle */

using UnityEngine;
using System.Collections.Generic;


/** `PathwaysEngine.Puzzle` : **`namespace`**
|*
|* This namespace of the engine deals with `Puzzle`s, their
|* `Piece`s (which, in this engine's somewhat idiosyncratic
|* vocabulary, simply refers to a component / constituent part
|* of a larger puzzle structure), and all manner of other bits
|* which relate to having interactive puzzles in a game.
|**/
namespace PathwaysEngine.Puzzle {


    /** `OnSolved()` : **`event`**
    |*
    |* Multicast delegate for the transfer of control from the
    |* lowest level `Piece` to the highest level structures.
    |* When The particular `Piece` is `Solve`d, then the parent
    |* is notified via `OnSolve`.
    |*
    |* - `sender` : **`object`**
    |*     object which sent this event
    |*
    |* - `e` : **`EventArgs`**
    |*     typical event arguments
    |*
    |* - `wasSolved` : **`bool`**
    |*     was the base `IPiece` solved when this was sent?
    |**/
    delegate bool OnSolved(
                    object sender,
                    System.EventArgs e,
                    bool wasSolved);


    /** `IPiece` : **`interface`**
    |*
    |* In order to have a fully-modular system for solving any
    |* puzzle, the interfaces to a puzzle must be as generic as
    |* possible. A `Piece` is a component of a larger `Puzzle`
    |* system, which changes the state of that `Puzzle` as the
    |* `Player` attempts to solve it.
    |**/
    interface IPiece {


        /** `OnSolved()` : **`event`**
        |*
        |* Event callback for inversion of control when solving
        |* puzzles via `Solve`ing `Piece`s in this namespace.
        |**/
        event OnSolved SolveEvent;


        /** `IsSolved` : **`bool`**
        |*
        |* This property represents the final game-state of the
        |* `IPiece`. It has a getter & setter because it may be
        |* adventageous for other `IPiece`s (or anyone else) to
        |* be able to override if a child `IPiece` is solved or
        |* not. This should allow for chaining behavior, i.e.,
        |* if a particular part of a puzzle is solved, and if
        |* that particular aspect of the puzzle makes this part
        |* irrelevant, then it will be able to set this
        |* property to true foreach of its children.
        |**/
        bool IsSolved { get; set; }


        /** `Solve()` : **`bool`**
        |*
        |* Generic approach to solving / resolving aspects of a
        |* larger puzzle, or perhaps just this piece. This may
        |* mean `Solve`-ing a lever by pulling it, or putting
        |* the last piece of an actual, cardboard puzzle in
        |* place. The function call is made to attempt to solve
        |* the puzzle, while the above property determines if
        |* it has been solved or not.
        |**/
        bool Solve();

    }


    /** `IPieceIterator` : **`interface`**
    |*
    |* For puzzles where there are sub-groups of `IPiece`s who
    |* need to have their state changed on the basis of higher-
    |* level components, this allows other classes to deal with
    |* this in an abstract way. If declared on a value type, it
    |* will simly operate on those values. An example of this
    |* usage would be if it were declared on `int`: this would
    |* likely be representative of a single breaker in a
    |* combination lock. If declared on a reference type (any
    |* deriving type of `IPiece` should be the type constraint)
    |* then this iterates through those `IPiece`s.
    |**/
    interface IPieceIterator<T> : IPiece, ICollection<T>
                        where T : IPiece {


        /** `Current` : **`IPiece`**
        |*
        |* Represents the next `IPiece` in this collection.
        |* has no setter, as this state shouldn't be changed
        |* externally.
        |**/
        T Current { get; }


        /** `Next` : **`<T>`**
        |*
        |* Returns the next `T` in the collection.
        |**/
        T Next { get; }


        /** `Advance()` : **`<T>`**
        |*
        |* This changes the state of the iterator, and returns
        |* the next element.
        |**/
        T Advance();
    }


    /** `IPieceCombinator` : **`interface`**
    |*
    |* This interface provides a common way to have individual
    |* `IPiece`s behave together in aggregate, rather than as
    |* individuals. This could be used to define the complex
    |* relationship between different breakers in a lock, or
    |* the behavior between some external forces and a group
    |* of `IPiece`s that all need to be solved in unison.
    |**/
    interface IPieceCombinator<T> : IPiece, ICollection<T> {


        /** `AreSolved` : **`bool`**
        |*
        |* Defines if all the components are accounted for or
        |* solved for by whatever heuristic is provided.
        |**/
        bool AreSolved { get; set; }
    }
}
