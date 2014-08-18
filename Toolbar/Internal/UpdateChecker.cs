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
	internal class UpdateChecker {
		private const string VERSION_URL = "http://blizzy.de/toolbar/version2.txt";

		internal bool CheckForUpdates;
		internal bool Done;
		internal string[] KspVersions = null;
		internal bool KspVersionsFromConfig;
		internal int Sh = 0;

		private WWW www;

		internal UpdateChecker() {
		}

		internal void update() {
			Log.trace("UpdateChecker.update()");

			if (!CheckForUpdates) {
				Done = true;
			}

			if (!Done) {
				if (www == null) {
					Log.debug("getting version from {0}", VERSION_URL);
					www = new WWW(VERSION_URL);
				}

				if (www.isDone) {
					try {
						bool updateAvailable = false;
						if (String.IsNullOrEmpty(www.error)) {
							string text = www.text.Replace("\r", string.Empty);
							Log.debug("version text: {0}", text);
							string[] lines = text.Split(new char[] { '\n' }, StringSplitOptions.None);
							try {
								int version = int.Parse(lines[0]);
								updateAvailable = version > ToolbarManager.VERSION;
							} catch (Exception) {
								// ignore
							}
							KspVersions = lines[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
							KspVersionsFromConfig = false;
							try {
								Sh = int.Parse(lines[2]);
							} catch (Exception) {
								// ignore
							}
						}

						if (updateAvailable) {
							Log.info("update found, adding notification button: {0} vs {1}", www.text, ToolbarManager.VERSION);
							addUpdateAvailableButton();
						} else {
							Log.info("no update found: {0} vs {1}", www.text, ToolbarManager.VERSION);
						}
					} finally {
						www = null;
						Done = true;
					}
				}
			}
		}

		private void addUpdateAvailableButton() {
			IButton button = ToolbarManager.Instance.add(ToolbarManager.NAMESPACE_INTERNAL, "__updateAvailable");
			button.TexturePath = "000_Toolbar/update-available";
			button.ToolTip = "Toolbar Plugin Update Available (Right-Click to Dismiss)";
			button.Important = true;
			button.OnClick += (e) => {
				if (e.MouseButton == 1) {
					button.Destroy();
				} else {
					Application.OpenURL(ToolbarManager.FORUM_THREAD_URL);
					button.Important = false;
				}
			};
		}
	}
}
