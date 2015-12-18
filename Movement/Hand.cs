/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-07 * Hand */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using inv=PathwaysEngine.Inventory;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine.Movement {
    public class Hand : MonoBehaviour {
        public bool ikActive;
        public Transform objHand;
        public Animator animator;
        public AvatarIKGoal handGoal;
        public Hands hand = Hands.Left;
        public inv::IWieldable heldItem;
        public util::key fire, lamp;

        public Hand() {
            fire = new util::key((n)=>fire.input=n);
            lamp = new util::key((n)=>lamp.input=n);
        }

        public void SwitchItem(inv::IWieldable item) {
        //  if (backpack && (heldItem!=null)) heldItem.Stow();
        //  else heldItem.Drop();
            heldItem = item;

        }

        void Awake() {
            //if (hand==Hands.Right) Player.right = this;
            //else if (hand==Hands.Left) Player.left = this;
        }

        void Start() {
            animator = Pathways.player.GetComponent<Animator>();
            handGoal = (hand==Hands.Left)?
                (AvatarIKGoal.LeftHand):(AvatarIKGoal.RightHand);
        }

        public void Update() {
            if (heldItem!=null && (hand==Hands.Left && lamp.input
            || hand==Hands.Right && fire.input)) heldItem.Use();
        }
    }
}