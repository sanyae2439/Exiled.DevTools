using Exiled.API.Features;
using HarmonyLib;
using Mirror;

namespace DevTools.Patches
{
	[HarmonyPatch(typeof(NetworkConnection), nameof(NetworkConnection.UnpackAndInvoke))]
	public static class ReceivingMessageLoggingPatch
	{
		public static void Prefix(NetworkReader reader)
		{
			if(!DevTools.Instance.Config.LoggingNetworkMessages) return;

			var newreader = NetworkReaderPool.GetReader(reader.buffer);
			newreader.Position = reader.Position;

			if(!MessagePacking.Unpack(newreader, out var key, out _)) return;
			if(NetworkServer.handlers.TryGetValue(key, out var networkMessageDelegate) && networkMessageDelegate.Method.DeclaringType.IsGenericType)
			{
				string methodName = networkMessageDelegate.Method.DeclaringType.GetGenericArguments()[0].Name;
				if(methodName == "CommandMessage") return;
				if(DevTools.Instance.Config.DisabledLoggingNetworkMessages.Contains(methodName)) return;
				Log.Debug($"[Receiving: {methodName}]");
			}
			NetworkReaderPool.Recycle(newreader);
		}
	}
}