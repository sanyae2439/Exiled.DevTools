using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using Mirror.RemoteCalls;

namespace Exiled.DevTools
{
	[HarmonyPatch(typeof(RemoteCallHelper), "GetInvokerForHash")]
	public static class CmdAndRpcLoggingPatch
	{
		public static void Postfix(Invoker invoker)
		{
			var methodname = invoker.invokeFunction.GetMethodName().Substring(15);
			if(DevTools.Instance.Config.DisabledLoggingNetworkMethods.Contains(methodname)) return;
			Log.Debug($"[{methodname}]");
		}
	}
}