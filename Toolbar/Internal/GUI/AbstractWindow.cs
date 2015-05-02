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
	internal abstract class AbstractWindow {
		internal event Action OnDestroy;

		internal Rect Rect = new Rect(0, 0, 0, 0);
		internal bool Dialog;
		internal bool Modal;
		internal bool AutoClampToScreen = true;

		protected string Title;
		protected GUIStyle GUIStyle;
		protected GUILayoutOption[] GUILayoutOptions = {};
		protected bool Draggable = true;

		private readonly int id = new System.Random().Next(int.MaxValue);
		private EditorLock editorLock;
		private bool useWindowList;

		internal AbstractWindow(bool useWindowList = true) {
			this.useWindowList = useWindowList;

			if (useWindowList) {
				WindowList.Instance.add(this);
			}

			editorLock = new EditorLock("Toolbar_window_" + id);
		}

		internal void destroy() {
			if (useWindowList) {
				WindowList.Instance.remove(this);
			}

			editorLock.draw(false);

			if (OnDestroy != null) {
				OnDestroy();
			}
		}

		internal virtual void draw() {
			if (GUIStyle == null) {
				GUIStyle = GUI.skin.window;
			}

			Rect = GUILayout.Window(id, AutoClampToScreen ? Rect.clampToScreen() : Rect, windowId => drawContentsInternal(), Title, GUIStyle, GUILayoutOptions);

			editorLock.draw(Modal || Rect.Contains(Utils.getMousePosition()));
		}

		internal bool contains(Vector2 pos) {
			return Rect.Contains(pos);
		}

		private void drawContentsInternal() {
			drawContents();

			if (Draggable) {
				GUI.DragWindow();
			}
		}

		internal abstract void drawContents();
	}
}
