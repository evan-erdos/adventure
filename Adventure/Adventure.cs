/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-13 * Adventure */

using stat=PathwaysEngine.Statistics;

/** `PathwaysEngine.Adventure` : **`namespace`**
 *
 * One of the largest namespaces in the engine, and deals
 * with the class hierarchy among `Actor`s, the `Parser`
 * class and all related text-interface aspects of the
 * engine (it's namespace alias is `intf`, short for
 * "Interactive Fiction").
 **/
namespace PathwaysEngine.Adventure {

	/** `Corpus` : **`enum`**
	 *
	 * Defines hex values for parts of the body, for use with
	 * both the `Movement` & `Inventory` namespaces.
	 **/
	public enum Corpus : byte {
		Head  = 0x0, Neck  = 0x1,
		Chest = 0x2, Back  = 0x3,
		Waist = 0x4, Frock = 0x5,
		Arms  = 0x6, Legs  = 0x7,
		Hands = 0x8, Feet  = 0x9,
		HandL = 0xA, HandR = 0xB,
		Other = 0xE, All   = 0xF};

	/** `Parse()` : **`delegate`**
	 *
	 * The standard `Event`/`Delegate` for commands from
	 * the `Parser` class.
	 *
	 * - `cmd` : **`command`**
	 *     Default `command` struct, sometimes unused, but
	 *     means that this function is a `Parse` delegate.
	 **/
	public delegate void Parse(command cmd);

	/** `IStorable` : **`interface`**
	 *
	 * Interface for anything that needs to be serialized
	 * from the `yaml` dictionary.
	 **/
	public interface IStorable {

		/** `uuid` : **`string`**
		 *
		 * Unique ID for any serialized value in the `yaml`
		 * dictionary.
		 *
		 * @TODO: Make them only need to be unique for the
		 * local file, and only for particular types.
		 **/
		string uuid { get; }
	}

	/** `ILoggable` : **`interface`**
	 *
	 * That can be logged by the `Terminal`.
	 **/
	public interface ILoggable {

		/** `format` : **`string`**
		 *
		 * base string to be `string.Format`-ed by the
		 * `Terminal` to render it as Markdown.
		 * Could make the name a header, the text italic,
		 * or any combination of other effects. There
		 * should be a specific order of arguments, i.e.,
		 * name first, then desc, etc.
		 **/
		string format { get; }

		/** `Log()` : **`string`**
		 *
		 * base string to be `string.Format`-ed by the
		 * `Terminal` to render it as Markdown.
		 * Could make the name a header, the text italic,
		 * or any combination of other effects. There
		 * should be a specific order of arguments, i.e.,
		 * name first, then desc, etc.
		 **/
		string Log();
	}

	/** `ILiving` : **`interface`**
	 *
	 * Common interface for any entity which can take
	 * damage and be killed.
	 **/
	public interface ILiving : IDescribable {
		bool isDead { get; set; }
		stat::Set stats { get; set; }
		void ApplyDamage(float damage);
	}
}

