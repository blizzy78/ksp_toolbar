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
	internal class Button : IButton, IPopupMenuOption {
		internal const string NAMESPACE_INTERNAL = "__TOOLBAR_INTERNAL";
		
		private static readonly Vector2 UNSIZED = new Vector2(float.NaN, float.NaN);
		private const string TEXTURE_PATH_DROPDOWN = "000_Toolbar/toolbar-dropdown";
		private const int MAX_TEX_WIDTH = 24;
		private const int MAX_TEX_HEIGHT = 24;
		private const int DROPDOWN_TEX_WIDTH = 10;
		private const int DROPDOWN_TEX_HEIGHT = 7;
		private const int PADDING = 4;

		private string text_;
		public string Text {
			set {
				if (!string.Equals(text_, value)) {
					text_ = value;
					if (text_.Trim() == "") {
						text_ = null;
					}

					// clear caches
					content_ = null;
					style_ = null;
					size_ = UNSIZED;
				}
			}
			get {
				return text_;
			}
		}

		private Color textColor_ = Color.white;
		public Color TextColor {
			set {
				if (!value.Equals(textColor_)) {
					textColor_ = value;

					// clear caches
					style_ = null;
				}
			}
			get {
				return textColor_;
			}
		}

		private string texturePath_;
		public string TexturePath {
			set {
				if ((value != null) && value.Contains('\\')) {
					throw new ArgumentException("texture path must use forward slash instead of backslash: " + value);
				}

				if (!string.Equals(texturePath_, value)) {
					texturePath_ = value;

					// clear caches
					texture_ = null;
					content_ = null;
					style_ = null;
					size_ = UNSIZED;
				}
			}
			get {
				return texturePath_;
			}
		}

		private GUIContent content_;
		internal GUIContent Content {
			get {
				if (content_ == null) {
					content_ = IsTextured ? new GUIContent(Texture) : new GUIContent(Text ?? "???");
				}
				return content_;
			}
		}

		private Texture2D texture_;
		private Texture2D Texture {
			get {
				if ((texture_ == null) && (texturePath_ != null)) {
					try {
						texture_ = GameDatabase.Instance.GetTexture(TexturePath, false);
						if (texture_ != null) {
							if ((texture_.width > MAX_TEX_WIDTH) || (texture_.height > MAX_TEX_HEIGHT)) {
								Log.error("button texture exceeds {0}x{1} pixels, ignoring texture: {2}.{3}", MAX_TEX_WIDTH, MAX_TEX_HEIGHT, ns, id);
								texture_ = null;
								texturePath_ = null;
							}
						} else {
							Log.error("button texture not found: {0}", TexturePath);
							texturePath_ = null;
						}
					} catch (Exception e) {
						Log.error(e, "error while loading button texture: {0}", TexturePath);
						texture_ = null;
						texturePath_ = null;
					}
				}
				return texture_;
			}
		}

		public string ToolTip {
			set;
			get;
		}

		private bool visible_ = true;
		public bool Visible {
			set {
				visible_ = value;

				if (!visible_) {
					// we don't need these for now
					texture_ = null;
					content_ = null;
					style_ = null;
				}
			}
			get {
				return visible_;
			}
		}

		public IVisibility Visibility {
			get;
			set;
		}

		private bool userVisible_;
		internal bool UserVisible {
			set {
				userVisible_ = value;

				if (!userVisible_) {
					// we don't need these for now
					texture_ = null;
					content_ = null;
					style_ = null;
				}
			}
			get {
				return userVisible_;
			}
		}

		public bool EffectivelyVisible {
			get {
				if (Visible && (TexturePath != null)) {
					try {
						return (Visibility == null) || Visibility.Visible;
					} catch (Exception e) {
						Log.error(e, "error while calling IButton.Visibility.Visible for button {0}.{1}", ns, id);
						return false;
					}
				} else {
					return false;
				}
			}
		}

		internal bool EffectivelyUserVisible {
			get {
				return EffectivelyVisible && UserVisible;
			}
		}

		private GUIStyle style_;
		internal GUIStyle Style {
			get {
				if (style_ == null) {
					style_ = new GUIStyle(toolbarDropdown ? GUIStyle.none : GUI.skin.button);
					style_.alignment = TextAnchor.MiddleCenter;
					style_.normal.textColor = TextColor;
					style_.onHover.textColor = TextColor;
					style_.hover.textColor = TextColor;
					style_.onActive.textColor = TextColor;
					style_.active.textColor = TextColor;
					style_.onFocused.textColor = TextColor;
					style_.focused.textColor = TextColor;
					if (IsTextured) {
						style_.padding = new RectOffset(0, 0, 0, 0);
					}
				}
				return style_;
			}

			private set {
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

		private Vector2 size_ = UNSIZED;
		internal Vector2 Size {
			get {
				if (size_.Equals(UNSIZED)) {
					if (toolbarDropdown) {
						size_ = new Vector2(DROPDOWN_TEX_WIDTH, DROPDOWN_TEX_HEIGHT);
					} else if (IsTextured) {
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

		public bool Enabled {
			get;
			set;
		}

		public bool Important {
			get;
			set;
		}

		internal bool IsTextured {
			get {
				return TexturePath != null;
			}
		}

		public event ClickHandler OnClick;
		public event MouseEnterHandler OnMouseEnter;
		public event MouseLeaveHandler OnMouseLeave;

		internal event DestroyHandler OnDestroy;

		internal readonly string ns;
		internal readonly string id;

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

		private Toolbar toolbar;
		private bool toolbarDropdown;
		private bool showTooltip;

		internal Button(string ns, string id, Toolbar toolbar = null) {
			checkId(ns, "namespace");
			checkId(id, "ID");

			this.ns = ns;
			this.id = id;
			this.toolbar = toolbar;

			Enabled = true;
			if (ns == NAMESPACE_INTERNAL) {
				UserVisible = true;
			}

			OnMouseEnter += (e) => showTooltip = true;
			OnMouseLeave += (e) => showTooltip = false;

			if (toolbar != null) {
				// reset style when skin changes
				toolbar.onSkinChange += skinChanged;
			}
		}

		internal static Button createToolbarDropdown() {
			Button button = new Button(NAMESPACE_INTERNAL, "dropdown");
			button.toolbarDropdown = true;
			button.TexturePath = TEXTURE_PATH_DROPDOWN;
			return button;
		}

		internal static Button createMenuOption(string text) {
			Button button = new Button("dummy", "dummy");
			button.Text = text;
			return button;
		}

		private void checkId(string id, string label) {
			if (id.Contains('.') || id.Contains(' ') || id.Contains('/') || id.Contains(':') || id.Contains(',') || id.Contains(';')) {
				throw new ArgumentException(label + " contains invalid characters: " + id);
			}
		}

		internal void draw(Rect rect, bool enabled) {
			bool oldEnabled = GUI.enabled;
			GUI.enabled = enabled && Enabled;

			bool clicked = GUI.Button(rect, Content, Style);

			GUI.enabled = oldEnabled;

			if (clicked && (OnClick != null)) {
				OnClick(new ClickEvent(this, Event.current.button));
			}
		}

		internal void drawPlain() {
			GUIStyle style = new GUIStyle();
			style.alignment = TextAnchor.MiddleCenter;
			GUILayout.Label(Content, style, GUILayout.Width(Size.x), GUILayout.Height(Size.y));
		}

		public void drawMenuOption() {
			bool oldEnabled = GUI.enabled;
			GUI.enabled = Enabled;

			bool clicked = GUILayout.Button(Text, MenuOptionStyle, GUILayout.ExpandWidth(true));

			GUI.enabled = oldEnabled;

			if (clicked && (OnClick != null)) {
				try {
					OnClick(new ClickEvent(this, Event.current.button));
				} catch (Exception e) {
					Log.error(e, "error while handling click event: {0}.{1}", ns, id);
				}
			}
		}

		internal void drawToolTip() {
			if (showTooltip && (ToolTip != null) && (ToolTip.Trim().Length > 0)) {
				Vector2 mousePos = Utils.getMousePosition();
				Vector2 size = TooltipStyle.CalcSize(new GUIContent(ToolTip));
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
				GUILayout.Label(ToolTip, TooltipStyle);
				GUILayout.EndArea();
				GUI.depth = oldDepth;
			}
		}

		internal void mouseEnter() {
			if (OnMouseEnter != null) {
				try {
					OnMouseEnter(new MouseEnterEvent(this));
				} catch (Exception e) {
					Log.error(e, "error while handling mouse enter event: {0}.{1}", ns, id);
				}
			}
		}

		internal void mouseLeave() {
			if (OnMouseLeave != null) {
				try {
					OnMouseLeave(new MouseLeaveEvent(this));
				} catch (Exception e) {
					Log.error(e, "error while handling mouse leave event: {0}.{1}", ns, id);
				}
			}
		}

		public void Destroy() {
			if (toolbar != null) {
				toolbar.onSkinChange -= skinChanged;
			}

			if (OnDestroy != null) {
				OnDestroy(new DestroyEvent(this));
			}
		}

		private void skinChanged() {
			style_ = null;
		}
	}
}
