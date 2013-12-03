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

namespace Toolbar {
	/// <summary>
	/// A toolbar manager.
	/// </summary>
	public interface IToolbarManager {
		/// <summary>
		/// Adds a new button.
		/// </summary>
		/// <remarks>
		/// To replace an existing button, just add a new button using the old button's namespace and ID.
		/// Note that the new button will inherit the screen position of the old button.
		/// </remarks>
		/// <param name="ns">The new button's namespace. This is usually the plugin's name. Must not include special characters like '.'</param>
		/// <param name="id">The new button's ID. This ID must be unique across all buttons in the namespace. Must not include special characters like '.'</param>
		/// <returns>The button created.</returns>
		IButton add(string ns, string id);
	}
}
