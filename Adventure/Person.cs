/* Ben Scott * bescott@andrew.cmu.edu * 2015-08-04 * Person */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using type=System.Type;
using invt=PathwaysEngine.Inventory;
using mvmt=PathwaysEngine.Movement;
using util=PathwaysEngine.Utilities;
using maps=PathwaysEngine.Adventure.Setting;
using stat=PathwaysEngine.Statistics;

namespace PathwaysEngine.Adventure {


    /** `Person` : **`class`**
    |*
    |* This is a pretty important class, as it defines some of
    |* the most important behaviours that apply to `Person`s.
    |**/
    partial class Person : Creature {

        public bool sloppyIFHASKEY = false;

        public float radius = 25f;
        Body body;
        public proper_name fullName;
        public maps::Area area;
        public maps::Room room;
        mvmt::IMotor motor;

        public LayerMask layerMask;

        public override bool IsDead {
            get { return motor.IsDead; }
            set { IsDead = value; } }

        public Vector3 Position {
            get { return motor.Position; } }

        public override stat::Set stats { get; set; }

        public List<invt::Item> nearbyItems {
            get {
                _nearbyItems = GetNearbyItems();
                return _nearbyItems;
            }
        } List<invt::Item> _nearbyItems;

        public List<Thing> NearbyThings {
            get { return GetNearbyThings(); } }

        public invt::ItemSet holdall {
            get { if (_holdall==null)
                _holdall = new invt::ItemSet();
                return _holdall;
            } set {
                foreach (var item in _holdall)
                    value.Add(item);
                _holdall = value;
            }
        } invt::ItemSet _holdall;

        public override void Awake() { base.Awake();
            var temp = new util::Anchor[(int) Corpus.All];
            layerMask = LayerMask.NameToLayer("Thing")|
                        LayerMask.NameToLayer("Item");
            motor = GetComponentInChildren<mvmt::IMotor>();
            foreach (var elem in GetComponentsInChildren<util::Anchor>())
                temp[(int) elem.bodyPart] = elem;
            body = new Person.Body(temp);
            //_rigidbody = GetComponentInChildren<Rigidbody>();
        }

        public override void ApplyDamage(float damage) {
            if (stats!=null) return; // set dead to true
        }

        public virtual void Take() { Take(nearbyItems); }

        public virtual void Take(List<invt::Item> list) {
            foreach (var item in list) Take(item); }

        public virtual void Take(invt::Item item) {
            if (holdall.Contains(item))
                throw new System.Exception("Person already has item.");
            item.transform.parent = transform;
            holdall.Add(item);
            item.Take();
        }

        public virtual void Drop() { // Drop(holdall);
            var temp = new List<invt::Item>();
            foreach (var item in holdall) temp.Add(item);
            foreach (var item in temp) Drop(item);
        }

        public virtual void Drop(invt::Item item) {
            if (!holdall.Contains(item))
                throw new System.Exception("Person does not have item to drop!");
            item.transform.parent = null;
            holdall.Remove(item);
            item.Drop();
        }

        public virtual void Wear() {
            foreach (var item in holdall)
                if (item is invt::IWearable)
                    Wear((invt::IWearable) item);
        }

        public virtual void Stow() {
            foreach (var item in holdall)
                if (item is invt::IWearable)
                    Stow((invt::IWearable) item);
        }

        public virtual void Wear(invt::IWearable item) {
            body[item.GetType()] = item; item.Wear(); }

        public virtual void Stow(invt::IWearable item) { item.Stow(); }

        public virtual void Push(IPushable o) { o.Push(); }

        public virtual void Pull(IPullable o) { o.Pull(); }

        public virtual bool Open(IOpenable o) { return o.Open(); }

        public virtual bool Shut(IOpenable o) { return o.Shut(); }

        public void Goto(maps::Area tgt) {
            StartCoroutine(Goto(tgt.level)); }

