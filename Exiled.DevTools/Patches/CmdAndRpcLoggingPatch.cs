using Exiled.API.Features;
using HarmonyLib;
using Mirror;

namespace Exiled.DevTools
{
	[HarmonyPatch(typeof(NetworkBehaviour), "GetInvokerForHash")]
	public static class CmdAndRpcLoggingPatch
	{
		public static void Postfix(NetworkBehaviour.Invoker invoker)
		{
			var methodname = invoker.invokeFunction.GetMethodName().Substring(9);
			if(DevTools.Instance.Config.DisabledLoggingNetworkMethods.Contains(methodname)) return;
			Log.Debug($"[{methodname}]");
		}
	}
}