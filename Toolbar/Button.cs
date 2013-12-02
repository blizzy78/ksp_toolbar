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
	internal class Button : IButton {
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
					Size = new Vector2(-1, -1);
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
					Style = null;
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
					Size = new Vector2(-1, -1);
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

		internal bool IsTextured {
			get {
				return TexturePath != null;
			}
		}

		private Texture2D texture_;
		private Texture2D Texture {
			get {
				if ((texture_ == null) && (texturePath_ != null)) {
					try {
						texture_ = GameDatabase.Instance.GetTexture(TexturePath, false);
					} catch {
						Debug.LogError("error loading button texture: " + TexturePath);
						TexturePath = null;
					}
				}
				return texture_;
			}
		}

		internal Rect Rect {
			get {
				return new Rect(Position.x, Position.y, Size.x, Size.y);
			}
		}

		public bool Visible {
			get;
			set;
		}

		public IVisibility Visibility {
			get;
			set;
		}

		internal bool EffectivelyVisible {
			get {
				return Visible &&
					((Visibility == null) || Visibility.Visible);
			}
		}

		public bool Enabled {
			get;
			set;
		}

		public event ClickHandler OnClick;

		internal readonly string ns;
		internal readonly string id;
		internal Vector2 Position = new Vector2(-1, -1);
		internal Vector2 Size = new Vector2(-1, -1);
		internal bool PositionLocked = true;
		internal bool Rotated;
		internal GUIStyle Style;

		private ToolbarManager toolbar;

		internal Button(string ns, string id, ToolbarManager toolbar) {
			this.ns = ns;
			this.id = id;
			this.toolbar = toolbar;

			Visible = true;
			Enabled = true;
		}

		internal bool contains(Vector2 pos) {
			return Rotated ? Rect.rotate().Contains(pos.rotate()) : Rect.Contains(pos);
		}

		internal void save(ConfigNode node) {
			node.AddValue("x", Position.x.ToString("F0"));
			node.AddValue("y", Position.y.ToString("F0"));
			if (Rotated) {
				node.AddValue("rotated", Rotated.ToString());
			}
		}

		internal void load(ConfigNode node) {
			float x = float.Parse(node.GetValue("x"));
			float y = float.Parse(node.GetValue("y"));
			Position = new Vector2(x, y);
			if (node.HasValue("rotated")) {
				Rotated = bool.Parse(node.GetValue("rotated"));
			}
		}

		internal void clicked() {
			if (OnClick != null) {
				OnClick(this);
			}
		}
	}
}
