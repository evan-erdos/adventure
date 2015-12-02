/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Weapon */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Inventory {
    public partial class Weapon : Item, IWieldable {
        public enum WeaponStates : byte { Stowed, Held, Aimed, Crossed };
        public WeaponStates WeaponState = WeaponStates.Held;
        public float rate;
        public AudioClip[] attackSounds;
        public LayerMask layerMask;

        public bool Worn { get; set; }

        public uint Uses { get; set; }

        public void Use() { Attack(); }

        public virtual void Attack() { Terminal.Log(" ahh ohhh nooo"); }

        public void Stow() { }

        public void Wear() { }
    }
}