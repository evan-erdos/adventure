/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-23 * Random List */

using System.Collections.Generic;
using Random=System.Random;


/** `RandList<T>` : **`List<T>`**
|*
|* Extremely simple wrapper class for `List<T>`, which
|* adds the ability to return a random element from the
|* list.
|*
|* - `<T>` : **`Type`**
|**/
public class RandList<T> : List<T> {
    Random random = new Random();


    /** `Next()` : **`<T>`**
     *
     * Returns a random element from the list.
     **/
    public T Next() {
        return this[random.Next(this.Count)]; }
}