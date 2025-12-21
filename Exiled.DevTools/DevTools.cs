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
		public static DevTools Instance { get; private set; }
		public Harmony Harmony { get; private set; }

        private readonly List<(IExiledEvent @event, Delegate handler)> _DynamicHandlers = new();
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
                    if (propertyInfo.GetValue(null) is not IExiledEvent @event)
                    {
                        Log.Warn("Properety find inside the events class but is not an event: " + propertyInfo.Name);
                        continue;
                    }

                    if (@event is Event simpleEvent)
					{
                        // No idea if you can do a cast like (CustomEventHandler)MessageHandlerForEmptyArgs
                        CustomEventHandler customEvent = MessageHandlerForEmptyArgs;
                        simpleEvent.Subscribe(customEvent);
						handler = customEvent;
                    }
					else if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Event<>))
					{
                        // Need to use reflection, no non-genreic interface existe to subsribe non-generic event handler
                        handler = typeof(DevTools)
                            .GetMethod(nameof(DevTools.MessageHandler))
                            .MakeGenericMethod(propertyInfo.PropertyType.GenericTypeArguments)
                            .CreateDelegate(typeof(CustomEventHandler<>)
                            .MakeGenericType(propertyInfo.PropertyType.GenericTypeArguments));

                        MethodInfo subscribeMethod = propertyInfo.PropertyType.GetMethod(
							nameof(Event</*dummy type*/int>.Subscribe),
							new Type[] { handler.GetType() });

                        subscribeMethod.Invoke(@event, new[] { handler });
					}
					else
					{
						Log.Warn("Unknow type of event: " + propertyInfo.Name);
						continue;
					}

					_DynamicHandlers.Add((@event, handler));
				}

            isHandlerAdded = true;
		}

		private void RemoveEventHandlers()
		{
			if(!isHandlerAdded) return;

            for (int i = 0; i < _DynamicHandlers.Count; i++)
			{
                (IExiledEvent @event, Delegate handler) = _DynamicHandlers[i];

                if (@event is Event simpleEvent)
				{
					simpleEvent.Unsubscribe(handler as CustomEventHandler);
				}
                else
                {
                    MethodInfo unsubscribeMethod = @event.GetType().GetMethod(
                           nameof(Event</*dummy type*/int>.Unsubscribe),
                           new Type[] { handler.GetType() });
                    unsubscribeMethod.Invoke(@event, new[] { handler });
				}
				_DynamicHandlers.RemoveAt(i);
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

                    if (!Instance.Config.LoggingClassNameToNest.Contains(targetType.FullName)) continue;

                    if (Instance.Config.LoggingIenumerables
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

					if(targetType.IsClass || (targetType.IsValueType && !targetType.IsPrimitive && !targetType.IsEnum))
					{
						foreach(PropertyInfo propertyInClass in targetType.GetProperties(_NestSearchFlags))
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

						foreach(FieldInfo fieldInClass in targetType.GetFields(_NestSearchFlags))
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

		public static void MessageHandlerForEmptyArgs() => Log.Debug($"[    Event: {new StackFrame(3).GetMethod().Name}]");
	}
}