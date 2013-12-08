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
	internal class Draggable {
		private static readonly Vector2 CURSOR_HOTSPOT = new Vector2(10, 10);

		internal bool Dragging {
			get;
			private set;
		}

		private bool enabled_;
		internal bool Enabled {
			set {
				enabled_ = value;
				if (!enabled_) {
					cursorTexture_ = null;
				}
			}
			get {
				return enabled_;
			}
		}

		internal event Action onChange;

		private Texture2D cursorTexture_;
		private Texture2D CursorTexture {
			get {
				if (cursorTexture_ == null) {
					cursorTexture_ = GameDatabase.Instance.GetTexture("000_Toolbar/move-cursor", false);
				}
				return cursorTexture_;
			}
		}

		private Rectangle rect;
		private float clampOverscan;
		private Func<Vector2, bool> handleAreaCheck;
		private bool cursorActive;

		internal Draggable(Rectangle initialPosition, float clampOverscan, Func<Vector2, bool> handleAreaCheck) {
			this.rect = initialPosition;
			this.clampOverscan = clampOverscan;
			this.handleAreaCheck = handleAreaCheck;
		}

		internal void update() {
			if (Enabled) {
				handleDrag();
			}
		}

		private void handleDrag() {
			Vector2 mousePos = Utils.getMousePosition();
			bool inArea = rect.contains(mousePos) && ((handleAreaCheck == null) || handleAreaCheck(mousePos));
			if (inArea && Input.GetMouseButtonDown(0)) {
				Dragging = true;
			}
			if (inArea || Dragging) {
				Cursor.SetCursor(CursorTexture, CURSOR_HOTSPOT, CursorMode.Auto);
				cursorActive = true;
			} else if (cursorActive) {
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
				cursorActive = false;
			}

			if (Dragging) {
				if (Input.GetMouseButton(0)) {
					rect.Rect = new Rect(mousePos.x - rect.width / 2, mousePos.y - rect.height / 2, rect.width, rect.height).clampToScreen(clampOverscan);
				} else {
					Dragging = false;
					Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
					cursorActive = false;
				}
				if (onChange != null) {
					onChange();
				}
			}
		}
	}
}
