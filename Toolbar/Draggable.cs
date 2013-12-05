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
	internal class Draggable {
		internal bool Dragging {
			get;
			private set;
		}

		internal event Action onChange;

		private bool clampToScreen;
		private Rectangle rect;
		private Func<Vector2, bool> handleAreaCheck;

		internal Draggable(Rectangle initialPosition, bool clampToScreen, Func<Vector2, bool> handleAreaCheck) {
			this.rect = initialPosition;
			this.clampToScreen = clampToScreen;
			this.handleAreaCheck = handleAreaCheck;
		}

		internal void update() {
			handleDrag();
		}

		private void handleDrag() {
			if (Input.GetMouseButtonDown(0) && !Dragging) {
				Vector2 mousePos = Utils.getMousePosition();
				Dragging = rect.contains(mousePos) && ((handleAreaCheck == null) || handleAreaCheck(mousePos));
			}

			if (Dragging) {
				if (Input.GetMouseButton(0)) {
					Vector2 mousePos = Utils.getMousePosition();
					Rect newRect = new Rect(mousePos.x - rect.width / 2, mousePos.y - rect.height / 2, rect.width, rect.height);
					if (clampToScreen) {
						newRect = newRect.clampToScreen();
					}
					rect.Rect = newRect;
				} else {
					Dragging = false;
				}
				if (onChange != null) {
					onChange();
				}
			}
		}
	}
}
