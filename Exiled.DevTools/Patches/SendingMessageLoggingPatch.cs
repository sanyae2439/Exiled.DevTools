using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;

namespace DevTools
{
	[HarmonyPatch]
	public static class SendingMessageLoggingPatch
	{
		public static IEnumerable<MethodBase> TargetMethods()
		{
			var genericMethod = typeof(NetworkConnection).GetMethods().First(x => x.IsGenericMethod && x.Name == nameof(NetworkConnection.Send));		
			foreach(var messageType in typeof(ServerConsole).Assembly.GetTypes().Where(x => x.IsValueType && x.GetInterface(nameof(NetworkMessage)) != null))
				yield return genericMethod.MakeGenericMethod(messageType);
			yield break;
		}

		public static void Prefix(MethodBase __originalMethod)
		{
			if(!DevTools.Instance.Config.LoggingNetworkMethods) return;
			var messageName = __originalMethod.GetParameters()[0].ParameterType.Name;
			if(DevTools.Instance.Config.DisabledLoggingNetworkMessages.Contains(messageName)) return;
			Log.Debug($"[  Sending: {messageName}]");
		}
	}
}