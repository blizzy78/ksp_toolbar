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
	internal class Command : IButton, IComparable<Command> {
		private string text_;
		public string Text {
			set {
				if (!destroyed) {
					if (!string.Equals(text_, value)) {
						text_ = value;
						if ((text_ != null) && (text_ == string.Empty)) {
							text_ = null;
						}

						fireChange();
					}
				}
			}
			get {
				return text_;
			}
		}

		private Color textColor_ = Color.white;
		public Color TextColor {
			set {
				if (!destroyed) {
					if (!value.Equals(textColor_)) {
						textColor_ = value;

						fireChange();
					}
				}
			}
			get {
				return textColor_;
			}
		}

		private string texturePath_;
		public string TexturePath {
			set {
				if (!destroyed) {
					if ((value != null) && value.Contains('\\')) {
						throw new ArgumentException("texture path must use forward slash instead of backslash: " + value);
					}

					if (!string.Equals(texturePath_, value)) {
						texturePath_ = value;

						fireChange();
					}
				}
			}
			get {
				return texturePath_;
			}
		}

		private string toolTip_;
		public string ToolTip {
			set {
				if (!destroyed) {
					if (!string.Equals(toolTip_, value)) {
						toolTip_ = value;
						if ((toolTip_ != null) && (toolTip_ == string.Empty)) {
							toolTip_ = null;
						}

						fireChange();
					}
				}
			}
			get {
				return toolTip_;
			}
		}

		private bool visible_ = true;
		public bool Visible {
			set {
				if (!destroyed) {
					if (visible_ != value) {
						visible_ = value;

						fireChange();
					}
				}
			}
			get {
				return visible_;
			}
		}

		private IVisibility visibility_;
		public IVisibility Visibility {
			set {
				if (!destroyed) {
					if (visibility_ != value) {
						visibility_ = value;

						fireChange();
					}
				}
			}
			get {
				return visibility_;
			}
		}

		public bool EffectivelyVisible {
			get {
				if (!destroyed) {
					if (Visible && (TexturePath != null)) {
						try {
							return (Visibility == null) || Visibility.Visible;
						} catch (Exception e) {
							Log.error(e, "error while calling IButton.Visibility.Visible for button {0}", FullId);
							return false;
						}
					} else {
						return false;
					}
				} else {
					return false;
				}
			}
		}

		private bool enabled_ = true;
		public bool Enabled {
			set {
				if (!destroyed) {
					if (enabled_ != value) {
						enabled_ = value;

						fireChange();
					}
				}
			}
			get {
				return enabled_;
			}
		}

		private bool important_;
		public bool Important {
			set {
				if (!destroyed) {
					if (important_ != value) {
						important_ = value;

						fireChange();
					}
				}
			}
			get {
				return important_;
			}
		}

		internal bool IsTextured {
			get {
				return TexturePath != null;
			}
		}

		public event ClickHandler OnClick;
		public event MouseEnterHandler OnMouseEnter;
		public event MouseLeaveHandler OnMouseLeave;

		internal event Action OnChange;
		internal event Action OnDestroy;

		internal readonly string Namespace;
		internal readonly string Id;
		internal readonly string FullId;
		internal readonly bool IsInternal;

		private bool destroyed;

		internal Command(string @namespace, string id) {
			checkId(@namespace, "namespace");
			checkId(id, "ID");

			this.Namespace = @namespace;
			this.Id = id;
			this.FullId = @namespace + "." + id;
			this.IsInternal = @namespace == ToolbarManager.NAMESPACE_INTERNAL;
		}

		public void Destroy() {
			if (!destroyed) {
				destroyed = true;
				fireDestroy();
			}
		}

		private void checkId(string id, string label) {
			if (id.Contains('.') || id.Contains(' ') || id.Contains('/') || id.Contains(':') || id.Contains(',') || id.Contains(';')) {
				throw new ArgumentException(label + " contains invalid characters: " + id);
			}
		}

		internal void click() {
			if (!destroyed) {
				if (OnClick != null) {
					try {
						OnClick(new ClickEvent(this, Event.current.button));
					} catch (Exception e) {
						Log.error(e, "error while handling click event: {0}", FullId);
					}
				}
			}
		}

		internal void mouseEnter() {
			if (!destroyed) {
				if (OnMouseEnter != null) {
					try {
						OnMouseEnter(new MouseEnterEvent(this));
					} catch (Exception e) {
						Log.error(e, "error while handling mouse enter event: {0}", FullId);
					}
				}
			}
		}

		internal void mouseLeave() {
			if (!destroyed) {
				if (OnMouseLeave != null) {
					try {
						OnMouseLeave(new MouseLeaveEvent(this));
					} catch (Exception e) {
						Log.error(e, "error while handling mouse leave event: {0}", FullId);
					}
				}
			}
		}

		private void fireChange() {
			if (OnChange != null) {
				OnChange();
			}
		}

		private void fireDestroy() {
			if (OnDestroy != null) {
				OnDestroy();
			}
		}

		public int CompareTo(Command other) {
			return StringComparer.CurrentCultureIgnoreCase.Compare(FullId, other.FullId);
		}
	}
}
