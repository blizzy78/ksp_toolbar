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
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar {
	internal class DropMarker {
		internal const float MARKER_LINE_WIDTH = 2;

		private static readonly Rect NO_POSITION = new Rect(float.MinValue, float.MinValue, float.MinValue, float.MinValue);

		internal Rect Rect = NO_POSITION;
		internal bool Visible = true;

		private Texture2D orangeBgTex;
		private GUIStyle style;
		private bool styleInitialized;

		internal void draw() {
			if (Visible && !Rect.Equals(NO_POSITION)) {
				initStyle();

				GUI.Label(Rect, (string) null, style);
			}
		}

		private void initStyle() {
			if (!styleInitialized) {
				orangeBgTex = new Texture2D(1, 1);
				orangeBgTex.SetPixel(0, 0, XKCDColors.DarkOrange);
				orangeBgTex.Apply();

				style = new GUIStyle(GUI.skin.label);
				style.normal.background = orangeBgTex;
				style.border = new RectOffset(0, 0, 0, 0);
				style.padding = new RectOffset(0, 0, 0, 0);

				styleInitialized = true;
			}
		}
	}
}
