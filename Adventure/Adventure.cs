/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-13 * Adventure */

using stat=PathwaysEngine.Statistics;


/** `PathwaysEngine.Adventure` : **`namespace`**
|*
|* One of the largest namespaces in the engine, and deals
|* with the class hierarchy among `Actor`s, the `Parser`
|* class and all related text-interface aspects of the
|* engine (it's namespace alias is `intf`, short for
|* "Interactive Fiction").
|**/
namespace PathwaysEngine.Adventure {


    /** `Corpus` : **`enum`**
    |*
    |* Defines hex values for parts of the body, for use with
    |* both the `Movement` & `Inventory` namespaces.
    |**/
    public enum Corpus : byte {
        Head  = 0x0, Neck  = 0x1,
        Chest = 0x2, Back  = 0x3,
        Waist = 0x4, Frock = 0x5,
        Arms  = 0x6, Legs  = 0x7,
        Hands = 0x8, Feet  = 0x9,
        HandL = 0xA, HandR = 0xB,
        Other = 0xE, All   = 0xF};


    /** `Parse()` : **`delegate`**
    |*
    |* The standard `Event`/`Delegate` for commands from
    |* the `Parser` class.
    |*
    |* - `c` : **`Command`**
    |*     Default `Command` struct, sometimes unused, but
    |*     means that this function is a `Parse` delegate.
    |**/
    public delegate void Parse(Command c);


    /** `IDescribable` : **`interface`**
    |*
    |* Shared interface for all sorts of describable things.
    |**/
    public interface IDescribable : ILoggable, IStorable {


        /** `description` : **`Description`**
        |*
        |* Description object, to be queried by `Terminal`.
        |**/
        Description description { get; set; }
    }


    /** `IReadable` : **`interface`**
    |*
    |* Interface to anything that can be read.
    |**/
    public interface IReadable {


        /** `Read()` : **`function`**
        |*
        |* Function to call when reading something.
        |**/
        void Read();
    }


    /** `IOpenable` : **`interface`**
    |*
    |* Interface to anything that can be opened & closed.
    |**/
    public interface IOpenable {


        /** `Open()` : **`bool`**
        |*
        |* This is called when the instance needs to be opened,
        |* and returns a boolean value denoting if this action
        |* was successful.
        |**/
        bool Open();


        /** `Shut()` : **`bool`**
        |*
        |* This is called when the instance needs to be closed,
        |* and returns a boolean value denoting if this action
        |* was successful.
        |**/
        bool Shut();
    }


    /** `IPushable` : **`interface`**
    |*
    |* Interface to anything that can be pushed.
    |**/
    public interface IPushable {


        /** `Push` : **`function`**
        |*
        |* This is called when the instance should be pushed.
        |**/
        void Push();
    }


    /** `IPullable` : **`interface`**
    |*
    |* Interface to anything that can be pulled.
    |**/
    public interface IPullable {


        /** `Pull` : **`bool`**
        |*
        |* This is called when the instance should be pulled.
        |**/
        void Pull();
    }

#if FAIL
    public interface IDescribable<in T> : IDescribable
    where T : ILoggable {
        Description<Thing> description { get; set; } }
#endif


    /** `ILiving` : **`interface`**
    |*
    |* Common interface for any entity which can take
    |* damage and be killed.
    |**/
    interface ILiving : IDescribable {


        /** `IsDead` : **`bool`**
        |*
        |* Is this being dead?
        |**/
        bool IsDead { get; set; }


        /** `Stats` : **`stat::Set`**
        |*
        |* Set of statistics for this being.
        |**/
        stat::Set stats { get; set; }


        /** `ApplyDamage()` : **`function`**
        |*
        |* Applies damage to this being.
        |* - `damage` : **`real`**
        |**/
        void ApplyDamage(float damage);
    }
}



