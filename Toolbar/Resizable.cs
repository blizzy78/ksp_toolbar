/*
Toolbar - Common API for GUI toolbars for Kerbal Space Program.
Copyright (C) 2013 Maik Schreiber

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
