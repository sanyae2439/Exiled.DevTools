using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Exiled.DevTools
{
	public class Config : IConfig
	{
		[Description("Indicates whether the plugin is enabled or not")]
		public bool IsEnabled { get; set; } = true;

		[Description("Ignored event names")]
		public List<string> DisabledEvents { get; set; } = new List<string>() { "SyncingData" };

		[Description("Ignored Rpc names")]
		public List<string> DisabledRpcs { get; set; } = new List<string>() { "SyncingData" };
	}
}