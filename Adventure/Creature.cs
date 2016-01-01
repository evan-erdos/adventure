/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-31 * Creature */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using static PathwaysEngine.Literature.Terminal;
using stat=PathwaysEngine.Statistics;


namespace PathwaysEngine.Adventure {


    /** `Creature` : **`class`**
     *
     * A base class for anything that needs to live breathe, or
     * be killed. Some pretty important classes derive from it,
     * such as `Person`, `Player`, and all sorts of things!
     **/
    partial class Creature : Thing, ILiving {
        public virtual stat::Set stats {get;set;}


        /** `IsDead` : **`bool`**
         *
         * Defines if the `Creature` is alive or dead, and in
         * deriving classes, sometimes has side effects if it's
         * assigned to. This allows for a quite clean assigning
         * syntax, i.e., `creature.IsDead = true;`.
         **/
        public virtual bool IsDead {
            get { return isDead; }
            set { isDead = value; }
        } protected bool isDead = false;


        /** `ApplyDamage` : **`function`**
         *
         * Does damage to the `Creature`, will call `Kill()` if
         * health goes below `0`.
         *
         * - `n` : **`real`**
         *     Damage to be dealt (or health to be added).
         **/
        public virtual void ApplyDamage(float n) { }


        /** `Kill` : **`function`**
         *
         * Kills the `Creature` and logs a message.
         **/
        public virtual void Kill() {
            IsDead = true;
            PathwaysEngine.Literature.Terminal.LogCommand(
                $"{Name} has died.");
        }
    }
}