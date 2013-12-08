/*
Copyright (c) 2013, Maik Schreiber
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar {
	internal class Toolbar {
		private const float BUTTON_SPACING = 1;
		private const float PADDING = 3;

		internal event Action onChange;

		private delegate void ButtonPositionCalculatedHandler(Button button, float x, float y);
		private Rectangle rect;
		private Draggable draggable;
		private Resizable resizable;
		private List<Button> buttons = new List<Button>();
		private Dictionary<Button, bool> buttonVisibility = new Dictionary<Button, bool>();
		private Button dropdownMenuButton;
		private Menu dropdownMenu;
		private bool locked = true;
		private bool autoHide;
		private bool autoHidden;
		private Vector2 rectPositionBeforeAutoHide;

		internal Toolbar() {
			rect = new Rectangle(new Rect(300, 300, float.MinValue, float.MinValue));

			dropdownMenuButton = Button.createToolbarDropdown();
			dropdownMenuButton.OnClick += (e) => toggleDropdownMenu();
			buttons.Add(dropdownMenuButton);

			draggable = new Draggable(rect, PADDING,
				(pos) => !getRect(dropdownMenuButton).shift(new Vector2(rect.x + PADDING, rect.y + PADDING)).Contains(pos) && !resizable.HandleRect.Contains(pos));
			resizable = new Resizable(rect, PADDING,
				(pos) => !getRect(dropdownMenuButton).shift(new Vector2(rect.x + PADDING, rect.y + PADDING)).Contains(pos));

			draggable.onChange += dragged;
			resizable.onChange += resized;
		}

		private void dragged() {
			if (!draggable.Dragging) {
				fireChange();
			}
		}

		private void resized() {
			if (resizable.Resizing) {
				float maxButtonWidth = buttons.Where(b => b.EffectivelyVisible).Max(b => b.Size.x);
				if (rect.width < (maxButtonWidth + PADDING * 2)) {
					rect.width = maxButtonWidth + PADDING * 2;
				}
				float minHeight = getMinHeightForButtons();
				if (rect.height < minHeight) {
					rect.height = minHeight;
				}
			} else {
				rect.width = getMinWidthForButtons();
				rect.height = getMinHeightForButtons();
				fireChange();
			}
		}

		internal void draw() {
			// only show toolbar if there is at least one visible button
			// that is not the drop-down menu button
			if (buttons.Any((b) => !b.Equals(dropdownMenuButton) && b.EffectivelyVisible)) {
				forceAutoSizeIfButtonVisibilitiesChanged();
				autoSize();

				if (autoHide && (dropdownMenu == null)) {
					handleAutoHide();
				}

				int oldDepth = GUI.depth;
				GUI.depth = -99;
				drawToolbar();
				GUI.depth = -100;
				drawButtons();
				GUI.depth = oldDepth;

				if (locked && !draggable.Dragging && !resizable.Resizing && (dropdownMenu == null)) {
					drawButtonToolTips();
				}

				if (dropdownMenu != null) {
					dropdownMenu.draw();
				}
			}
		}

		private void handleAutoHide() {
			if (rect.contains(Utils.getMousePosition())) {
				if (autoHidden) {
					rect.x = rectPositionBeforeAutoHide.x;
					rect.y = rectPositionBeforeAutoHide.y;
					autoHidden = false;
				}
			} else {
				if (!autoHidden) {
					if (rect.x <= 0) {
						rectPositionBeforeAutoHide = new Vector2(rect.x, rect.y);
						rect.x = -rect.width + PADDING;
						autoHidden = true;
					} else if (rect.x >= (Screen.width - rect.width)) {
						rectPositionBeforeAutoHide = new Vector2(rect.x, rect.y);
						rect.x = Screen.width - PADDING;
						autoHidden = true;
					} else if (rect.y <= 0) {
						rectPositionBeforeAutoHide = new Vector2(rect.x, rect.y);
						rect.y = -rect.height + PADDING;
						autoHidden = true;
					} else if (rect.y >= (Screen.height - rect.height)) {
						rectPositionBeforeAutoHide = new Vector2(rect.x, rect.y);
						rect.y = Screen.height - PADDING;
						autoHidden = true;
					}
				}
			}
		}

		private void autoSize() {
			if (rect.width < 0) {
				rect.width = Screen.width;
				rect.width = getMinWidthForButtons();
			}
			if (rect.height < 0) {
				rect.height = getMinHeightForButtons();
			}

			if (!autoHidden) {
				rect.clampToScreen(PADDING);
			}
		}

		private float getMinWidthForButtons() {
			float width = 0;
			calculateButtonPositions((button, x, y) => {
				float currentWidth = x + button.Size.x;
				if (currentWidth > width) {
					width = currentWidth;
				}
			});
			width += PADDING;
			return width;
		}

		private float getMinHeightForButtons() {
			float height = 0;
			calculateButtonPositions((button, x, y) => {
				float currentHeight = y + button.Size.y;
				if (currentHeight > height) {
					height = currentHeight;
				}
			});
			height += PADDING;
			return height;
		}

		private void calculateButtonPositions(ButtonPositionCalculatedHandler buttonPositionCalculatedHandler) {
			float x = PADDING;
			float y = PADDING;
			float lineHeight = float.MinValue;
			float widestLineWidth = float.MinValue;
			float currentLineWidth = 0;
			foreach (Button button in buttons) {
				if (button.EffectivelyVisible && button.IsTextured && !button.Equals(dropdownMenuButton)) {
					if (((x + button.Size.x) > (rect.width - PADDING)) && (lineHeight > 0)) {
						x = PADDING;
						y += lineHeight + BUTTON_SPACING;
						lineHeight = float.MinValue;
						if (currentLineWidth > widestLineWidth) {
							widestLineWidth = currentLineWidth;
						}
						currentLineWidth = 0;
					}
					if (button.Size.y > lineHeight) {
						lineHeight = button.Size.y;
					}
					buttonPositionCalculatedHandler(button, x, y);

					x += button.Size.x + BUTTON_SPACING;
					currentLineWidth += ((currentLineWidth > 0) ? BUTTON_SPACING : 0) + button.Size.x;
				}
			}
			if (currentLineWidth > widestLineWidth) {
				widestLineWidth = currentLineWidth;
			}

			if (y == PADDING) {
				// all buttons on a single line
				buttonPositionCalculatedHandler(dropdownMenuButton, x + 2, (lineHeight - dropdownMenuButton.Size.y) / 2 + PADDING);
			} else {
				// multiple lines
				buttonPositionCalculatedHandler(dropdownMenuButton, (widestLineWidth - dropdownMenuButton.Size.x) / 2 + PADDING, y + lineHeight + BUTTON_SPACING + 2);
			}
		}

		private void drawToolbar() {
			GUILayout.BeginArea(rect.Rect, GUI.skin.box);
			GUILayout.EndArea();
		}

		private void drawButtons() {
			calculateButtonPositions((button, x, y) => {
				Rect buttonRect = new Rect(rect.x + x, rect.y + y, button.Size.x, button.Size.y);
				button.draw(buttonRect, locked || button.Equals(dropdownMenuButton));
			});
		}

		private void forceAutoSizeIfButtonVisibilitiesChanged() {
			bool anyButtonVisibilityChanged = false;
			foreach (Button button in buttons) {
				bool newVisible = button.EffectivelyVisible;
				if (buttonVisibility.ContainsKey(button)) {
					if (buttonVisibility[button] != newVisible) {
						anyButtonVisibilityChanged = true;
					}
					buttonVisibility[button] = newVisible;
				} else {
					anyButtonVisibilityChanged = true;
					buttonVisibility.Add(button, newVisible);
				}
			}
			if (anyButtonVisibilityChanged) {
				Debug.Log("button visibilities have changed, forcing auto-size ");
				if (isSingleLine()) {
					// expand width to fit new button
					rect.width = Screen.width;
					rect.width = getMinWidthForButtons();
				} else {
					// keep width (removing excess space), and expand height instead
					rect.width = getMinWidthForButtons();
				}
				rect.height = getMinHeightForButtons();
				fireChange();
			}
		}

		private bool isSingleLine() {
			float maxButtonHeight = buttons.Where(b => b.EffectivelyVisible).Max(b => b.Size.y);
			return rect.height <= (maxButtonHeight + PADDING * 2);
		}

		internal Vector2 getPosition(Button button) {
			Vector2 position = new Vector2(float.MinValue, float.MinValue);
			bool done = false;
			calculateButtonPositions((b, x, y) => {
				if (!done && b.Equals(button)) {
					position.x = x - PADDING;
					position.y = y - PADDING;
					done = true;
				}
			});
			return position;
		}

		internal Rect getRect(Button button) {
			Rect rect = new Rect(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
			bool done = false;
			calculateButtonPositions((b, x, y) => {
				if (!done && b.Equals(button)) {
					rect = new Rect(x - PADDING, y - PADDING, b.Size.x, b.Size.y);
					done = true;
				}
			});
			return rect;
		}

		private void drawButtonToolTips() {
			Vector2 mousePos = Utils.getMousePosition();
			bool done = false;
			calculateButtonPositions((button, x, y) => {
				if (!done) {
					Rect buttonRect = new Rect(rect.x + x, rect.y + y, button.Size.x, button.Size.y);
					if (buttonRect.Contains(mousePos)) {
						button.drawToolTip();
						done = true;
					}
				}
			});
		}

		internal void update() {
			draggable.update();
			resizable.update();

			if ((dropdownMenu != null) &&
				(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) &&
				!dropdownMenu.contains(Utils.getMousePosition())) {

				dropdownMenu = null;
			}
		}

		internal void add(Button button) {
			Button oldButton = buttons.SingleOrDefault(b => (b.ns == button.ns) && (b.id == button.id));

			if (oldButton != null) {
				oldButton.Destroy();
			}

			button.OnDestroy += () => buttonDestroyed(button);

			buttons.Add(button);

			buttons.Remove(dropdownMenuButton);
			buttons.Sort((b1, b2) => StringComparer.CurrentCultureIgnoreCase.Compare(b1.ns + "." + b1.id, b2.ns + "." + b2.id));
			buttons.Add(dropdownMenuButton);
		}

		private void buttonDestroyed(Button button) {
			buttons.Remove(button);
			if (buttonVisibility.ContainsKey(button)) {
				buttonVisibility.Remove(button);
			}
		}

		internal void loadSettings(ConfigNode parentNode) {
			if (parentNode.HasNode("toolbar")) {
				ConfigNode toolbarNode = parentNode.GetNode("toolbar");
				if (toolbarNode.HasValue("x")) {
					rect.x = float.Parse(toolbarNode.GetValue("x"));
				}
				if (toolbarNode.HasValue("y")) {
					rect.y = float.Parse(toolbarNode.GetValue("y"));
				}
				if (toolbarNode.HasValue("width")) {
					rect.width = float.Parse(toolbarNode.GetValue("width"));
				}
				if (toolbarNode.HasValue("height")) {
					rect.height = float.Parse(toolbarNode.GetValue("height"));
				}
				if (toolbarNode.HasValue("autoHide")) {
					autoHide = bool.Parse(toolbarNode.GetValue("autoHide"));
				}
			}
		}

		internal void saveSettings(ConfigNode parentNode) {
			ConfigNode toolbarNode = parentNode.AddNode("toolbar");
			toolbarNode.AddValue("x", rect.x.ToString("F0"));
			toolbarNode.AddValue("y", rect.y.ToString("F0"));
			toolbarNode.AddValue("width", rect.width.ToString("F0"));
			toolbarNode.AddValue("height", rect.height.ToString("F0"));
			toolbarNode.AddValue("autoHide", autoHide.ToString());
		}

		private void fireChange() {
			if (onChange != null) {
				onChange();
			}
		}

		private void toggleDropdownMenu() {
			if (dropdownMenu == null) {
				dropdownMenu = new Menu(new Vector2(rect.x + PADDING + getPosition(dropdownMenuButton).x, rect.y + rect.height + BUTTON_SPACING));

				Button toggleLockButton = Button.createMenuOption(locked ? "Unlock Position and Size" : "Lock Position and Size");
				toggleLockButton.OnClick += (e) => {
					locked = !locked;
					draggable.Enabled = !locked;
					resizable.Enabled = !locked;
					autoHide = false;
					fireChange();
				};
				dropdownMenu += toggleLockButton;

				Button toggleAutoHideButton = Button.createMenuOption(autoHide ? "Deactivate Auto-Hide" : "Activate Auto-Hide");
				toggleAutoHideButton.OnClick += (e) => {
					autoHide = !autoHide;
					fireChange();
				};
				toggleAutoHideButton.Enabled = locked;
				dropdownMenu += toggleAutoHideButton;

				// close drop-down menu when player clicks on an option
				foreach (Button option in dropdownMenu.Options) {
					option.OnClick += (e) => dropdownMenu = null;
				}
			} else {
				dropdownMenu = null;
			}
		}
	}
}
