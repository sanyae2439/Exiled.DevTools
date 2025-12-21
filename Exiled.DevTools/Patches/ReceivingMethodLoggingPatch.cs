using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using Mirror.RemoteCalls;

namespace DevTools.Patches
{
	[HarmonyPatch(typeof(RemoteProcedureCalls), nameof(RemoteProcedureCalls.GetInvokerForHash))]
	public static class ReceivingMethodLoggingPatch
	{
		public static void Postfix(ushort functionHash, RemoteCallType remoteCallType, ref Invoker invoker)
		{
			if(!DevTools.Instance.Config.LoggingNetworkMethods) return;
			if (invoker == null) return;
			var methodName = invoker.function.GetMethodName().Substring(15);
			if(DevTools.Instance.Config.DisabledLoggingNetworkMethods.Contains(methodName)) return;
			Log.Debug($"[Receiving: {methodName}]");
		}
	}
}