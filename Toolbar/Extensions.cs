/*
Toolbar - Common API for GUI Buttons for Kerbal Space Program.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar {
	internal static class Extensions {
		internal static Rect rotate(this Rect rect) {
			float newX = -rect.y;
			float newY = rect.x;
			rect.x = newX;
			rect.y = newY;
			return rect;
		}

		internal static Vector2 rotate(this Vector2 pos) {
			float newX = -pos.y;
			float newY = pos.x;
			pos.x = newX;
			pos.y = newY;
			return pos;
		}

		internal static Rect clampToScreen(this Rect rect) {
			rect.width = Mathf.Clamp(rect.width, 0, Screen.width);
			rect.height = Mathf.Clamp(rect.height, 0, Screen.height);
			rect.x = Mathf.Clamp(rect.x, 0, Screen.width - rect.width);
			rect.y = Mathf.Clamp(rect.y, 0, Screen.height - rect.height);
			return rect;
		}
	}
}
