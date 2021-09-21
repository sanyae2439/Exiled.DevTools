using System;
using Exiled.API.Features;
using HarmonyLib;
using Mirror.RemoteCalls;

namespace DevTools
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