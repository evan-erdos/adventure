/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-07 * Flashlight@wield */

using UnityEngine;
using System.Collections;
using adv=PathwaysEngine.Adventure;
using inv=PathwaysEngine.Inventory;


namespace PathwaysEngine.Movement.StateMachine {


    public class FlashlightWield : StateMachineBehaviour {

        inv::Flashlight flashlight;

        override public void OnStateEnter(Animator a,AnimatorStateInfo asi,int i) {
            if (!flashlight)
                flashlight = ((adv::Person) Pathways.player).left.objHand.GetComponent<inv::Flashlight>();
            flashlight.Worn = true;
        }
    }
}
