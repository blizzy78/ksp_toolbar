using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Toolbar;

[KSPAddon(KSPAddon.Startup.EveryScene, false)]
class TestButtons : MonoBehaviour {
	internal TestButtons() {
		IButton button1 = ToolbarManager.Instance.add("test", "button1");
		button1.Text = "Button 1";
		button1.TextColor = Color.yellow;
		button1.OnClick += (button) => Debug.Log("button1 clicked");

		bool state2 = false;
		IButton button2 = null;
		button2 = ToolbarManager.Instance.add("test", "button2");
		button2.TexturePath = "blizzy/Toolbar/img_buttonTypeMNode";
		button2.OnClick += (button) => {
			Debug.Log("button2 clicked");
			button2.TexturePath = state2 ? "blizzy/Toolbar/img_buttonTypeMNode" : "blizzy/Toolbar/icon";
			state2 = !state2;
		};

		IButton button3 = ToolbarManager.Instance.add("test2", "button3");
		button3.Text = "Button 3 (disabled)";
		button3.Enabled = false;
		button3.OnClick += (button) => Debug.Log("button3 clicked");

		bool state4 = false;
		IButton button4 = null;
		button4 = ToolbarManager.Instance.add("test2", "button4");
		button4.TexturePath = "blizzy/Toolbar/img_buttonTypeMNode";
		button4.OnClick += (button) => {
			Debug.Log("button4 clicked");
			button4.TexturePath = state4 ? "blizzy/Toolbar/img_buttonTypeMNode" : "blizzy/Toolbar/icon";
			state4 = !state4;
			button2.Visible = !button2.Visible;
		};

		IButton button5 = ToolbarManager.Instance.add("test2", "button5");
		button5.Text = "Visible in Editors";
		button5.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.SPH);

		IButton button6 = ToolbarManager.Instance.add("test2", "button6");
		button6.Text = "Visible in Flight";
		button6.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
	}
}
