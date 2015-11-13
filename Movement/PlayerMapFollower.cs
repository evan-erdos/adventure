/* Ben Scott * bescott@andrew.cmu.edu * 2014-07-07 * Map Follower */

using UnityEngine;
using System.Collections;

namespace PathwaysEngine.Movement {
	public class PlayerMapFollower : MonoBehaviour {
		private Transform rTransform, playerTR;
		private Vector3 playerXY = Vector3.zero;
		private float rotationPlayerY, deltaX, deltaY;

		void Awake () {
			playerTR = GameObject.FindGameObjectWithTag("Player").transform;
			rTransform = transform;
		}

		void Update () {
			if (!playerTR) playerTR = GameObject.FindGameObjectWithTag("Player").transform;
			if (!playerTR) return;
			playerXY.x = Mathf.SmoothDamp(playerXY.x, playerTR.position.x, ref deltaX, 0.1f, 16, Time.smoothDeltaTime);
			playerXY.z = Mathf.SmoothDamp(playerXY.z, playerTR.position.z, ref deltaY, 0.1f, 16, Time.smoothDeltaTime);
			rTransform.position = playerXY;
	//		rTransform.rotation = Quaternion.Euler(0, playerTR.rotation.eulerAngles.y, 0); // For spinning map, player-local
		}
	}
}
