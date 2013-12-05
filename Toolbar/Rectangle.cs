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
