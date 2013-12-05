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
	/// Determines visibility of a button in relation to the currently running game scene.
	/// </summary>
	/// <example>
	/// <code>
	/// IButton button = ...
	/// button.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.SPH);
	/// </code>
	/// </example>
	/// <seealso cref="IButton.Visibility"/>
	public class GameScenesVisibility : IVisibility {
		private GameScenes[] gameScenes;

		public bool Visible {
			get {
				return gameScenes.Contains(HighLogic.LoadedScene);
			}
		}

		public GameScenesVisibility(params GameScenes[] gameScenes) {
			this.gameScenes = gameScenes;
		}
	}
}
