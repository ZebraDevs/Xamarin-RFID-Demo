# Xamarin-RFID-Demo
Sample Xamarin app showing how to interact with our Zebra handled RFID readers.
It includes the possibility to choose the Memory Banks to be read by using the operation sequence.

## Prerequisites
- Visual Studio 2019 or above with Xamarin component installed.
- A Zebra handled RFID reader.

## Supported Readers
- RFD40XX
- RFD90XX
- MC33XXR
- RFD2000
- RFD8500

## Examples

### Getting Started 
The full documentation is available on our Zebra TechDocs: https://techdocs.zebra.com/dcs/rfid/xamarin/2-0-2-94/guide/about/

### Code snippets

- Connect to reader
```csharp
 // Get available readers (you can choose the connection method)
var availableReaderContainer = new Readers(Android.App.Application.Context, ENUM_TRANSPORT.All);
// Get first reader from list
var readerDevice = availableReaderContainer.AvailableRFIDReaderList[0];
reader = readerDevice.RFIDReader;
// Connect
reader.Connect();
```

- Configure connected reader
```csharp
// Setup triggers
TriggerInfo triggerInfo = new TriggerInfo();
triggerInfo.StartTrigger.TriggerType = START_TRIGGER_TYPE.StartTriggerTypeImmediate;
triggerInfo.StopTrigger.TriggerType = STOP_TRIGGER_TYPE.StopTriggerTypeImmediate;
reader.Config.StartTrigger = triggerInfo.StartTrigger;
reader.Config.StopTrigger = triggerInfo.StopTrigger;

// Add event handlers
var readerEventsHandler = new ReaderEventsHandler(reader, memoryBanksToRead);
reader.Events.AddEventsListener(readerEventsHandler);
reader.Events.SetHandheldEvent(true);
reader.Events.SetTagReadEvent(true);
reader.Events.SetAttachTagDataWithReadEvent(false);

// Force RFID mode
reader.Config.SetTriggerMode(ENUM_TRIGGER_MODE.RfidMode, true);
```

- Handle reader events class
```csharp
internal class ReaderEventsHandler : Java.Lang.Object, IRfidEventsListener
{
    // Trigger actions (start/stop reading)
    public void EventStatusNotify(RfidStatusEvents rfidStatusEvents)
    {
      ...
    }
    
    // Read event
    public void EventReadNotify(RfidReadEvents e)
    {
      ...
    }
}
```

- Setup Operation sequence to read specific Memory banks when trigger is received
```csharp
public void EventStatusNotify(RfidStatusEvents rfidStatusEvents)
{
     if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.HandheldTriggerEvent)
     {
         // Add tag sequence to read specific memory banks
         foreach (MEMORY_BANK bank in memoryBanksToRead)
         {
             TagAccess ta = new TagAccess();
             TagAccess.Sequence sequence = new TagAccess.Sequence(ta, ta);
             TagAccess.Sequence.Operation op = new TagAccess.Sequence.Operation(sequence);
             op.AccessOperationCode = ACCESS_OPERATION_CODE.AccessOperationRead;
             op.ReadAccessParams.MemoryBank = bank ?? throw new ArgumentNullException(nameof(bank));
             reader.Actions.TagAccess.OperationSequence.Add(op);
         }
         reader.Actions.TagAccess.OperationSequence.PerformSequence();
     }
     if (rfidStatusEvents.StatusEventData.HandheldTriggerEventData.HandheldEvent == HANDHELD_TRIGGER_EVENT_TYPE.HandheldTriggerReleased)
     {
        // STOP
        ...
     }
}
```

- Handle Read event and get Memory Banks data.
```csharp
public void EventReadNotify(RfidReadEvents e)
{
    var readTags = reader.Actions.GetReadTags(100);
    if (readTags != null)
    {
        // Group tag by tag id
        // Each tag might have several TagData (one per each Memory Bank read)
        var readTagsList = readTags.ToList();
        var tagReadGroup = readTagsList.GroupBy(x => x.TagID).ToDictionary(grp => grp.Key);

        foreach (var tagKey in tagReadGroup.Keys)
        {
            // Get tag data.
            var tagValueList = tagReadGroup[tagKey];
            var myTag = new TagReadData();
            foreach (TagData tagData in tagValueList)
            {
                if (tagData.OpCode == ACCESS_OPERATION_CODE.AccessOperationRead && tagData.OpStatus == ACCESS_OPERATION_STATUS.AccessSuccess)
                {
                    if (tagData.MemoryBankData.Length == 0)
                        continue;

                    // Parse MemoryBankData
                    if (tagData.MemoryBank.Ordinal == MEMORY_BANK.MemoryBankEpc.Ordinal)
                        myTag.EPC = tagData.MemoryBankData;
                    else if (tagData.MemoryBank.Ordinal == MEMORY_BANK.MemoryBankTid.Ordinal)
                        myTag.TID = tagData.MemoryBankData;
                    else if (tagData.MemoryBank.Ordinal == MEMORY_BANK.MemoryBankUser.Ordinal)
                        myTag.UMB = tagData.MemoryBankData;
                }
            }
        }
    }
}
```

## Contributing
This project welcomes contributions. Pleae check out our Contributing guide to learn more on how to get started.

## License
[Zebra EULA](ZEBRA_EULA_LICENSE.md) 
