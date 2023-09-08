using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events;
using Exiled.Events.EventArgs.Interfaces;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.Features;
using HarmonyLib;
using Utf8Json.Internal;

namespace DevTools
{
	public sealed class DevTools : Plugin<Config>
	{
		public override string Name => "DevTools";
		public override string Author => "sanyae2439";
		public override string Prefix => "devtools";
		public override PluginPriority Priority => PluginPriority.Highest;
		public override Version Version => new Version(Assembly.GetName().Version.Major, Assembly.GetName().Version.Minor, Assembly.GetName().Version.Build);
		public override Version RequiredExiledVersion => new Version(8, 2, 0);

		public static DevTools Instance { get; private set; }
		public Harmony Harmony { get; private set; }

        private readonly List<Tuple<EventInfo, Delegate>> _DynamicHandlers = new List<Tuple<EventInfo, Delegate>>();
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
			foreach (var eventClass in EventsAssembly.Assembly.GetTypes().Where(x => x.Namespace == "Exiled.Events.Handlers"))
				foreach (PropertyInfo propertyInfo in eventClass.GetAllProperties())
				{
					Delegate handler = null;
					EventInfo eventInfo = propertyInfo.PropertyType.GetEvent("InnerEvent", (BindingFlags)(-1));

					if (propertyInfo.PropertyType == typeof(Event))
					{
						handler = new CustomEventHandler(MessageHandlerForEmptyArgs);

						MethodInfo addMethod = eventInfo.DeclaringType.GetMethod($"add_{eventInfo.Name}", BindingFlags.Instance | BindingFlags.NonPublic);
						addMethod.Invoke(propertyInfo.GetValue(null), new object[] { handler });
					}
					else if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Event<>))
					{
						handler = typeof(DevTools)
							.GetMethod(nameof(DevTools.MessageHandler))
							.MakeGenericMethod(eventInfo.EventHandlerType.GenericTypeArguments)
							.CreateDelegate(typeof(CustomEventHandler<>)
							.MakeGenericType(eventInfo.EventHandlerType.GenericTypeArguments));

						MethodInfo addMethod = eventInfo.GetAddMethod(true);
						addMethod.Invoke(propertyInfo.GetValue(null), new[] { handler });
					}
					else
					{
						Log.Warn(propertyInfo.Name);
						continue;
					}

					_DynamicHandlers.Add(new Tuple<EventInfo, Delegate>(eventInfo, handler));
				}

            isHandlerAdded = true;
		}

		private void RemoveEventHandlers()
		{
			if(!isHandlerAdded) return;

            for (int i = 0; i < _DynamicHandlers.Count; i++)
			{
				Tuple<EventInfo, Delegate> tuple = _DynamicHandlers[i];
                EventInfo eventInfo = tuple.Item1;
                Delegate handler = tuple.Item2;

                if (eventInfo.DeclaringType != null)
				{
					MethodInfo removeMethod = eventInfo.DeclaringType.GetMethod($"remove_{eventInfo.Name}", BindingFlags.Instance | BindingFlags.NonPublic);
					removeMethod.Invoke(null, new object[] { handler });
				}
				else
				{
					MethodInfo removeMethod = eventInfo.GetRemoveMethod(true);
					removeMethod.Invoke(null, new[] { handler });
				}
				_DynamicHandlers.Remove(tuple);
            }
			isHandlerAdded = false;
		}

		private void RegistPatch()
		{
			try
			{
				Harmony = new Harmony(Name + DateTime.Now.Ticks);
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

			if(Instance.Config.PreventingEventName.Contains(eventname) && ev is IDeniableEvent evdeniable)
			{
				Log.Warn($" [  Prevent: {eventname}]");
				evdeniable.IsAllowed = false;
			}

			if(Instance.Config.DisabledLoggingEvents.Contains(eventname)) 
				return;

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

					if(Instance.Config.LoggingIenumerables
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

					if(!Instance.Config.LoggingClassNameToNest.Contains(targetType.FullName)) continue;

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