/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Weapon */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using lit=PathwaysEngine.Literature;

namespace PathwaysEngine.Inventory {
    public partial class Weapon : Item, IWieldable {
        public enum WeaponStates : byte { Stowed, Held, Aimed, Crossed };
        public WeaponStates WeaponState = WeaponStates.Held;
        public float rate;
        public AudioClip[] attackSounds;
        public LayerMask layerMask;

        public bool Worn { get; set; }

        public uint Uses { get; set; }

        public override bool Use() { Attack(); return false; }

        public virtual void Attack() { lit::Terminal.Log(" ahh ohhh nooo"); }

        public bool Stow() { return false; }

        public bool Wear() { return false; }
    }
}