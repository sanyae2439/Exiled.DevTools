using Exiled.API.Features;
using HarmonyLib;
using Mirror.RemoteCalls;

namespace DevTools.Patches
{
	[HarmonyPatch(typeof(RemoteProcedureCalls), nameof(RemoteProcedureCalls.GetDelegate))]
	public static class SendingMethodLoggingPatch
	{
		public static void Postfix(ushort functionHash)
		{
			if(!DevTools.Instance.Config.LoggingNetworkMethods) return;
            if (!RemoteProcedureCalls.remoteCallDelegates.TryGetValue(functionHash, out Invoker invoker))
                return;
			string methodName = invoker.function.Method.Name;
            if (DevTools.Instance.Config.DisabledLoggingNetworkMethods.Contains(methodName)) return;
			Log.Debug($"[  Sending: {methodName}]");
		}
	}
}