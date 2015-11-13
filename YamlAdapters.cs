/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-11 * YamlAdapters */

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using invt=PathwaysEngine.Inventory;
using intf=PathwaysEngine.Adventure;
using maps=PathwaysEngine.Adventure.Setting;
using stat=PathwaysEngine.Statistics;
using util=PathwaysEngine.Utilities;

/** YamlAdapters
 *
 * These classes act as adapters for the `MonoBehaviour`-
 * derived classes, which `YamlDotNet` cannot instantiate.
 * Instances of these classes are instantiated instead, and
 * then populate the main classes in `Awake()`.
 **/

namespace PathwaysEngine {

	namespace Adventure {

		public partial class Thing : MonoBehaviour, IDescribable {
			public class yml : IStorable {
				public string uuid { get; set; }
				public Description description { get; set; }
				public void Deserialize(Thing o) {
					o.description = this.description;
				}
			}
		}

		public partial class Creature : Thing, ILiving {
			public new class yml : Thing.yml {
				public bool isDead { get; set; }

				public void Deserialize(Creature o) {
					base.Deserialize((Thing) o);
					o.isDead = isDead;
				}
			}
		}

		public partial class Person : Creature {
			public new class yml : Creature.yml {
				public maps::Area area { get; set; }
				public maps::Room room { get; set; }

				public void Deserialize(Person o) {
					base.Deserialize((Creature) o);
					o.area = this.area;
					o.room = this.room;
				}
			}
		}

		public partial class Encounter : Thing {
			public new class yml : Thing.yml {
				public bool reuse { get; set; }
				public float time { get; set; }
				public Inputs input { get; set; }

				public void Deserialize(Encounter o) {
					base.Deserialize((Thing) o);
					o.reuse = this.reuse;  o.time = this.time;
					o.input = this.input;
				}
			}
		}

		namespace Setting {
			public partial class Room : Thing {
				public new class yml : Thing.yml {
					public int depth { get; set; }
					public List<invt::Item> items { get; set; }
					public List<maps::Room> nearbyRooms { get; set; }

					public void Deserialize(Room o) {
						base.Deserialize((Thing) o);
						o.items = this.items;
						o.nearbyRooms = this.nearbyRooms;
					}
				}
			}

			public partial class Area : Thing {
				public new class yml : Thing.yml {
					public bool safe { get; set; }
					public int level { get; set; }
					public List<Room.yml> rooms { get; set; }
					public List<Area.yml> areas { get; set; }

					public void Deserialize(Area o) {
						base.Deserialize((Thing) o);
						o.safe = this.safe;  o.level = this.level;
						//o.rooms = this.rooms;  o.areas = this.areas;
					}
				}
			}
		}
	}

	namespace Inventory {
		public partial class Item : intf::Thing, IGainful {
			public new class yml : intf::Thing.yml {
				public int cost { get; set; }
				public float mass { get; set; }
				public void Deserialize(Item o) {
					base.Deserialize((intf::Thing) o);
					o.Mass = this.mass;  o.Cost = this.cost;
				}
			}
		}

		public partial class Lamp : Item, IWearable {
			public new class yml : Item.yml {
				public float time { get; set; }
				public void Deserialize(Lamp o) {
					base.Deserialize((Item) o);
					o.time = time;
				}
			}
		}

		public partial class Crystal : Item, IWieldable {
			public new class yml : Item.yml {
				public uint shards { get; set; }
				public void Deserialize(Crystal o) {
					base.Deserialize((Item) o);
					o.Shards = this.shards;
				}
			}
		}

		public partial class Weapon : Item, IWieldable {
			public new class yml : Item.yml {
				public float rate { get; set; }
				public void Deserialize(Weapon o) {
					base.Deserialize((Item) o);
					o.rate = rate;
				}
			}
		}
	}
}
