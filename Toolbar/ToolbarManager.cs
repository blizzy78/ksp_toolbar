/*
Toolbar - Common API for GUI Buttons for Kerbal Space Program.
Copyright (C) 2013 Maik Schreiber

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar {
	[KSPAddonFixed(KSPAddon.Startup.EveryScene, true, typeof(ToolbarManager))]
	public class ToolbarManager : MonoBehaviour, IToolbarManager {
		public static IToolbarManager Instance {
			get;
			private set;
		}

		private static readonly Vector2 AUTO_ARRANGE_START_POS = new Vector2(100, 200);
		private static readonly Matrix4x4 ROTATE_MATRIX = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 0, -90)), Vector3.one);
		private static readonly string settingsFile = KSPUtil.ApplicationRootPath + "GameData/toolbar-settings.dat";
		private const float BUTTON_SPACING = 3;

		private Dictionary<string, Button> buttons = new Dictionary<string, Button>();
		private RenderingManager renderingManager;
		private Button draggedButton;
		private bool settingsLoaded;

		internal ToolbarManager() {
			Instance = this;

			GameObject.DontDestroyOnLoad(this);
		}

		internal void OnGUI() {
			if (!showGUI()) {
				return;
			}

			loadSettings();

			styleButtons();
			sizeButtons();
			autoPositionButtons();
			drawButtons();
		}

		internal void Update() {
			handleButtonDrag();
			handleButtonLockToggle();
		}

		private void loadSettings() {
			if (!settingsLoaded) {
				ConfigNode root = ConfigNode.Load(settingsFile) ?? new ConfigNode();
				if (root.HasNode("buttons")) {
					ConfigNode buttonsNode = root.GetNode("buttons");
					loadButtons(buttonsNode);
				}

				settingsLoaded = true;
			}
		}

		private void saveSettings() {
			ConfigNode root = new ConfigNode();
			ConfigNode buttonsNode = root.AddNode("buttons");
			saveButtons(buttonsNode);
			root.Save(settingsFile);
		}

		private void styleButtons() {
			foreach (Button button in buttons.Values) {
				if (button.Style == null) {
					GUIStyle style = new GUIStyle(GUI.skin.button);
					style.alignment = TextAnchor.MiddleCenter;
					style.normal.textColor = button.TextColor;
					style.onHover.textColor = button.TextColor;
					style.hover.textColor = button.TextColor;
					style.onActive.textColor = button.TextColor;
					style.active.textColor = button.TextColor;
					style.onFocused.textColor = button.TextColor;
					style.focused.textColor = button.TextColor;
					button.Style = style;
				}
			}
		}

		private void sizeButtons() {
			foreach (Button button in buttons.Values) {
				if (button.Size.x < 0) {
					if (button.IsTextured) {
						button.Size = new Vector2(32, 32);
					} else {
						Vector2 size = button.Style.CalcSize(button.Content);
						size.x += button.Style.padding.left + button.Style.padding.right;
						size.y += button.Style.padding.top + button.Style.padding.bottom;
						button.Size = size;
					}
				}
			}
		}

		private void autoPositionButtons() {
			List<string> idsSorted = new List<string>(buttons.Keys.ToList());
			idsSorted.Sort(StringComparer.CurrentCultureIgnoreCase);
			float x = AUTO_ARRANGE_START_POS.x;
			foreach (string id in idsSorted) {
				Button button = buttons[id];
				if (button.Position.x < 0) {
					button.Position = new Vector2(x, AUTO_ARRANGE_START_POS.y);
					x += button.Size.x + BUTTON_SPACING;
				}
			}
		}

		private void drawButtons() {
			foreach (Button button in buttons.Values) {
				if (button.EffectivelyVisible) {
					Rect buttonRect = button.Rect;
					if (button.Rotated) {
						GUI.matrix = ROTATE_MATRIX;
						buttonRect = buttonRect.rotate();
					}
					int oldDepth = GUI.depth;
					GUI.depth = -100;
					bool oldEnabled = GUI.enabled;
					GUI.enabled = button.Enabled;
					bool clicked = GUI.Button(buttonRect, button.Content, button.Style);
					GUI.enabled = oldEnabled;
					GUI.depth = oldDepth;
					if (button.Rotated) {
						GUI.matrix = Matrix4x4.identity;
					}
					if (clicked) {
						if (button.PositionLocked && Input.GetMouseButtonUp(0) && (draggedButton == null)) {
							button.clicked();
						}
					}
				}
			}
		}

		private void handleButtonDrag() {
			if (Input.GetMouseButtonDown(0) && (draggedButton == null)) {
				Vector2 mousePos = getMousePosition();
				draggedButton = buttons.Values.FirstOrDefault(b => b.EffectivelyVisible && !b.PositionLocked && b.contains(mousePos));
			}

			if (draggedButton != null) {
				if (Input.GetMouseButton(0)) {
					Vector2 mousePos = getMousePosition();
					Vector2 newButtonPos = new Vector2(mousePos.x - draggedButton.Size.x / 2, mousePos.y - draggedButton.Size.y / 2);
					draggedButton.Rotated = !draggedButton.IsTextured &&
						((newButtonPos.x < 0) || (newButtonPos.x > (Screen.width - draggedButton.Size.x)));
					if (draggedButton.Rotated) {
						newButtonPos = new Vector2(mousePos.x - draggedButton.Size.y / 2, mousePos.y + draggedButton.Size.x / 2);
					}
					// prevent moving button off screen edges
					newButtonPos.x = Mathf.Clamp(newButtonPos.x, 0, Screen.width - (draggedButton.Rotated ? draggedButton.Size.y : draggedButton.Size.x));
					newButtonPos.y = Mathf.Clamp(newButtonPos.y, draggedButton.Rotated ? draggedButton.Size.x : 0, Screen.height - (draggedButton.Rotated ? 0 : draggedButton.Size.y));
					draggedButton.Position = newButtonPos;
				} else {
					draggedButton = null;

					saveSettings();
				}
			}
		}

		private void handleButtonLockToggle() {
			if (Input.GetMouseButtonUp(1)) {
				Vector2 mousePos = getMousePosition();
				Button button = buttons.Values.FirstOrDefault(b => b.EffectivelyVisible && b.contains(mousePos));
				if (button != null) {
					button.PositionLocked = !button.PositionLocked;
				}
			}
		}

		private Vector2 getMousePosition() {
			Vector3 mousePos = Input.mousePosition;
			return new Vector2(mousePos.x, Screen.height - mousePos.y);
		}

		private bool showGUI() {
			if (renderingManager == null) {
				renderingManager = (RenderingManager) GameObject.FindObjectOfType(typeof(RenderingManager));
			}

			if (renderingManager != null) {
				GameObject o = renderingManager.uiElementsToDisable.FirstOrDefault();
				return (o == null) || o.activeSelf;
			}

			return false;
		}

		private void loadButtons(ConfigNode buttonsNode) {
			foreach (ConfigNode nsNode in buttonsNode.nodes) {
				string ns = nsNode.name;
				foreach (ConfigNode buttonNode in nsNode.nodes) {
					string id = buttonNode.name;
					if (buttons.ContainsKey(ns + "." + id)) {
						Button button = buttons[ns + "." + id];
						button.load(buttonNode);
					}
				}
			}
		}

		private void saveButtons(ConfigNode buttonsNode) {
			foreach (Button button in buttons.Values) {
				ConfigNode nsNode;
				if (buttonsNode.HasNode(button.ns)) {
					nsNode = buttonsNode.GetNode(button.ns);
				} else {
					nsNode = buttonsNode.AddNode(button.ns);
				}
				ConfigNode buttonNode = nsNode.AddNode(button.id);
				button.save(buttonNode);
			}
		}

		public IButton add(string ns, string id) {
			if (ns.Contains('.')) {
				throw new ArgumentException("namespace must not contain '.': " + ns);
			}
			if (id.Contains('.')) {
				throw new ArgumentException("ID must not contain '.': " + id);
			}

			Button button = new Button(ns, id, this);
			if (buttons.ContainsKey(ns + "." + id)) {
				buttons[ns + "." + id] = button;
			} else {
				buttons.Add(ns + "." + id, button);
			}

			// re-load saved button positions
			settingsLoaded = false;

			return button;
		}
	}
}
