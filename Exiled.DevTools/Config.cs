using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace DevTools
{
	public class Config : IConfig
	{
		[Description("Indicates whether the plugin is enabled or not")]
		public bool IsEnabled { get; set; } = true;

		[Description("Indicates whether logging events args")]
		public bool LoggingEventArgs { get; set; } = true;

		[Description("Indicates whether logging IEnumerables")]
		public bool LoggingIenumerables { get; set; } = true;

		[Description("Indicates whether logging network methods")]
		public bool LoggingNetworkMethods { get; set; } = true;

		[Description("Indicates whether logging network messages")]
		public bool LoggingNetworkMessages { get; set; } = true;

		[Description("Ignored event names for logging")]
		public List<string> DisabledLoggingEvents { get; set; } = new List<string>() { 
			"UsingRadioBattery"
		};

		[Description("Ignored network method names(Rpc,Cmd,Target) for logging")]
		public List<string> DisabledLoggingNetworkMethods { get; set; } = new List<string>() { 
			"TargetReplyEncrypted",
			"TargetSyncGameplayData",
			"CmdSendEncryptedQuery",
			"CmdSetTime", 
			"CmdUpdateCameraPosition", 
			"RpcUpdateCameraPostion" 
		};

		[Description("Ignored NetworkMessage names for logging")]
		public List<string> DisabledLoggingNetworkMessages { get; set; } = new List<string>() {
			"SpawnMessage",
			"ObjectDestroyMessage",
			"NetworkPingMessage",
			"PositionMessage",
			"PositionMessage2D",
			"PositionPPMMessage",
			"RotationMessage"
		};

		[Description("Class name to nest logging. (Must be Fullname)")]
		public List<string> LoggingClassNameToNest { get; set; } = new List<string>();

		[Description("Event name to preventing.")]
		public List<string> PreventingEventName { get; set; } = new List<string>();
	}
}