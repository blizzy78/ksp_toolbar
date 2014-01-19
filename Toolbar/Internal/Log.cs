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
	internal enum LogLevel {
		DEBUG = 0,
		INFO = 1,
		WARN = 2,
		ERROR = 3
	}

	internal static class Log {
		private const string CATEGORY = "Toolbar";

		internal static LogLevel Level = LogLevel.WARN;

		internal static void debug(string message, params object[] @params) {
			if (doLog(LogLevel.DEBUG)) {
				Debug.Log(getLogMessage(LogLevel.DEBUG, message, @params));
			}
		}

		internal static void info(string message, params object[] @params) {
			if (doLog(LogLevel.INFO)) {
				Debug.Log(getLogMessage(LogLevel.INFO, message, @params));
			}
		}

		internal static void warn(string message, params object[] @params) {
			warn(null, message, @params);
		}

		internal static void warn(Exception e, string message, params object[] @params) {
			if (doLog(LogLevel.WARN)) {
				Debug.LogWarning(getLogMessage(LogLevel.WARN, message, @params));
				if (e != null) {
					Debug.LogException(e);
				}
			}
		}

		internal static void error(string message, params object[] @params) {
			error(null, message, @params);
		}

		internal static void error(Exception e, string message, params object[] @params) {
			if (doLog(LogLevel.ERROR)) {
				Debug.LogError(getLogMessage(LogLevel.ERROR, message, @params));
				if (e != null) {
					Debug.LogException(e);
				}
			}
		}

		private static bool doLog(LogLevel level) {
			return level >= Level;
		}

		private static string getLogMessage(LogLevel level, string message, params object[] @params) {
			return string.Format("[{0}] [{1}] {2}", CATEGORY, level, formatMessage(message, @params));
		}

		private static string formatMessage(string message, params object[] @params) {
			return ((@params != null) && (@params.Length > 0)) ? string.Format(message, @params) : message;
		}
	}
}
