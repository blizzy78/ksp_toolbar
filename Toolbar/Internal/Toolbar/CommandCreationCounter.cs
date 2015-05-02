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
	internal class CommandCreationCounter {
		internal static readonly CommandCreationCounter Instance = new CommandCreationCounter();

		private Dictionary<string, long> firstCreation = new Dictionary<string, long>();
		private Dictionary<string, int> creationCounts = new Dictionary<string, int>();
		private HashSet<string> allTimeBadIds = new HashSet<string>();

		private CommandCreationCounter() {
		}

		internal bool add(Command command) {
			string key = command.FullId;
			if (!creationCounts.addOrUpdate(key, 1, c => c + 1)) {
				firstCreation.Add(key, DateTime.UtcNow.getSeconds());
			}

			check();

			return !allTimeBadIds.Contains(key);
		}

		private void check() {
			long now = DateTime.UtcNow.getSeconds();
			List<string> badIds = new List<string>(firstCreation.Keys.Where(
				id => (creationCounts[id] >= 100) && ((now - firstCreation[id]) <= 10000)));
			foreach (string id in badIds) {
				Log.warn("button {0} has been created excessively often during the last 10 sec - respective plugin may be broken", id);

				// disable warnings for this button
				firstCreation[id] = -1;
				creationCounts[id] = -1;
				allTimeBadIds.Add(id);
			}
		}
	}
}
