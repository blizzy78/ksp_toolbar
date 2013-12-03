using System;
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
