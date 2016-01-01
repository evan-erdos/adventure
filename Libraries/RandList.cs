/* Ben Scott * bescott@andrew.cmu.edu * 2016-01-01 * Random List */

using System.Collections.Generic;
using Random=System.Random;


/** `RandList<T>` : **`List<T>`**
 *
 * A simple wrapper class for `List<T>`, which adds the
 * ability to return a random element from the list.
 **/
public class RandList<T> : List<T> {
    Random random = new Random();


    /** `Next()` : **`T`**
     *
     * Returns a random element from the list.
     **/
    public T Next() =>
    	this[random.Next(this.Count)];
}
