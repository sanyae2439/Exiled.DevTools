# Exiled.DevTools
Logging tools for __developers__.

Required [EXILED](https://github.com/Exiled-Team/EXILED) 8.8.0+.

# Configs
```yaml
# Indicates whether the plugin is enabled or not
is_enabled: true
# Indicates whether the debug message is enabled or not
debug: true
# Indicates whether logging events args
logging_event_args: true
# Indicates whether logging IEnumerables
logging_ienumerables: true
# Indicates whether logging network methods
logging_network_methods: true
# Indicates whether logging network messages
logging_network_messages: true
# Ignored event names for logging
disabled_logging_events:
- 'UsingRadioBattery'
- 'UsingRadioPickupBattery'
- 'Transmitting'
- 'SpawningItem'
# Ignored network method names(Rpc,Cmd,Target) for logging
disabled_logging_network_methods:
- 'TargetReplyEncrypted'
- 'TargetSyncGameplayData'
- 'CmdSendEncryptedQuery'
# Ignored NetworkMessage names for logging
disabled_logging_network_messages:
- 'SpawnMessage'
- 'ObjectDestroyMessage'
- 'NetworkPingMessage'
- 'NetworkPongMessage'
- 'FpcFromClientMessage'
- 'SubroutineMessage'
- 'StatMessage'
- 'VoiceMessage'
- 'TransmitterPositionMessage'
- 'ElevatorSyncMsg'
- 'FpcOverrideMessage'
- 'TimeSnapshotMessage'
- 'EntityStateMessage'
- 'FpcPositionMessage'
- 'EncryptedMessageOutside'
# Class name to nest logging. (Must be Fullname)
logging_class_name_to_nest:
- 'Byte[]'
# Event name to preventing.
preventing_event_name: []
```
