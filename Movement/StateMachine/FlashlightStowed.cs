/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-07 * Flashlight@wield */

using UnityEngine;
using System.Collections;
using adv=PathwaysEngine.Adventure;
using inv=PathwaysEngine.Inventory;


namespace PathwaysEngine.Movement.StateMachine {


    public class FlashlightStowed : StateMachineBehaviour {


        override public void OnStateExit(
        				Animator a,
        				AnimatorStateInfo asi,
        				int i) {
            ((inv::Flashlight) ((adv::Person) Pathways.player)
            	.left.objHand.GetComponent<inv::Flashlight>()).on = false;
        }
    }
}