        IEnumerator Goto(int n) {
            util::CameraFade.StartAlphaFade(Color.black,false,1f);
            yield return new WaitForSeconds(1.1f);
            Application.LoadLevel(n);
        }

        public bool Unlock(Door door) {
            if (!door) return false;
            if (!door.IsLocked) return true;
            if (sloppyIFHASKEY) return true;
            return false;
        }

        public virtual List<T> GetNearby<T>(float r) where T : Thing {
            var temp = Physics.OverlapSphere(
                motor.Position,r,layerMask,
                QueryTriggerInteraction.Collide);
            var list = new List<T>();
            foreach (var elem in temp) {
                var thing = (elem.attachedRigidbody==null)
                    ? elem.gameObject.GetComponent<T>()
                    : elem.attachedRigidbody.GetComponent<T>();
                if (thing && !list.Contains(thing)) list.Add(thing);
            } return list;
        }

        public virtual List<Thing> GetNearbyThings() {
            return GetNearby<Thing>(8f); // radius
        }

        public virtual List<invt::Item> GetNearbyItems() {
            var temp = Physics.OverlapSphere(
                motor.Position,
                4f,LayerMask.NameToLayer("Items"));
            var list = new List<invt::Item>();
            foreach (var elem in temp) {
                if (elem.attachedRigidbody==null) continue;
                var item = elem.attachedRigidbody.GetComponent<invt::Item>();
                if (item && !list.Contains(item) && !item.Held) list.Add(item);
            } return list;
        }

        public void AddCondition(stat::Condition cond) { }

        public void AddCondition(stat::Condition cond,stat::Severity svrt) { }

        class Body {
            invt::IWearable[] list;
            util::Anchor[] anchors;

            public Body(util::Anchor[] anchors) {
                this.list = new invt::IWearable[(int) Corpus.All];
                this.anchors = anchors;
            }

            public invt::IWearable this[Corpus n] {
                get { return list[(int) n]; }
                set { var temp = (invt::IWearable) list[(int) n];
                    if (temp!=null && temp!=value) Player.Stow(temp);
                    var item = (invt::Item) value;
                    item.transform.parent = anchors[(int) n].transform;
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localRotation = Quaternion.identity;
                    list[(int) n] = value;
                }
            }

            public invt::IWearable this[type T] {
                get { return list[Type_Index(T)]; }
                set { var temp = list[Type_Index(T)];
                    if (temp!=null && temp!=value) Player.Stow(temp);
                    var item = (invt::Item) value;
                    item.transform.parent =
                        anchors[(int) Type_Index(T)].transform;
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localRotation = Quaternion.identity;
                    list[Type_Index(T)] = value;
                }
            }

            static public int Type_Index(type T) {
                if (T.DerivesFrom<invt::Helmet>())
                    return (int) Corpus.Head;
                if (T.DerivesFrom<invt::Necklace>())
                    return (int) Corpus.Neck;
                if (T.DerivesFrom<invt::Armor>())
                    return (int) Corpus.Chest;
                if (T.DerivesFrom<invt::Cloak>())
                    return (int) Corpus.Back;
                if (T.DerivesFrom<invt::Backpack>())
                    return (int) Corpus.Back;
                if (T.DerivesFrom<invt::Belt>())
                    return (int) Corpus.Waist;
                if (T.DerivesFrom<invt::Clothes>())
                    return (int) Corpus.Frock;
                if (T.DerivesFrom<invt::Bracers>())
                    return (int) Corpus.Arms;
                if (T.DerivesFrom<invt::Pants>())
                    return (int) Corpus.Legs;
                if (T.DerivesFrom<invt::Gloves>())
                    return (int) Corpus.Hands;
                if (T.DerivesFrom<invt::Shoes>())
                    return (int) Corpus.Feet;
                if (T.DerivesFrom<invt::Flashlight>())
                    return (int) Corpus.Other;
                if (T.DerivesFrom<invt::Lamp>())
                    return (int) Corpus.HandL;
                if (T.DerivesFrom<invt::Weapon>())
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


