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
