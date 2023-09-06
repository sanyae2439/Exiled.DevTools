using Exiled.API.Features;
using HarmonyLib;
using Mirror.RemoteCalls;

namespace DevTools.Patches
{
	//[HarmonyPatch(typeof(RemoteProcedureCalls), nameof(RemoteProcedureCalls.GetMethodHash))]
	public static class SendingMethodLoggingPatch
	{
		public static void Postfix(string methodName)
		{
			if(!DevTools.Instance.Config.LoggingNetworkMethods) return;
			if(DevTools.Instance.Config.DisabledLoggingNetworkMethods.Contains(methodName)) return;
			Log.Debug($"[  Sending: {methodName}]");
		}
	}
}