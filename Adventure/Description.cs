/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-04 * Description */

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using type=System.Type;
using YamlDotNet.Serialization;

namespace PathwaysEngine.Adventure {

	public interface IDescribable : ILoggable, IStorable {
		Description description { get; set; }
	}

#if FAIL
	public interface IDescribable<in T> : IDescribable
	where T : ILoggable {
		Description<Thing> description { get; set; } }
#endif

	public class Description : ILoggable {
		public bool seen = false;
		public virtual string format {
			get { return "\n### {0} ###\n{{0}}\n\n{1}"; }
		}
		public Formats[] formats { get; set; }

		public string desc {
			get { return _desc; }
			set { _desc_base = value; }
		} protected string _desc, _desc_base;

		[YamlMember(Alias="init_description")]
		public string init { get; set; }
		[YamlMember(Alias="nouns")]
		public Regex nouns { get; set; }
		[YamlMember(Alias="rand_descriptions")]
		public RandList<string> rand { get; set; }

		public bool IsMatch(string s) {
			return nouns.IsMatch(s); }

		public Description() : this(" ") { }

		public Description(string desc) : this(desc," ") { }

		public Description(string desc,string init)
			: this(desc,init,new Regex(" ")) { }

		public Description(
				string desc,
				string init,
				string nouns)
			: this(desc,init,new Regex(nouns)) { }

		public Description(string desc,string init, Regex vocab) {
			this.desc = desc;
			this.init = init;
			this.nouns = nouns;
		}

		public Description(
				string desc,
				string init,
				params Formats[] formats)
		: this(desc,init) {
			this.formats = formats; }

		public Description(
				string desc,
				string init,
				Regex nouns,
				params Formats[] formats)
		: this(desc,init,nouns) {
			this.formats = formats;
		}

		public virtual string Log() { return desc; }

		public virtual void SetFormat(string s) {
			_desc = Terminal.Format(string.Format(s,
				(seen || init!=null)?(_desc_base):(init)).md());
		}

		public override string ToString() { return desc; }

		public static implicit operator string(Description description) {
			return description.desc; }
	}

	public class Description<T> : Description
						where T : ILoggable {
		public override string format {
			get { return _format; }
		} string _format;

		public override void SetFormat(string s) { base.SetFormat(s);
			_format = s;
		}

		public override string Log() {
			return string.Format(format,desc); }
	}
}