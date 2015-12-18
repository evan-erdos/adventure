/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-04 * Person */

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Type=System.Type;
using EventArgs=System.EventArgs;
using map=PathwaysEngine.Adventure.Setting;
using inv=PathwaysEngine.Inventory;
using lit=PathwaysEngine.Literature;
using mv=PathwaysEngine.Movement;
using stat=PathwaysEngine.Statistics;
using util=PathwaysEngine.Utilities;


namespace PathwaysEngine.Adventure {


    /** `Person` : **`class`**
    |*
    |* This is a pretty important class, as it defines some of
    |* the most important behaviours that apply to `Person`s.
    |**/
    partial class Person : Creature {
        public float radius = 25f, dist = 4f;
        Body body;
        public mv::Hand right, left;
        public mv::Feet feet;
        internal mv::IMotor motor;
        public lit::proper_name fullName;
        public map::Area area;
        public map::Room room;

        public LayerMask layerMask;

        public override bool IsDead {
            get { return motor.IsDead; }
            set { IsDead = value; } }

        public Vector3 Position {
            get { return motor.Position; } }

        public override stat::Set stats { get; set; }

        public List<inv::Item> nearbyItems {
            get {
                _nearbyItems = GetNearbyItems();
                return _nearbyItems;
            }
        } List<inv::Item> _nearbyItems;

        public List<Thing> NearbyThings {
            get { return GetNearby<Thing>(dist); } }

        public inv::IItemSet Items {
            get { if (items==null)
                items = new inv::ItemList();
                return items;
            } set {
                foreach (var item in items)
                    value.Add(item);
                items = value;
            }
        } inv::IItemSet items;

        public override void Awake() { base.Awake();
            var temp = new util::Anchor[(int) Corpus.All];
            layerMask = ~(LayerMask.NameToLayer("Thing")
                & LayerMask.NameToLayer("Room")
                & LayerMask.NameToLayer("Item"));
            motor = GetComponent<mv::IMotor>();
            if (motor==null)
                motor = GetComponentInChildren<mv::IMotor>();
            if (motor==null)
                throw new System.Exception("!motor");
            feet = GetComponentInChildren<mv::Feet>();
            var hands = GetComponentsInChildren<mv::Hand>();
            foreach (var hand in hands)
                if (hand.hand==mv::Hands.Left) left = hand;
                else right = hand;
            foreach (var elem in GetComponentsInChildren<util::Anchor>())
                temp[(int) elem.bodyPart] = elem;
            body = new Person.Body(temp);
            //_rigidbody = GetComponentInChildren<Rigidbody>();
        }

        public override void ApplyDamage(float damage) {
            if (stats!=null) return; // set dead to true
        }


        public virtual bool Take(lit::Command c) {
            if (nearbyItems.Count==0) return false;
            var temp = new List<inv::Item>();
            if ((new Regex(@"\b(all)\b")).IsMatch(c.input))
                return Player.Take();
            else foreach (var item in nearbyItems)
                if (item.IsMatch(c.input))
                    temp.Add(item);
            if (temp.Count==1)
                return Player.Take(temp[0]);
            return false;
            //else if (temp.Count!=0)
            //    lit::Terminal.Resolve(c,temp);
        }

        public virtual bool Take(List<inv::Item> list) {
            foreach (var item in list) Take(item);
            return false;
        }

        public virtual bool Take(inv::Item item) {
            if (Items.Contains(item)) return false;
            item.transform.parent = transform;
            Items.Add(item);
            return item.Take();
        }

        public virtual bool Take() {
            return Take(nearbyItems); }



        public virtual bool Drop(lit::Command c) {
            var temp = new List<inv::Item>();
            if (Items.Count==0) return false;
            if ((new Regex(@"\ball\b")).IsMatch(c.input))
                return Drop();
            else foreach (var item in Items)
                if (item.description.IsMatch(c.input))
                    temp.Add(item);
            if (temp.Count==1)
                return Drop(temp[0]);
            return false;
        }

        public virtual bool Drop(inv::Item item) {
            if (!Items.Contains(item)) return false;
            item.transform.parent = null;
            Items.Remove(item);
            return item.Drop();
        }

        public virtual bool Drop() {
            var temp = new List<inv::Item>();
            foreach (var item in Items) temp.Add(item);
            foreach (var item in temp) Drop(item);
            return false;
        }


        public virtual bool Wear() {
            foreach (var item in Items)
                if (item is inv::IWearable)
                    Wear((inv::IWearable) item);
            return false;
        }

