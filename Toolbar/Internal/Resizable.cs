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
	internal class Resizable {
		private const float HANDLE_SIZE = 8;

		internal bool Resizing {
			get;
			private set;
		}

		internal Rect HandleRect {
			get {
				return new Rect(rect.x + rect.width - HANDLE_SIZE, rect.y + rect.height - HANDLE_SIZE, HANDLE_SIZE, HANDLE_SIZE);
			}
		}

		internal event Action onChange;

		private Rectangle rect;
		private bool clampToScreen;
		private Func<Vector2, bool> handleAreaCheck;
		private Rect resizingStartRect;
		private Vector2 resizingStartMousePos;

		internal Resizable(Rectangle initialPosition, bool clampToScreen, Func<Vector2, bool> handleAreaCheck) {
			this.rect = initialPosition;
			this.clampToScreen = clampToScreen;
			this.handleAreaCheck = handleAreaCheck;
		}

		internal void update() {
			handleResize();
		}

		private void handleResize() {
			if (Input.GetMouseButtonDown(0) && !Resizing) {
				Vector2 mousePos = Utils.getMousePosition();
				Resizing = HandleRect.Contains(mousePos) && ((handleAreaCheck == null) || handleAreaCheck(mousePos));
				resizingStartRect = rect.Rect;
				resizingStartMousePos = mousePos;
			}

			if (Resizing) {
				if (Input.GetMouseButton(0)) {
					Vector2 mousePos = Utils.getMousePosition();
					Rect newRect = new Rect(rect.x, rect.y,
						resizingStartRect.width + mousePos.x - resizingStartMousePos.x,
						resizingStartRect.height + mousePos.y - resizingStartMousePos.y);
					if (clampToScreen) {
						newRect = newRect.clampToScreen();
					}
					rect.Rect = newRect;
				} else {
					Resizing = false;
				}
				if (onChange != null) {
					onChange();
				}
			}
		}
	}
}
