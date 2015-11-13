/* Ben Scott * bescott@andrew.cmu.edu * 2015-10-27 * Enum Extension */

using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine {

	/* `Enum.GetValues<T>()`
	 *
	 * Basic extension class to allow iteration though enumerations.
	 * I may leave the name `Enum` even though it shadows `System.Enum`,
	 * because it's in my namespace already.
	 * - `<T>` **Type**: Type of the `enum` to get values from.
	 * - `return values` **T[]**: array of values in the `enum`.
	 */
	public static class Enum {
		public static IEnumerable<T> GetValues<T>() {
			return (T[]) System.Enum.GetValues(typeof(T));
		}
	}
}