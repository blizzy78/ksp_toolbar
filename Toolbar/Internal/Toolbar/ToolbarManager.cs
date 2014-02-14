/*
Copyright (c) 2013-2014, Maik Schreiber
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
	public partial class ToolbarManager : MonoBehaviour, IToolbarManager {
		private static readonly string SETTINGS_FILE = KSPUtil.ApplicationRootPath + "GameData/toolbar-settings.dat";
		internal const string FORUM_THREAD_URL = "http://forum.kerbalspaceprogram.com/threads/60863";
		internal const string DONATE_URL = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PCRP5Y2MUS62A";
		internal const int VERSION = 13;

		private RenderingManager renderingManager;
		private Toolbar toolbar;
		private ConfigNode settings;
		private UpdateChecker updateChecker;
		private bool running = true;

		internal ToolbarManager() {
			Log.trace("ToolbarManager()");

			if (Instance == null) {
				Instance = this;
				GameObject.DontDestroyOnLoad(this);

				loadSettings(GameScenes.MAINMENU);

				toolbar = new Toolbar();
				toolbar.onChange += toolbarChanged;

				updateChecker = new UpdateChecker();
				updateChecker.OnDone += () => updateChecker = null;

				GameEvents.onGameSceneLoadRequested.Add(gameSceneLoadRequested);
			} else {
				Log.warn("ToolbarManager already running, marking this instance as stale");
				running = false;
			}
		}

		internal void OnDestroy() {
			Log.trace("ToolbarManager.OnDestroy()");

			if (running) {
				GameEvents.onGameSceneLoadRequested.Remove(gameSceneLoadRequested);

				toolbar.destroy();
			}
		}

		internal void OnGUI() {
			if (running && showGUI()) {
				toolbar.draw();
				WindowList.Instance.draw();
			}
		}

		internal void Update() {
			if (running) {
				toolbar.update();
				if (updateChecker != null) {
					updateChecker.update();
				}
				if (showGUI()) {
					CursorGrabbing.Instance.update();
				}
			}
		}

		private void toolbarChanged() {
			saveSettings();
		}

		private void gameSceneLoadRequested(GameScenes scene) {
			if (isRelevantGameScene(scene)) {
				loadSettings(scene);
			}
		}

		private void loadSettings(GameScenes scene) {
			Log.info("loading settings (game scene: {0})", scene);

			ConfigNode root = loadSettings();

			if (root.HasValue("logLevel")) {
				Log.Level = (LogLevel) int.Parse(root.GetValue("logLevel"));
			}

			if (root.HasNode("toolbars")) {
				ConfigNode toolbarsNode = root.GetNode("toolbars");
				if (updateChecker != null) {
					updateChecker.CheckForUpdates = toolbarsNode.get("checkForUpdates", true);
				}

				if (toolbar != null) {
					toolbar.loadSettings(toolbarsNode, scene);
				}
			}
		}

		private ConfigNode loadSettings() {
			if (settings == null) {
				settings = ConfigNode.Load(SETTINGS_FILE) ?? new ConfigNode();
			}
			return settings;
		}

		private void saveSettings() {
			GameScenes scene = HighLogic.LoadedScene;
			Log.info("saving settings (game scene: {0})", scene);

			ConfigNode root = loadSettings();
			toolbar.saveSettings(root.getOrCreateNode("toolbars"), scene);
			root.Save(SETTINGS_FILE);
		}

		private bool showGUI() {
			if (!isRelevantGameScene(HighLogic.LoadedScene)) {
				return false;
			}

			if (renderingManager == null) {
				renderingManager = (RenderingManager) GameObject.FindObjectOfType(typeof(RenderingManager));
			}

			if (renderingManager != null) {
				GameObject o = renderingManager.uiElementsToDisable.FirstOrDefault();
				return (o == null) || o.activeSelf;
			}

			return false;
		}

		private bool isRelevantGameScene(GameScenes scene) {
			return (scene != GameScenes.LOADING) && (scene != GameScenes.LOADINGBUFFER) &&
				(scene != GameScenes.MAINMENU) && (scene != GameScenes.PSYSTEM) && (scene != GameScenes.CREDITS);
		}

		public IButton add(string ns, string id) {
			if (running) {
				Button button = new Button(ns, id, toolbar);
				toolbar.add(button);
				return button;
			} else {
				throw new NotSupportedException("cannot add button to stale ToolbarManager instance");
			}
		}
	}
}
