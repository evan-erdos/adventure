using UnityEngine;
using System.Collections;


namespace PathwaysEngine.Utilities {


	public class ExitButton : MonoBehaviour {
		public Cursors cursor = Cursors.Back;

		void OnMouseEnter() {
			Pathways.CursorGraphic = cursor; }

		void OnMouseExit() {
			Pathways.CursorGraphic = Cursors.None; }

		public void OnPointerDown() {
			print("ADF");
			Pathways.GameState = GameStates.Game; }
	}
}