using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar {
	internal class Rectangle {
		private Rect rect_;
		internal Rect Rect {
			get {
				return rect_;
			}

			set {
				rect_ = value;
			}
		}

		internal float x {
			get {
				return Rect.x;
			}
			set {
				rect_.x = value;
			}
		}

		internal float y {
			get {
				return Rect.y;
			}
			set {
				rect_.y = value;
			}
		}

		internal float width {
			get {
				return Rect.width;
			}

			set {
				rect_.width = value;
			}
		}

		internal float height {
			get {
				return Rect.height;
			}

			set {
				rect_.height = value;
			}
		}

		internal Rectangle(Rect rect) {
			this.Rect = rect;
		}

		internal bool contains(Vector2 pos) {
			return Rect.Contains(pos);
		}

		internal void clampToScreen() {
			rect_ = rect_.clampToScreen();
		}
	}
}
