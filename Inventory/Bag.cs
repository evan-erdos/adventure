/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-24 * Bag */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PathwaysEngine.Inventory {


    public class Bag : ItemCollection {

        public string descContents {
            get { if (this.Count<1) return "";
                var s = $"<cmd>The</cmd> {Name} <cmd>contains:</cmd>";
                foreach (var item in this)
                    s += $"\n<cmd>-</cmd> {item}";
                return s;
            }
        }

        public override string Template => $@"
{base.Template}
{descContents}";

        public void DropAll() {
            foreach (var item in this) item.Drop(); }

        public virtual void OnDestroy() => DropAll();

        public override void Deserialize() =>
            Pathways.Deserialize<Bag,Bag_yml>(this);

    }
}
