/* Ben Scott * bescott@andrew.cmu.edu * 2015-07-01 * Physics Restore */

using UnityEngine;
using System.Collections;


public class PhysicControlRestore : MonoBehaviour {

    public Transform netherReplacement;

    public void PlayerReplace() {
        GameObject childPlayer = gameObject.GetComponentInChildren<CharacterController>().gameObject;
        Transform netherPlayer = Instantiate(netherReplacement, childPlayer.transform.position, childPlayer.transform.rotation) as Transform;
    //  netherPlayer.rigidbody.velocity = gameObject.GetComponentInChildren<CharacterMotor>().velocity;
        netherPlayer.parent = null;
        Destroy(gameObject);
    }
}