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
		private const float DEFAULT_X = 300;
		private const float DEFAULT_Y = 300;
		private const float DEFAULT_WIDTH = 500;

		internal event Action onChange;

		private delegate void ButtonPositionCalculatedHandler(Button button, Vector2 position);
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
		private Color autoHideUnimportantButtonAlpha = Color.white;
		private Button mouseHoverButton;
		private float savedMaxWidth = DEFAULT_WIDTH;
		private bool drawBox = true;
		private bool useKSPSkin;

		internal Toolbar() {
			autoHideUnimportantButtonAlpha.a = 0.4f;

			rect = new Rectangle(new Rect(DEFAULT_X, DEFAULT_Y, DEFAULT_WIDTH, float.MinValue));

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
				savedMaxWidth = rect.width;
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
				GUISkin oldSkin = GUI.skin;
				if (useKSPSkin) {
					GUI.skin = HighLogic.Skin;
				}
				drawButtons();
				GUI.skin = oldSkin;

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
			bool anyButtonImportant = buttons.Any(b => b.Important);
			if (rect.contains(Utils.getMousePosition()) || anyButtonImportant) {
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

		private bool shouldAutoHide() {
			return autoHide && !autoHidden && !rect.contains(Utils.getMousePosition()) &&
				((rect.x <= 0) || (rect.x >= (Screen.width - rect.width)) || (rect.y <= 0) || (rect.y >= (Screen.height - rect.height)));
		}

		private void autoSize() {
			if (rect.width < 0) {
				rect.width = DEFAULT_WIDTH;
				rect.width = getMinWidthForButtons();
				savedMaxWidth = rect.width;
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
			calculateButtonPositions((button, pos) => {
				float currentWidth = pos.x + button.Size.x;
				if (currentWidth > width) {
					width = currentWidth;
				}
			});
			width += PADDING;
			return width;
		}

		private float getMinHeightForButtons() {
			float height = 0;
			calculateButtonPositions((button, pos) => {
				float currentHeight = pos.y + button.Size.y;
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
					buttonPositionCalculatedHandler(button, new Vector2(x, y));

					x += button.Size.x + BUTTON_SPACING;
					currentLineWidth += ((currentLineWidth > 0) ? BUTTON_SPACING : 0) + button.Size.x;
				}
			}
			if (currentLineWidth > widestLineWidth) {
				widestLineWidth = currentLineWidth;
			}

			// calculate position of drop-down menu button
			if (y == PADDING) {
				// all buttons on a single line
				buttonPositionCalculatedHandler(dropdownMenuButton, new Vector2(x + 2, (lineHeight - dropdownMenuButton.Size.y) / 2 + PADDING));
			} else {
				// multiple lines
				buttonPositionCalculatedHandler(dropdownMenuButton, new Vector2((widestLineWidth - dropdownMenuButton.Size.x) / 2 + PADDING, y + lineHeight + BUTTON_SPACING + 2));
			}
		}

		private void drawToolbar() {
			if (drawBox || !locked) {
				Color oldColor = GUI.color;
				if (shouldAutoHide() && !autoHidden) {
					GUI.color = autoHideUnimportantButtonAlpha;
				}
				GUILayout.BeginArea(rect.Rect, GUI.skin.box);
				GUILayout.EndArea();
				GUI.color = oldColor;
			}
		}

		private void drawButtons() {
			// must create a copy because Button.draw() also handles the button click,
			// which can potentially modify our list of buttons
			Dictionary<Button, Rect> buttonsToDraw = new Dictionary<Button, Rect>();
			calculateButtonPositions((button, pos) => {
				Rect buttonRect = new Rect(rect.x + pos.x, rect.y + pos.y, button.Size.x, button.Size.y);
				buttonsToDraw.Add(button, buttonRect);
			});
			
			bool shouldHide = shouldAutoHide();
			Button currentMouseHoverButton = null;
			Vector2 mousePos = Utils.getMousePosition();
			foreach (KeyValuePair<Button, Rect> entry in buttonsToDraw) {
				Button button = entry.Key;
				Rect buttonRect = entry.Value;
				Color oldColor = GUI.color;
				if (shouldHide && !autoHidden && !button.Important) {
					GUI.color = autoHideUnimportantButtonAlpha;
				}
				button.draw(buttonRect, (locked || button.Equals(dropdownMenuButton)) && !isPauseMenuOpen());
				GUI.color = oldColor;

				if (buttonRect.Contains(mousePos)) {
					currentMouseHoverButton = button;
				}
			}

			handleMouseHover(currentMouseHoverButton);
		}

		private void handleMouseHover(Button currentMouseHoverButton) {
			if ((mouseHoverButton == null) && (currentMouseHoverButton != null)) {
				currentMouseHoverButton.mouseEnter();
			} else if ((mouseHoverButton != null) && (currentMouseHoverButton == null)) {
				mouseHoverButton.mouseLeave();
			} else if ((mouseHoverButton != null) && (currentMouseHoverButton != null) && !currentMouseHoverButton.Equals(mouseHoverButton)) {
				mouseHoverButton.mouseLeave();
				currentMouseHoverButton.mouseEnter();
			}
			mouseHoverButton = currentMouseHoverButton;
		}

		private bool isPauseMenuOpen() {
			// PauseMenu.isOpen may throw NullReferenceException on occasion, even if HighLogic.LoadedScene==GameScenes.FLIGHT
			try {
				return (HighLogic.LoadedScene == GameScenes.FLIGHT) && PauseMenu.isOpen;
			} catch {
				return false;
			}
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
					if (autoHidden) {
						// docked at right screen edge -> keep it that way by moving to screen edge
						if (rectPositionBeforeAutoHide.x >= (Screen.width - rect.width)) {
							rectPositionBeforeAutoHide.x = Screen.width;
						}
					} else {
						// docked at right screen edge -> keep it that way by moving to screen edge
						if (rect.x >= (Screen.width - rect.width)) {
							rect.x = Screen.width;
						}
					}
				} else {
					if (autoHidden) {
						// docked at bottom screen edge -> keep it that way by moving to screen edge
						if (rectPositionBeforeAutoHide.y >= (Screen.height - rect.height)) {
							rectPositionBeforeAutoHide.y = Screen.height;
						}
					} else {
						// docked at bottom screen edge -> keep it that way by moving to screen edge
						if (rect.y >= (Screen.height - rect.height)) {
							rect.y = Screen.height;
						}
					}
				}

				// expand width to last saved width, then resize for buttons, and expand height to fit new buttons
				rect.width = savedMaxWidth;
				rect.width = getMinWidthForButtons();
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
			calculateButtonPositions((b, pos) => {
				if (!done && b.Equals(button)) {
					position.x = pos.x - PADDING;
					position.y = pos.y - PADDING;
					done = true;
				}
			});
			return position;
		}

		internal Rect getRect(Button button) {
			Rect rect = new Rect(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
			bool done = false;
			calculateButtonPositions((b, pos) => {
				if (!done && b.Equals(button)) {
					rect = new Rect(pos.x - PADDING, pos.y - PADDING, b.Size.x, b.Size.y);
					done = true;
				}
			});
			return rect;
		}

		private void drawButtonToolTips() {
			foreach (Button button in buttons) {
				button.drawToolTip();
			}
		}

		internal void update() {
			draggable.update();
			resizable.update();

			if (dropdownMenu != null) {
				if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) && !dropdownMenu.contains(Utils.getMousePosition())) {
					dropdownMenu = null;
				}
				if (isPauseMenuOpen()) {
					dropdownMenu = null;
				}
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

		internal void loadSettings(ConfigNode parentNode, GameScenes scene) {
			// hide this
			dropdownMenu = null;
			// deactivate these
			draggable.Enabled = false;
			resizable.Enabled = false;
			// pretend we're not auto-hidden right now
			autoHidden = false;

			if (parentNode.HasNode("toolbar")) {
				ConfigNode toolbarNode = parentNode.GetNode("toolbar");
				ConfigNode settingsNode = toolbarNode.HasNode(scene.ToString()) ? toolbarNode.GetNode(scene.ToString()) : toolbarNode;
				rect.x = settingsNode.get("x", DEFAULT_X);
				rect.y = settingsNode.get("y", DEFAULT_Y);
				rect.width = settingsNode.get("width", DEFAULT_WIDTH);
				rect.height = settingsNode.get("height", 0f);
				autoHide = settingsNode.get("autoHide", false);
				drawBox = settingsNode.get("drawBox", true);
				useKSPSkin = settingsNode.get("useKSPSkin", false);
			}

			savedMaxWidth = rect.width;
		}

		internal void saveSettings(ConfigNode parentNode, GameScenes scene) {
			ConfigNode toolbarNode = parentNode.getOrCreateNode("toolbar");
			ConfigNode settingsNode = toolbarNode.getOrCreateNode(scene.ToString());
			settingsNode.overwrite("x", rect.x.ToString("F0"));
			settingsNode.overwrite("y", rect.y.ToString("F0"));
			settingsNode.overwrite("width", rect.width.ToString("F0"));
			settingsNode.overwrite("height", rect.height.ToString("F0"));
			settingsNode.overwrite("autoHide", autoHide.ToString());
			settingsNode.overwrite("drawBox", drawBox.ToString());
			settingsNode.overwrite("useKSPSkin", useKSPSkin.ToString());
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

				Button toggleBoxButton = Button.createMenuOption(drawBox ? "Hide Box Around Buttons" : "Show Box Around Buttons");
				toggleBoxButton.OnClick += (e) => {
					drawBox = !drawBox;
					fireChange();
				};
				toggleBoxButton.Enabled = locked;
				dropdownMenu += toggleBoxButton;

				Button toggleKSPSkinButton = Button.createMenuOption(useKSPSkin ? "Use Unity 'Smoke' Skin" : "Use KSP Skin");
				toggleKSPSkinButton.OnClick += (e) => {
					useKSPSkin = !useKSPSkin;
					foreach (Button button in buttons) {
						button.resetStyle();
					}
					fireChange();
				};
				toggleKSPSkinButton.Enabled = locked;
				dropdownMenu += toggleKSPSkinButton;

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
