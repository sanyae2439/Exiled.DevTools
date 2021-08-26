using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using MonoMod.Utils;

namespace Exiled.DevTools
{
	public sealed class DevTools : Plugin<Config>
	{
		public override string Name => "Exiled.DevTools";
		public override string Author => "sanyae2439";
		public override string Prefix => "exiled_devtools";
		public override PluginPriority Priority => PluginPriority.Highest;
		public override Version Version => new Version(Assembly.GetName().Version.Major, Assembly.GetName().Version.Minor, Assembly.GetName().Version.Build);
		public override Version RequiredExiledVersion => new Version(2, 10, 0);

		public static DevTools Instance { get; private set; }
		public Harmony Harmony { get; private set; }

		private readonly Dictionary<EventInfo, Delegate> _DynamicHandlers = new Dictionary<EventInfo, Delegate>();
		private const BindingFlags _NestSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
		private bool isHandlerAdded = false;

		public override void OnEnabled()
		{
			Instance = this;

			AddEventHandlers();
			RegistPatch();

			base.OnEnabled();
		}

		public override void OnDisabled()
		{
			Instance = null;

			RemoveEventHandlers();
			UnRegistPatch();

			base.OnDisabled();
		}

		private void AddEventHandlers()
		{
			var EventsAssembly = Exiled.Loader.Loader.Plugins.FirstOrDefault(x => x.Name == "Exiled.Events");

			if(EventsAssembly == null)
			{
				Log.Warn($"Exiled.Events not found. Skipping AddEventHandlers.");
				return;
			}

			foreach(var eventClass in EventsAssembly.Assembly.GetTypes().Where(x => x.Namespace == "Exiled.Events.Handlers"))
				foreach(EventInfo eventInfo in eventClass.GetEvents())
				{
					Delegate handler = null;
					if(eventInfo.EventHandlerType.GenericTypeArguments.Any())
						handler = typeof(DevTools)
							.GetMethod(nameof(DevTools.MessageHandler))
							.MakeGenericMethod(eventInfo.EventHandlerType.GenericTypeArguments[0])
							.CreateDelegate(typeof(Events.Events.CustomEventHandler<>).MakeGenericType(eventInfo.EventHandlerType.GenericTypeArguments[0]));
					else
						handler = typeof(DevTools)
							.GetMethod(nameof(DevTools.MessageHandlerForEmptyArgs))
							.CreateDelegate<Events.Events.CustomEventHandler>();
					eventInfo.AddEventHandler(null, handler);
					this._DynamicHandlers.Add(eventInfo, handler);
				}

			isHandlerAdded = true;
		}

		private void RemoveEventHandlers()
		{
			if(!isHandlerAdded) return;

			foreach(var eventClass in Events.Events.Instance.Assembly.GetTypes().Where(x => x.Namespace == "Exiled.Events.Handlers"))
				foreach(EventInfo eventInfo in eventClass.GetEvents())
					if(this._DynamicHandlers.ContainsKey(eventInfo))
					{
						eventInfo.RemoveEventHandler(null, this._DynamicHandlers[eventInfo]);
						this._DynamicHandlers.Remove(eventInfo);
					}

			isHandlerAdded = false;
		}

		private void RegistPatch()
		{
			try
			{
				Harmony = new Harmony(this.Name + DateTime.Now.Ticks);
				Harmony.PatchAll();
			}
			catch(Exception ex)
			{
				Log.Error($"Patching Failed : {ex}");
			}
		}

		private void UnRegistPatch()
		{
			try
			{
				Harmony.UnpatchAll(Harmony.Id);
				Harmony = null;
			}
			catch(Exception ex)
			{
				Log.Error($"Unpatching Failed : {ex}");
			}
		}

		public static void MessageHandler<T>(T ev) where T : EventArgs
		{
			string eventname = ev.GetType().Name.Replace("EventArgs", string.Empty);
			if(Instance.Config.DisabledLoggingEvents.Contains(eventname)) return;

			string message = $"[{eventname}]\n";
			if(Instance.Config.LoggingEventArgs)
			{
				foreach(var propertyInfo in ev.GetType().GetProperties())
				{
					try
					{
						message += $"{propertyInfo.Name} : {propertyInfo.GetValue(ev)}\n";
					}
					catch(Exception e)
					{
						message += $"{propertyInfo.Name} : Error[{e.Message}]\n";
					}

					if(DevTools.Instance.Config.DisabledLoggingClassNameForNest.Contains(propertyInfo.PropertyType.FullName)) continue;

					if(propertyInfo.PropertyType.IsClass || (propertyInfo.PropertyType.IsValueType && !propertyInfo.PropertyType.IsPrimitive && !propertyInfo.PropertyType.IsEnum))
					{

						bool isString = propertyInfo.PropertyType.Name == nameof(System.String);
						bool isEnumerable = propertyInfo.PropertyType.GetInterfaces().Any(t => t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

						if(!isString && !isEnumerable)
						{
							foreach(var propertyInClass in propertyInfo.PropertyType.GetProperties(_NestSearchFlags))
							{
								if(propertyInClass.GetIndexParameters().Length > 0) continue;

								try
								{
									message += $"    {propertyInClass.Name} : {propertyInClass.GetValue(propertyInfo.GetValue(ev))}\n";
								}
								catch(Exception e)
								{
									message += $"    {propertyInClass.Name} : Exception({e.Message})\n";
								}
							}

							foreach(var fieldInClass in propertyInfo.PropertyType.GetFields(_NestSearchFlags))
							{
								if(fieldInClass.Name.Contains("<")) continue;

								try
								{
									message += $"    {fieldInClass.Name} : {fieldInClass.GetValue(propertyInfo.GetValue(ev))}\n";
								}
								catch(Exception e)
								{
									message += $"    {fieldInClass.Name} : Exception({e.Message})\n";
								}
							}
						}

						if(isEnumerable && !isString)
						{
							int counter = 0;
							foreach(var item in (IEnumerable)propertyInfo.GetValue(ev))
								message += $"    [{counter++}] : {item}\n";

							if(counter == 0)
								message += $"    No items\n";
						}
					}
				}
			}
			Log.Debug(message.TrimEnd('\n'));
		}

		public static void MessageHandlerForEmptyArgs(Events.Events.CustomEventHandler _) => Log.Debug($"[{new StackFrame(2).GetMethod().Name}]");
	}
}