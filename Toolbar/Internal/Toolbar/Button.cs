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
	internal class Button : IPopupMenuOption {
		private static readonly Vector2 UNSIZED = new Vector2(float.NaN, float.NaN);
		private const string TEXTURE_PATH_DROPDOWN = "000_Toolbar/toolbar-dropdown";
		private const int MAX_TEX_WIDTH = 24;
		private const int MAX_TEX_HEIGHT = 24;
		private const int DROPDOWN_TEX_WIDTH = 10;
		private const int DROPDOWN_TEX_HEIGHT = 7;
		private const int PADDING = 4;

		internal string Namespace {
			get {
				return command.Namespace;
			}
		}

		internal string FullId {
			get {
				return command.FullId;
			}
		}

		internal bool IsInternal {
			get {
				return command.IsInternal;
			}
		}

		private Vector2 size_ = UNSIZED;
		internal Vector2 Size {
			get {
				checkDestroyed();

				if (size_.Equals(UNSIZED)) {
					if (toolbarDropdown) {
						size_ = new Vector2(DROPDOWN_TEX_WIDTH, DROPDOWN_TEX_HEIGHT);
					} else if (command.IsTextured) {
						size_ = new Vector2(MAX_TEX_WIDTH + PADDING * 2, MAX_TEX_HEIGHT + PADDING * 2);
					} else {
						size_ = Style.CalcSize(Content);
						size_.x += Style.padding.left + Style.padding.right;
						size_.y += Style.padding.top + Style.padding.bottom;
					}
				}
				return size_;
			}
		}

		private GUIContent content_;
		private GUIContent Content {
			get {
				if (content_ == null) {
					content_ = command.IsTextured ? new GUIContent(Texture) : new GUIContent(command.Text ?? "???");
				}
				return content_;
			}
		}

		private Texture2D texture_;
		private Texture2D Texture {
			get {
				if ((texture_ == null) && (command.TexturePath != null)) {
					try {
						texture_ = GameDatabase.Instance.GetTexture(command.TexturePath, false);
						if (texture_ != null) {
							if ((texture_.width > MAX_TEX_WIDTH) || (texture_.height > MAX_TEX_HEIGHT)) {
								Log.error("button texture exceeds {0}x{1} pixels, ignoring texture: {2}", MAX_TEX_WIDTH, MAX_TEX_HEIGHT, command.FullId);
								texture_ = null;
								command.TexturePath = null;
							}
						} else {
							Log.error("button texture not found: {0}", command.TexturePath);
							command.TexturePath = null;
						}
					} catch (Exception e) {
						Log.error(e, "error while loading button texture: {0}", command.TexturePath);
						texture_ = null;
						command.TexturePath = null;
					}
				}
				return texture_;
			}
		}

		private GUIStyle style_;
		private GUIStyle Style {
			get {
				if (style_ == null) {
					style_ = new GUIStyle(toolbarDropdown ? GUIStyle.none : GUI.skin.button);
					style_.alignment = TextAnchor.MiddleCenter;
					style_.normal.textColor = command.TextColor;
					style_.onHover.textColor = command.TextColor;
					style_.hover.textColor = command.TextColor;
					style_.onActive.textColor = command.TextColor;
					style_.active.textColor = command.TextColor;
					style_.onFocused.textColor = command.TextColor;
					style_.focused.textColor = command.TextColor;
					if (command.IsTextured) {
						style_.padding = new RectOffset(0, 0, 0, 0);
					}
				}
				return style_;
			}

			set {
				style_ = value;
			}
		}

		private GUIStyle tooltipStyle_;
		private GUIStyle TooltipStyle {
			get {
				if (tooltipStyle_ == null) {
					tooltipStyle_ = new GUIStyle(GUI.skin.box);
					tooltipStyle_.wordWrap = false;
				}
				return tooltipStyle_;
			}
		}

		private GUIStyle menuOptionStyle_;
		private GUIStyle MenuOptionStyle {
			get {
				if (menuOptionStyle_ == null) {
					Texture2D orangeBgTex = new Texture2D(1, 1);
					orangeBgTex.SetPixel(0, 0, XKCDColors.DarkOrange);
					orangeBgTex.Apply();

					menuOptionStyle_ = new GUIStyle(GUI.skin.label);
					menuOptionStyle_.hover.background = orangeBgTex;
					menuOptionStyle_.hover.textColor = Color.white;
					menuOptionStyle_.onHover.background = orangeBgTex;
					menuOptionStyle_.onHover.textColor = Color.white;
					menuOptionStyle_.wordWrap = false;
					menuOptionStyle_.margin = new RectOffset(0, 0, 1, 1);
					menuOptionStyle_.padding = new RectOffset(8, 8, 3, 3);
				}
				return menuOptionStyle_;
			}
		}

		public event ClickHandler OnClick {
			add {
				checkDestroyed();

				command.OnClick += value;
			}
			remove {
				checkDestroyed();

				command.OnClick -= value;
			}
		}

		internal event Action OnMouseEnter;
		internal event Action OnMouseLeave;
		internal event DestroyHandler OnDestroy;

		internal readonly Command command;

		internal bool destroyed;

		private Toolbar toolbar;
		private bool toolbarDropdown;
		private bool showTooltip;

		internal Button(Command command, Toolbar toolbar = null) {
			this.command = command;
			this.toolbar = toolbar;

			OnMouseEnter += () => {
				showTooltip = true;
				command.mouseEnter();
			};
			OnMouseLeave += () => {
				showTooltip = false;
				command.mouseLeave();
			};

			command.OnChange += () => clearCaches();
			command.OnDestroy += () => Destroy();

			if (toolbar != null) {
				toolbar.OnSkinChange += clearCaches;
			}
		}

		private void clearCaches() {
			texture_ = null;
			content_ = null;
			style_ = null;
			size_ = UNSIZED;
		}

		internal static Button createToolbarDropdown() {
			Command dropdownCommand = new Command(ToolbarManager.NAMESPACE_INTERNAL, "dropdown");
			dropdownCommand.TexturePath = TEXTURE_PATH_DROPDOWN;
			Button button = new Button(dropdownCommand);
			button.toolbarDropdown = true;
			return button;
		}

		internal static Button createMenuOption(string text) {
			Command menuOptionCommand = new Command(ToolbarManager.NAMESPACE_INTERNAL, "menuOption");
			menuOptionCommand.Text = text;
			Button button = new Button(menuOptionCommand);
			return button;
		}

		internal void draw(Rect rect, bool enabled) {
			checkDestroyed();

			bool oldEnabled = GUI.enabled;
			GUI.enabled = enabled && command.Enabled;

			bool clicked = GUI.Button(rect, Content, Style);

			GUI.enabled = oldEnabled;

			if (clicked) {
				click();
			}
		}

		internal void drawPlain() {
			checkDestroyed();

			GUIStyle style = new GUIStyle();
			style.alignment = TextAnchor.MiddleCenter;
			GUILayout.Label(Content, style, GUILayout.Width(Size.x), GUILayout.Height(Size.y));
		}

		public void drawMenuOption() {
			checkDestroyed();

			bool oldEnabled = GUI.enabled;
			GUI.enabled = command.Enabled;

			bool clicked = GUILayout.Button(command.Text, MenuOptionStyle, GUILayout.ExpandWidth(true));

			GUI.enabled = oldEnabled;

			if (clicked) {
				click();
			}
		}

		internal void drawToolTip() {
			checkDestroyed();

			if (showTooltip && (command.ToolTip != null) && (command.ToolTip.Trim().Length > 0)) {
				Vector2 mousePos = Utils.getMousePosition();
				Vector2 size = TooltipStyle.CalcSize(new GUIContent(command.ToolTip));
				Rect rect = new Rect(mousePos.x, mousePos.y + 20, size.x, size.y);
				float origY = rect.y;
				rect = rect.clampToScreen();
				// clamping moved the tooltip up -> reposition above mouse cursor
				if (rect.y < origY) {
					rect.y = mousePos.y - size.y - 5;
					rect = rect.clampToScreen();
				}

				int oldDepth = GUI.depth;
				GUI.depth = -1000;
				GUILayout.BeginArea(rect);
				GUILayout.Label(command.ToolTip, TooltipStyle);
				GUILayout.EndArea();
				GUI.depth = oldDepth;
			}
		}

		private void click() {
			command.click();
		}

		internal void mouseEnter() {
			checkDestroyed();

			if (OnMouseEnter != null) {
				OnMouseEnter();
			}
		}

		internal void mouseLeave() {
			checkDestroyed();

			if (OnMouseLeave != null) {
				OnMouseLeave();
			}
		}

		public void Destroy() {
			if (!destroyed) {
				destroyed = true;
				if (toolbar != null) {
					toolbar.OnSkinChange -= clearCaches;
				}
				fireDestroy();
			}
		}

		private void fireDestroy() {
			if (OnDestroy != null) {
				OnDestroy(new DestroyEvent(this));
			}
		}

		private void checkDestroyed() {
			if (destroyed) {
				throw new NotSupportedException("button is destroyed: " + FullId);
			}
		}
	}
}
