/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-07 * Flashlight@wield */

using UnityEngine;
using System.Collections;
using invt=PathwaysEngine.Inventory;

namespace PathwaysEngine.Movement.StateMachine {
    public class FlashlightStowed : StateMachineBehaviour {
        override public void OnStateExit(Animator a,AnimatorStateInfo asi,int i) {
            ((invt::Flashlight) Player.left.objHand
                .GetComponent<invt::Flashlight>()).on = false;
        }
    }
}
