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
	internal class VisibleButtonsSelector : AbstractWindow {
		internal event Action<Button> OnButtonSelectionChanged;

		private List<Button> buttons;
		private Vector2 scrollPos;

		internal VisibleButtonsSelector(List<Button> buttons) : base() {
			this.buttons = buttons;

			Rect = new Rect(300, 300, 0, 0);
			Title = "Button Visibility";
			Dialog = true;
		}

		internal override void drawContents() {
			GUILayout.BeginVertical();

				GUILayout.Label("Configure which buttons should be visible in the current game scene.");
				GUILayout.Label("Note: Plugins may still decide to hide buttons from any game scene even if those buttons are active here.");

				GUILayout.Space(5);

				scrollPos = GUILayout.BeginScrollView(scrollPos,
					GUILayout.Width(Mathf.Max(Screen.width / 4, 350)), GUILayout.Height(Mathf.Max(Screen.height / 3, 350)));

				GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
				labelStyle.wordWrap = false;

				string lastNamespace = buttons.First().ns;
				foreach (Button button in buttons) {
					if (button.ns != lastNamespace) {
						Separator.Instance.drawMenuOption();
					}

					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						bool visible = button.UserVisible;
						bool selected = GUILayout.Toggle(visible, (string) null);
						if (selected != visible) {
							button.UserVisible = selected;
							fireButtonSelectionChanged(button);
						}
						button.drawPlain();
						GUILayout.Label(button.Text ?? button.ToolTip, labelStyle);
						GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					lastNamespace = button.ns;
				}

				GUILayout.EndScrollView();

				GUILayout.Space(15);

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Close")) {
						destroy();
					}
				GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}

		private void fireButtonSelectionChanged(Button button) {
			if (OnButtonSelectionChanged != null) {
				OnButtonSelectionChanged(button);
			}
		}
	}
}
