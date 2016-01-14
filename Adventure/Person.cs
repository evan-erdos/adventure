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
//using static PathwaysEngine.Literature.Terminal;
using mv=PathwaysEngine.Movement;
using stat=PathwaysEngine.Statistics;
using u=PathwaysEngine.Utilities;


namespace PathwaysEngine.Adventure {


    /** `Person` : **`Creature`**
     *
     * This is a pretty important class, as it defines some of
     * the most significant behaviours that apply to people.
     **/
    public class Person : Creature {
        Body body;
        public mv::Hand right, left;
        public mv::Feet feet;
        internal mv::IMotor motor;

        public LayerMask layerMask;

        public override bool IsDead => motor.IsDead;

        public override float Radius => 4f;

        public override Vector3 Position => motor.Position;

        public override stat::Set stats {get;set;}

        public map::Room Room {
            get { return room; }
            set { if (room==value || value==null) return;
                if (room==null || value.Depth>room.Depth)
                    room = value;
            }
        } map::Room room;

        public map::Area Area {
            get { return area; }
            set { if (area==value || value==null) return;
                area = value;
            }
        } map::Area area;

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


        public override void ApplyDamage(float damage) { }


        public virtual bool Take(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            if ((new Regex(@"\b(all)\b")).IsMatch(input))
                return sender.Take();
            var list = new List<inv::Item>();
            foreach (var item in sender.GetNearby<inv::Item>())
                if (item.Fits(input)) list.Add(item);
            if (list.Count<1)
                throw new lit::TextException(
                    "You don't see anything you can take.");
            if (list.Count>1)
                throw new lit::AmbiguityException<inv::Item>(
                    "Which did you want to take: ",list);
            return sender.Take(list[0]);
        }

        public bool Take(List<inv::Item> list) {
            foreach (var item in list)
                if (!Take(item))
                    throw new lit::TextException(
                        $"You can't take the {item}.");
            return true;
        }

        public bool Take(inv::Item item) {
            if (Items.Contains(item)) return false;
            item.transform.parent = transform;
            Items.Add(item);
            return item.Take();
        }

        public bool Take() => Take(GetNearby<inv::Item>());


        public virtual bool Drop(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string s) {
            if ((new Regex(@"\ball\b")).IsMatch(s))
                return sender.Drop();
            var list = new List<inv::Item>();
            foreach (var item in sender.Items)
                if (item.Fits(s)) list.Add(item);
            if (list.Count<1) throw new lit::TextException(
                "You don't have anything you can drop.");
            if (list.Count>1)
                throw new lit::AmbiguityException<inv::Item>(
                    "Which did you want to drop:", list);
            return sender.Drop(list[0]);
        }

        public bool Drop(inv::Item item) {
            if (!Items.Contains(item)) return false;
            if (!item.Drop()) return false;
            Items.Remove(item);
            item.transform.parent = null;
            item.transform.position = transform.position;
            return true; //item.Drop();
        }

        public bool Drop() {
            var temp = new List<inv::Item>();
            foreach (var item in Items) temp.Add(item);
            foreach (var item in temp) Drop(item);
            return false;
        }

        public virtual bool Read(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            var list = new List<inv::Item>();
            foreach (var elem in sender.GetNearby<inv::Item>())
                if (elem is lit::IReadable && elem.Fits(input))
                    list.Add(elem);
            if (list.Count<1)
                throw new lit::TextException(
                    "There's nothing to read nearby.");
            if (list.Count>1)
                throw new lit::AmbiguityException<inv::Item>(
                    "Which did you want to read: ", list);
            return sender.Read((lit::IReadable) list[0]);
        }

        public bool Read(lit::IReadable o) => o.Read();


        public virtual bool Push(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            var list = new List<Thing>();
            foreach (var elem in sender.GetNearby<Thing>())
                if (elem is IPushable && elem.Fits(input))
                    list.Add(elem);
            if (list.Count<1)
                throw new lit::TextException(
                    "You can't push anything nearby.");
            if (list.Count>1)
                throw new lit::AmbiguityException<Thing>(
                    "Which did you want to push: ", list);
            return sender.Push((IPushable) list[0]);
        }

        public bool Push(IPushable o) => o.Push();


        public virtual bool Pull(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            var list = new List<Thing>();
            foreach (var elem in sender.GetNearby<Thing>())
                if (elem is IPushable && elem.Fits(input))
                    list.Add(elem);
            if (list.Count<1)
                throw new lit::TextException(
                    "You can't pull anything nearby.");
            if (list.Count>1)
                throw new lit::AmbiguityException<Thing>(
                    "Which did you want to pull: ", list);
            return sender.Pull((IPushable) list[0]);
        }

