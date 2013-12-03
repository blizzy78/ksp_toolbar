using System;
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
