using System;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using Mirror.RemoteCalls;

namespace Exiled.DevTools
{
	[HarmonyPatch(typeof(NetworkConnection), nameof(NetworkConnection.UnpackAndInvoke))]
	public static class MessageLoggingPatch
	{
		public static void Prefix(NetworkConnection __instance, NetworkReader reader, int channelId)
		{
			if(!DevTools.Instance.Config.LoggingNetworkMethods) return;

			var newreader = NetworkReaderPool.GetReader(reader.buffer);
			if(!MessagePacking.Unpack(newreader, out var key)) return;

			if(__instance.messageHandlers.TryGetValue(key, out var networkMessageDelegate) && networkMessageDelegate.Method.DeclaringType.IsGenericType)
			{
				string methodName = networkMessageDelegate.Method.DeclaringType.GetGenericArguments()[0].Name;
				if(methodName == "RpcMessage" || methodName == "CommandMessage" || methodName == "UpdateVarsMessage") return;
				if(DevTools.Instance.Config.DisabledLoggingNetworkMethods.Contains(methodName)) return;
				Log.Debug($"[{methodName}]");
			}
			NetworkReaderPool.Recycle(newreader);
		}
	}
}