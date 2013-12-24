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
			get;
		}

		/// <summary>
		/// The color the button text is displayed with. Defaults to Color.white.
		/// </summary>
		/// <remarks>
		/// The text color can be changed at any time to modify the button's appearance.
		/// </remarks>
		Color TextColor {
			set;
			get;
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
		/// The texture size must not exceed 24x24 pixels.
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
			get;
		}

		/// <summary>
		/// The button's tool tip text. Set to null if no tool tip is desired.
		/// </summary>
		/// <remarks>
		/// Tool Tip Text Should Always Use Headline Style Like This.
		/// </remarks>
		string ToolTip {
			set;
			get;
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
		/// Whether this button is currently effectively visible or not. This is a combination of
		/// <see cref="Visible"/> and <see cref="Visibility"/>.
		/// </summary>
		/// <remarks>
		/// Note that the toolbar is not visible in certain game scenes, for example the loading screens. This property
		/// does not reflect button invisibility in those scenes.
		/// </remarks>
		bool EffectivelyVisible {
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
		/// Whether this button is currently "important." Set to false to return to normal button behaviour.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This can be used to temporarily force the button to be shown on screen regardless of the toolbar being
		/// currently in auto-hidden mode. For example, a button that signals the arrival of a private message in
		/// a chat room could mark itself as "important" as long as the message has not been read.
		/// </para>
		/// <para>
		/// Setting this property does not change the appearance of the button. Use <see cref="TexturePath"/> to
		/// change the button's icon.
		/// </para>
		/// <para>
		/// This feature should be used only sparingly, if at all, since it forces the button to be displayed on
		/// screen even when it normally wouldn't.
		/// </para>
		/// </remarks>
		bool Important {
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
		/// Event handler that can be registered with to receive "on mouse enter" events.
		/// </summary>
		/// <example>
		/// <code>
		/// IButton button = ...
		/// button.OnMouseEnter += (e) => {
		///     Debug.Log("mouse entered button");
		/// };
		/// </code>
		/// </example>
		event MouseEnterHandler OnMouseEnter;

		/// <summary>
		/// Event handler that can be registered with to receive "on mouse leave" events.
		/// </summary>
		/// <example>
		/// <code>
		/// IButton button = ...
		/// button.OnMouseLeave += (e) => {
		///     Debug.Log("mouse left button");
		/// };
		/// </code>
		/// </example>
		event MouseLeaveHandler OnMouseLeave;

		/// <summary>
		/// Permanently destroys this button so that it is no longer displayed.
		/// Should be used when a plugin is stopped to remove leftover buttons.
		/// </summary>
		void Destroy();
	}
}
