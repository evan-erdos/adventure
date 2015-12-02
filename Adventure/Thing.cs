/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-22 * Thing */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathwaysEngine.Adventure {


    /** `Thing` : **`class`**
    |*
    |* An extremely important class, `Thing` is the base class
    |* for anything that has any interface with the `Adventure`
    |* namespace, the `Parser`, or the `Statistics`namespace,
    |* or the `Terminal` or any deriving/text-based interfaces.
    |**/
    public partial class Thing : MonoBehaviour, IDescribable {


        /** `Seen` : **`bool`**
        |*
        |* Specifies whether or not the `Player` knows about
        |* this item, or has any other knowledge about `this`.
        |**/
        public virtual bool seen { get; set; }


        /** `uuid` : **`string`**
        |*
        |* This should be an unique identifier that the `*.yml`
        |* `Deserializer` should look for in files.
        |**/
        public string uuid { get { return gameObject.name; } }


        /** `Format` : **`string`**
        |*
        |* Defines how the description for `this` should be
        |* `Log`ged by the `Terminal`. I should specify some
        |* sort of protocol or convention for the arguments, as
        |* deriving classes may rely on this implementation.
        |**/
        public virtual string Format {
            get { return format; }
        } string format = "{0}";


        /** `description` : **`Description`**
        |*
        |* This property encapsulates a `Description` `object`
        |* that every deriving class (e.g., half of all the
        |* `class`es in this engine) interfaces with.
        |**/
        public virtual Description description { get; set; }


        /** `Thing` : **`constructor`**
        |*
        |* Currently unused, as `Unity` doesn't play very well
        |* with the usual methods of instantiating classes, and
        |* prefers to use whatever it uses to create its native
        |* script type, `MonoBehaviour`.
        |**/
        public Thing() { }


        /** `Find()` : **`function`**
        |*
        |* Unimplemented. Will eventually allow the `Player` or
        |* whoever else to use commands to find things.
        |**/
        public virtual void Find() { }


        /** `View()` : **`function`**
        |*
        |* When the `Player` writes a command to **examine** or
        |* **look at** something, the most derived function for
        |* whatever `class` it's called on. This allows any of
        |* the numerous subclasses to customize the way they're
        |* displayed by the `Terminal`.
        |**/
        public virtual void View() {
            Terminal.Log(description); }


        /** `FormatDescription()` : **`function`**
        |*
        |* This externally defines the formatting `string` for
        |* the `Description`, as the formatting `string` isn't
        |* known until after `Awake()`.
        |**/
        public virtual void FormatDescription() {
            description.SetFormat(Format); }


        /** `Log()` : **`function`**
        |*
        |* Returns the description object.
        |**/
        public virtual string Log() { return description; }


        public virtual void Awake() {
            //description = new Description<Thing>();
            GetYAML();
            FormatDescription();
            gameObject.layer = LayerMask.NameToLayer("Thing");
            var _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody!=null) {
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }
        }

        public virtual void Start() { }

        public override string ToString() { return uuid; }

        public static bool operator !(Thing i) { return (i==null); }
    }
}
