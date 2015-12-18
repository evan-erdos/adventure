/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-13 * Adventure */

using EventArgs=System.EventArgs;
using lit=PathwaysEngine.Literature;
using inv=PathwaysEngine.Inventory;
using stat=PathwaysEngine.Statistics;


/** `PathwaysEngine.Adventure` : **`namespace`**
|*
|* One of the largest namespaces in the engine, and deals
|* with the class hierarchy among `Actor`s, the
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
        Other = 0xE, All   = 0xF}


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

    /** `ILockable` : **`interface`**
    |*
    |* Interface to anything which can be locked or unlocked.
    |* Also specifies a `Key` object to unlock with.
    |**/
    public interface ILockable {

        /** `IsLocked` : **`bool`**
        |*
        |* Indicates whether or not the object is locked.
        |**/
        bool IsLocked {get;}

        /** `KeyMatch` : **`Key`**
        |*
        |* a `Key` to be checked against when making attempts
        |* to `Lock()` or `Unlock()` the object.
        |**/
        inv::Key LockKey {get;}


        /** `Lock()` : **`bool`**
        |*
        |* Called when an attempt is made to lock this object,
        |* returns true if the action succeeded.
        |*
        |* -`key` : **`Key`**
        |*     optional key to use to lock the object
        |**/
        bool Lock(inv::Key key);


        /** `Unlock()` : **`bool`**
        |*
        |* Called when an attempt is made to unlock the object,
        |* returns true if the action succeeded.
        |*
        |* -`key` : **`Key`**
        |*     optional key to use to unlock the object
        |**/
        bool Unlock(inv::Key key);
    }



    /** `IPushable` : **`interface`**
    |*
    |* Interface to anything that can be pushed.
    |**/
    public interface IPushable {


        /** `Push` : **`bool`**
        |*
        |* This is called when the instance should be pushed.
        |**/
        bool Push();
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
        bool Pull();
    }


    /** `IThing` : **`interface`**
    |*
    |* An interface to all things, such that any class can act
    |* as a `Thing` without having to derive from one.
    |**/
    interface IThing : lit::IDescribable {


        /** `Near` : **`bool`**
        |*
        |* **DEPRECATED**
        |*
        |* Specifies whether or not something is closeby, and
        |* subscribes to `event`s if it is the case. Deriving
        |* classes should ensure that setting it is effective,
        |* especially if it's being set by physics events, as
        |* is the case in the main implementation of `Thing`.
        |**/
        //bool Near {get;set;}


        /** `Seen` : **`bool`**
        |*
        |* Used to determine if the `Player` has knowledge of
        |* what and where the thing is, what it can do, or more
        |* simply, if it's hidden or something.
        |**/
        bool Seen {get;set;}


        /** `ViewEvent` : **`event`**
        |*
        |* This event handles the `View()`ing of anything that
        |* can be seen, which is every `Thing` by default.
        |**/
        //event lit::CommandEvent ViewEvent;
        //void AddEvents(IThing thing);
        //void RemoveEvents(IThing thing);



        /** `Find()` : **`bool`**
        |*
        |* Allows the `Player` to find things via commands.
        |*
        |* - `throw` : **`TextException`**
        |*     the thing cannot be found
        |**/
        bool Find();


        /** `View()` : **`bool`**
        |*
        |* When the `Player` writes a command to **examine** or
        |* **look at** something, the most derived function for
        |* whatever `class` it's called on. This allows any of
        |* the numerous subclasses to customize the way they're
        |* displayed by the `Terminal`. Also handles `event`s
        |* for examining things.
        |*
        |* - `source` : **`object`**
        |*     reference to whatever started the `event`
        |*
        |* - `target` : **`Thing`**
        |*     what should be acted on / should act upon
        |*
        |* - `throw` : **`TextException`**
        |*     the thing cannot be examined
        |**/
        bool View(
            object source,
            Thing target,
            EventArgs e,
            lit::Command c);
    }



    /** `ILiving` : **`interface`**
    |*
    |* Common interface for any entity which can take
    |* damage and be killed.
    |**/
    interface ILiving : lit::IDescribable {


        /** `IsDead` : **`bool`**
        |*
        |* Is this being dead?
        |**/
        bool IsDead { get; set; }


        /** `Stats` : **`Set`**
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



