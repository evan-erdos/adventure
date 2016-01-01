/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-13 * Statistics */

using System.Collections;
using System.Collections.Generic;
using Flags=System.FlagsAttribute;
using Type=System.Type;


/** `PathwaysEngine.Statistics` : **`namespace`**
 *
 * Handles all statistical combat / event interactions,
 * including all manner of powers, abilities, dice rolls,
 * and resistances.
 **/
namespace PathwaysEngine.Statistics {


    /** `Stat` : **`class`**
     *
     * Base class of a statistic. Can perform `Check`s and
     * can be used to process a `Hit` or some other roll /
     * event based on statistics.
     **/
    public class Stat {
        public StatTypes statType;
        public bool Check() { return true; }
        public bool Check(Stat stat) { return true; }
        protected uint @value {get;set;}

        public Stat() {  }

        public Stat(StatTypes statType) {
            this.statType = statType; }

        public Stat(StatTypes statType, uint @value)
            : this(statType) { this.@value = @value; }
    }

    public class Set : Stat {//, ICollection<Stat> {
        List<Stat> stats;
        public bool IsSynchronized { get { return false; } }
        public bool IsReadOnly { get { return false; } }
        public int Count { get { return stats.Count; } }
        public object SyncRoot { get { return default (object); } }

        public Set() { }

        public Set(Stat[] stats) {
            this.stats = new List<Stat>(stats); }

        public Set(List<Stat> stats) {
            this.stats = stats; }

        /** `this[]` : **`Stat`**
         *
         * Get stat by name.
         **/
        public Stat this[string s] {
            get { foreach (var elem in stats)
                    if (s==elem.GetType().ToString()) return elem;
                return default (Stat);
            }
        }

        /** `this[]` : **`Stat`**
         *
         * Get stat by Type.
         **/
        public Stat this[Type T] {
            get { foreach (var elem in stats)
                    if (T==elem.GetType()) return elem;
                return default (Stat);
            }
        }

        public void CopyTo(System.Array arr, int i) {
            arr = stats.ToArray(); }

        public IEnumerator GetEnumerator() {
            return stats.GetEnumerator(); }
    }

    public class HealthStats : Set {
        Faculties faculties {get;set;}
        Condition condition {get;set;}
        Diagnosis diagnosis {get;set;}
        Prognosis prognosis {get;set;}

        public HealthStats(StatTypes statType) {
            this.statType = statType; }

        public HealthStats(StatTypes statType, uint @value)
            : this(statType) { this.@value = @value; }

        public void AddCondition(Condition cond) { }

        public void AddCondition(Condition cond,Severity svrt) { }

        public void AddConditions(params Condition[] conds) {
            foreach (var cond in conds) AddCondition(cond); }
    }

    public enum Hits : byte {
        Miss = 0, Graze = 1,
        Hit  = 2, Crit  = 3}

    /** `Damages` : **`enum`**
     *
     * Represents the many types of damage.
     *
     * - `Default` : **`Damages`**
     *     Default damage is direct, and factors into damage
     *     calculations against default resistances only.
     *
     * - `Pierce` : **`Damages`**
     *     Penetrative damage, applies to sharp & very fast
     *     kinds of weapons / missiles.
     *
     * - `Crush` : **`Damages`**
     *     Brute force damage, usually as a result of very
     *     heavy impacts and very strong people.
     *
     * - `Fire` : **`Damages`**
     *     Burning damage.
     *
     * - `Magic` : **`Damages`**
     *     Magical damage.
     **/
    public enum Damages { Default, Pierce, Crush, Fire, Magic }

    /** `Affinities` : **`enum`**
     *
     * Represents the many types of hits.
     *
     * - `Miss` : **`Affinities`**
     *     No hit takes place.
     *
     * - `Graze` : **`Affinities`**
     *     Glancing blow, or extremely ineffective hit.
     *
     * - `Hit` : **`Affinities`**
     *     Default hit, normal effectiveness.
     *
     * - `Crit` : **`Affinities`**
     *     Critical hit, very damaging / extremely effective
     *     against the receiver's resistances.
     **/
    public enum Affinities : byte { Miss, Graze, Hit, Crit }

    public enum StatTypes : byte {
        Health     = 0x00, Endurance  = 0x01,
        Strength   = 0x02, Agility    = 0x03,
        Dexterity  = 0x04, Perception = 0x05,
        Intellect  = 0x06, Memory     = 0x07}

    public enum Severity : byte {
        None     = 0x00, Mild   = 0x01,
        Moderate = 0x02, Severe = 0x03}

    [Flags] public enum Faculties : byte {
        Thinking = 0x00, Breathing = 0x04,
        Moving   = 0x08, Seeing    = 0x0C,
        Walking  = 0x10, Jumping   = 0x14}

    [Flags] public enum Condition : byte {
        Dead      = 0x00, Wounded  = 0x04,
        Shocked   = 0x08, Poisoned = 0x0C,
        Psychotic = 0x10, Stunned  = 0x14,
        Injured   = 0x18, Healthy  = 0x1C}

    [Flags] public enum Diagnosis : byte {
        None         = 0x00, Unknown      = 0x04,
        Polytrauma   = 0x08, Paralysis    = 0x0C,
        Necrosis     = 0x10, Infection    = 0x14,
        Fracture     = 0x18, Ligamentitis = 0x1C,
        Radiation    = 0x20, Enterotoxia  = 0x24,
        Hypovolemia  = 0x28, Hemorrhage   = 0x2C,
        Frostbite    = 0x30, Thermosis    = 0x34,
        Hypothermia  = 0x38, Hyperthermia = 0x3C,
        Hypohydratia = 0x40, Inanition    = 0x44,
        Psychosis    = 0x48, Depression   = 0x4C}

    public enum Prognosis : byte {
        None       = 0x00, Unknown  = 0x04,
        Fatal      = 0x08, Mortal   = 0x0C,
        Grievous   = 0x10, Critical = 0x14,
        Survivable = 0x18, Livable  = 0x1C}
}



