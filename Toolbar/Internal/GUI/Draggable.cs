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
	internal class Draggable : ICursorGrabber {
		private const string CURSOR_TEXTURE = "000_Toolbar/move-cursor";
		private const float CURSOR_HOT_SPOT_X = 10;
		private const float CURSOR_HOT_SPOT_Y = 10;

		internal bool Dragging {
			get;
			private set;
		}

		private bool enabled_;
		internal bool Enabled {
			set {
				if (value != enabled_) {
					enabled_ = value;

					if (!enabled_) {
						if (Dragging) {
							stopDragging();
							fireDrag();
						}

						cursorTexture_ = null;
					}
				}
			}
			get {
				return enabled_;
			}
		}

		internal event DragHandler OnDrag;

		private Texture2D cursorTexture_;
		private Texture2D CursorTexture {
			get {
				if (cursorTexture_ == null) {
					cursorTexture_ = GameDatabase.Instance.GetTexture(cursorTexturePath, false);
				}
				return cursorTexture_;
			}
		}

		protected Rectangle rect;

		private float clampOverscan;
		private Func<Vector2, bool> handleAreaCheck;
		private string cursorTexturePath;
		private Vector2 cursorHotSpot;
		private Rect startRect;
		private Vector2 startMousePos;

		internal Draggable(Rectangle initialPosition, float clampOverscan, Func<Vector2, bool> handleAreaCheck,
			string cursorTexturePath = CURSOR_TEXTURE, float cursorHotSpotX = CURSOR_HOT_SPOT_X, float cursorHotSpotY = CURSOR_HOT_SPOT_Y) {

			this.rect = initialPosition;
			this.clampOverscan = clampOverscan;
			this.handleAreaCheck = handleAreaCheck;
			this.cursorTexturePath = cursorTexturePath;
			this.cursorHotSpot = new Vector2(cursorHotSpotX, cursorHotSpotY);
		}

		internal void update() {
			if (Enabled) {
				handleDrag();
			}
		}

		private void handleDrag() {
			Vector2 mousePos = Utils.getMousePosition();
			bool inArea = isInArea(mousePos) && ((handleAreaCheck == null) || handleAreaCheck(mousePos));
			if (inArea && Input.GetMouseButtonDown(0)) {
				startDragging(mousePos);
				fireDrag();
			}

			if (Dragging) {
				if (Input.GetMouseButton(0)) {
					rect.Rect = getNewRect(mousePos, startRect, startMousePos).clampToScreen(clampOverscan);
				} else {
					stopDragging();
				}
				fireDrag();
			}
		}

		protected virtual bool isInArea(Vector2 mousePos) {
			return rect.contains(mousePos);
		}

		protected virtual Rect getNewRect(Vector2 mousePos, Rect startRect, Vector2 startMousePos) {
			return new Rect(mousePos.x - rect.width / 2, mousePos.y - rect.height / 2, rect.width, rect.height);
		}

		public bool grabCursor() {
			if (Enabled) {
				Vector2 mousePos = Utils.getMousePosition();
				bool inArea = isInArea(mousePos) && ((handleAreaCheck == null) || handleAreaCheck(mousePos));
				bool setCursor = inArea || Dragging;
				if (setCursor) {
					Cursor.SetCursor(CursorTexture, cursorHotSpot, CursorMode.ForceSoftware);
				}
				return setCursor;
			} else {
				return false;
			}
		}

		private void startDragging(Vector2 mousePos) {
			Dragging = true;
			startRect = rect.Rect;
			startMousePos = mousePos;
		}

		private void stopDragging() {
			Dragging = false;
		}

		private void fireDrag() {
			if (OnDrag != null) {
				OnDrag(new DragEvent(this));
			}
		}
	}
}
