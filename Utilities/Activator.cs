/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-11 * Activator */

using UnityEngine;
using System.Collections.Generic;


namespace PathwaysEngine.Utilities {


    /** `IActivator` : **`interface`**
     *
     * Interface to an `Activator<T>`, which encapsulates the
     * oftentimes messy business of enabling and disabling
     * enormous numbers of `Component`s in a complex structure.
     **/
    public interface IActivator {


        /** `IsActivated` : **`bool`**
         *
         * Readonly value, representing if the elements in
         * `list` are active or not.
         **/
        bool IsActivated { get; }


        /** `Initialize` : **`function`**
         *
         * Deriving classes must override this function,
         * which is called after creating `Activator`.
         **/
        void Initialize();


        /** `Activate` : **`function`**
         *
         * Enables everything in `list` and sets `IsActivated`
         * to true.
         **/
        void Activate();


        /** `Deactivate` : **`function`**
         *
         * Disables everything in `list` and sets
         * `IsActivated` to false.
         **/
        void Deactivate();
    }


    /** `Activator<T>` : **`abstract`**
     *
     * Abstract base for an `Activator<T>`, which encapsulates
     * the oftentimes messy business of enabling and disabling
     * enormous numbers of `Component`s and `GameObject`s in a
     * complex structure. Requires deriving classes to
     * implement `Initialize`, to add things to the list.
     * - `<T>` : **`Type`**
     *    - Type, must be `Behaviour` to use the `enable`
     *      property for deactivation.
     **/
    public abstract class Activator<T> : IActivator
                               where T : Behaviour {
        protected List<T> list = new List<T>();

        public bool IsActivated {
            get { return isActivated; }
        } bool isActivated = true;

        public abstract void Initialize();

        public void Activate() { Activate(true); }

        public void Deactivate() { Activate(false); }

        void Activate(bool t) {
            isActivated = t;
            foreach (var elem in list)
                elem.enabled = t;
        }
    }
}
