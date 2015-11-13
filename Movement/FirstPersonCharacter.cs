

using UnityEngine;
using System.Collections;

public class FirstPersonCharacter : MonoBehaviour {
	[SerializeField] private float runSpeed = 8f;               // The speed at which we want the character to move
	[SerializeField] private float strafeSpeed = 4f;            // The threshold at which to strafe
	[SerializeField] private float jumpPower = 5f;
	[SerializeField] private bool walkByDefault = true;			// controls how the walk/run modifier key behaves.
	[SerializeField] private float walkSpeed = 3f;				// The speed at which we want the character to move
	[SerializeField] private AdvancedSettings advanced = new AdvancedSettings();
	[SerializeField] private bool lockCursor = true;

	[System.Serializable] public class AdvancedSettings {
		public float gravityMultiplier = 1f;					// Changes the way gravity effect the player
		public PhysicMaterial zeroFrictionMaterial;            	// Material used for zero friction simulation
		public PhysicMaterial highFrictionMaterial;            	// Material used to stop character sliding down slopes
		public float groundStickyEffect = 5f;					// power of effect - prevents bumping down slopes.
	}

	private CapsuleCollider capsule;                         	// The capsule collider for the first person character
	private const float jumpRayLength = 0.7f;                  	// The length of the ray used for testing against the ground
	public bool grounded { get; private set; }
	private Vector2 input;
	private IComparer rayHitComparer;

	public void Awake() {
		capsule = GetComponent<Collider>() as CapsuleCollider;					// Set up a reference to the capsule collider.
		grounded = true;
		Cursor.lockState = (lockCursor)?(CursorLockMode.Locked):(CursorLockMode.None);
		rayHitComparer = new RayHitComparer();
	}

	void OnDisable() { Cursor.lockState = CursorLockMode.Locked; }

	void Update() {
		if (Input.GetMouseButtonUp(0)) Cursor.lockState = (lockCursor)?(CursorLockMode.Locked):(CursorLockMode.None);
	}


	public void FixedUpdate () {
		float speed = runSpeed;
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		bool jump = Input.GetButton("Jump");
		bool walkOrRun =  Input.GetKey(KeyCode.LeftShift);
		speed = walkByDefault ? (walkOrRun ? runSpeed : walkSpeed) : (walkOrRun ? walkSpeed : runSpeed);
		input = new Vector2(h,v);
		if (input.sqrMagnitude > 1) input.Normalize(); 			// normalize input if it exceeds 1 in combined length:
		// Get a vector which is desired move as a world-relative direction, including speeds
		Vector3 desiredMove = transform.forward * input.y * speed + transform.right * input.x * strafeSpeed;
		float yv = GetComponent<Rigidbody>().velocity.y;						// preserving current y velocity (for falling, gravity)
		if (grounded && jump) { yv += jumpPower; grounded = false;}
		GetComponent<Rigidbody>().velocity = desiredMove + Vector3.up * yv;		// Set the rigidbody's velocity and the ground angle
		if (desiredMove.magnitude > 0 || !grounded) GetComponent<Collider>().material = advanced.zeroFrictionMaterial;
		else GetComponent<Collider>().material = advanced.highFrictionMaterial;
		Ray ray = new Ray(transform.position, -transform.up); 	// Create a ray that points down from the centre
		RaycastHit[] hits = Physics.RaycastAll(ray, capsule.height * jumpRayLength );
		System.Array.Sort (hits, rayHitComparer);
		if (grounded || GetComponent<Rigidbody>().velocity.y < jumpPower * .5f) {
			grounded = false;									// Default value if nothing is detected:
			for (int i = 0; i < hits.Length; i++) {				// Check every collider hit by the ray
				if (!hits[i].collider.isTrigger) {				// Check it's not a trigger
					grounded = true;							// store the ground angle (calculated from the normal)
					GetComponent<Rigidbody>().position = Vector3.MoveTowards(	GetComponent<Rigidbody>().position,
																hits[i].point + Vector3.up * capsule.height*.5f,
																Time.deltaTime * advanced.groundStickyEffect);
					GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, GetComponent<Rigidbody>().velocity.z);
					break;
				}
			}
		}
		Debug.DrawRay(ray.origin, ray.direction * capsule.height * jumpRayLength, grounded ? Color.green : Color.red );
		GetComponent<Rigidbody>().AddForce(Physics.gravity * (advanced.gravityMultiplier - 1));
	}

	class RayHitComparer: IComparer { 							// used for comparing distances
		public int Compare(object x, object y) { return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance); }
	}
}
