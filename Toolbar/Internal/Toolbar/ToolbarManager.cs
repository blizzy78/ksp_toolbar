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
		internal const string NAMESPACE_INTERNAL = "__TOOLBAR_INTERNAL";
		internal const int VERSION = 17;

		internal static ToolbarManager InternalInstance;

		private HashSet<Command> commands_;
		internal IEnumerable<Command> Commands {
			get {
				return commands_;
			}
		}

		internal int ToolbarsCount {
			get {
				return toolbars.Count();
			}
		}

		internal event Action OnCommandAdded;

		private RenderingManager renderingManager;
		private Dictionary<string, Toolbar> toolbars;
		private ConfigNode settings;
		private UpdateChecker updateChecker;
		private bool running = true;
		private ToolbarGameScene gameScene = ToolbarGameScene.LOADING;

		internal ToolbarManager() {
			Log.trace("ToolbarManager()");

			if (Instance == null) {
				Instance = this;
				InternalInstance = this;
				GameObject.DontDestroyOnLoad(this);

				commands_ = new HashSet<Command>();
				toolbars = new Dictionary<string, Toolbar>();

				loadSettings(ToolbarGameScene.MAINMENU);

				updateChecker = new UpdateChecker();
				updateChecker.OnDone += () => updateChecker = null;
			} else {
				Log.warn("ToolbarManager already running, marking this instance as stale");
				running = false;
			}
		}

		internal void OnDestroy() {
			Log.trace("ToolbarManager.OnDestroy()");

			if (running) {
				foreach (Toolbar toolbar in toolbars.Values) {
					toolbar.destroy();
				}
			}
		}

		internal void OnGUI() {
			if (running && showGUI()) {
				foreach (Toolbar toolbar in toolbars.Values) {
					toolbar.draw();
				}
				WindowList.Instance.draw();
			}
		}

		internal void Update() {
			if (running) {
				handleGameSceneChange();

				foreach (Toolbar toolbar in toolbars.Values) {
					toolbar.update();
				}
				if (updateChecker != null) {
					updateChecker.update();
				}
				if (showGUI()) {
					CursorGrabbing.Instance.update();
				}
			}
		}

		private void handleGameSceneChange() {
			ToolbarGameScene scene = ToolbarGameScenes.getCurrent();
			if (scene != gameScene) {
				gameScene = scene;
				gameSceneChanged(scene);
			}
		}

		private void toolbarChanged() {
			saveSettings();
		}

		private void gameSceneChanged(ToolbarGameScene scene) {
			if (isRelevantGameScene(scene)) {
				loadSettings(scene);
			}
		}

		private void loadSettings(ToolbarGameScene scene) {
			Log.info("loading settings (game scene: {0})", scene);

			foreach (Toolbar toolbar in toolbars.Values) {
				toolbar.destroy();
			}
			toolbars.Clear();

			WindowList.Instance.destroyDialogs();

			bool checkForUpdates = true;

			ConfigNode root = loadSettings();
			if (root.HasNode("toolbars")) {
				ConfigNode toolbarsNode = root.GetNode("toolbars");
				Log.Level = (LogLevel) int.Parse(toolbarsNode.get("logLevel", ((int) LogLevel.WARN).ToString()));
				checkForUpdates = toolbarsNode.get("checkForUpdates", true);

				if (toolbarsNode.HasNode(scene.ToString())) {
					ConfigNode sceneNode = toolbarsNode.GetNode(scene.ToString());
					foreach (ConfigNode toolbarNode in sceneNode.nodes) {
						Toolbar toolbar = addToolbar(toolbarNode.name);
						toolbar.loadSettings(toolbarNode);
					}
				}
			}

			// ensure there is at least one toolbar in the scene
			if (ToolbarsCount == 0) {
				Log.info("no toolbars in current game scene, adding default toolbar");
				addToolbar();
			}

			if (updateChecker != null) {
				Log.info("update check {0}", checkForUpdates ? "enabled" : "disabled");
				updateChecker.CheckForUpdates = checkForUpdates;
			}
		}

		private ConfigNode loadSettings() {
			if (settings == null) {
				settings = ConfigNode.Load(SETTINGS_FILE) ?? new ConfigNode();
			}
			convertSettings();
			return settings;
		}

		private void convertSettings() {
			if (settings.HasNode("toolbars")) {
				ConfigNode toolbarsNode = settings.GetNode("toolbars");
				if (toolbarsNode.HasNode("toolbar")) {
					ConfigNode toolbarNode = toolbarsNode.GetNode("toolbar");

					Log.info("converting settings from old to new format");

					foreach (ConfigNode sceneNode in toolbarNode.nodes) {
						string scene = sceneNode.name;
						ConfigNode newSceneNode = toolbarsNode.getOrCreateNode(scene);
						ConfigNode newToolbarNode = newSceneNode.getOrCreateNode("toolbar");
						foreach (ConfigNode.Value value in sceneNode.values) {
							newToolbarNode.AddValue(value.name, value.value);
						}
						foreach (ConfigNode childNode in sceneNode.nodes) {
							newToolbarNode.AddNode(childNode);
						}
					}

					toolbarsNode.RemoveNode("toolbar");

					saveSettings(ToolbarGameScene.MAINMENU);
					settings = ConfigNode.Load(SETTINGS_FILE);
				}
			}
		}

		private void saveSettings() {
			saveSettings(gameScene);
		}

		private void saveSettings(ToolbarGameScene scene) {
			Log.info("saving settings (game scene: {0})", scene);

			ConfigNode root = loadSettings();
			ConfigNode toolbarsNode = root.getOrCreateNode("toolbars");
			ConfigNode sceneNode = toolbarsNode.getOrCreateNode(scene.ToString());
			foreach (KeyValuePair<string, Toolbar> entry in toolbars) {
				ConfigNode toolbarNode = sceneNode.getOrCreateNode(entry.Key);
				entry.Value.saveSettings(toolbarNode);
			}
			root.Save(SETTINGS_FILE);
		}

		private bool showGUI() {
			if (!isRelevantGameScene(gameScene)) {
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

		private bool isRelevantGameScene(ToolbarGameScene scene) {
			return (scene != ToolbarGameScene.LOADING) && (scene != ToolbarGameScene.LOADINGBUFFER) &&
				(scene != ToolbarGameScene.MAINMENU) && (scene != ToolbarGameScene.PSYSTEM) && (scene != ToolbarGameScene.CREDITS);
		}

		private void fireCommandAdded() {
			if (OnCommandAdded != null) {
				OnCommandAdded();
			}
		}

		internal void destroyToolbar(Toolbar toolbar) {
			string toolbarId = toolbars.Single(kv => kv.Value.Equals(toolbar)).Key;
			toolbars.Remove(toolbarId);
			toolbar.destroy();

			if (settings.HasNode("toolbars")) {
				ConfigNode toolbarsNode = settings.GetNode("toolbars");
				string scene = gameScene.ToString();
				if (toolbarsNode.HasNode(scene)) {
					ConfigNode sceneNode = toolbarsNode.GetNode(scene);
					if (sceneNode.HasNode(toolbarId)) {
						sceneNode.RemoveNode(toolbarId);
						saveSettings();
					}
				}
			}
		}

		internal void addToolbar() {
			string toolbarId = "toolbar_" + new System.Random().Next();
			Toolbar toolbar = addToolbar(toolbarId);
			toolbar.loadSettings(new ConfigNode());
		}

		private Toolbar addToolbar(string toolbarId) {
			Toolbar toolbar = new Toolbar();
			toolbar.OnChange += toolbarChanged;
			toolbars.Add(toolbarId, toolbar);
			return toolbar;
		}

		public IButton add(string ns, string id) {
			if (running) {
				Command command = new Command(ns, id);

				Log.info("adding button: {0}", command.FullId);

				command.OnDestroy += () => {
					Log.info("button destroyed: {0}", command.FullId);
					commands_.Remove(command);
				};

				// destroy old command with the same id
				foreach (Command oldCommand in new HashSet<Command>(commands_.Where(c => c.FullId == command.FullId))) {
					Log.info("destroying old button with same ID: {0}", oldCommand.FullId);
					oldCommand.Destroy();
				}

				commands_.Add(command);
				CommandCreationCounter.Instance.add(command);

				fireCommandAdded();

				return command;
			} else {
				throw new NotSupportedException("cannot add button to stale ToolbarManager instance");
			}
		}
	}
}
