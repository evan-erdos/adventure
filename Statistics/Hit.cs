/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-13 * Hit */

using UnityEngine;
using System.Collections.Generic;


namespace PathwaysEngine.Statistics {


    /** `Hit` : **`struct`**
     *
     * Low-level struct to represent a `Hit`. Factors in the
     * damage rolls, the resistances, & its own affinity.
     **/
    public struct Hit {
        public int value;
        public Damages damage;
        public Affinities affinity;

        public Hit(int value)
            : this(value, Damages.Default) { }

        public Hit(int value, Damages damage)
            : this(value, Damages.Default, Affinities.Miss) { }

        public Hit(int value, Affinities affinity)
            : this(value,Damages.Default,affinity) { }

        public Hit(int value, Damages damage, Affinities affinity) {
            this.value = value;
            this.damage = damage;
            this.affinity = affinity;
        }

        public override string ToString() =>
            $@"~{value}:|{affinity}|~";
    }
}
