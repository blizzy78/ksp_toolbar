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
	public partial class ToolbarManager : MonoBehaviour, IToolbarManager {
		internal const string FORUM_THREAD_URL = "http://forum.kerbalspaceprogram.com/threads/60863";

		private static readonly string settingsFile = KSPUtil.ApplicationRootPath + "GameData/toolbar-settings.dat";

		private const int VERSION = 5;

		private static WWW versionWWW;

		private RenderingManager renderingManager;
		private Toolbar toolbar = new Toolbar();
		private ConfigNode settings;
		private bool checkForUpdates;

		internal ToolbarManager() {
			Instance = this;
			GameObject.DontDestroyOnLoad(this);

			toolbar.onChange += toolbarChanged;

			GameEvents.onGameSceneLoadRequested.Add(gameSceneLoadRequested);
		}

		internal void OnDestroy() {
			GameEvents.onGameSceneLoadRequested.Remove(gameSceneLoadRequested);
		}

		internal void OnGUI() {
			if (showGUI()) {
				toolbar.draw();
				WindowList.Instance.draw();
			}
		}

		internal void Update() {
			toolbar.update();

			checkForNewVersion();
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
			Debug.Log("loading toolbar settings (" + scene + ")");

			ConfigNode root = loadSettings();
			if (root.HasNode("toolbars")) {
				ConfigNode toolbarsNode = root.GetNode("toolbars");
				checkForUpdates = toolbarsNode.get("checkForUpdates", true);

				toolbar.loadSettings(toolbarsNode, scene);
			}
		}

		private ConfigNode loadSettings() {
			if (settings == null) {
				settings = ConfigNode.Load(settingsFile) ?? new ConfigNode();
			}
			return settings;
		}

		private void saveSettings() {
			GameScenes scene = HighLogic.LoadedScene;
			Debug.Log("saving toolbar settings (" + scene + ")");

			ConfigNode root = loadSettings();
			toolbar.saveSettings(root.getOrCreateNode("toolbars"), scene);
			root.Save(settingsFile);
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

		private void checkForNewVersion() {
			if (checkForUpdates) {
				if (versionWWW == null) {
					versionWWW = new WWW("http://blizzy.de/toolbar/version.txt");
				}

				if (versionWWW.isDone) {
					if (String.IsNullOrEmpty(versionWWW.error)) {
						try {
							if (int.Parse(versionWWW.text) > VERSION) {
								addUpdateAvailableButton();
							}
						} catch (Exception) {
							// ignore
						}
					}

					checkForUpdates = false;
					versionWWW = null;
				}
			}
		}

		private void addUpdateAvailableButton() {
			IButton button = add(Button.NAMESPACE_INTERNAL, "updateAvailable");
			button.TexturePath = "000_Toolbar/update-available";
			button.ToolTip = "Toolbar Plugin Update Available";
			button.Important = true;
			button.OnClick += (e) => {
				Application.OpenURL(FORUM_THREAD_URL);
				button.Important = false;
			};
		}

		public IButton add(string ns, string id) {
			Button button = new Button(ns, id, toolbar);
			toolbar.add(button);

			return button;
		}
	}
}
