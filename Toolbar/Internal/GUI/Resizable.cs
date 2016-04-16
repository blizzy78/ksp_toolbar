/*
Copyright (c) 2013-2016, Maik Schreiber
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar {
	internal class Resizable : Draggable {
		private const string CURSOR_TEXTURE = "000_Toolbar/resize-cursor";
		private const float CURSOR_HOT_SPOT_X = 7;
		private const float CURSOR_HOT_SPOT_Y = 7;
		private const float HANDLE_SIZE = 10;

		internal Rect HandleRect {
			get {
				return new Rect(rect.x + rect.width - HANDLE_SIZE, rect.y + rect.height - HANDLE_SIZE, HANDLE_SIZE, HANDLE_SIZE);
			}
		}
		
		internal Resizable(Rectangle initialPosition, float clampOverscan, Func<Vector2, bool> handleAreaCheck)
			: base(initialPosition, clampOverscan, handleAreaCheck, CURSOR_TEXTURE, CURSOR_HOT_SPOT_X, CURSOR_HOT_SPOT_Y) {
		}

		protected override bool isInArea(Vector2 mousePos) {
			return HandleRect.Contains(mousePos);
		}

		protected override Rect getNewRect(Vector2 mousePos, Rect startRect, Vector2 startMousePos) {
			return new Rect(rect.x, rect.y,
				startRect.width + mousePos.x - startMousePos.x,
				startRect.height + mousePos.y - startMousePos.y);
		}
	}
}
