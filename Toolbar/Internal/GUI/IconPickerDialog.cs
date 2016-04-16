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
	internal class IconPickerDialog : AbstractWindow {
		private const int BUTTON_TRIM = 4;
		private const int BUTTONS_PER_ROW = 8;
		// hardcoded in Button.Style
		private const int BUTTON_MARGIN = 1;
		private const int ROWS = 8;

		private Action<string> onButtonSelected;
		private List<Button> buttons = new List<Button>();
		private Vector2 scrollPos;

		internal IconPickerDialog(string title, Vector2 maxSize, Action<string> onButtonSelected) : base() {
			Rect = new Rect(400, 400, (maxSize.x + BUTTON_TRIM * 2 + BUTTON_MARGIN) * BUTTONS_PER_ROW + 40, (maxSize.x + BUTTON_TRIM * 2 + BUTTON_MARGIN) * ROWS);
			Title = title;
			Dialog = true;
			Modal = true;

			this.onButtonSelected = onButtonSelected;

			Command folderCommand = new Command(ToolbarManager.NAMESPACE_INTERNAL, "iconPicker_folder");
			folderCommand.TexturePath = "000_Toolbar/folder";
			folderCommand.OnClick += (e) => {
				buttonSelected("000_Toolbar/folder");
			};
			buttons.Add(new Button(folderCommand));

			foreach (GameDatabase.TextureInfo info in GameDatabase.Instance.databaseTexture) {
				if (!info.isNormalMap && !info.name.StartsWith("000_Toolbar/")) {
					Texture2D tex = info.texture;
					if ((tex.width <= maxSize.x) && (tex.height <= maxSize.y)) {
						Command command = new Command(ToolbarManager.NAMESPACE_INTERNAL, "iconPicker_" + new System.Random().Next(int.MaxValue));
						command.TexturePath = info.name;
						command.OnClick += (e) => {
							buttonSelected(info.name);
						};
						buttons.Add(new Button(command));
					}
				}
			}
		}

		internal override void drawContents() {
			GUILayout.BeginVertical();

				scrollPos = GUILayout.BeginScrollView(scrollPos);
					int numInRow = 0;
					foreach (Button button in buttons) {
						if (numInRow == 0) {
							GUILayout.BeginHorizontal();
						}
						button.drawButton();
						numInRow++;
						if (numInRow >= BUTTONS_PER_ROW) {
							GUILayout.FlexibleSpace();
							GUILayout.EndHorizontal();
							numInRow = 0;
						}
					}
					if (numInRow > 0) {
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
					}
				GUILayout.EndScrollView();

				GUILayout.Space(15);

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Cancel")) {
						destroy();
					}
				GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}

		private void buttonSelected(string texturePath) {
			destroy();
			onButtonSelected(texturePath);
		}
	}
}
