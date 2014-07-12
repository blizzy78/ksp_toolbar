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
	private IButton button8;
	private IButton button9;
	private IButton button10;
	private IButton button11;
	private IButton button12;
	private BoxDrawable boxDrawable;

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
		button4.OnMouseEnter += (e) => Debug.Log("button4 mouse enter");
		button4.OnMouseLeave += (e) => Debug.Log("button4 mouse leave");

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

		// button that is only visible in the flight scene and flight map
		button7 = ToolbarManager.Instance.add("test", "button7");
		button7.TexturePath = "000_Toolbar/icon";
		button7.ToolTip = "Button Visible Only in Flight Scene";
		button7.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
		button7.OnClick += (e) => Debug.Log("button7 clicked");

		// button that is only visible in the flight map
		button8 = ToolbarManager.Instance.add("test", "button8");
		button8.TexturePath = "000_Toolbar/icon";
		button8.ToolTip = "Button Visible Only in Flight Map";
		button8.Visibility = FlightMapVisibility.Instance;
		button8.OnClick += (e) => Debug.Log("button8 clicked");

		// button that opens a popup menu on click
		button9 = ToolbarManager.Instance.add("test", "button9");
		button9.TexturePath = "000_Toolbar/img_buttonTypeMNode";
		button9.ToolTip = "Menu Button (Click)";
		button9.OnClick += (e) => togglePopupMenu(button9);

		// button that opens an informative window on hover
		bool drawableVisible = false;
		button10 = ToolbarManager.Instance.add("test", "button10");
		button10.TexturePath = "000_Toolbar/img_buttonTypeMNode";
		button10.ToolTip = "Info Button (Hover)";
		button10.OnMouseEnter += (e) => {
			if (!drawableVisible) {
				button10.Drawable = new BoxDrawable();
			}
		};
		button10.OnMouseLeave += (e) => {
			button10.Drawable = null;
		};

		// button that opens an informative window on click
		bool drawable2Visible = false;
		button11 = ToolbarManager.Instance.add("test", "button11");
		button11.TexturePath = "000_Toolbar/img_buttonTypeMNode";
		button11.ToolTip = "Info Button (Click)";
		button11.OnClick += (e) => {
			switch (e.MouseButton) {
				case 0:
					if (!drawable2Visible) {
						boxDrawable = new BoxDrawable();
						button11.Drawable = boxDrawable;
					} else {
						boxDrawable = null;
						button11.Drawable = null;
					}
					drawable2Visible = !drawable2Visible;
					break;

				case 1:
					if (boxDrawable != null) {
						boxDrawable.changeSize();
					}
					break;
			}
		};

		// button that has a nonexistent texture (plugin installed incorrectly etc.)
		button12 = ToolbarManager.Instance.add("test", "button12");
		button12.TexturePath = "000_Toolbar/nonexistent";
		button12.ToolTip = "Broken Button";
	}

	private void togglePopupMenu(IButton button) {
		if (button.Drawable == null) {
			createPopupMenu(button);
		} else {
			destroyPopupMenu(button);
		}
	}

	private void createPopupMenu(IButton button) {
		// create menu drawable
		PopupMenuDrawable menu = new PopupMenuDrawable();

		// create menu options
		IButton option1 = menu.AddOption("Option 1");
		option1.OnClick += (e2) => Debug.Log("menu option 1 clicked");
		IButton option2 = menu.AddOption("Option 2");
		option2.OnClick += (e2) => Debug.Log("menu option 2 clicked");
		menu.AddSeparator();
		IButton option3 = menu.AddOption("Option 3");
		option3.OnClick += (e2) => Debug.Log("menu option 3 clicked");

		// auto-close popup menu when any option is clicked
		menu.OnAnyOptionClicked += () => destroyPopupMenu(button);

		// hook drawable to button
		button.Drawable = menu;
	}

	private void destroyPopupMenu(IButton button) {
		// PopupMenuDrawable must be destroyed explicitly
		((PopupMenuDrawable) button.Drawable).Destroy();

		// unhook drawable
		button.Drawable = null;
	}

	internal void OnDestroy() {
		button1.Destroy();
		button2.Destroy();
		button3.Destroy();
		button4.Destroy();
		button5.Destroy();
		button6.Destroy();
		button7.Destroy();
		button8.Destroy();
		button9.Destroy();
		button10.Destroy();
		button11.Destroy();
		button12.Destroy();
	}
}
