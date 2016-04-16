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
	internal class FolderSettingsDialog : AbstractWindow {
		internal event Action OnOkClicked;
		internal event Action OnCancelClicked;

		internal string TexturePath {
			get {
				return iconPickerCommand.TexturePath;
			}
		}

		internal string ToolTip;

		private Command iconPickerCommand;
		private Button iconPickerButton;

		internal FolderSettingsDialog(string texturePath, string toolTip) : base() {
			this.ToolTip = toolTip;

			Rect = new Rect(300, 300, Mathf.Max(Screen.width / 4, 350), 0);
			Title = "Folder Settings";
			Dialog = true;
			Modal = true;

			iconPickerCommand = new Command(ToolbarManager.NAMESPACE_INTERNAL, "openIconPicker");
			iconPickerCommand.TexturePath = texturePath;
			iconPickerCommand.OnClick += (e) => {
				openIconPicker();
			};

			iconPickerButton = new Button(iconPickerCommand);
		}

		private void openIconPicker() {
			IconPickerDialog dlg = new IconPickerDialog("Select Icon", new Vector2(Button.MAX_TEX_WIDTH, Button.MAX_TEX_HEIGHT),
				(texturePath) => {
					iconPickerCommand.TexturePath = texturePath;
				});
			dlg.OnDestroy += () => {
				iconPickerCommand.Enabled = true;
			};
			iconPickerCommand.Enabled = false;
		}

		internal override void drawContents() {
			GUILayout.BeginVertical();

				GUI.enabled = iconPickerCommand.Enabled;

				GUILayout.BeginHorizontal();
					GUILayout.Label("Button icon:", GUILayout.ExpandWidth(false));
					iconPickerButton.drawButton();
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.Label("Button tooltip text:", GUILayout.ExpandWidth(false));
					ToolTip = GUILayout.TextField(ToolTip, GUILayout.ExpandWidth(true));
				GUILayout.EndHorizontal();

				GUILayout.Space(15);

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("OK")) {
						fireButtonClicked(OnOkClicked);
					}
					if (GUILayout.Button("Cancel")) {
						fireButtonClicked(OnCancelClicked);
					}
				GUILayout.EndHorizontal();

				GUI.enabled = true;

			GUILayout.EndVertical();
		}

		private void fireButtonClicked(Action evt) {
			destroy();
			if (evt != null) {
				evt();
			}
		}
	}
}
