using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbar;
using UnityEngine;

internal class BoxDrawable : IDrawable {
	private int width;
	private int height;

	internal BoxDrawable() {
		// random size for testing purposes
		changeSize();
	}

	public void Update() {
		// nothing to do
	}

	public Vector2 Draw(Vector2 position) {
		GUILayout.BeginArea(new Rect(position.x, position.y, width, height), GUI.skin.box);
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("something useful here");
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
		GUILayout.EndArea();

		return new Vector2(width, height);
	}

	internal void changeSize() {
		width = new System.Random().Next(200) + 50;
		height = new System.Random().Next(200) + 50;
	}
}
