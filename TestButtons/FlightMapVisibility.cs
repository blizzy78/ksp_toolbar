using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbar;

class FlightMapVisibility : IVisibility {
	internal static readonly FlightMapVisibility Instance = new FlightMapVisibility();

	private static readonly IVisibility FLIGHT_VISIBILITY = new GameScenesVisibility(GameScenes.FLIGHT);

	public bool Visible {
		get {
			return FLIGHT_VISIBILITY.Visible && MapView.MapIsEnabled;
		}
	}

	private FlightMapVisibility() {
	}
}
