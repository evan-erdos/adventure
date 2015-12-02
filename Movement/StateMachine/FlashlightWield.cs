/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-07 * Flashlight@wield */

using UnityEngine;
using System.Collections;
using invt=PathwaysEngine.Inventory;

namespace PathwaysEngine.Movement.StateMachine {
    public class FlashlightWield : StateMachineBehaviour {
        invt::Flashlight flashlight;

        override public void OnStateEnter(Animator a,AnimatorStateInfo asi,int i) {
            if (!flashlight)
                flashlight = Player.left.objHand.GetComponent<invt::Flashlight>();
            flashlight.Worn = true;
        }
    }
}
