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
	/// <summary>
	/// A drawable that draws a popup menu.
	/// </summary>
	public partial class PopupMenuDrawable : IDrawable {
		/// <summary>
		/// Event handler that can be registered with to receive "any menu option clicked" events.
		/// </summary>
		public event Action OnAnyOptionClicked {
			add {
				menu.OnAnyOptionClicked += value;
			}
			remove {
				menu.OnAnyOptionClicked -= value;
			}
		}

		public PopupMenuDrawable() {
			// clamping is done by Toolbar.drawDrawables()
			menu.AutoClampToScreen = false;
		}

		public void Update() {
			// nothing to do
		}

		public Vector2 Draw(Vector2 position) {
			menu.Rect.x = position.x;
			menu.Rect.y = position.y;

			// we're not using WindowList, so we need to draw here
			menu.draw();

			return new Vector2(menu.Rect.width, menu.Rect.height);
		}

		/// <summary>
		/// Adds a new option to the popup menu.
		/// </summary>
		/// <param name="text">The text of the option.</param>
		/// <returns>A button that can be used to register clicks on the menu option.</returns>
		public IButton AddOption(string text) {
			Button option = Button.createMenuOption(text);
			menu += option;
			return option.command;
		}

		/// <summary>
		/// Adds a separator to the popup menu.
		/// </summary>
		public void AddSeparator() {
			menu += Separator.Instance;
		}

		/// <summary>
		/// Destroys this drawable. This must always be called before disposing of this drawable.
		/// </summary>
		public void Destroy() {
			menu.destroy();
		}
	}
}
