using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events;
using Exiled.Events.EventArgs.Interfaces;
using HarmonyLib;

namespace DevTools
{
	public sealed class DevTools : Plugin<Config>
	{
		public override string Name => "DevTools";
		public override string Author => "sanyae2439";
		public override string Prefix => "devtools";
		public override PluginPriority Priority => PluginPriority.Highest;
		public override Version Version => new Version(Assembly.GetName().Version.Major, Assembly.GetName().Version.Minor, Assembly.GetName().Version.Build);
		public override Version RequiredExiledVersion => new Version(7, 0, 0);

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
			RemoveEventHandlers();
			UnRegistPatch();

			Instance = null;

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
							.MakeGenericMethod(eventInfo.EventHandlerType.GenericTypeArguments)
							.CreateDelegate(typeof(Events.CustomEventHandler<>).MakeGenericType(eventInfo.EventHandlerType.GenericTypeArguments));
					else
						handler = typeof(DevTools)
							.GetMethod(nameof(DevTools.MessageHandlerForEmptyArgs))
							.CreateDelegate(typeof(Events.CustomEventHandler));
					eventInfo.AddEventHandler(null, handler);
					_DynamicHandlers.Add(eventInfo, handler);
				}

			isHandlerAdded = true;
		}

		private void RemoveEventHandlers()
		{
			if(!isHandlerAdded) return;

			foreach(var eventClass in Events.Instance.Assembly.GetTypes().Where(x => x.Namespace == "Exiled.Events.Handlers"))
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

		public static void MessageHandler<T>(T ev) where T : IExiledEvent
		{
			Type eventType = ev.GetType();
			string eventname = eventType.Name.Replace("EventArgs", string.Empty);		

			if(Instance.Config.PreventingEventName.Contains(eventname))
			{
				Log.Warn($" [  Prevent: {eventname}]");
				eventType.GetProperty("IsAllowed").SetValue(ev, false);
			}

			if(Instance.Config.DisabledLoggingEvents.Contains(eventname)) return;

			string message = $"[    Event: {eventname}]\n";
			if(Instance.Config.LoggingEventArgs)
			{
				foreach(var propertyInfo in ev.GetType().GetProperties())
				{
					try
					{
						message += $"  {propertyInfo.Name} : {propertyInfo.GetValue(ev) ?? "null"}\n";
					}
					catch(Exception e)
					{
						message += $"  {propertyInfo.Name} : Error[{e.Message}]\n";
					}

					Type targetType = propertyInfo.GetValue(ev)?.GetType();
					if(targetType == null) continue;

					if(DevTools.Instance.Config.LoggingIenumerables
						&& propertyInfo.PropertyType.GetInterfaces().Any(t => t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
						&& targetType.Name != nameof(System.String))
					{
						int counter = 0;
						foreach(var item in (IEnumerable)propertyInfo.GetValue(ev))
							message += $"    [{counter++}] : {item ?? "null"}\n";

						if(counter == 0)
							message += $"    No items\n";

						continue;
					}

					if(!DevTools.Instance.Config.LoggingClassNameToNest.Contains(targetType.FullName)) continue;

					if(targetType.IsClass || (targetType.IsValueType && !targetType.IsPrimitive && !targetType.IsEnum))
					{
						foreach(var propertyInClass in targetType.GetProperties(_NestSearchFlags))
						{
							if(propertyInClass.GetIndexParameters().Length > 0) continue;

							try
							{
								message += $"    {propertyInClass.Name} : {propertyInClass.GetValue(propertyInfo.GetValue(ev)) ?? "null"}\n";
							}
							catch(Exception e)
							{
								message += $"    {propertyInClass.Name} : Exception({e.Message})\n";
							}
						}

						foreach(var fieldInClass in targetType.GetFields(_NestSearchFlags))
						{
							if(fieldInClass.Name.Contains("<")) continue;

							try
							{
								message += $"    {fieldInClass.Name} : {fieldInClass.GetValue(propertyInfo.GetValue(ev)) ?? "null"}\n";
							}
							catch(Exception e)
							{
								message += $"    {fieldInClass.Name} : Exception({e.Message})\n";
							}
						}
					}
				}
			}
			Log.Debug(message.TrimEnd('\n'));
		}

		public static void MessageHandlerForEmptyArgs() => Log.Debug($"[    Event: {new StackFrame(2).GetMethod().Name}]");
	}
}