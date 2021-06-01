# Exiled.DevTools
Logging tools for __developers__.

Required [EXILED](https://github.com/Exiled-Team/EXILED) 2.10.0+.

# Configs
```yaml
exiled_devtools:
  # Indicates whether the plugin is enabled or not
  is_enabled: true
  # Indicates whether logging events args
  logging_event_args: true
  # Ignored event names for logging
  disabled_logging_events:
  - SyncingData
  - Blinking
  - ChangingDurability
  - UsingRadioBattery
  # Ignored network method names(Rpc,Cmd,Target) for logging
  disabled_logging_network_methods:
  - RpcBlinkTime
  - CmdAltIsActive
  - CmdSyncItem
  - CmdSyncData
  - CmdChangeSpeedState
  - CmdScp939Noise
  # Ignored nest output for logging (Must be Fullname
  disabled_logging_class_name_for_nest:
  - Exiled.API.Features.Player
```
