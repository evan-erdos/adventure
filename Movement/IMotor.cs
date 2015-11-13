/* Ben Scott * bescott@andrew.cmu.edu * 2015-10-27 * IMotor */

using UnityEngine;

namespace PathwaysEngine.Movement {
	public interface IMotor {
		bool isGrounded { get; }
		bool wasGrounded { get; }
		bool isSliding { get; }
		bool isSprinting { get; }
		bool isJumping { get; }
		bool wasJumping { get; }

		bool isDead { get; set; }
		//float Gravity { get; set; }
		Vector3 velocity { get; set; }
		Vector3 position { get; set; }
		Vector3 localPosition { get; set; }

		void Kill();

		void OnCollisionEvent(Collision collision);
	}
}
