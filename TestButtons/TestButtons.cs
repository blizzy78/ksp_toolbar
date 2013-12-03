using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Toolbar;

[KSPAddon(KSPAddon.Startup.EveryScene, false)]
class TestButtons : MonoBehaviour {
	private IButton button1;
	private IButton button2;
	private IButton button3;
	private IButton button4;
	private IButton button5;
	private IButton button6;

	internal TestButtons() {
		// button that toggles its icon when clicked
		bool state1 = false;
		button1 = ToolbarManager.Instance.add("test", "button1");
		button1.TexturePath = "blizzy/Toolbar/img_buttonTypeMNode";
		button1.ToolTip = "Toggle This Button's Icon";
		button1.OnClick += (e) => {
			Debug.Log("button1 clicked, mouseButton: " + e.MouseButton);
			button1.TexturePath = state1 ? "blizzy/Toolbar/img_buttonTypeMNode" : "blizzy/Toolbar/icon";
			state1 = !state1;
		};

		// disabled button
		button2 = ToolbarManager.Instance.add("test", "button2");
		button2.TexturePath = "blizzy/Toolbar/img_buttonTypeMNode";
		button2.ToolTip = "Disabled Button";
		button2.Enabled = false;
		button2.OnClick += (e) => Debug.Log("button2 clicked");

		// regular button
		button3 = ToolbarManager.Instance.add("test", "button3");
		button3.TexturePath = "blizzy/Toolbar/img_buttonTypeMNode";
		button3.ToolTip = "Regular Button";
		button3.OnClick += (e) => Debug.Log("button3 clicked");

		// button that toggles visibility of the previous button
		button4 = ToolbarManager.Instance.add("test", "button4");
		button4.TexturePath = "blizzy/Toolbar/icon";
		button4.ToolTip = "Toggle Previous Button's Visibility";
		button4.OnClick += (e) => button3.Visible = !button3.Visible;
		button4.OnClick += (e) => Debug.Log("button4 clicked");

		// button that is only visible in the editors
		button5 = ToolbarManager.Instance.add("test", "button5");
		button5.TexturePath = "blizzy/Toolbar/img_buttonTypeMNode";
		button5.ToolTip = "Button Visible Only in Editors";
		button5.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.SPH);
		button5.OnClick += (e) => Debug.Log("button5 clicked");

		// button that is only visible in the flight scene
		button6 = ToolbarManager.Instance.add("test", "button6");
		button6.TexturePath = "blizzy/Toolbar/icon";
		button6.ToolTip = "Button Visible Only in Flight Scene";
		button6.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
		button6.OnClick += (e) => Debug.Log("button6 clicked");
	}

	internal void OnDestroy() {
		button1.Destroy();
		button2.Destroy();
		button3.Destroy();
		button4.Destroy();
		button5.Destroy();
		button6.Destroy();
	}
}