        public bool Pull(IPushable o) => o.Pull();


        public virtual bool Open(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            var list = new List<Thing>();
            foreach (var elem in sender.GetNearby<Thing>())
                if (elem is IOpenable && elem.Fits(input))
                    list.Add(elem);
            if (list.Count<1)
                throw new lit::TextException(
                    "You can't open anything nearby.");
            if (list.Count>1)
                throw new lit::AmbiguityException<Thing>(
                    "Which did you want to open: ", list);
            return sender.Open((IOpenable) list[0]);
        }

        public bool Open(IOpenable o) => o.Open();


        public virtual bool Shut(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            var list = new List<Thing>();
            foreach (var elem in sender.GetNearby<Thing>())
                if (elem is IOpenable && elem.Fits(input))
                    list.Add(elem);
            if (list.Count<1)
                throw new lit::TextException(
                    "You can't shut anything nearby.");
            if (list.Count>1)
                throw new lit::AmbiguityException<Thing>(
                    "Which did you want to shut: ", list);
            return sender.Shut((IOpenable) list[0]);
        }

        public bool Shut(IOpenable o) => o.Shut();


        IEnumerator Goto(int n) {
            u::CameraFade.StartAlphaFade(
                Color.black,false,1f);
            yield return new WaitForSeconds(1.1f);
            SceneManager.LoadScene(n);
        }

        public bool Goto(map::Area a) => false;


        public virtual bool Kill(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) => sender.Kill();


        public bool Kill(string s) {
            if (IsDead) return false;
            lit::Terminal.Show();
            lit::Terminal.Clear();
            lit::Terminal.LogImmediate(
                new lit::Message(s,lit::Styles.Alert));
            IsDead = true;
            return motor.Kill();
        }


        public virtual bool Use(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            var list = new List<Thing>();
            foreach (var item in sender.GetNearby<Thing>())
                if (item is inv::IUsable && item.Fits(input))
                    list.Add(item);
            if (list.Count<1)
                throw new lit::TextException(
                    "You don't see anything you can use.");
            if (list.Count>1)
                throw new lit::AmbiguityException<Thing>(
                    "Which did you want to use: ",list);
            return sender.Use((inv::IUsable) list[0]);
        }

        public virtual bool Use(inv::IUsable o) => o.Use();


        public bool Lock(ILockable o) {
            if (o?.IsLocked==true) return true;
            foreach (var key in Items.GetItems<inv::Key>())
                if (o.LockKey==key) return true;
            return false;
        }

        public bool Unlock(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            foreach (var door in sender.GetNearby<map::Door>())
                if (door.Fits(input)) return Unlock(door);
            return false;
        }


        public bool Unlock(ILockable o) {
            if (o?.IsLocked==false) return true;
            foreach (var key in Items.GetItems<inv::Key>())
                if (o.LockKey==key) return true;
            return false;
        }


        public override bool View(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            if (sender==null) return false;
            var list = new List<Thing>();
            foreach (var thing in sender.GetNearby<Thing>())
                if (thing.Fits(input)) list.Add(thing);
            if (list.Count>1)
                throw new lit::AmbiguityException<Thing>(
                    "Which did you want to view: ", list);
            if (list.Count<1)
                foreach (var item in sender.Items)
                    if (item.Fits(input)) list.Add(item);
            if (list.Count<1 && room!=null)
                return room.View(this,e,c,input);
            else if (!room) throw new lit::TextException(
                "You can't see anything like that here.");
            return list[0].View(this,e,c,input);
        }

        public override bool View() {
            lit::Terminal.Log(description);
            return true;
        }

        public bool Look(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) => false;


        public virtual bool Wear(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            if ((new Regex(@"\b(all)\b")).IsMatch(input))
                return sender.Wear();
            var list = new List<inv::Item>();
            foreach (var item in sender.Items)
                if (item is inv::IWearable && item.Fits(input))
                    list.Add(item);
            if (list.Count<1)
                throw new lit::TextException(
                    "You have nothing to wear. ");
            if (list.Count>1)
                throw new lit::AmbiguityException<inv::Item>(
                    "Which did you want to wear: ", list);
            return sender.Wear((inv::IWearable) list[0]);
        }

        public virtual bool Wear(inv::IWearable item) {
            if ((Corpus) Body.Type_Index(item.GetType())==Corpus.HandL)
                left.SwitchItem((inv::IWieldable) item);
            else body[item.GetType()] = item;
            if (item is inv::Lamp) {
                var lamp = ((inv::Lamp) item);
                lamp.transform.parent = left.transform;
                lamp.transform.localPosition = Vector3.zero;
            }

            return item.Wear();
        }