        public virtual bool Stow() {
            foreach (var item in Items)
                if (item is inv::IWearable)
                    Stow((inv::IWearable) item);
            return false;
        }

        public virtual bool Wear(inv::IWearable item) {
            if ((Corpus) Body.Type_Index(item.GetType())==Corpus.HandL)
                left.SwitchItem((inv::IWieldable) item);
            else body[item.GetType()] = item;
            return item.Wear();
        }

        public virtual bool Stow(inv::IWearable o) {
            return o.Stow(); }

        public virtual bool Push(IPushable o) {
            return o.Push(); }

        public virtual bool Pull(IPullable o) {
            return o.Pull(); }

        public virtual bool Open(IOpenable o) {
            return o.Open(); }

        public virtual bool Shut(IOpenable o) {
            return o.Shut(); }

        public bool Goto(map::Area tgt) {
            //StartCoroutine(Goto(tgt.level));
            return false;
        }

        IEnumerator Goto(int n) {
            util::CameraFade.StartAlphaFade(
                Color.black,false,1f);
            yield return new WaitForSeconds(1.1f);
            SceneManager.LoadScene(n);
        }


        public bool Use(lit::Command c) {
            foreach (var elem in GetNearby<Thing>(dist))
                if (elem is inv::IUsable && c.Fits(elem))
                    return ((inv::IUsable) elem).Use();
            return false;
        }

        public bool Unlock(lit::Command c) {
            foreach (var door in GetNearby<map::Door>(dist))
                if (door.IsMatch(c.input))
                    return Unlock(door);
            return false;
        }

        public bool Lock(ILockable o) {
            if (o==null) return false;
            if (o.IsLocked) return true;
            foreach (var key in Items.GetItems<inv::Key>())
                if (o.LockKey==key) return true;
            return false;
        }

        public bool Unlock(ILockable o) {
            if (o==null) return false;
            if (!o.IsLocked) return true;
            foreach (var key in Items.GetItems<inv::Key>())
                if (o.LockKey==key) return true;
            return false;
        }


        public override bool View(
                        object source,
                        Thing target,
                        EventArgs e,
                        lit::Command c) {
            if (target==this) return View();
            foreach (var thing in GetNearby<Thing>(dist))
                if (c.Fits(thing))
                    return thing.View(this,thing,e,c);
            if (room) return room.View(this,room,e,c);
            throw new lit::TextException(
                "You can't see anything like that here.");
        }

        public override bool View() {
            lit::Terminal.Log(description);
            return true;
        }


        public virtual List<T> GetNearby<T>(float r)
                        where T : Thing {
            var temp = Physics.OverlapSphere(
                Position,r,layerMask,
                QueryTriggerInteraction.Collide);
            var list = new List<T>();
            foreach (var elem in temp) {
                var thing = (elem.attachedRigidbody==null)
                    ? elem.gameObject.GetComponent<T>()
                    : elem.attachedRigidbody.GetComponent<T>();
                if (thing && !list.Contains(thing))
                    list.Add(thing);
            } return list;
        }

        public virtual List<inv::Item> GetNearbyItems() {
            var temp = Physics.OverlapSphere(
                motor.Position,
                4f,LayerMask.NameToLayer("Items"));
            var list = new List<inv::Item>();
            foreach (var elem in temp) {
                if (elem.attachedRigidbody==null) continue;
                var item = elem.attachedRigidbody.GetComponent<inv::Item>();
                if (item && !list.Contains(item) && !item.Held) list.Add(item);
            } return list;
        }

        public void AddCondition(stat::Condition cond) { }

        public void AddCondition(stat::Condition cond,stat::Severity svrt) { }

        class Body {
            inv::IWearable[] list;
            util::Anchor[] anchors;

            public Body(util::Anchor[] anchors) {
                this.list = new inv::IWearable[(int) Corpus.All];
                this.anchors = anchors;
            }

            public inv::IWearable this[Corpus n] {
                get { return list[(int) n]; }
                set { var temp = (inv::IWearable) list[(int) n];
                    if (temp!=null && temp!=value) Player.Stow(temp);
                    var item = (inv::Item) value;
                    item.transform.parent = anchors[(int) n].transform;
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localRotation = Quaternion.identity;
                    list[(int) n] = value;
                }
            }

