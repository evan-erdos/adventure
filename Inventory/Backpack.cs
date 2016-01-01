/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Backpack */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using lit=PathwaysEngine.Literature;
//using static PathwaysEngine.Literature.Terminal;


namespace PathwaysEngine.Inventory {


    /** `Backpack` : **`class`**
    |*
    |* Acts as the main holdall for the `Player`, and cannot be
    |* stored, as `Take()`/`Drop()`-ing the backpack will also
    |* `Wear()`/`Stow()` it, so it can only act as a container
    |* for the `Player`.
    |**/
    class Backpack : Bag, IWearable {


        public bool Worn {get;set;}


        public override bool Take() { base.Take();
            return Player.Wear(this); }


        public override bool Drop() { base.Drop();
            return Player.Stow(this); }


        /** `Backpack` : **`destructor`**
        |*
        |* Drops all the contained `Item`s if `this` is garbage
        |* collected for whatever reason (or is `Destroy()`ed).
        |**/
        ~Backpack() { DropAll(); }

        public bool Wear() {
            Worn = true;
            gameObject.SetActive(true);
            PathwaysEngine.Literature.Terminal.LogCommand("You put on the backpack.");
            return false;
        }

        public bool Stow() {
            Worn = false;
            PathwaysEngine.Literature.Terminal.LogCommand("You take off the backpack.");
            gameObject.SetActive(false);
            return false;
        }
    }
}
