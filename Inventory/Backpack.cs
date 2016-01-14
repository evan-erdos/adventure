/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Backpack */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using static PathwaysEngine.Literature.Terminal;
using lit=PathwaysEngine.Literature;


namespace PathwaysEngine.Inventory {


    /** `Backpack` : **`Bag`**
     *
     * Acts as the main holdall for the `Player`, and cannot be
     * stored, as `Take()`/`Drop()`-ing the backpack will also
     * `Wear()`/`Stow()` it, so it can only act as a container
     * for the `Player`.
     **/
    class Backpack : Bag, IWearable {

        public bool Worn {get;set;}


        public override bool Take() { base.Take();
            return Player.Current.Wear(this); }


        public override bool Drop() { base.Drop();
            return Player.Current.Stow(this); }


        public bool Wear() {
            Worn = true;
            gameObject.SetActive(true);
            Literature.Terminal.Log(
                $"<cmd>You put on the</cmd> {Name.ToLower()}<cmd>.</cmd>");
            return false;
        }


        public bool Stow() {
            Worn = false;
            Literature.Terminal.Log(
                $"<cmd>You take off the</cmd> {Name.ToLower()}<cmd>.</cmd>");
            gameObject.SetActive(false);
            return false;
        }


        public override void Deserialize() =>
            Pathways.Deserialize<Backpack,Backpack_yml>(this);
    }
}
