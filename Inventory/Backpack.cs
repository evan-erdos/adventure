/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Backpack */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using lit=PathwaysEngine.Literature;

namespace PathwaysEngine.Inventory {


    /** `Backpack` : **`class`**
     *
     * Acts as the main holdall for the `Player`, and cannot be
     * stored, as `Take()`/`Drop()`-ing the backpack will also
     * `Wear()`/`Stow()` it, so it can only act as a container
     * for the `Player`.
     **/
    public class Backpack : Bag, IWearable {

        public bool Worn {
            get { return worn; }
            set { worn = value;
                if (worn) Wear();
                else Stow(); }
        } bool worn;


        public override bool Take() { base.Take();
            return Player.Wear(this); }


        public override bool Drop() { base.Drop();
            return Player.Stow(this); }


        /** `Backpack` : **`destructor`**
         *
         * Drops all the contained `Item`s if `this` is garbage
         * collected for whatever reason (or is `Destroy()`ed).
         **/
        ~Backpack() { DropAll(); }

        public bool Wear() {
            gameObject.SetActive(true);
            lit::Terminal.LogCommand(
                "You put on the backpack.\n");
            return false;
        }

        public bool Stow() {
            lit::Terminal.LogCommand(
                "You take off the backpack.\n");
            gameObject.SetActive(false);
            return false;
        }
    }
}
