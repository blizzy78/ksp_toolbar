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
	internal class PopupMenu {
		private readonly int id = new System.Random().Next(int.MaxValue);

		private Texture2D orangeBgTex;
		private GUIStyle optionStyle;
		private bool stylesInitialized;

		public static PopupMenu operator +(PopupMenu menu, Button option) {
			menu.options.Add(option);
			return menu;
		}

		internal IEnumerable<Button> Options {
			get {
				return options;
			}
		}

		private Rect rect;
		private List<Button> options = new List<Button>();

		internal PopupMenu(Vector2 position) {
			rect = new Rect(position.x, position.y, 0, 0);
		}

		internal void draw() {
			initStyles();

			rect = rect.clampToScreen();
			rect = GUILayout.Window(id, rect, drawWindow, (string) null, GUI.skin.box, GUILayout.ExpandWidth(true));
		}

		private void drawWindow(int id) {
			GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			foreach (Button option in options) {
				option.drawMenuOption(optionStyle);
			}
			GUILayout.EndVertical();
		}

		private void initStyles() {
			if (!stylesInitialized) {
				orangeBgTex = new Texture2D(1, 1);
				orangeBgTex.SetPixel(0, 0, XKCDColors.DarkOrange);
				orangeBgTex.Apply();

				optionStyle = new GUIStyle(GUI.skin.label);
				optionStyle.hover.background = orangeBgTex;
				optionStyle.hover.textColor = Color.white;
				optionStyle.onHover.background = orangeBgTex;
				optionStyle.onHover.textColor = Color.white;
				optionStyle.wordWrap = false;
				optionStyle.padding.left += 8;
				optionStyle.padding.right += 8;

				stylesInitialized = true;
			}
		}

		internal bool contains(Vector2 pos) {
			return rect.Contains(pos);
		}
	}
}
