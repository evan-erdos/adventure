/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-13 * Movement */

using UnityEngine;
using System.Collections.Generic;
using adv=PathwaysEngine.Adventure;


/** `PathwaysEngine.Movement` : **`namespace`**
|*
|* Deals with the mathematical & effect-based subsystems
|* which define how the `Player` & other `Actor`s move,
|* make sounds, animate, and interact, physically.
|**/
namespace PathwaysEngine.Movement {


    /** `Hands` : **`enum`**
    |*
    |* Defined for left & right hands, this enum indicates the
    |* handedness for items that can be held or otherwise used
    |* with hands.
    |**/
    public enum Hands : byte { Left, Right }


    /** `StepType` : **`enum`**
    |*
    |* Defines all the kinds of unique materials that I have
    |* sound effects for. It uses `PhysicMaterial`s to define
    |* which set of sound effects to use. Values represent the
    |* indexes of each category of step sound in the list.
    |**/
    public enum StepTypes : int {
        Default = 0,   Dirt     = 1,
        Gravel  = 2,   Puddle   = 3,
        Sand    = 4,   Swamp    = 5,
        Water   = 6,   Wood     = 7,
        Glass   = 8,   Concrete = 9}


    /** `IMotor` : **`interface`**
    |*
    |* Common interface to any motor that moves any entity in
    |* the game. This could be a `Person`, the `Player`, or any
    |* other animate entity.
    |**/
    interface IMotor {

        /** `IsGrounded` : **`bool`**
        |*
        |* Is the motor currently grounded?
        |**/
        bool IsGrounded { get; }

        /** `WasGrounded` : **`bool`**
        |*
        |* Was the motor grounded last frame?
        |**/
        bool WasGrounded { get; }

        /** `IsSliding` : **`bool`**
        |*
        |* Is the motor sliding?
        |**/
        bool IsSliding { get; }

        /** `IsSprinting` : **`bool`**
        |*
        |* Is the motor sprinting?
        |**/
        bool IsSprinting { get; }

        /** `IsJumping` : **`bool`**
        |*
        |* Is the motor jumping?
        |**/
        bool IsJumping { get; }

        /** `WasJumping` : **`bool`**
        |*
        |* Was the motor jumping last frame?
        |**/
        bool WasJumping { get; }

        /** `IsDead` : **`bool`**
        |*
        |* Is the motor responsive?
        |**/
        bool IsDead {get;set;}

        /** `Position` : **`<real,real,real>`**
        |*
        |* The global position of this motor.
        |**/
        Vector3 Position {get;set;}

        /** `LocalPosition` : **`<real,real,real>`**
        |*
        |* The local transform of this motor.
        |**/
        Vector3 LocalPosition {get;set;}

        /** `Velocity` : **`<real,real,real>`**
        |*
        |* Current velocity of this motor.
        |**/
        Vector3 Velocity {get;set;}

        /** `Kill()` : **`bool`**
        |*
        |* Kills the motor.
        |**/
        bool Kill();

        /** `OnCollisionEvent()` : **`function`**
        |*
        |* @TODO: Should be removed.
        |**/
        void OnCollisionEvent(Collision collision);
    }
}
