/* Ben Scott * bescott@andrew.cmu.edu * 2015-09-02 * Message Window */

using UnityEngine;
using ui=UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using intf=PathwaysEngine.Adventure;
using util=PathwaysEngine.Utilities;

namespace PathwaysEngine {
    public class MessageWindow : MonoBehaviour {
        bool wait = false;
        static float delay = 0.5f;
        static ui::Text message_title, message_body;
        public util::key accept;

        public MessageWindow() {
            accept = new util::key((n)=> {
                if (!accept.input && n) {
                    accept.input = n;
                    Disable();
                } else if (!n) accept.input = n; });
        }

        void Awake() {
            Pathways.StateChange += new StateHandler(EventListener);
            Pathways.messageWindow = this;
            foreach (var child in GetComponentsInChildren<ui::Text>())
                if (child.name=="Title") message_title = child;
                else if (child.name=="Body") message_body = child;
            if (!message_title || !message_body)
                Debug.LogError("missing title / body");
        }

        void Start() {
            util::Controls.AddInputListener(this);
            Disable();
        }

        public static void EventListener(
                        object sender,
                        System.EventArgs e,
                        GameStates gameState) {
            if (Pathways.messageWindow.gameObject)
                Pathways.messageWindow.gameObject.SetActive(gameState==GameStates.Msgs); }


        public IEnumerator BeginDisplay(float delay) {
            if (!wait) {
                wait = true;
                yield return new WaitForSeconds(delay);
                wait = false;
            }
        }


        public static void Display(intf::Message m) {
            message_title.text = m.uuid;
            message_body.text = m.desc;
            Pathways.GameState = GameStates.Msgs;
            Pathways.messageWindow.StartCoroutine(
                Pathways.messageWindow.BeginDisplay(delay));
        }

        public void Disable() {
            if (wait) return;
            Pathways.GameState = GameStates.Game;
            gameObject.SetActive(false);
        }
    }
}