            public inv::IWearable this[Type T] {
                get { return list[Type_Index(T)]; }
                set { var temp = list[Type_Index(T)];
                    if (temp!=null && temp!=value) Player.Stow(temp);
                    var item = (inv::Item) value;
                    item.transform.parent =
                        anchors[(int) Type_Index(T)].transform;
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localRotation = Quaternion.identity;
                    list[Type_Index(T)] = value;
                }
            }

            static public int Type_Index(Type T) {
                if (T.DerivesFrom<inv::Helmet>())
                    return (int) Corpus.Head;
                if (T.DerivesFrom<inv::Necklace>())
                    return (int) Corpus.Neck;
                if (T.DerivesFrom<inv::Armor>())
                    return (int) Corpus.Chest;
                if (T.DerivesFrom<inv::Cloak>())
                    return (int) Corpus.Back;
                if (T.DerivesFrom<inv::Backpack>())
                    return (int) Corpus.Back;
                if (T.DerivesFrom<inv::Belt>())
                    return (int) Corpus.Waist;
                if (T.DerivesFrom<inv::Clothes>())
                    return (int) Corpus.Frock;
                if (T.DerivesFrom<inv::Bracers>())
                    return (int) Corpus.Arms;
                if (T.DerivesFrom<inv::Pants>())
                    return (int) Corpus.Legs;
                if (T.DerivesFrom<inv::Gloves>())
                    return (int) Corpus.Hands;
                if (T.DerivesFrom<inv::Shoes>())
                    return (int) Corpus.Feet;
                if (T.DerivesFrom<inv::Flashlight>())
                    return (int) Corpus.Other;
                if (T.DerivesFrom<inv::Lamp>())
                    return (int) Corpus.HandL;
                if (T.DerivesFrom<inv::Weapon>())
                    return (int) Corpus.HandR;
                return (int) Corpus.All;
            }
        }
    }
}

#if TODO
    public class Vitality : MonoBehaviour, ILiving {
        public enum DamageStates : byte { Heal, Hurt, Crit, Dead};
        public int weightCurrent, weightCritical;
        public float healthCurrent, healthCritical, RUL, TTF;
        public float bodyMass, bodyTemp, bodyWater;
        public double staminaCurrent, staminaCritical;
        public Faculties faculties { get; set; }
        public Condition condition { get; set; }
        public Diagnosis diagnosis { get; set; }
        public Prognosis prognosis { get; set; }
        public Severity[] severityFaculties { get; set; }
        public Severity[] severityCondition { get; set; }
        public Severity[] severityDiagnosis { get; set; }
        public Severity[] severityPrognosis { get; set; }
        public AudioSource au;
        public AudioClip[] soundsHurt, soundsHeal;

        public Vitality() {
            bodyMass            = 80;
            weightCurrent       = 0;
            weightCritical      = 40;
            healthCurrent       = 128.0f;
            healthCritical      = 128.0f;
            staminaCurrent      = 64.0;
            staminaCritical     = 64.0;
            condition           = Condition.Healthy;
            faculties           = Faculties.All;
            diagnosis           = Diagnosis.None;
            prognosis           = Prognosis.None;
            severityFaculties   = new Severity[8];
            severityCondition   = new Severity[8];
            severityDiagnosis   = new Severity[20];
            severityPrognosis   = new Severity[8];
        }

        public void Awake() { au = (GetComponent<AudioSource>()) ?? (gameObject.AddComponent<AudioSource>()); }

        public void ApplyDamage(float damage) {
            if (damage==0) return;
            DamageStates state = (damage>0) ? (DamageStates.Hurt):(DamageStates.Heal);
            byte ind = 0;
            healthCurrent -= damage;
            state = (healthCurrent>0) ? (state):(DamageStates.Dead);
            switch (state) {
                case DamageStates.Heal :
                    ind = (byte) Random.Range(0,soundsHeal.Length); break;
                case DamageStates.Hurt :
                    ind = (byte) Random.Range(0,soundsHurt.Length>>1); break;
                case DamageStates.Crit :
                    ind = (byte) Random.Range(soundsHurt.Length>>1,soundsHurt.Length); break;
                case DamageStates.Dead :
                    ind = (byte) soundsHurt.Length; break;
            } au.PlayOneShot(soundsHurt[(int)ind], (Mathf.Abs((int) damage)>>2)/au.volume);
        }

        public void AddCondition(Condition cond) { severityFaculties[((byte) cond)/4]++; }

        public void AddCondition(Condition cond, Severity srvt) { severityFaculties[((byte) cond)/4]+=(byte) srvt; }
    }
}

#endif


