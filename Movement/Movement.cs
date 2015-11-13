/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-13 * Movement */

/** `PathwaysEngine.Movement` : **`namespace`**
 *
 * Deals with the mathematical & effect-based subsystems
 * which define how the `Player` & other `Actor`s move,
 * make sounds, animate, and interact, physically.
 **/
namespace PathwaysEngine.Movement {

	/** `Hands` : **`enum`**
	 *
	 * Defined for left & right hands, this enum indicates
	 * handedness for use with held items / other events.
	 **/
	public enum Hands : byte { Left, Right };
}
