using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using Mirror.RemoteCalls;

namespace DevTools.Patches
{
	[HarmonyPatch(typeof(RemoteProcedureCalls), nameof(RemoteProcedureCalls.GetInvokerForHash))]
	public static class ReceivingMethodLoggingPatch
	{
		public static void Postfix(ushort cmdHash)
		{
			if(!DevTools.Instance.Config.LoggingNetworkMethods) return;
			if(!RemoteProcedureCalls.GetInvokerForHash(cmdHash, RemoteCallType.Command, out Invoker invoker)) return;
			var methodName = invoker.function.GetMethodName().Substring(15);
			if(DevTools.Instance.Config.DisabledLoggingNetworkMethods.Contains(methodName)) return;
			Log.Debug($"[Receiving: {methodName}]");
		}
	}
}