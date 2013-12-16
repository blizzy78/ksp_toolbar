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
	private IButton button7;

	internal TestButtons() {
		// button that toggles its icon when clicked
		bool state1 = false;
		button1 = ToolbarManager.Instance.add("test", "button1");
		button1.TexturePath = "000_Toolbar/img_buttonTypeMNode";
		button1.ToolTip = "Toggle This Button's Icon";
		button1.OnClick += (e) => {
			Debug.Log("button1 clicked, mouseButton: " + e.MouseButton);
			button1.TexturePath = state1 ? "000_Toolbar/img_buttonTypeMNode" : "000_Toolbar/icon";
			state1 = !state1;
		};

		// disabled button
		button2 = ToolbarManager.Instance.add("test", "button2");
		button2.TexturePath = "000_Toolbar/img_buttonTypeMNode";
		button2.ToolTip = "Disabled Button";
		button2.Enabled = false;
		button2.OnClick += (e) => Debug.Log("button2 clicked");

		// important button
		button3 = ToolbarManager.Instance.add("test", "button3");
		button3.TexturePath = "000_Toolbar/img_buttonTypeMNode";
		button3.ToolTip = "Toggle This Button's Importance";
		button3.Important = true;
		button3.OnClick += (e) => {
			Debug.Log("button3 clicked");
			button3.Important = !button3.Important;
		};
		
		// regular button
		button4 = ToolbarManager.Instance.add("test", "button4");
		button4.TexturePath = "000_Toolbar/img_buttonTypeMNode";
		button4.ToolTip = "Regular Button";
		button4.OnClick += (e) => Debug.Log("button4 clicked");

		// button that toggles visibility of the previous button
		button5 = ToolbarManager.Instance.add("test", "button5");
		button5.TexturePath = "000_Toolbar/icon";
		button5.ToolTip = "Toggle Previous Button's Visibility";
		button5.OnClick += (e) => button4.Visible = !button4.Visible;
		button5.OnClick += (e) => Debug.Log("button5 clicked");

		// button that is only visible in the editors
		button6 = ToolbarManager.Instance.add("test", "button6");
		button6.TexturePath = "000_Toolbar/img_buttonTypeMNode";
		button6.ToolTip = "Button Visible Only in Editors";
		button6.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.SPH);
		button6.OnClick += (e) => Debug.Log("button6 clicked");

		// button that is only visible in the flight scene
		button7 = ToolbarManager.Instance.add("test", "button7");
		button7.TexturePath = "000_Toolbar/icon";
		button7.ToolTip = "Button Visible Only in Flight Scene";
		button7.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
		button7.OnClick += (e) => Debug.Log("button7 clicked");
	}

	internal void OnDestroy() {
		button1.Destroy();
		button2.Destroy();
		button3.Destroy();
		button4.Destroy();
		button5.Destroy();
		button6.Destroy();
		button7.Destroy();
	}
}
