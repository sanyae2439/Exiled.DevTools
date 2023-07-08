using Exiled.API.Features;
using HarmonyLib;

namespace DevTools.Patches
{
	/*[HarmonyPatch(typeof(RemoteCallHelper), nameof(RemoteCallHelper.GetMethodHash))]
	public static class SendingMethodLoggingPatch
	{
		public static void Postfix(string methodName)
		{
			if(!DevTools.Instance.Config.LoggingNetworkMethods) return;
			if(DevTools.Instance.Config.DisabledLoggingNetworkMethods.Contains(methodName)) return;
			Log.Debug($"[  Sending: {methodName}]");
		}
	}*/
}