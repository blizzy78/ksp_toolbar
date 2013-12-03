using System;
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
