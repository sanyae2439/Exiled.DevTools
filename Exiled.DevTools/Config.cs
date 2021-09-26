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

		[Description("Indicates whether logging network methods")]
		public bool LoggingNetworkMethods { get; set; } = true;

		[Description("Ignored event names for logging")]
		public List<string> DisabledLoggingEvents { get; set; } = new List<string>() { 
			"SyncingData", 
			"UsingRadioBattery" 
		};

		[Description("Ignored network method names(Rpc,Cmd,Target,Message) for logging")]
		public List<string> DisabledLoggingNetworkMethods { get; set; } = new List<string>() { 
			"SpawnMessage", 
			"ObjectDestroyMessage", 
			"NetworkPingMessage", 
			"PositionMessage", 
			"PositionMessage2D", 
			"RotationMessage", 
			"TargetReplyEncrypted",
			"TargetSyncGameplayData",
			"CmdSendEncryptedQuery",
			"CmdSetTime", 
			"CmdUpdateCameraPosition", 
			"RpcUpdateCameraPostion" 
		};

		[Description("Class name to nest logging. (Must be Fullname)")]
		public List<string> LoggingClassNameToNest { get; set; } = new List<string>();
	}
}