        public virtual bool Wear() {
            foreach (var item in Items)
                if (item is inv::IWearable)
                    Wear((inv::IWearable) item);
            return false;
        }


        public virtual bool Stow(
                        Person sender,
                        EventArgs e,
                        lit::Command c,
                        string input) {
            if ((new Regex(@"\b(all)\b")).IsMatch(input))
                return sender.Stow();
            var list = new List<inv::Item>();
            foreach (var item in sender.Items)
                if (item is inv::IWearable && item.Fits(input))
                    list.Add(item);
            if (list.Count<1)
                throw new lit::TextException(
                    "You have nothing to stow. ");
            if (list.Count>1)
                throw new lit::AmbiguityException<inv::Item>(
                    "Which did you want to stow: ", list);
            return sender.Stow((inv::IWearable) list[0]);
        }

        public virtual bool Stow() {
            foreach (var item in Items)
                if (item is inv::IWearable)
                    Stow((inv::IWearable) item);
            return false;
        }

        public virtual bool Stow(inv::IWearable o) => o.Stow();


        public virtual List<T> GetNearby<T>()
            where T : Thing => GetNearby<T>(Radius);

        public virtual List<T> GetNearby<T>(float radius)
                        where T : Thing {
            var temp = Physics.OverlapSphere(
                Position, radius, layerMask,
                QueryTriggerInteraction.Collide);
            var list = new List<T>();
            foreach (var elem in temp) {
                var thing = (elem.attachedRigidbody==null)
                    ? elem.gameObject.GetComponent<T>()
                    : elem.attachedRigidbody?.GetComponent<T>();
                if (thing && !list.Contains(thing))
                    list.Add(thing);
            } return list;
        }


        public void AddCondition(stat::Condition cond) { }

        public void AddCondition(
                    stat::Condition cond,
                    stat::Severity svrt) { }



        public override void Awake() { base.Awake();
            layerMask =
                ~(LayerMask.NameToLayer("Thing")
                & LayerMask.NameToLayer("Room")
                & LayerMask.NameToLayer("Item"));
            var temp = new u::Anchor[(int) Corpus.All];
            motor = GetComponent<mv::IMotor>()
                ?? GetComponentInChildren<mv::IMotor>();
            if (motor==null)
                throw new System.Exception("!motor");
            feet = GetComponentInChildren<mv::Feet>();
            var hands = GetComponentsInChildren<mv::Hand>();
            foreach (var hand in hands)
                if (hand.hand==mv::Hands.Left) left = hand;
                else right = hand;
            foreach (var elem in GetComponentsInChildren<u::Anchor>())
                temp[(int) elem.bodyPart] = elem;
            body = new Person.Body(temp);
            //_rigidbody = GetComponentInChildren<Rigidbody>();
            motor.KillEvent += Kill;
        }

        public virtual void OnDestroy() {
            motor.KillEvent -= Kill;
        }




        class Body {
            inv::IWearable[] list;
            u::Anchor[] anchors;

            public Body(u::Anchor[] anchors) {
                this.list = new inv::IWearable[(int) Corpus.All];
                this.anchors = anchors;
            }

            public inv::IWearable this[Corpus n] {
                get { return list[(int) n]; }
                set { if (value==null) return;
                    //var temp = (inv::IWearable) list[(int) n];
                    //if (temp!=value) Person.Stow(temp);
                    var item = (inv::Item) value;
                    item.transform.parent = anchors[(int) n].transform;
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localRotation = Quaternion.identity;
                    list[(int) n] = value;
                }
            }

            public inv::IWearable this[Type T] {
                get { return list[Type_Index(T)]; }
                set { if (value==null) return;
                    //var temp = list[Type_Index(T)];
                    //if (temp!=value) Person.Stow(temp);
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


        public override void Deserialize() =>
            Pathways.Deserialize<Person,Person_yml>(this);
    }
}

#if TODO
    public class Vitality : MonoBehaviour, ILiving {
        public enum DamageStates : byte { Heal, Hurt, Crit, Dead};
        public int weightCurrent, weightCritical;
        public float healthCurrent, healthCritical, RUL, TTF;
        public float bodyMass, bodyTemp, bodyWater;
        public double staminaCurrent, staminaCritical;
        public Faculties faculties {get;set;}
        public Condition condition {get;set;}
        public Diagnosis diagnosis {get;set;}
        public Prognosis prognosis {get;set;}
        public Severity[] severityFaculties {get;set;}
        public Severity[] severityCondition {get;set;}
        public Severity[] severityDiagnosis {get;set;}
        public Severity[] severityPrognosis {get;set;}
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


