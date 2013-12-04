using System;
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

		internal Toolbar() {
			rect = new Rectangle(new Rect(300, 300, float.MinValue, float.MinValue));

			draggable = new Draggable(rect, true, (pos) => !anyButtonContains(pos) && !resizable.HandleRect.Contains(pos));
			resizable = new Resizable(rect, true, (pos) => !anyButtonContains(pos));

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
			if (buttons.Count > 0) {
				forceAutoSizeIfButtonVisibilitiesChanged();
				autoSize();

				int oldDepth = GUI.depth;
				GUI.depth = -99;
				drawToolbar();
				GUI.depth = -100;
				drawButtons();
				GUI.depth = oldDepth;

				drawButtonToolTips();
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

			rect.clampToScreen();
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
				height = y + button.Size.y;
			});
			height += PADDING;
			return height;
		}

		private void calculateButtonPositions(ButtonPositionCalculatedHandler buttonPositionCalculatedHandler) {
			float x = PADDING;
			float y = PADDING;
			float lineHeight = float.MinValue;
			foreach (Button button in buttons) {
				if (button.EffectivelyVisible && button.IsTextured) {
					if (((x + button.Size.x) > (rect.width - PADDING)) && (lineHeight > 0)) {
						x = PADDING;
						y += lineHeight + BUTTON_SPACING;
						lineHeight = float.MinValue;
					}
					if (button.Size.y > lineHeight) {
						lineHeight = button.Size.y;
					}
					buttonPositionCalculatedHandler(button, x, y);

					x += button.Size.x + BUTTON_SPACING;
				}
			}
		}

		private bool anyButtonContains(Vector2 pos) {
			bool result = false;
			calculateButtonPositions((button, x, y) => {
				if (!result) {
					Rect buttonRect = new Rect(rect.x + x, rect.y + y, button.Size.x, button.Size.y);
					result = buttonRect.Contains(pos);
				}
			});
			return result;
		}

		private void drawToolbar() {
			GUILayout.BeginArea(rect.Rect, GUI.skin.box);
			GUILayout.EndArea();
		}

		private void drawButtons() {
			calculateButtonPositions((button, x, y) => {
				Rect buttonRect = new Rect(rect.x + x, rect.y + y, button.Size.x, button.Size.y);
				button.draw(buttonRect);
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
		}

		internal void add(Button button) {
			Button oldButton = buttons.SingleOrDefault(b => (b.ns == button.ns) && (b.id == button.id));

			if (oldButton != null) {
				oldButton.Destroy();
			}

			button.OnDestroy += () => buttonDestroyed(button);

			buttons.Add(button);
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
			}
		}

		internal void saveSettings(ConfigNode parentNode) {
			ConfigNode toolbarNode = parentNode.AddNode("toolbar");
			toolbarNode.AddValue("x", rect.x.ToString("F0"));
			toolbarNode.AddValue("y", rect.y.ToString("F0"));
			toolbarNode.AddValue("width", rect.width.ToString("F0"));
			toolbarNode.AddValue("height", rect.height.ToString("F0"));
		}

		private void fireChange() {
			if (onChange != null) {
				onChange();
			}
		}
	}
}
