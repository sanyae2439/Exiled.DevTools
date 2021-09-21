using System;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using Mirror.RemoteCalls;

namespace Exiled.DevTools
{
	[HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.UnpackAndInvoke))]
	public static class MessageLoggingPatch
	{
		public static void Prefix(NetworkReader reader)
		{
			if(!DevTools.Instance.Config.LoggingNetworkMethods) return;

			var newreader = NetworkReaderPool.GetReader(reader.buffer);
			newreader.Position = reader.Position;

			if(!MessagePacking.Unpack(newreader, out var key)) return;
			if(NetworkServer.handlers.TryGetValue(key, out var networkMessageDelegate) && networkMessageDelegate.Method.DeclaringType.IsGenericType)
			{
				string methodName = networkMessageDelegate.Method.DeclaringType.GetGenericArguments()[0].Name;
				if(methodName == "CommandMessage") return;
				if(DevTools.Instance.Config.DisabledLoggingNetworkMethods.Contains(methodName)) return;
				Log.Debug($"[{methodName}]");
			}
			NetworkReaderPool.Recycle(newreader);
		}
	}
}