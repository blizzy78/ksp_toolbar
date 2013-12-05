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

namespace Toolbar {
	/// <summary>
	/// Event describing a click on a button.
	/// </summary>
	public class ClickEvent : EventArgs {
		/// <summary>
		/// The button that has been clicked.
		/// </summary>
		public readonly IButton Button;

		/// <summary>
		/// The mouse button which the button was clicked with.
		/// </summary>
		/// <remarks>
		/// Is 0 for left mouse button, 1 for right mouse button, and 2 for middle mouse button.
		/// </remarks>
		public readonly int MouseButton;

		internal ClickEvent(IButton button, int mouseButton) {
			this.Button = button;
			this.MouseButton = mouseButton;
		}
	}
}
