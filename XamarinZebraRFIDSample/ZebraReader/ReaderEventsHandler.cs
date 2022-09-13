using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Zebra.Rfid.Api3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamarinZebraRFIDSample.Model;

namespace XamarinZebraRFIDSample.ZebraReader
{
    internal class ReaderEventsHandler : Java.Lang.Object, IRfidEventsListener
    {
        private readonly RFIDReader reader;
        private readonly List<MEMORY_BANK> memoryBanksToRead;


        public ReaderEventsHandler(RFIDReader reader, List<MEMORY_BANK> memoryBanksToRead)
        {
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.memoryBanksToRead = memoryBanksToRead ?? throw new ArgumentNullException(nameof(memoryBanksToRead));
        }

        public EventHandler<string> ReaderOutputNotification { get; set; }
        public EventHandler<TagReadData> ReaderTagDataEventOutput { get; set; }

        public void EventReadNotify(RfidReadEvents e)
        {
            var readTagsList = reader.Actions.GetReadTags(100).DefaultIfEmpty().ToList();
            if (readTagsList.Any())
            {
                var tagReadGroup = readTagsList.GroupBy(x => x.TagID).ToDictionary(grp => grp.Key);
                foreach (var tagKey in tagReadGroup.Keys)
                {
                    var tagValueList = tagReadGroup[tagKey];
                    var myTag = new TagReadData();
                    foreach (TagData tagData in tagValueList)
                    {
                        if (tagData.OpCode == ACCESS_OPERATION_CODE.AccessOperationRead && tagData.OpStatus == ACCESS_OPERATION_STATUS.AccessSuccess)
                        {
                            if (tagData.MemoryBankData.Length == 0)
                                continue;

                            if (tagData.MemoryBank.Ordinal == MEMORY_BANK.MemoryBankEpc.Ordinal)
                                myTag.EPC = tagData.MemoryBankData;
                            else if (tagData.MemoryBank.Ordinal == MEMORY_BANK.MemoryBankTid.Ordinal)
                                myTag.TID = tagData.MemoryBankData;
                            else if (tagData.MemoryBank.Ordinal == MEMORY_BANK.MemoryBankUser.Ordinal)
                                myTag.UMB = tagData.MemoryBankData;
                        }
                    }

                    ReaderTagDataEventOutput.Invoke(this, myTag);
                }
            }
        }

        // Status Event Notification
        public void EventStatusNotify(RfidStatusEvents rfidStatusEvents)
        {
            if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.HandheldTriggerEvent)
            {
                if (rfidStatusEvents.StatusEventData.HandheldTriggerEventData.HandheldEvent == HANDHELD_TRIGGER_EVENT_TYPE.HandheldTriggerPressed)
                {
                    ReaderOutputNotification?.Invoke(this, "Start Reading");
                    Task.Run(() => StartReading());
                }
                if (rfidStatusEvents.StatusEventData.HandheldTriggerEventData.HandheldEvent == HANDHELD_TRIGGER_EVENT_TYPE.HandheldTriggerReleased)
                {
                    ReaderOutputNotification?.Invoke(this, "Stop Reading");
                    Task.Run(() => StopReading());
                }
            }
        }

        private bool StartReading()
        {
            try
            {
                reader.Config.DPOState = DYNAMIC_POWER_OPTIMIZATION.Disable;

                // Add tag sequence to read each memory as configured.
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

                return true;
            }
            catch (Exception ex)
            {
                ReaderOutputNotification?.Invoke(this, "Exception " + ex.Message);
                return false;
            }
        }

        private bool StopReading()
        {
            try
            {
                reader.Actions.TagAccess.OperationSequence.StopSequence();
                return true;
            }
            catch (Exception ex)
            {
                ReaderOutputNotification?.Invoke(this, "Exception " + ex.Message);
                return false;
            }
        }
    }
}