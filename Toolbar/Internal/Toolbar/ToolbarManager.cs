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
		internal const int VERSION = 22;

		internal static ToolbarManager InternalInstance;

		private HashSet<Command> commands_;
		internal IEnumerable<Command> Commands {
			get {
				return commands_;
			}
		}

		private bool ShowGUI {
			get {
				return !uiHidden && isRelevantGameScene(gameScene);
			}
		}

		internal int ToolbarsCount {
			get {
				return toolbars.Count();
			}
		}

        internal event Action OnCommandAdded;
        internal event Action OnSceneChanged;
		internal readonly UpdateChecker UpdateChecker;

		private Dictionary<string, Toolbar> toolbars;
		private ConfigNode settings;
		private bool running = true;
		private ToolbarGameScene gameScene = ToolbarGameScene.LOADING;
		private bool uiHidden;

        internal ApplicationLauncher appLauncher;
        private Dictionary<Command, ApplicationLauncherButton> appLauncherButtons;

		internal ToolbarManager() {
			Log.trace("ToolbarManager()");

			if (Instance == null) {
				Instance = this;
				InternalInstance = this;
				GameObject.DontDestroyOnLoad(this);

				commands_ = new HashSet<Command>();
				toolbars = new Dictionary<string, Toolbar>();
                appLauncherButtons = new Dictionary<Command, ApplicationLauncherButton>();

				UpdateChecker = new UpdateChecker();

				loadSettings(ToolbarGameScene.MAINMENU);

				GameEvents.onHideUI.Add(onHideUI);
				GameEvents.onShowUI.Add(onShowUI);
                GameEvents.onGUIApplicationLauncherReady.Add(onAppLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(onAppLauncherDestroy);
			} else {
				Log.warn("ToolbarManager already running, marking this instance as stale");
				running = false;
			}
		}

        private void onAppLauncherReady()
        {
            Log.trace("ToolbarManager.onAppLauncherReady()");

            appLauncher = ApplicationLauncher.Instance;

            Command[] keys = new Command[appLauncherButtons.Count];
            appLauncherButtons.Keys.CopyTo(keys, 0);
            foreach (Command command in keys)
                removeAppLauncherButton(command);
            
            foreach (Command command in Commands)
                addAppLauncherButton(command);
        }

        private bool removeAppLauncherButton(Command command)
        {
            Log.trace("ToolbarManager.removeAppLauncherButton()");

            ApplicationLauncherButton button;
            if (!appLauncherButtons.TryGetValue(command, out button))
                return false;
            Log.info("Removing command {0}.{1} from` the app launcher", command.Namespace, command.Id);
            appLauncher.RemoveModApplication(button);
            appLauncherButtons.Remove(command);
            return true;
        }

        private bool addAppLauncherButton(Command command)
        {
            Log.trace("ToolbarManager.addAppLauncherButton()");

            if (appLauncher == null || command.destroyed || command.IsInternal || !command.EffectivelyVisible)
                return false;
            
            Log.info("Adding command {0}.{1} to the app launcher", command.Namespace, command.Id);
            
            ApplicationLauncher.AppScenes scenes = ApplicationLauncher.AppScenes.ALWAYS;
            Texture texture = GameDatabase.Instance.GetTexture(command.TexturePath, false);
            ApplicationLauncherButton button = appLauncher.AddModApplication(command.click, command.click, command.mouseEnter, command.mouseLeave, null, null, scenes, texture);
            
            appLauncherButtons.Add(command, button);
            
            return true;
        }

        internal bool updateAppLauncherButton(Command command)
        {
            Log.trace("ToolbarManager.updateAppLauncherButton()");
            
            if (!Commands.Contains(command) || command.destroyed || command.IsInternal)
                return false;

            ApplicationLauncherButton button;
            if (!appLauncherButtons.TryGetValue(command, out button))
            {
                if (!command.EffectivelyVisible)
                    return false;
                addAppLauncherButton(command);
            }
            else
            {
                if (command.EffectivelyVisible)
                {
                    button.SetTexture(GameDatabase.Instance.GetTexture(command.TexturePath, false));
                }
                else
                {
                    removeAppLauncherButton(command);
                }
            }
            
            return true;
        }

        private void onAppLauncherDestroy()
        {
            Log.trace("ToolbarManager.onAppLauncherDestroy()");
        }

		internal void OnDestroy() {
			Log.trace("ToolbarManager.OnDestroy()");

			saveSettings(ToolbarGameScene.MAINMENU);

			GameEvents.onHideUI.Remove(onHideUI);
			GameEvents.onShowUI.Remove(onShowUI);

			if (running) {
				foreach (Toolbar toolbar in toolbars.Values) {
					toolbar.destroy();
				}
			}
		}

		internal void OnGUI() {
			if (running && ShowGUI) {
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
				UpdateChecker.update();
				if (ShowGUI) {
					CursorGrabbing.Instance.update();
				}
			}
		}

		private void handleGameSceneChange() {
			ToolbarGameScene scene = ToolbarGameScenes.getCurrent();
			if (scene != gameScene) {
				gameScene = scene;
				gameSceneChanged(scene);
                if (OnSceneChanged != null)
                    OnSceneChanged();
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

				string[] kspVersions = toolbarsNode.get("kspVersions", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (kspVersions.Length > 0) {
					UpdateChecker.KspVersions = kspVersions;
					UpdateChecker.KspVersionsFromConfig = true;
				}

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

			Log.info("update check {0}", checkForUpdates ? "enabled" : "disabled");
			UpdateChecker.CheckForUpdates = checkForUpdates;
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

			if (UpdateChecker.Done &&
				(UpdateChecker.KspVersions != null) &&
				(UpdateChecker.KspVersions.Length > 0)) {

				toolbarsNode.overwrite("kspVersions", string.Join(",", UpdateChecker.KspVersions));
			}

			ConfigNode sceneNode = toolbarsNode.getOrCreateNode(scene.ToString());
			foreach (KeyValuePair<string, Toolbar> entry in toolbars) {
				ConfigNode toolbarNode = sceneNode.getOrCreateNode(entry.Key);
				entry.Value.saveSettings(toolbarNode);
			}
			root.Save(SETTINGS_FILE);
		}

		private void onHideUI() {
			uiHidden = true;
		}

		private void onShowUI() {
			uiHidden = false;
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
