/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Potion */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using stat=PathwaysEngine.Statistics;

namespace PathwaysEngine.Inventory {


    /** `Potion` : **`class`**
    |*
    |* This kind of `Item` can be consumed, and will then have
    |* some arbitrary effect on game state, or not, who knows?
    |**/
    public partial class Potion : Item {

        //public stat::StatSet Effect {get;set;}

        public override void Awake() { base.Awake();

        }
    }
}
