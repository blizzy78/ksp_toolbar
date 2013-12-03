using System;
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
