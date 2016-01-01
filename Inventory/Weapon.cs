/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Weapon */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using static PathwaysEngine.Literature.Terminal;


namespace PathwaysEngine.Inventory {


    public partial class Weapon : Item, IWieldable {
        public enum WeaponStates : byte { Stowed, Held, Aimed, Crossed };
        public WeaponStates WeaponState = WeaponStates.Held;
        public float rate;
        public AudioClip[] attackSounds;
        public LayerMask layerMask;

        public bool Worn {get;set;}

        public uint Uses {get;set;}

        public override bool Use() => Attack();

        public virtual bool Attack() {
            Literature.Terminal.Log(
                " ahh ohhh nooo"); return false; }

        public bool Stow() => false;

        public bool Wear() => false;
    }
}