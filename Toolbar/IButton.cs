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
	/// <summary>
	/// Represents a clickable button.
	/// </summary>
	public interface IButton {
		/// <summary>
		/// The text displayed on the button. Set to null to hide text.
		/// </summary>
		/// <remarks>
		/// The text can be changed at any time to modify the button's appearance. Note that since this will also
		/// modify the button's size, this feature should be used sparingly, if at all.
		/// </remarks>
		/// <seealso cref="TexturePath"/>
		string Text {
			set;
		}

		/// <summary>
		/// The color the button text is displayed with. Defaults to Color.white.
		/// </summary>
		/// <remarks>
		/// The text color can be changed at any time to modify the button's appearance.
		/// </remarks>
		Color TextColor {
			set;
		}

		/// <summary>
		/// The path of a texture file to display an icon on the button. Set to null to hide icon.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A texture path on a button will have precedence over text. That is, if both text and texture path
		/// have been set on a button, the button will show the texture, not the text.
		/// </para>
		/// <para>
		/// The texture will be resized to 24x24 pixels, even if it is larger than that.
		/// </para>
		/// <para>
		/// The texture path must be relative to the "GameData" directory, and must not specify a file name suffix.
		/// Valid example: MyAddon/Textures/icon_mybutton
		/// </para>
		/// <para>
		/// The texture path can be changed at any time to modify the button's appearance.
		/// </para>
		/// </remarks>
		/// <seealso cref="Text"/>
		string TexturePath {
			set;
		}

		/// <summary>
		/// The button's tool tip text. Set to null if no tool tip is desired.
		/// </summary>
		string ToolTip {
			set;
		}

		/// <summary>
		/// Whether this button is currently visible or not. Can be used in addition to or as a replacement for <see cref="Visibility"/>.
		/// </summary>
		bool Visible {
			set;
			get;
		}

		/// <summary>
		/// Determines this button's visibility. Can be used in addition to or as a replacement for <see cref="Visible"/>.
		/// </summary>
		IVisibility Visibility {
			set;
			get;
		}

		/// <summary>
		/// Whether this button is currently enabled (clickable) or not. This will not affect the player's ability to
		/// position the button on their screen.
		/// </summary>
		bool Enabled {
			set;
			get;
		}

		/// <summary>
		/// Event handler that can be registered with to receive "on click" events.
		/// </summary>
		/// <example>
		/// <code>
		/// IButton button = ...
		/// button.OnClick += (e) => {
		///     Debug.Log("button clicked, mouseButton: " + e.MouseButton);
		/// };
		/// </code>
		/// </example>
		event ClickHandler OnClick;

		/// <summary>
		/// Permanently destroys this button so that it is no longer displayed.
		/// Should be used when a plugin is stopped to remove leftover buttons.
		/// </summary>
		void Destroy();
	}
}
