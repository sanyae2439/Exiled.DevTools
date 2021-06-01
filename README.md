# Exiled.DevTools
Logging tools for __developers__.

# Configs
```yaml
exiled_devtools:
  # Indicates whether the plugin is enabled or not
  is_enabled: true
  # Ignored event names for logging
  disabled_logging_events:
  - SyncingData
  # Ignored network method names(Rpc,Cmd,Target) for logging
  disabled_logging_network_methods:
  - RpcBlinkTime
  - CmdAltIsActive
  - CmdSyncItem
  - CmdSyncData
  # Ignored nest output for logging (Should be Fullname) (Example:Exiled.API.Features.Player) 
  disabled_logging_class_name_for_nest: []
```
