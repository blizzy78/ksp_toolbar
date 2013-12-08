/*
Copyright (c) 2013, Maik Schreiber
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
			return rect.clampToScreen(0);
		}

		internal static Rect clampToScreen(this Rect rect, float overscan) {
			rect.width = Mathf.Clamp(rect.width, 0, Screen.width);
			rect.height = Mathf.Clamp(rect.height, 0, Screen.height);
			rect.x = Mathf.Clamp(rect.x, -overscan, Screen.width - rect.width + overscan);
			rect.y = Mathf.Clamp(rect.y, -overscan, Screen.height - rect.height + overscan);
			return rect;
		}

		internal static Rect shift(this Rect rect, Vector2 shiftBy) {
			return new Rect(rect.x + shiftBy.x, rect.y + shiftBy.y, rect.width, rect.height);
		}
	}
}
