/*
Copyright (c) 2013-2015, Maik Schreiber
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
	internal class TextureMenuOption : IPopupMenuOption {

		public event ClickHandler OnClick;

		private Texture2D tex;
		private Vector2 topBottomMargins;

		private GUIStyle style_;
		private GUIStyle Style {
			get {
				if (style_ == null) {
					style_ = new GUIStyle(GUI.skin.label);
					style_.normal.background = tex;
					style_.onNormal.background = tex;
					style_.margin.top = (int) topBottomMargins.x;
					style_.margin.bottom = (int) topBottomMargins.y;
					style_.padding = new RectOffset(0, 0, 0, 0);
				}
				return style_;
			}
		}

		internal TextureMenuOption(Texture2D tex, Vector2 topBottomMargins) {
			this.tex = tex;
			this.topBottomMargins = topBottomMargins;
		}

		public void drawMenuOption() {
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(string.Empty, Style,
					GUILayout.Width(tex.width), GUILayout.MinWidth(tex.width), GUILayout.MaxWidth(tex.width),
					GUILayout.Height(tex.height), GUILayout.MinHeight(tex.height), GUILayout.MaxHeight(tex.height),
					GUILayout.ExpandWidth(true))) {

					fireClick();
				}
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		private void fireClick() {
			if (OnClick != null) {
				OnClick(new ClickEvent(null, Event.current.button));
			}
		}
	}
}
