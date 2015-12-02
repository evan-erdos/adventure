/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-13 * Libraries */

using System.Collections.Generic;


/** `PathwaysEngine.Libraries` : **`namespace`**
|*
|* This namespace contains very generic library & framework
|* code, external libraries, and other loose ends. Be aware
|* that code in this folder is not necessarily part of the
|* `PathwaysEngine`, and may not even be in the namespace.
|**/
namespace PathwaysEngine.Libraries {


    /** `IGraph<T>` : **`interface`**
    |*
    |* Interface to a generic Graph implementation.
    |**/
    public interface IGraph<T> where T : INode<T> {


        /** `AddEdge` : **`bool`**
        |*
        |* Adds an `IEdge<T>` between `vertex0` & `vertex1`.
        |**/
        bool AddEdge(T vertex0, T vertex1);


        /** `AddVertex` : **`bool`**
        |*
        |* Adds a vertex to the graph.
        |**/
        bool AddVertex(T vertex);


        /** `GetDistance` : **`int`**
        |*
        |* Gets the Distance between two vertices.
        |**/
        int GetDistance(T vertex0, T vertex1);
    }


    /** `INode<T>` : **`interface`**
    |*
    |* Interface to a generic node to be used with `IGraph<T>`.
    |**/
    public interface INode<T> {


        /** `Depth` : **`int`**
        |*
        |* Depth represents this node's distance from the
        |* parent node.
        |**/
        int Depth { get; }


        /** `Count` : **`int`**
        |*
        |* The number of adjacent nodes.
        |**/
        int Count { get; }


        /** `Name` : **`string`**
        |*
        |* An optional name for this node.
        |**/
        string Name { get; }


        /** `Nodes` : **`ICollection<INode<T>>`**
        |*
        |* The collection of this node's adjacent nodes.
        |**/
        ICollection<INode<T>> Nodes { get; }
    }
}
