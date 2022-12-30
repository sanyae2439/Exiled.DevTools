# Exiled.DevTools
Logging tools for __developers__.

Required [EXILED](https://github.com/Exiled-Team/EXILED) 3.0.0+.

# Configs
```yaml
devtools:
# Indicates whether the plugin is enabled or not
  is_enabled: true
  # Indicates whether logging events args
  Debug: true
  # Indicate if you want to see Debug log
  logging_event_args: true
  # Indicates whether logging IEnumerables
  logging_ienumerables: true
  # Indicates whether logging network methods
  logging_network_methods: true
  # Indicates whether logging network messages
  logging_network_messages: true
  # Ignored event names for logging
  disabled_logging_events:
  - UsingRadioBattery
  - Transmitting
  - SpawningItem
  # Ignored network method names(Rpc,Cmd,Target) for logging
  disabled_logging_network_methods:
  - TargetReplyEncrypted
  - TargetSyncGameplayData
  - CmdSendEncryptedQuery
  - CmdSetTime
  - CmdUpdateCameraPosition
  - RpcUpdateCameraPostion
  - RpcMakeSound
  # Ignored NetworkMessage names for logging
  disabled_logging_network_messages:
  - SpawnMessage
  - ObjectDestroyMessage
  - NetworkPingMessage
  - PositionMessage
  - PositionMessage2D
  - PositionPPMMessage
  - RotationMessage
  - FpcPositionMessage
  - FpcFromClientMessage
  - SubroutineMessage
  - StatMessage
  - VoiceMessage
  - TransmitterPositionMessage
  - ElevatorSyncMsg
  - FpcOverrideMessage

  # Class name to nest logging. (Must be Fullname)
  logging_class_name_to_nest: []
  # Event name to preventing.
  preventing_event_name: []
```
