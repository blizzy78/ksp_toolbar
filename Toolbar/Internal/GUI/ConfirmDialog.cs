/*
Copyright (c) 2013-2016, Maik Schreiber
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar {
	internal class ConfirmDialog : AbstractWindow {
		private string text;
		private Action onOk;
		private Action onCancel;
		private string okText;
		private string cancelText;

		internal ConfirmDialog(string title, string text, Action onOk, Action onCancel, string okText = "OK", string cancelText = "Cancel") : base() {
			Rect = new Rect(300, 300, Screen.width / 4, 0);
			Title = title;
			Dialog = true;
			Modal = true;

			this.text = text;
			this.onOk = onOk;
			this.onCancel = onCancel;
			this.okText = okText;
			this.cancelText = cancelText;
		}

		internal override void drawContents() {
			GUILayout.BeginVertical();

			GUILayout.Label(text, GUILayout.ExpandWidth(true));

			GUILayout.Space(15);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(okText)) {
				onOk();
			}
			if (GUILayout.Button(cancelText)) {
				onCancel();
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}

		internal static void confirm(string title, string text, Action onOk, string okText = "OK", string cancelText = "Cancel") {
			ConfirmDialog dialog = null;
			dialog = new ConfirmDialog(title, text,
				() => {
					dialog.destroy();
					onOk();
				},
				() => dialog.destroy(),
				okText, cancelText);
		}
	}
}
