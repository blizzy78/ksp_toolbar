/*
Copyright (c) 2013-2014, Maik Schreiber
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar {
	internal static class Extensions {
		internal static Rect clampToScreen(this Rect rect) {
			return rect.clampToScreen(0);
		}

		internal static Rect clampToScreen(this Rect rect, float overscan) {
			return rect.clampToScreen(new Vector2(overscan, overscan));
		}

		internal static Rect clampToScreen(this Rect rect, Vector2 overscan) {
			rect.width = Mathf.Clamp(rect.width, 0, Screen.width);
			rect.height = Mathf.Clamp(rect.height, 0, Screen.height);
			rect.x = Mathf.Clamp(rect.x, -overscan.x, Screen.width - rect.width + overscan.x);
			rect.y = Mathf.Clamp(rect.y, -overscan.y, Screen.height - rect.height + overscan.y);
			return rect;
		}

		internal static Vector2 clampToScreen(this Vector2 pos) {
			pos.x = Mathf.Clamp(pos.x, 0, Screen.width - 1);
			pos.y = Mathf.Clamp(pos.y, 0, Screen.height - 1);
			return pos;
		}

		internal static Rect shift(this Rect rect, Vector2 shiftBy) {
			return new Rect(rect.x + shiftBy.x, rect.y + shiftBy.y, rect.width, rect.height);
		}

		internal static ConfigNode getOrCreateNode(this ConfigNode configNode, string nodeName) {
			return configNode.HasNode(nodeName) ? configNode.GetNode(nodeName) : configNode.AddNode(nodeName);
		}

		internal static void overwrite(this ConfigNode configNode, string name, object value) {
			if (configNode.HasValue(name)) {
				configNode.RemoveValue(name);
			}
			configNode.AddValue(name, value);
		}

		internal static ConfigNode overwriteNode(this ConfigNode configNode, string nodeName) {
			if (configNode.HasNode(nodeName)) {
				configNode.RemoveNode(nodeName);
			}
			return configNode.AddNode(nodeName);
		}

		internal static T get<T>(this ConfigNode configNode, string name, T defaultValue) {
			if (configNode.HasValue(name)) {
				Type type = typeof(T);
				TypeConverter converter = TypeDescriptor.GetConverter(type);
				string value = configNode.GetValue(name);
				return (T) converter.ConvertFromInvariantString(value);
			} else {
				return defaultValue;
			}
		}

		internal static long getSeconds(this DateTime date) {
			return date.Ticks / 10000;
		}

		internal static bool intersectsImportantGUI(this Rect rect) {
			return rect.intersectsAltimeter() || rect.intersectsNavBall();
		}

		private static bool intersectsAltimeter(this Rect rect) {
			Rect altimeterRect = new Rect((Screen.width - 245) / 2, 0, 245, 66);
			return rect.intersects(altimeterRect);
		}

		private static bool intersectsNavBall(this Rect rect) {
			Rect navBallUpperRect = new Rect((Screen.width - 175) / 2, Screen.height - 151 - 38, 175, 38);
			Rect navBallLowerRect = new Rect((Screen.width - 215) / 2, Screen.height - 151, 215, 151);
			return rect.intersects(navBallUpperRect) || rect.intersects(navBallLowerRect);
		}

		private static bool intersects(this Rect rect, Rect r) {
			return (r.x <= (rect.x + rect.width - 1)) && (r.y <= (rect.y + rect.height - 1)) &&
				((r.x + r.width - 1) >= rect.x) && ((r.y + r.height - 1) >= rect.y);
		}
	}
}
