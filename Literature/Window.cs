/* Ben Scott * bescott@andrew.cmu.edu * 2015-09-02 * Window */

using UnityEngine;
using ui=UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using adv=PathwaysEngine.Adventure;
using util=PathwaysEngine.Utilities;


namespace PathwaysEngine.Literature {


    public class Window : MonoBehaviour {

        bool wait;
        static float delay = 0.5f;
        static ui::Text message_title, message_body;
        static ui::Image message_image;
        public util::key accept;


        public Window() {
            accept = new util::key((n)=> {
                if (!accept.input && n) {
                    accept.input = n;
                    Disable();
                } else if (!n) accept.input = n; });
        }


        void Awake() {
            Pathways.StateChange += this.EventListener;
            Pathways.window = this;
            foreach (var child in GetComponentsInChildren<ui::Text>())
                if (child.name=="Title")
                    message_title = child;
                else if (child.name=="Body")
                    message_body = child;
            foreach (var child in GetComponentsInChildren<ui::Image>())
                if (child.name=="Image")
                    message_image = child;
            if (!message_title || !message_body)
                Debug.LogError("missing title / body");
        }


        void Start() {
            util::Controls.AddInputListener(this);
            Disable();
        }

        public void EventListener(
                        object sender,
                        System.EventArgs e,
                        GameStates gameState) { }


        public IEnumerator BeginDisplay(float delay) {
            if (!wait) {
                wait = true;
                yield return new WaitForSeconds(delay);
                wait = false;
            }
        }


        public static void Display(Message m) {
            message_title.text = m.Name;
            message_body.text = m.Entry;
            Pathways.GameState = GameStates.Msgs;
            Pathways.window.gameObject.SetActive(true);
            Pathways.window.StartCoroutine(
                Pathways.window.BeginDisplay(delay));
        }

        public static void ShowImage(ui::Image image) {
            message_body.text = "";
            message_title.text = "";
            message_image = image;
            message_image.enabled = true;
            Pathways.GameState = GameStates.Msgs;
            Pathways.window.gameObject.SetActive(true);
            Pathways.window.StartCoroutine(
                Pathways.window.BeginDisplay(delay));
        }


        public void Disable() {
            if (wait) return;
            Pathways.GameState = GameStates.Game;
            gameObject.SetActive(false);
        }
    }
}
