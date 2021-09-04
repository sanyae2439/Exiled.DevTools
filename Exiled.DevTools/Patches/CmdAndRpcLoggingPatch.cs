using System;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using Mirror.RemoteCalls;

namespace Exiled.DevTools
{
	[HarmonyPatch(typeof(RemoteCallHelper), "GetMethodHash")]
	public static class CmdAndRpcLoggingPatch
	{
		public static void Postfix(Type invokeClass, string methodName)
		{
			if(!DevTools.Instance.Config.LoggingNetworkMethods) return;
			if(DevTools.Instance.Config.DisabledLoggingNetworkMethods.Contains(methodName)) return;
			Log.Debug($"[{methodName}]");
		}
	}
}