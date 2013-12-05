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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar {
	[KSPAddonFixed(KSPAddon.Startup.EveryScene, true, typeof(ToolbarManager))]
	public class ToolbarManager : MonoBehaviour, IToolbarManager {
		public static IToolbarManager Instance {
			get;
			private set;
		}

		private static readonly string settingsFile = KSPUtil.ApplicationRootPath + "GameData/toolbar-settings.dat";

		private RenderingManager renderingManager;
		private bool settingsLoaded;
		private Toolbar toolbar = new Toolbar();

		internal ToolbarManager() {
			Instance = this;

			toolbar.onChange += toolbarChanged;

			GameObject.DontDestroyOnLoad(this);
		}

		internal void OnGUI() {
			if (!showGUI()) {
				return;
			}

			loadSettings();

			toolbar.draw();
		}

		internal void Update() {
			toolbar.update();
		}

		private void toolbarChanged() {
			saveSettings();
		}

		private void loadSettings() {
			if (!settingsLoaded) {
				Debug.Log("loading toolbar settings");

				ConfigNode root = ConfigNode.Load(settingsFile) ?? new ConfigNode();
				if (root.HasNode("toolbars")) {
					ConfigNode toolbarsNode = root.GetNode("toolbars");
					toolbar.loadSettings(toolbarsNode);
				}

				settingsLoaded = true;
			}
		}

		private void saveSettings() {
			Debug.Log("saving toolbar settings");

			ConfigNode root = new ConfigNode();
			ConfigNode toolbarsNode = root.AddNode("toolbars");
			toolbar.saveSettings(toolbarsNode);
			root.Save(settingsFile);
		}

		private bool showGUI() {
			if (renderingManager == null) {
				renderingManager = (RenderingManager) GameObject.FindObjectOfType(typeof(RenderingManager));
			}

			if (renderingManager != null) {
				GameObject o = renderingManager.uiElementsToDisable.FirstOrDefault();
				return (o == null) || o.activeSelf;
			}

			return false;
		}

		public IButton add(string ns, string id) {
			checkId(ns, "namespace");
			checkId(id, "ID");

			Button button = new Button(ns, id);
			toolbar.add(button);

			return button;
		}

		private void checkId(string id, string label) {
			if (id.Contains('.') || id.Contains(' ') || id.Contains('/') || id.Contains(':')) {
				throw new ArgumentException(label + " contains invalid characters: " + id);
			}
		}
	}
}
