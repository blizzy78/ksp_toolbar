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
		private const string VERSION_URL = "http://blizzy.de/toolbar/version.txt";

		internal bool CheckForUpdates;

		internal event Action OnDone;

		private WWW www;
		private bool done;

		internal UpdateChecker() {
		}

		internal void update() {
			if (CheckForUpdates && !done) {
				if (www == null) {
					www = new WWW(VERSION_URL);
				}

				if (www.isDone) {
					bool updateAvailable = false;
					if (String.IsNullOrEmpty(www.error)) {
						try {
							updateAvailable = int.Parse(www.text) > ToolbarManager.VERSION;
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

					www = null;
					done = true;

					if (OnDone != null) {
						OnDone();
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
