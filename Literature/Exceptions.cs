/* Ben Scott * bescott@andrew.cmu.edu * 2015-12-04 * Exceptions */

using Exception=System.Exception;
using adv=PathwaysEngine.Adventure;
using System.Collections.Generic;

namespace PathwaysEngine.Literature {

    /** `TextException` : **`Exception`**
     *
     * `Command`s can be malformed or impossible to achieve for
     * some logical reason (e.g., you cannot put an item
     * container into itself), and therefore need some recourse
     * in such situations to indicate to the `event` handlers
     * that the action has failed.
     **/
    class TextException : Exception {

        internal TextException() : this("Do what, exactly?") { }

        internal TextException(string message)
            : base(message) { }

        internal TextException(string message, Exception inner)
            : base(message,inner) { }
    }



    class AmbiguityException<T> : TextException
                        where T : IDescribable {
        public IList<T> options = new List<T>();

        internal AmbiguityException(
                        string message,
                        IList<T> options)
                : base(message,new TextException()) {
            if (options!=null && options.Count>0)
                this.options = options;
        }

        internal AmbiguityException(
                        string message,
                        params T[] options)
            : this(message,new List<T>(options)) { }

        internal AmbiguityException(IList<T> options)
            : this("Which did you mean:",options) { }

        internal AmbiguityException()
            : this("You will have to be more specific.") { }
    }
}
