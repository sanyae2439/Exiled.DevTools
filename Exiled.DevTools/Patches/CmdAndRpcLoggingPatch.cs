using Exiled.API.Features;
using HarmonyLib;
using Mirror;

namespace ExiledEventsDebugger.Patches
{
	[HarmonyPatch(typeof(NetworkBehaviour), "GetInvokerForHash")]
	public static class CmdAndRpcLoggingPatch
	{
		public static void Postfix(ref NetworkBehaviour.Invoker invoker)
		{
			var methodname = invoker.invokeFunction.GetMethodName().Substring(10).Insert(0, "Call");
			Log.Debug($"{methodname}\n");


			if(methodname == "InvokeCmdCmdAltIsActive" || methodname == "InvokeCmdCmdSyncItem" || methodname == "InvokeRpcRpcBlinkTime" || methodname == "InvokeRpcRpcPlaySound") return;
		}
	}
}