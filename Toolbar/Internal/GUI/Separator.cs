/*
Copyright (c) 2013-2016, Maik Schreiber
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar {
	internal class Separator : IPopupMenuOption {
		internal static readonly Separator Instance = new Separator();

#pragma warning disable 67
		public event ClickHandler OnClick;
#pragma warning restore 67

		private GUIStyle style_;
		private GUIStyle Style {
			get {
				if (style_ == null) {
					Texture2D bgTex = new Texture2D(1, 1);
					bgTex.SetPixel(0, 0, new Color(0.4f, 0.4f, 0.4f));
					bgTex.Apply();

					style_ = new GUIStyle(GUI.skin.label);
					style_.normal.background = bgTex;
					style_.onNormal.background = bgTex;
					style_.margin = new RectOffset(0, 0, 2, 2);
					style_.padding = new RectOffset(0, 0, 0, 0);
				}
				return style_;
			}
		}

		private Separator() {
		}

		public void drawMenuOption() {
			GUILayout.Label(string.Empty, Style, GUILayout.Height(1), GUILayout.MinHeight(1), GUILayout.MaxHeight(1), GUILayout.ExpandWidth(true));
		}
	}
}
