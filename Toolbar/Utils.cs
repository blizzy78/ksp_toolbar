using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar {
	internal static class Utils {
		internal static Vector2 getMousePosition() {
			Vector3 mousePos = Input.mousePosition;
			return new Vector2(mousePos.x, Screen.height - mousePos.y);
		}
	}
}
