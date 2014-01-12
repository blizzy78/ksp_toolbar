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
	// TODO: this class does almost the same as Draggable, it should subclass that
	internal class Resizable {
		private const float HANDLE_SIZE = 10;
		private static readonly Vector2 CURSOR_HOTSPOT = new Vector2(7, 7);

		internal bool Resizing {
			get;
			private set;
		}

		internal Rect HandleRect {
			get {
				return new Rect(rect.x + rect.width - HANDLE_SIZE, rect.y + rect.height - HANDLE_SIZE, HANDLE_SIZE, HANDLE_SIZE);
			}
		}

		private bool enabled_;
		internal bool Enabled {
			set {
				if (value != enabled_) {
					enabled_ = value;

					if (!enabled_) {
						if (Resizing) {
							stopResizing();
							fireResize();
						}

						cursorTexture_ = null;
					}
				}
			}
			get {
				return enabled_;
			}
		}

		internal event Action OnResize;

		private Texture2D cursorTexture_;
		private Texture2D CursorTexture {
			get {
				if (cursorTexture_ == null) {
					cursorTexture_ = GameDatabase.Instance.GetTexture("000_Toolbar/resize-cursor", false);
				}
				return cursorTexture_;
			}
		}

		private Rectangle rect;
		private float clampOverscan;
		private Func<Vector2, bool> handleAreaCheck;
		private Rect resizingStartRect;
		private Vector2 resizingStartMousePos;
		private bool cursorActive;

		internal Resizable(Rectangle initialPosition, float clampOverscan, Func<Vector2, bool> handleAreaCheck) {
			this.rect = initialPosition;
			this.clampOverscan = clampOverscan;
			this.handleAreaCheck = handleAreaCheck;
		}

		internal void update() {
			if (Enabled) {
				handleResize();
			}
		}

		private void handleResize() {
			Vector2 mousePos = Utils.getMousePosition();
			bool inArea = HandleRect.Contains(mousePos) && ((handleAreaCheck == null) || handleAreaCheck(mousePos));
			if (inArea && Input.GetMouseButtonDown(0)) {
				Resizing = true;
				resizingStartRect = rect.Rect;
				resizingStartMousePos = mousePos;
				fireResize();
			}
			if (inArea || Resizing) {
				Cursor.SetCursor(CursorTexture, CURSOR_HOTSPOT, CursorMode.ForceSoftware);
				cursorActive = true;
			} else if (cursorActive) {
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
				cursorActive = false;
			}

			if (Resizing) {
				if (Input.GetMouseButton(0)) {
					rect.Rect = new Rect(rect.x, rect.y,
						resizingStartRect.width + mousePos.x - resizingStartMousePos.x,
						resizingStartRect.height + mousePos.y - resizingStartMousePos.y).clampToScreen(clampOverscan);
				} else {
					stopResizing();
				}
				fireResize();
			}
		}

		private void stopResizing() {
			Resizing = false;
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			cursorActive = false;
		}

		private void fireResize() {
			if (OnResize != null) {
				OnResize();
			}
		}
	}
}
