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

namespace Toolbar {
	internal class VisibleButtons {
		private List<Button> buttons;
		private Func<Button, bool> effectivelyUserVisibleFunc;
		private Dictionary<string, bool> visibleButtons = new Dictionary<string, bool>();
		private HashSet<string> visibleButtonIds = new HashSet<string>();
		private bool needsCheck = true;

		internal VisibleButtons(List<Button> buttons, Func<Button, bool> effectivelyUserVisibleFunc) {
			this.buttons = buttons;
			this.effectivelyUserVisibleFunc = effectivelyUserVisibleFunc;
		}

		internal void reset() {
			needsCheck = true;
		}

		internal bool update() {
			bool changed = false;
			if (needsCheck) {
				changed = (buttons.Count() != visibleButtonIds.Count()) ||
					buttons.Any((b) => {
						if (visibleButtons.ContainsKey(b.FullId)) {
							bool oldVisible = visibleButtons[b.FullId];
							bool newVisible = effectivelyUserVisibleFunc(b);
							return oldVisible != newVisible;
						} else {
							return true;
						}
					});
				if (changed) {
					Log.info("button visibilities have changed");

					visibleButtons.Clear();
					foreach (Button button in buttons) {
						visibleButtons.Add(button.FullId, effectivelyUserVisibleFunc(button));
					}
					visibleButtonIds = new HashSet<string>(buttons.Select((b) => b.FullId));
				}

				needsCheck = false;
			}

			return changed;
		}
	}
}
