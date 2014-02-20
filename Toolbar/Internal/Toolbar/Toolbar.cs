/*
Copyright (c) 2013-2014, Maik Schreiber
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
	internal class Toolbar : ICursorGrabber {
		internal enum Mode {
			TOOLBAR, FOLDER
		}

		internal enum DisplayMode {
			VISIBLE, HIDDEN, SLIDING_IN, SLIDING_OUT
		}

		private const float BUTTON_SPACING = 1;
		private const float PADDING = 3;
		private const float DEFAULT_X = 300;
		private const float DEFAULT_Y = 300;
		private const float DEFAULT_WIDTH = 250;
		private const float DEFAULT_HEIGHT_FOLDER = 100;
		private const long SLIDE_INTERVAL = 100;

		internal event Action OnChange;
		internal event Action OnSkinChange;

		private delegate void ButtonPositionCalculatedHandler(Button button, Vector2 position);

		private bool Visible {
			set {
				if (value != visible_) {
					if (!value) {
						rectLocked = true;
						buttonOrderLocked = true;
						WindowList.Instance.destroyDialogs();
						dropdownMenu = null;
						hookButtonOrderDraggables(false);
					}

					visible_ = value;
				}
			}
			get {
				return visible_;
			}
		}
		private bool visible_ = true;

		private bool Enabled {
			set {
				if (value != enabled_) {
					if (!value) {
						rectLocked = true;
						buttonOrderLocked = true;
						WindowList.Instance.destroyDialogs();
						dropdownMenu = null;
						hookButtonOrderDraggables(false);
					}

					enabled_ = value;
				}
			}
			get {
				return enabled_;
			}
		}
		private bool enabled_ = true;

		private bool UseKSPSkin {
			set {
				if (value != useKSPSkin_) {
					useKSPSkin_ = value;
					fireSkinChange();
				}
			}
			get {
				return useKSPSkin_;
			}
		}
		private bool useKSPSkin_;

		private bool AtScreenEdge {
			get {
				return AtLeftScreenEdge || AtRightScreenEdge || AtTopScreenEdge || AtBottomScreenEdge;
			}
		}

		private bool AtLeftScreenEdge {
			get {
				return rect.x <= 0;
			}
		}

		private bool AtRightScreenEdge {
			get {
				return rect.x >= (Screen.width - rect.width);
			}
		}

		private bool AtTopScreenEdge {
			get {
				return rect.y <= 0;
			}
		}

		private bool AtBottomScreenEdge {
			get {
				return rect.y >= (Screen.height - rect.height);
			}
		}

		private bool SingleRow {
			get {
				if (buttons.Count(b => isEffectivelyUserVisible(b)) > 0) {
					float maxButtonHeight = buttons.Where(b => isEffectivelyUserVisible(b)).Max(b => b.Size.y);
					return rect.height <= (maxButtonHeight + PADDING * 2);
				} else {
					return true;
				}
			}
		}

		private bool SingleColumn {
			get {
				if (buttons.Count(b => isEffectivelyUserVisible(b)) > 0) {
					float maxButtonWidth = buttons.Where(b => isEffectivelyUserVisible(b)).Max(b => b.Size.x);
					return rect.width <= (maxButtonWidth + PADDING * 2);
				} else {
					return true;
				}
			}
		}
		
		private Mode mode;
		private Toolbar parentToolbar;
		private Rectangle rect;
		private Draggable draggable;
		private Resizable resizable;
		private List<Button> buttons = new List<Button>();
		private HashSet<string> visibleButtonIds = new HashSet<string>();
		private Button dropdownMenuButton;
		private PopupMenu dropdownMenu;
		private bool rectLocked = true;
		private bool buttonOrderLocked = true;
		private bool autoHide;
		private DisplayMode displayMode = DisplayMode.VISIBLE;
		private long slideInOrOutStartTime;
		private FloatCurveXY slideInOrOutCurve;
		private Color autoHideUnimportantButtonAlpha = Color.white;
		private Button mouseHoverButton;
		private float savedMaxWidth = DEFAULT_WIDTH;
		private bool showBorder = true;
		private Dictionary<Draggable, Rectangle> buttonOrderDraggables = new Dictionary<Draggable, Rectangle>();
		private DropMarker buttonOrderDropMarker;
		private Button draggedButton;
		private Rect draggedButtonRect;
		private Button buttonOrderHoveredButton;
		private List<string> savedButtonOrder = new List<string>();
		private EditorLock editorLockToolbar = new EditorLock("ToolbarPlugin_toolbar");
		private EditorLock editorLockDrag = new EditorLock("ToolbarPlugin_drag");
		private EditorLock editorLockReorder = new EditorLock("ToolbarPlugin_buttonReorder");
		private Dictionary<string, Toolbar> folders = new Dictionary<string, Toolbar>();
		private Dictionary<Button, Toolbar> folderButtons = new Dictionary<Button, Toolbar>();
		private Dictionary<string, FolderSettings> savedFolderSettings = new Dictionary<string, FolderSettings>();
		private VisibleButtonsSelector visibleButtonsSelector;
		private HashSet<string> savedVisibleButtons = new HashSet<string>();

		internal Toolbar(Mode mode = Mode.TOOLBAR, Toolbar parentToolbar = null) {
			this.mode = mode;
			this.parentToolbar = parentToolbar;

			autoHideUnimportantButtonAlpha.a = 0.4f;

			rect = new Rectangle(new Rect(DEFAULT_X, DEFAULT_Y, DEFAULT_WIDTH, float.MinValue));

			if (mode == Mode.TOOLBAR) {
				dropdownMenuButton = Button.createToolbarDropdown();
				dropdownMenuButton.OnClick += (e) => toggleDropdownMenu();
				buttons.Add(dropdownMenuButton);

				draggable = new Draggable(rect, PADDING, (pos) => !dropdownMenuButtonContains(pos) && !resizable.HandleRect.Contains(pos));
				resizable = new Resizable(rect, PADDING, (pos) => !dropdownMenuButtonContains(pos));
				draggable.OnDrag += (e) => {
					resizable.Enabled = !draggable.Dragging;
					toolbarDrag();
				};
				resizable.OnDrag += (e) => {
					draggable.Enabled = !resizable.Dragging;
					toolbarResize();
				};
				CursorGrabbing.Instance.add(draggable);
				CursorGrabbing.Instance.add(resizable);

				CursorGrabbing.Instance.add(this);

				ToolbarManager.InternalInstance.OnCommandAdded += updateVisibleButtons;
			} else {
				showBorder = parentToolbar.showBorder;
				useKSPSkin_ = parentToolbar.UseKSPSkin;
			}
		}

		private void toolbarDrag() {
			if (!draggable.Dragging) {
				fireChange();
			}
		}

		private void toolbarResize() {
			if (resizable.Dragging) {
				float maxButtonWidth = buttons.Where(b => isEffectivelyUserVisible(b)).Max(b => b.Size.x);
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

		private bool dropdownMenuButtonContains(Vector2 pos) {
			return getRect(dropdownMenuButton).shift(new Vector2(rect.x + PADDING, rect.y + PADDING)).Contains(pos);
		}

		internal void destroy() {
			if (mode == Mode.TOOLBAR) {
				CursorGrabbing.Instance.remove(draggable);
				CursorGrabbing.Instance.remove(resizable);

				CursorGrabbing.Instance.remove(this);

				ToolbarManager.InternalInstance.OnCommandAdded -= updateVisibleButtons;
			}

			editorLockToolbar.draw(false);
			editorLockDrag.draw(false);
			editorLockReorder.draw(false);
		}

		internal void draw() {
			// only show toolbar if there is at least one visible button
			// that is not the drop-down menu button
			if (Visible &&
				((mode == Mode.FOLDER) || buttons.Any((b) => !b.Equals(dropdownMenuButton) && isEffectivelyUserVisible(b)))) {

				forceAutoSizeIfButtonVisibilitiesChanged();
				autoSize();

				if (mode == Mode.FOLDER) {
					autoPositionFolder();
				}

				if (autoHide && (dropdownMenu == null) && AtScreenEdge) {
					handleAutoHide();
				}

				int oldDepth = GUI.depth;

				GUI.depth = -99;
				drawToolbarBorder();

				GUI.depth = -100;
				if (buttonOrderDropMarker != null) {
					buttonOrderDropMarker.draw();
				}

				GUISkin oldSkin = GUI.skin;
				if (UseKSPSkin) {
					GUI.skin = HighLogic.Skin;
				}
				drawButtons();
				GUI.skin = oldSkin;

				foreach (Toolbar folder in folders.Values) {
					folder.draw();
				}

				if (Enabled && rectLocked && (buttonOrderLocked || (draggedButton == null)) && (dropdownMenu == null) && (displayMode == DisplayMode.VISIBLE)) {
					drawButtonToolTips();
				}

				GUI.depth = oldDepth;

				Vector2 mousePos = Utils.getMousePosition();
				editorLockToolbar.draw(rect.contains(mousePos));
				editorLockDrag.draw(!rectLocked);
				editorLockReorder.draw(!buttonOrderLocked);
			} else {
				editorLockToolbar.draw(false);
				editorLockDrag.draw(false);
				editorLockReorder.draw(false);
			}
		}

		private void autoPositionFolder() {
			// at this point, we should already have a good width/height

			if (parentToolbar.SingleColumn) {
				// position to right of parent toolbar
				rect.x = parentToolbar.rect.x + parentToolbar.rect.width + BUTTON_SPACING;
				float origX = rect.x;
				rect.y = parentToolbar.rect.y + (parentToolbar.rect.height - rect.height) / 2;
				rect.clampToScreen(0);
				// clamping to screen moved it to the left -> position to left of parent toolbar
				if (rect.x < origX) {
					rect.x = parentToolbar.rect.x - rect.width - BUTTON_SPACING;
					rect.clampToScreen(0);
				}
			} else {
				// position above parent toolbar
				rect.x = parentToolbar.rect.x + (parentToolbar.rect.width - rect.width) / 2;
				rect.y = parentToolbar.rect.y - rect.height - BUTTON_SPACING;
				float origY = rect.y;
				rect.clampToScreen(0);
				// clamping to screen moved it to the bottom -> position below parent toolbar
				if (rect.y > origY) {
					rect.y = parentToolbar.rect.y + parentToolbar.rect.height + BUTTON_SPACING;
					rect.clampToScreen(0);
				}
			}
		}

		private void handleAutoHide() {
			long now = DateTime.UtcNow.getSeconds();

			if (rect.contains(Utils.getMousePosition()) ||
				buttons.Any(b => b.command.Important && isEffectivelyUserVisible(b)) ||
				folders.Values.Any(f => f.Visible) ||
				(visibleButtonsSelector != null)) {

				if ((displayMode != DisplayMode.VISIBLE) && (displayMode != DisplayMode.SLIDING_IN)) {
					Log.debug("display mode is {0}, starting slide in", displayMode);
					if (displayMode == DisplayMode.HIDDEN) {
						slideInOrOutStartTime = now;
					} else {
						// do everything in reverse
						long timeSpent = now - slideInOrOutStartTime;
						long timeToGo = SLIDE_INTERVAL - timeSpent;
						slideInOrOutStartTime = now - timeToGo;
					}
					displayMode = DisplayMode.SLIDING_IN;

					slideInOrOutCurve = new FloatCurveXY();
					if (AtLeftScreenEdge || AtRightScreenEdge) {
						slideInOrOutCurve.add(0, new Vector2(AtLeftScreenEdge ? (-rect.width + PADDING) : (Screen.width - PADDING), rect.y));
						slideInOrOutCurve.add(SLIDE_INTERVAL, new Vector2(AtLeftScreenEdge ? -PADDING : (Screen.width - rect.width + PADDING), rect.y));
					} else if (AtTopScreenEdge || AtBottomScreenEdge) {
						slideInOrOutCurve.add(0, new Vector2(rect.x, AtTopScreenEdge ? (-rect.height + PADDING) : (Screen.height - PADDING)));
						slideInOrOutCurve.add(SLIDE_INTERVAL, new Vector2(rect.x, AtTopScreenEdge ? -PADDING : (Screen.height - rect.height + PADDING)));
					}
				}
			} else {
				if ((displayMode != DisplayMode.HIDDEN) && (displayMode != DisplayMode.SLIDING_OUT)) {
					Log.debug("display mode is {0}, starting slide out", displayMode);
					if (displayMode == DisplayMode.VISIBLE) {
						slideInOrOutStartTime = now;
					} else {
						// do everything in reverse
						long timeSpent = now - slideInOrOutStartTime;
						long timeToGo = SLIDE_INTERVAL - timeSpent;
						slideInOrOutStartTime = now - timeToGo;
					}
					displayMode = DisplayMode.SLIDING_OUT;

					slideInOrOutCurve = new FloatCurveXY();
					if (AtLeftScreenEdge || AtRightScreenEdge) {
						slideInOrOutCurve.add(0, new Vector2(AtLeftScreenEdge ? -PADDING : (Screen.width - rect.width + PADDING), rect.y));
						slideInOrOutCurve.add(SLIDE_INTERVAL, new Vector2(AtLeftScreenEdge ? (-rect.width + PADDING) : (Screen.width - PADDING), rect.y));
					} else if (AtTopScreenEdge || AtBottomScreenEdge) {
						slideInOrOutCurve.add(0, new Vector2(rect.x, AtTopScreenEdge ? -PADDING : (Screen.height - rect.height + PADDING)));
						slideInOrOutCurve.add(SLIDE_INTERVAL, new Vector2(rect.x, AtTopScreenEdge ? (-rect.height + PADDING) : (Screen.height - PADDING)));
					}
				}
			}

			if ((displayMode == DisplayMode.SLIDING_IN) || (displayMode == DisplayMode.SLIDING_OUT)) {
				long timeSinceStartTime = now - slideInOrOutStartTime;
				Vector2 newXY = slideInOrOutCurve.evaluate(Mathf.Min(timeSinceStartTime, SLIDE_INTERVAL));
				rect.x = newXY.x;
				rect.y = newXY.y;
				Log.debug("slide step, time since start: {0}, new position: {1},{2}", timeSinceStartTime, rect.x, rect.y);

				if (timeSinceStartTime >= SLIDE_INTERVAL) {
					displayMode = (displayMode == DisplayMode.SLIDING_IN) ? DisplayMode.VISIBLE : DisplayMode.HIDDEN;
					Log.debug("slide done, set display mode to {0}", displayMode);
					slideInOrOutCurve = null;
				}
			}
		}

		private bool shouldSlideOut() {
			return autoHide && (displayMode != DisplayMode.HIDDEN) && !rect.contains(Utils.getMousePosition()) && AtScreenEdge;
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

			if (displayMode == DisplayMode.VISIBLE) {
				rect.clampToScreen(PADDING);
			}
		}

		private float getMinWidthForButtons() {
			if (mode == Mode.FOLDER) {
				int count = buttons.Count((b) => !b.Equals(dropdownMenuButton) && isEffectivelyUserVisible(b));
				if (count == 0) {
					return DEFAULT_WIDTH;
				} else {
					// make it roughly a square
					int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
					// they're all the same size, so let's just take the first one
					Button firstVisibleButton = buttons.First((b) => !b.Equals(dropdownMenuButton) && isEffectivelyUserVisible(b));
					float buttonWidth = firstVisibleButton.Size.x;
					return buttonWidth * columns + BUTTON_SPACING * (columns - 1) + PADDING * 2;
				}
			} else {
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
		}

		private float getMinHeightForButtons() {
			if ((mode == Mode.FOLDER) && (buttons.Count((b) => !b.Equals(dropdownMenuButton) && isEffectivelyUserVisible(b)) == 0)) {
				return DEFAULT_HEIGHT_FOLDER;
			} else {
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
		}

		private void calculateButtonPositions(ButtonPositionCalculatedHandler buttonPositionCalculatedHandler) {
			float x = PADDING;
			float y = PADDING;
			float lineHeight = float.MinValue;
			float widestLineWidth = float.MinValue;
			float currentLineWidth = 0;
			foreach (Button button in buttons) {
				if (isEffectivelyUserVisible(button) && !button.Equals(dropdownMenuButton)) {
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
			if (dropdownMenuButton != null) {
				if (y == PADDING) {
					// all buttons on a single line
					buttonPositionCalculatedHandler(dropdownMenuButton, new Vector2(x + 2, (lineHeight - dropdownMenuButton.Size.y) / 2 + PADDING));
				} else {
					// multiple lines
					buttonPositionCalculatedHandler(dropdownMenuButton, new Vector2((widestLineWidth - dropdownMenuButton.Size.x) / 2 + PADDING, y + lineHeight + BUTTON_SPACING + 2));
				}
			}
		}

		private void drawToolbarBorder() {
			if (showBorder || !rectLocked || !buttonOrderLocked ||
				((mode == Mode.FOLDER) && !buttons.Any(b => isEffectivelyUserVisible(b)))) {

				Color oldColor = GUI.color;
				if (shouldSlideOut() && (displayMode == DisplayMode.VISIBLE) && (dropdownMenu == null)) {
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
				Rect buttonRect = button.Equals(draggedButton) ? draggedButtonRect : new Rect(rect.x + pos.x, rect.y + pos.y, button.Size.x, button.Size.y);
				buttonsToDraw.Add(button, buttonRect);
			});

			bool shouldHide = shouldSlideOut();
			Button currentMouseHoverButton = null;
			Vector2 mousePos = Utils.getMousePosition();
			foreach (KeyValuePair<Button, Rect> entry in buttonsToDraw) {
				Button button = entry.Key;
				if (!button.destroyed) {
					Rect buttonRect = entry.Value;
					Color oldColor = GUI.color;
					if (shouldHide && (displayMode != DisplayMode.HIDDEN) &&
						!button.command.Important &&
						(!folderButtons.ContainsKey(button) || !folderButtons[button].Visible) &&
						(dropdownMenu == null)) {

						GUI.color = autoHideUnimportantButtonAlpha;
					}
					button.draw(buttonRect,
						((Enabled && rectLocked && buttonOrderLocked) || button.Equals(dropdownMenuButton)) &&
						!Utils.isPauseMenuOpen() &&
						!WindowList.Instance.ModalDialogOpen);
					GUI.color = oldColor;

					if (!button.destroyed) {
						if (buttonRect.Contains(mousePos)) {
							currentMouseHoverButton = button;
						}
					}
				}
			}

			if (rectLocked && buttonOrderLocked) {
				handleMouseHover(currentMouseHoverButton);
			}
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

		private void forceAutoSizeIfButtonVisibilitiesChanged() {
			// ignore changes while sliding in/out
			if ((displayMode == DisplayMode.VISIBLE) || (displayMode == DisplayMode.HIDDEN)) {
				HashSet<string> newVisibleButtonIds = new HashSet<string>();
				foreach (Button button in buttons.Where(b => isEffectivelyUserVisible(b))) {
					newVisibleButtonIds.Add(button.FullId);
				}
				if (!newVisibleButtonIds.SetEquals(visibleButtonIds)) {
					Log.info("button visibilities have changed, forcing auto-size");
					visibleButtonIds = newVisibleButtonIds;

					if (SingleRow) {
						// docked at right screen edge -> keep it that way by moving to screen edge
						if (AtRightScreenEdge) {
							rect.x = Screen.width;
						}
					} else {
						// docked at bottom screen edge -> keep it that way by moving to screen edge
						if (AtBottomScreenEdge) {
							rect.y = Screen.height;
						}
					}

					// expand width to last saved width, then resize for buttons, and expand height to fit new buttons
					rect.width = savedMaxWidth;
					rect.width = getMinWidthForButtons();
					rect.height = getMinHeightForButtons();

					// move rect into view as necessary
					switch (displayMode) {
						case DisplayMode.VISIBLE:
							rect.clampToScreen(PADDING);
							break;
						case DisplayMode.HIDDEN:
							rect.clampToScreen(new Vector2(rect.width - PADDING, rect.height - PADDING));
							break;
					}

					Log.debug("new position after forcing auto-size: {0},{1} (display mode: {2})", rect.x, rect.y, displayMode);

					fireChange();
				}
			}
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
			if (draggable != null) {
				draggable.update();
			}
			if (resizable != null) {
				resizable.update();
			}

			if (dropdownMenu != null) {
				// auto-close drop-down menu when clicking outside
				if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) && !dropdownMenu.contains(Utils.getMousePosition())) {
					dropdownMenu.destroy();
					dropdownMenu = null;
				}

				// auto-close drop-down menu when pause menu is opened
				if (Utils.isPauseMenuOpen()) {
					dropdownMenu.destroy();
					dropdownMenu = null;
				}
			}

			if (buttonOrderDraggables.Count() > 0) {
				foreach (Draggable d in new List<Draggable>(buttonOrderDraggables.Keys)) {
					d.update();
				}
			}
		}

		/// <summary>
		/// Set up a button to configure visible buttons if there is currently no button visible,
		/// but if there are buttons that could be made visible by the player.
		/// </summary>
		private void setupConfigureVisibleButtonsButton() {
			if (mode == Mode.TOOLBAR) {
				Log.debug("setting up button to configure visible buttons");

				Command command = new Command(ToolbarManager.NAMESPACE_INTERNAL, "configureVisibleButtons");
				command.TexturePath = "000_Toolbar/new-button-available";
				command.ToolTip = "Configure Visible Toolbar Buttons";
				command.OnClick += (e) => {
					toggleVisibleButtonsSelector();
				};
				command.Visibility = new FunctionVisibility(() => {
					bool contentsExist = ToolbarManager.InternalInstance.Commands.Any(c => !c.IsInternal && c.EffectivelyVisible);
					bool contentsVisible = buttons.Any(b => !b.IsInternal || folderButtons.ContainsKey(b));
					return contentsExist && !contentsVisible;
				});
				Button button = new Button(command, this);
				add(button);
			}
		}

		internal void add(Button button) {
			// destroy old button with the same ID
			Button oldButton = buttons.SingleOrDefault(b => b.FullId == button.FullId);
			if (oldButton != null) {
				oldButton.Destroy();
			}
			// same with any folders
			Toolbar folder = folders.Values.SingleOrDefault(f => f.buttons.Any(b => b.FullId == button.FullId));
			if (folder != null) {
				oldButton = folder.buttons.SingleOrDefault(b => b.FullId == button.FullId);
				if (oldButton != null) {
					oldButton.Destroy();
				}
			}

			// move button to correct folder if necessary
			string buttonId = button.FullId;
			string folderId = savedFolderSettings.Where(kv => kv.Value.buttons.Contains(buttonId)).Select(kv => kv.Key).SingleOrDefault();
			if ((folderId != null) && folders.ContainsKey(folderId)) {
				// move to folder
				folders[folderId].add(button);
			} else {
				// add to toolbar
				button.OnDestroy += buttonDestroyed;
				buttons.Add(button);
				sortButtons(buttons, compareButtonsUserOrder);
			}
		}

		private void sortButtons(List<Button> buttons, Comparison<Button> comparison) {
			bool addDropDownMenuButton = buttons.Contains(dropdownMenuButton);
			if (addDropDownMenuButton) {
				buttons.Remove(dropdownMenuButton);
			}

			buttons.Sort(comparison);

			if (addDropDownMenuButton) {
				buttons.Add(dropdownMenuButton);
			}
		}

		private int compareButtonsUserOrder(Button b1, Button b2) {
			string id1 = b1.FullId;
			string id2 = b2.FullId;
			int idx1 = savedButtonOrder.IndexOf(id1);
			int idx2 = savedButtonOrder.IndexOf(id2);
			if ((idx1 >= 0) && (idx2 >= 0)) {
				return idx1 - idx2;
			} else if ((idx1 >= 0) && (idx2 < 0)) {
				return -1;
			} else if ((idx1 < 0) && (idx2 >= 0)) {
				return 1;
			} else {
				return b1.command.CompareTo(b2.command);
			}
		}

		private void remove(Button button) {
			button.OnDestroy -= buttonDestroyed;
			buttons.Remove(button);
			if (folderButtons.ContainsKey(button)) {
				folderButtons.Remove(button);
			}
			if ((mouseHoverButton != null) && mouseHoverButton.Equals(button)) {
				mouseHoverButton = null;
			}
		}

		private void buttonDestroyed(DestroyEvent e) {
			remove(e.button);
		}

		internal void loadSettings(ConfigNode toolbarNode) {
			rect.x = toolbarNode.get("x", DEFAULT_X);
			rect.y = toolbarNode.get("y", DEFAULT_Y);
			rect.width = toolbarNode.get("width", DEFAULT_WIDTH);
			rect.height = toolbarNode.get("height", 0f);
			autoHide = toolbarNode.get("autoHide", false);
			showBorder = toolbarNode.get("drawBorder", true);
			UseKSPSkin = toolbarNode.get("useKSPSkin", false);
			savedButtonOrder = toolbarNode.get("buttonOrder", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			savedVisibleButtons = new HashSet<string>(toolbarNode.get("visibleButtons", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

			if (toolbarNode.HasNode("folders")) {
				foreach (ConfigNode folderNode in toolbarNode.GetNode("folders").nodes) {
					string folderId = folderNode.name;
					string toolTip = folderNode.get("toolTip", string.Empty);
					HashSet<string> buttonIds = new HashSet<string>(folderNode.get("buttons", string.Empty).Split(new char[] { ',' }));

					Toolbar folder = createFolder(folderId, toolTip, false);

					savedFolderSettings[folderId].buttons = buttonIds;
				}
			}

			savedMaxWidth = rect.width;

			updateVisibleButtons();
			sortButtons(buttons, compareButtonsUserOrder);
		}

		internal void saveSettings(ConfigNode toolbarNode) {
			toolbarNode.overwrite("x", rect.x.ToString("F0"));
			toolbarNode.overwrite("y", rect.y.ToString("F0"));
			toolbarNode.overwrite("width", savedMaxWidth.ToString("F0"));
			toolbarNode.overwrite("height", rect.height.ToString("F0"));
			toolbarNode.overwrite("autoHide", autoHide.ToString());
			toolbarNode.overwrite("drawBorder", showBorder.ToString());
			toolbarNode.overwrite("useKSPSkin", UseKSPSkin.ToString());
			toolbarNode.overwrite("buttonOrder", string.Join(",", savedButtonOrder.ToArray()));
			toolbarNode.overwrite("visibleButtons", string.Join(",", savedVisibleButtons.ToArray()));

			ConfigNode foldersNode = toolbarNode.overwriteNode("folders");
			foreach (KeyValuePair<string, FolderSettings> entry in savedFolderSettings) {
				ConfigNode folderNode = foldersNode.getOrCreateNode(entry.Key);
				folderNode.overwrite("toolTip", entry.Value.toolTip ?? string.Empty);
				folderNode.overwrite("buttons", string.Join(",", entry.Value.buttons.ToArray()));
			}
			if (foldersNode.CountNodes == 0) {
				toolbarNode.RemoveNode("folders");
			}
		}

		private void fireChange() {
			if (OnChange != null) {
				OnChange();
			}
		}

		private void fireSkinChange() {
			if (OnSkinChange != null) {
				OnSkinChange();
			}
		}

		private void toggleDropdownMenu() {
			if (dropdownMenu == null) {
				dropdownMenu = new PopupMenu(new Vector2(rect.x + PADDING + getPosition(dropdownMenuButton).x, rect.y + rect.height + BUTTON_SPACING));

				bool regularEntriesEnabled = rectLocked && buttonOrderLocked;

				Button visibleButtonsButton = Button.createMenuOption("Configure Visible Buttons...");
				visibleButtonsButton.command.Enabled = regularEntriesEnabled && (visibleButtonsSelector == null);
				visibleButtonsButton.OnClick += (e) => {
					toggleVisibleButtonsSelector();
				};
				dropdownMenu += visibleButtonsButton;

				dropdownMenu += Separator.Instance;

				Button toggleRectLockButton = Button.createMenuOption(rectLocked ? "Unlock Position and Size" : "Lock Position and Size");
				toggleRectLockButton.OnClick += (e) => {
					rectLocked = !rectLocked;
					draggable.Enabled = !rectLocked;
					resizable.Enabled = !rectLocked;

					if (rectLocked) {
						fireChange();
					} else {
						autoHide = false;
						foreach (Toolbar folder in folders.Values) {
							folder.Visible = false;
						}
						if (visibleButtonsSelector != null) {
							visibleButtonsSelector.destroy();
						}
					}
				};
				toggleRectLockButton.command.Enabled = buttonOrderLocked;
				dropdownMenu += toggleRectLockButton;

				Button toggleButtonOrderLockButton = Button.createMenuOption(buttonOrderLocked ? "Unlock Button Order" : "Lock Button Order");
				toggleButtonOrderLockButton.OnClick += (e) => {
					buttonOrderLocked = !buttonOrderLocked;

					hookButtonOrderDraggables(!buttonOrderLocked);

					if (buttonOrderLocked) {
						fireChange();
					} else {
						autoHide = false;
					}
					foreach (Toolbar folder in folders.Values) {
						folder.Enabled = buttonOrderLocked;
					}
					if (visibleButtonsSelector != null) {
						visibleButtonsSelector.destroy();
					}
				};
				toggleButtonOrderLockButton.command.Enabled = rectLocked;
				dropdownMenu += toggleButtonOrderLockButton;

				Button toggleAutoHideButton = Button.createMenuOption(autoHide ? "Deactivate Auto-Hide at Screen Edge" : "Activate Auto-Hide at Screen Edge");
				toggleAutoHideButton.OnClick += (e) => {
					autoHide = !autoHide;
					fireChange();
				};
				toggleAutoHideButton.command.Enabled = regularEntriesEnabled && (autoHide || AtScreenEdge);
				dropdownMenu += toggleAutoHideButton;

				Button toggleDrawBorderButton = Button.createMenuOption(showBorder ? "Hide Border" : "Show Border");
				toggleDrawBorderButton.OnClick += (e) => {
					showBorder = !showBorder;
					foreach (Toolbar folder in folders.Values) {
						folder.showBorder = showBorder;
					}
					fireChange();
				};
				toggleDrawBorderButton.command.Enabled = regularEntriesEnabled;
				dropdownMenu += toggleDrawBorderButton;

				Button toggleKSPSkinButton = Button.createMenuOption(UseKSPSkin ? "Use Unity 'Smoke' Skin" : "Use KSP Skin");
				toggleKSPSkinButton.OnClick += (e) => {
					UseKSPSkin = !UseKSPSkin;
					foreach (Toolbar folder in folders.Values) {
						folder.UseKSPSkin = UseKSPSkin;
					}
					fireChange();
				};
				toggleKSPSkinButton.command.Enabled = regularEntriesEnabled;
				dropdownMenu += toggleKSPSkinButton;

				dropdownMenu += Separator.Instance;

				Button createFolderButton = Button.createMenuOption("Create New Folder...");
				createFolderButton.OnClick += (e) => createFolder();
				createFolderButton.command.Enabled = regularEntriesEnabled;
				dropdownMenu += createFolderButton;

				dropdownMenu += Separator.Instance;

				Button createToolbarButton = Button.createMenuOption("Create New Toolbar");
				createToolbarButton.OnClick += (e) => createToolbar();
				createToolbarButton.command.Enabled = regularEntriesEnabled;
				dropdownMenu += createToolbarButton;

				Button deleteToolbarButton = Button.createMenuOption("Delete Toolbar...");
				deleteToolbarButton.OnClick += (e) => deleteToolbar();
				deleteToolbarButton.command.Enabled = regularEntriesEnabled && (ToolbarManager.InternalInstance.ToolbarsCount > 1);
				dropdownMenu += deleteToolbarButton;

				dropdownMenu += Separator.Instance;

				Button aboutButton = Button.createMenuOption("About the Toolbar Plugin...");
				aboutButton.OnClick += (e) => Application.OpenURL(ToolbarManager.FORUM_THREAD_URL);
				dropdownMenu += aboutButton;

				IPopupMenuOption donateButton = new TextureMenuOption(GameDatabase.Instance.GetTexture("000_Toolbar/donate", false), new Vector2(10, 0));
				donateButton.OnClick += (e) => Application.OpenURL(ToolbarManager.DONATE_URL);
				dropdownMenu += donateButton;

				dropdownMenu.OnAnyOptionClicked += () => {
					dropdownMenu.destroy();
					dropdownMenu = null;
				};
			} else {
				dropdownMenu.destroy();
				dropdownMenu = null;
			}
		}

		private void hookButtonOrderDraggables(bool enabled) {
			if (enabled) {
				calculateButtonPositions((button, pos) => {
					if (!button.Equals(dropdownMenuButton)) {
						Rectangle buttonRect = new Rectangle(new Rect(rect.x + pos.x, rect.y + pos.y, button.Size.x, button.Size.y));
						Draggable draggable = new Draggable(buttonRect, 0, null);
						draggable.Enabled = true;
						draggable.OnDrag += buttonDrag;
						buttonOrderDraggables.Add(draggable, buttonRect);
					}
				});

				buttonOrderDropMarker = new DropMarker();
			} else {
				foreach (Draggable d in buttonOrderDraggables.Keys) {
					d.OnDrag -= buttonDrag;
				}
				buttonOrderDraggables.Clear();
				buttonOrderDropMarker = null;
			}
			draggedButton = null;
		}

		private void buttonDrag(DragEvent e) {
			if (e.draggable.Dragging) {
				Rectangle dragRect = buttonOrderDraggables[e.draggable];

				if (draggedButton == null) {
					draggedButton = buttons.SingleOrDefault(b => getRect(b).shift(new Vector2(rect.x + PADDING, rect.y + PADDING)).Equals(dragRect.Rect));
				}

				if (draggedButton != null) {
					draggedButtonRect = dragRect.Rect;

					Vector2 mousePos = Utils.getMousePosition();
					buttonOrderHoveredButton = buttons.SingleOrDefault(
						b => !b.Equals(draggedButton) && !b.Equals(dropdownMenuButton) && getRect(b).shift(new Vector2(rect.x + PADDING, rect.y + PADDING)).Contains(mousePos));
					if (buttonOrderHoveredButton != null) {
						Rect hoveredButtonRect = getRect(buttonOrderHoveredButton).shift(new Vector2(rect.x + PADDING, rect.y + PADDING));
						Toolbar folder = folderButtons.ContainsKey(buttonOrderHoveredButton) ? folderButtons[buttonOrderHoveredButton] : null;
						if ((folder != null) &&
							// disallow folders in folders
							!folderButtons.ContainsKey(draggedButton)) {

							float widthOneThird = hoveredButtonRect.width / 3;
							float middleX = hoveredButtonRect.x + widthOneThird;
							if (new Rect(middleX, hoveredButtonRect.y, widthOneThird, hoveredButtonRect.height).Contains(mousePos)) {
								// middle section
								buttonOrderDropMarker.Rect = hoveredButtonRect;
							} else if (new Rect(hoveredButtonRect.x, hoveredButtonRect.y, widthOneThird, hoveredButtonRect.height).Contains(mousePos)) {
								// left section
								buttonOrderDropMarker.Rect = new Rect(
									hoveredButtonRect.x - DropMarker.MARKER_LINE_WIDTH, hoveredButtonRect.y,
									DropMarker.MARKER_LINE_WIDTH, hoveredButtonRect.height);
							} else {
								// right section
								buttonOrderDropMarker.Rect = new Rect(
									hoveredButtonRect.x + hoveredButtonRect.width, hoveredButtonRect.y,
									DropMarker.MARKER_LINE_WIDTH, hoveredButtonRect.height);
							}
						} else {
							bool leftSide = new Rect(hoveredButtonRect.x, hoveredButtonRect.y, hoveredButtonRect.width / 2, hoveredButtonRect.height).Contains(mousePos);
							// TODO: improve this to show a horizontal drop marker instead of a vertical one for single-column toolbars
							buttonOrderDropMarker.Rect = new Rect(
								leftSide ? (hoveredButtonRect.x - DropMarker.MARKER_LINE_WIDTH) : (hoveredButtonRect.x + hoveredButtonRect.width),
								hoveredButtonRect.y,
								DropMarker.MARKER_LINE_WIDTH, hoveredButtonRect.height);
						}
					}
					buttonOrderDropMarker.Visible = buttonOrderHoveredButton != null;
				}
			} else {
				if ((draggedButton != null) && (buttonOrderHoveredButton != null)) {
					Rect hoveredButtonRect = getRect(buttonOrderHoveredButton).shift(new Vector2(rect.x + PADDING, rect.y + PADDING));
					Vector2 mousePos = Utils.getMousePosition();
					bool leftSide = false;
					bool intoFolder = false;
					Toolbar folder = folderButtons.ContainsKey(buttonOrderHoveredButton) ? folderButtons[buttonOrderHoveredButton] : null;
					if (folder != null) {
						float widthOneThird = hoveredButtonRect.width / 3;
						float middleX = hoveredButtonRect.x + widthOneThird;
						if (new Rect(middleX, hoveredButtonRect.y, widthOneThird, hoveredButtonRect.height).Contains(mousePos)) {
							intoFolder = true;
						} else if (new Rect(hoveredButtonRect.x, hoveredButtonRect.y, widthOneThird, hoveredButtonRect.height).Contains(mousePos)) {
							leftSide = true;
						}
					} else {
						leftSide = new Rect(hoveredButtonRect.x, hoveredButtonRect.y, hoveredButtonRect.width / 2, hoveredButtonRect.height).Contains(mousePos);
					}

					if (intoFolder) {
						moveButtonToFolder(draggedButton, folder);
					} else {
						int draggedButtonIdx = buttons.IndexOf(draggedButton);
						int hoveredButtonIdx = buttons.IndexOf(buttonOrderHoveredButton);
						if (!leftSide) {
							hoveredButtonIdx++;
						}

						buttons.RemoveAt(draggedButtonIdx);
						if (hoveredButtonIdx > draggedButtonIdx) {
							hoveredButtonIdx--;
						}
						buttons.Insert(hoveredButtonIdx, draggedButton);
					}

					savedButtonOrder = buttons.Where(b => !b.Equals(dropdownMenuButton)).Select(b => b.FullId).ToList();

					Dictionary<string, FolderSettings> newSavedFolderSettings = new Dictionary<string, FolderSettings>();
					foreach (KeyValuePair<string, Toolbar> entry in folders) {
						HashSet<string> folderButtonIds = new HashSet<string>(entry.Value.buttons.Select(b => b.FullId));
						newSavedFolderSettings.Add(entry.Key, new FolderSettings() {
							toolTip = savedFolderSettings[entry.Key].toolTip,
							buttons = folderButtonIds
						});
					}
					savedFolderSettings = newSavedFolderSettings;

					fireChange();
				}

				forceAutoSizeIfButtonVisibilitiesChanged();

				// reset draggables, drop marker, and dragged button
				hookButtonOrderDraggables(false);
				hookButtonOrderDraggables(true);
			}
		}

		private void moveButtonToFolder(Button button, Toolbar folder) {
			remove(button);
			folder.add(button);
		}

		private void createFolder() {
			FolderSettingsDialog folderSettingsDialog = new FolderSettingsDialog("New Folder");
			folderSettingsDialog.OnOkClicked += () => {
				createFolder("folder_" + new System.Random().Next(int.MaxValue), folderSettingsDialog.ToolTip, true);
			};
		}

		private void editFolder(Toolbar folder) {
			string folderId = folders.Single(kv => kv.Value.Equals(folder)).Key;
			string toolTip = savedFolderSettings[folderId].toolTip;
			FolderSettingsDialog folderSettingsDialog = new FolderSettingsDialog(toolTip);
			folderSettingsDialog.OnOkClicked += () => {
				toolTip = folderSettingsDialog.ToolTip;
				savedFolderSettings[folderId].toolTip = toolTip;
				Button folderButton = folderButtons.Single(kv => kv.Value.Equals(folder)).Key;
				folderButton.command.ToolTip = toolTip;
				fireChange();
			};
		}

		private Toolbar createFolder(string id, string toolTip, bool visible) {
			if (visible) {
				// close all other folders first
				foreach (Toolbar folder in folders.Values) {
					folder.Visible = false;
				}
			}

			Toolbar newFolder = new Toolbar(Mode.FOLDER, this);
			newFolder.Visible = visible;
			folders.Add(id, newFolder);

			Command folderCommand = new Command(ToolbarManager.NAMESPACE_INTERNAL, id);
			folderCommand.TexturePath = "000_Toolbar/folder";
			folderCommand.ToolTip = toolTip;
			Button folderButton = null;
			folderCommand.OnClick += (e) => {
				switch (e.MouseButton) {
					case 0:
						newFolder.Visible = !newFolder.Visible;
						if (newFolder.Visible) {
							foreach (Toolbar otherFolder in folders.Values.Where(f => !f.Equals(newFolder))) {
								otherFolder.Visible = false;
							}
						}
						break;

					case 1:
						openFolderButtonDropdownMenu(newFolder, getPosition(folderButton) + new Vector2(rect.x + PADDING, rect.y + PADDING + folderButton.Size.y + BUTTON_SPACING));
						break;
				}
			};
			folderButton = new Button(folderCommand, this);
			folderButtons.Add(folderButton, newFolder);
			add(folderButton);

			savedFolderSettings.Add(id, new FolderSettings() {
				buttons = new HashSet<string>(),
				toolTip = toolTip
			});

			return newFolder;
		}

		private void openFolderButtonDropdownMenu(Toolbar folder, Vector2 pos) {
			dropdownMenu = new PopupMenu(pos);

			Button editButton = Button.createMenuOption("Edit Folder Settings");
			editButton.OnClick += (e) => editFolder(folder);
			dropdownMenu += editButton;

			Button deleteButton = Button.createMenuOption("Delete Folder");
			deleteButton.OnClick += (e) => deleteFolder(folder);
			dropdownMenu += deleteButton;

			dropdownMenu.OnAnyOptionClicked += () => {
				dropdownMenu.destroy();
				dropdownMenu = null;
			};
		}

		private void deleteFolder(Toolbar folder) {
			ConfirmDialog.confirm("Delete Folder", "Delete this folder? Buttons inside the folder will be moved to the main toolbar.",
				() => removeFolder(folder));
		}

		private void removeFolder(Toolbar folder) {
			string folderId = folders.Single(kv => kv.Value.Equals(folder)).Key;
			folders.Remove(folderId);

			savedFolderSettings.Remove(folderId);

			Button folderButton = folderButtons.Single(kv => kv.Value.Equals(folder)).Key;
			folderButton.Destroy();

			foreach (Button b in new List<Button>(folder.buttons)) {
				folder.remove(b);
				add(b);
			}

			folder.destroy();
		}

		public bool grabCursor() {
			if (Visible && !buttonOrderLocked) {
				Vector2 mousePos = Utils.getMousePosition();
				Button hoveredButton = buttons.SingleOrDefault(
					b => !b.Equals(dropdownMenuButton) && getRect(b).shift(new Vector2(rect.x + PADDING, rect.y + PADDING)).Contains(mousePos));
				bool setCursor = (hoveredButton != null) || (draggedButton != null);
				if (setCursor) {
					Cursor.SetCursor(GameDatabase.Instance.GetTexture("000_Toolbar/move-cursor", false), new Vector2(10, 10), CursorMode.ForceSoftware);
				}
				return setCursor;
			} else {
				return false;
			}
		}

		private void toggleVisibleButtonsSelector() {
			if (visibleButtonsSelector == null) {
				visibleButtonsSelector = new VisibleButtonsSelector(savedVisibleButtons);
				visibleButtonsSelector.OnButtonSelectionChanged += () => {
					Log.info("user changed button visibilities");
					updateVisibleButtons();
					fireChange();
				};
				visibleButtonsSelector.OnDestroy += () => {
					visibleButtonsSelector = null;
				};
			} else {
				visibleButtonsSelector.destroy();
				// OnDestroy event will clear the variable
			}
		}

		private bool isEffectivelyUserVisible(Button button) {
			if (button.command.EffectivelyVisible) {
				if (!button.IsInternal) {
					string id = button.FullId;
					return (mode == Mode.TOOLBAR) ? savedVisibleButtons.Contains(id) : parentToolbar.savedVisibleButtons.Contains(id);
				} else {
					return true;
				}
			} else {
				return false;
			}
		}

		private void updateVisibleButtons() {
			Log.debug("updating visible buttons");

			// destroy all buttons except folder buttons
			foreach (Button button in new List<Button>(buttons.Where(b => !b.Equals(dropdownMenuButton) && !folderButtons.ContainsKey(b)))) {
				button.Destroy();
			}

			mouseHoverButton = null;

			// create buttons according to configured visible buttons
			HashSet<string> buttonIds = new HashSet<string>(savedVisibleButtons);
			foreach (Command command in ToolbarManager.InternalInstance.Commands.Where(c => c.IsInternal)) {
				buttonIds.Add(command.FullId);
			}
			foreach (string id in buttonIds) {
				Command command = ToolbarManager.InternalInstance.Commands.SingleOrDefault(c => c.FullId == id);
				if (command != null) {
					Button button = new Button(command, this);
					add(button);
				}
			}

			setupConfigureVisibleButtonsButton();

			// move existing buttons according to saved folder contents
			foreach (Button button in new List<Button>(buttons)) {
				string buttonId = button.FullId;
				string folderId = savedFolderSettings.SingleOrDefault(kv => kv.Value.buttons.Contains(buttonId)).Key;
				if (folderId != null) {
					moveButtonToFolder(button, folders[folderId]);
				}
			}
		}

		private void deleteToolbar() {
			ConfirmDialog.confirm("Delete Toolbar", "Delete this toolbar?",
				() => {
					ToolbarManager.InternalInstance.destroyToolbar(this);
				});
		}

		private void createToolbar() {
			ToolbarManager.InternalInstance.addToolbar();
		}
	}
}
