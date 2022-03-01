using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;

namespace DevTools.Patches
{
	[HarmonyPatch]
	public static class SendingMessageLoggingPatch
	{
		public static IEnumerable<MethodBase> TargetMethods()
		{
			MethodInfo genericMethod = typeof(MessagePacking).GetMethods().First(x => x.IsGenericMethod && x.Name == nameof(MessagePacking.Pack));
			foreach(var messageType in typeof(ServerConsole).Assembly.GetTypes().Where(x => x.IsValueType && x.GetInterface(nameof(NetworkMessage)) != null))
				yield return genericMethod.MakeGenericMethod(messageType);
			foreach(var messageType in typeof(NetworkServer).Assembly.GetTypes().Where(x => x.IsValueType && x.GetInterface(nameof(NetworkMessage)) != null))
				yield return genericMethod.MakeGenericMethod(messageType);
		}

		public static void Prefix(MethodBase __originalMethod)
		{
			if(!DevTools.Instance.Config.LoggingNetworkMessages) return;
			var messageName = __originalMethod.GetParameters()[0].ParameterType.Name;
			if(messageName == "CommandMessage" || messageName == "RpcMessage") return;
			if(DevTools.Instance.Config.DisabledLoggingNetworkMessages.Contains(messageName)) return;
			Log.Debug($"[  Sending: {messageName}]");
		}
	}
}