/*
Copyright (c) 2013-2015, Maik Schreiber
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
	/// <summary>
	/// A drawable that is tied to a particular button. This can be anything from a popup menu
	/// to an informational window.
	/// </summary>
	public interface IDrawable {
		/// <summary>
		/// Update any information. This is called once per frame.
		/// </summary>
		void Update();

		/// <summary>
		/// Draws GUI widgets for this drawable. This is the equivalent to the OnGUI() message in
		/// <see cref="MonoBehaviour"/>.
		/// </summary>
		/// <remarks>
		/// The drawable will be positioned near its parent toolbar according to the drawable's current
		/// width/height.
		/// </remarks>
		/// <param name="position">The left/top position of where to draw this drawable.</param>
		/// <returns>The current width/height of this drawable.</returns>
		Vector2 Draw(Vector2 position);
	}
}
