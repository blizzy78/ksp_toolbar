/*
Copyright (c) 2013-2015, Maik Schreiber
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

namespace Toolbar {
	internal class WindowList {
		internal bool ModalDialogOpen {
			get {
				return windows.Any(w => w.Dialog && w.Modal);
			}
		}

		internal static readonly WindowList Instance = new WindowList();

		private List<AbstractWindow> windows = new List<AbstractWindow>();
		private List<AbstractWindow> newWindows;

		private WindowList() {
		}

		internal void draw() {
			// if there is a newer list, use that one
			if (newWindows != null) {
				windows = newWindows;
				newWindows = null;
			}
			foreach (AbstractWindow window in windows) {
				window.draw();
			}
		}

		internal void destroyDialogs() {
			// do not use the actual list because we might be iterating over it right now
			List<AbstractWindow> windowsToDestroy = new List<AbstractWindow>(windows.Where(w => w.Dialog));
			foreach (AbstractWindow window in windowsToDestroy) {
				window.destroy();
			}
		}

		private List<AbstractWindow> createNewWindowList() {
			if (newWindows == null) {
				newWindows = new List<AbstractWindow>(windows);
			}
			return newWindows;
		}

		internal void add(AbstractWindow window) {
			// do not use the actual list because we might be iterating over it right now
			createNewWindowList().Add(window);
		}

		internal void remove(AbstractWindow window) {
			// do not use the actual list because we might be iterating over it right now
			createNewWindowList().Remove(window);
		}
	}
}
