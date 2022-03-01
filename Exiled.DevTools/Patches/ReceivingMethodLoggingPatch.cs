using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using Mirror.RemoteCalls;

namespace DevTools.Patches
{
	[HarmonyPatch(typeof(RemoteCallHelper), nameof(RemoteCallHelper.GetCommandInfo))]
	public static class ReceivingMethodLoggingPatch
	{
		public static void Postfix(int cmdHash)
		{
			if(!DevTools.Instance.Config.LoggingNetworkMethods) return;
			if(!RemoteCallHelper.GetInvokerForHash(cmdHash, MirrorInvokeType.Command, out Invoker invoker)) return;
			var methodName = invoker.invokeFunction.GetMethodName().Substring(15);
			if(DevTools.Instance.Config.DisabledLoggingNetworkMethods.Contains(methodName)) return;
			Log.Debug($"[Receiving: {methodName}]");
		}
	}
}