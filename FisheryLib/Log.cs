// Copyright (c) 2022 bradson
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using BepInEx.Logging;
using Logger = BepInEx.Logging.Logger;

namespace FisheryLib;

//[PublicAPI]
public static class Log
{
	public static void Message(string message) => Config.Message(message);
	public static void Warning(string message) => Config.Warning(message);
	public static void Error(string message) => Config.Error(message);

	//[PublicAPI]
	public static class Config
	{
		public static Action<string> Message { get; set; } = static text => Logger.Log(LogLevel.Message, text);

		public static Action<string> Warning { get; set; } = static text => Logger.Log(LogLevel.Warning, text);

		public static Action<string> Error { get; set; } = static text => Logger.Log(LogLevel.Error, text);
	}
}