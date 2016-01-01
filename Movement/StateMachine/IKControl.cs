/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-01 * IK Control */

using UnityEngine;
using System.Collections;


namespace PathwaysEngine.Movement.StateMachine {


    [RequireComponent(typeof(Animator))] //[ExecuteInEditMode]
    public class IKControl : MonoBehaviour {

        public enum Hands : byte { Left, Right };
        public Hands hand = Hands.Left;
        protected Animator animator;
        AvatarIKGoal handGoal;
        public bool ikActive = false;
        public Transform objHand = null;

        void Awake() {
            animator = GetComponent<Animator>();
            handGoal = (hand==Hands.Left)?
                (AvatarIKGoal.LeftHand):(AvatarIKGoal.RightHand);
        }

        void OnAnimatorIK() {
            if(animator) {
                if(ikActive) {
                    if(objHand!=null) { // Set the target position and rotation
                        animator.SetIKPositionWeight(handGoal,1);
                        animator.SetIKRotationWeight(handGoal,1);
                        animator.SetIKPosition(handGoal,objHand.position);
                        animator.SetIKRotation(handGoal,objHand.rotation);
                    }
                } else {
                    animator.SetIKPositionWeight(handGoal,0);
                    animator.SetIKRotationWeight(handGoal,0);
                    //animator.SetLookAtWeight(0);
                }
            }
        }
    }
}