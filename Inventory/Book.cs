/* Ben Scott * bescott@andrew.cmu.edu * 2015-11-04 * Book */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using lit=PathwaysEngine.Literature;

namespace PathwaysEngine.Inventory {

    partial class Book : Item, lit::IReadable {
        bool waitRead;

        public string Passage {
            get { return passage; }
            set { passage = value.md(); }
        } string passage = "It reads: ";


        public IEnumerator Reading() {
            if (!waitRead) {
                waitRead = true;
                Read();
                yield return new WaitForSeconds(2f);
            }
        }


        public bool Read() {
            lit::Terminal.Log(Passage, lit::Styles.Paragraph);
            return true;
        }


        public override IEnumerator OnMouseOver() {
            while (5f>(Player.Position-transform.position).sqrMagnitude) {
                Pathways.CursorGraphic = Cursors.Look;
                if (Input.GetButtonUp("Fire1") && !waitRead)
                    yield return StartCoroutine(Reading());
                else yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
