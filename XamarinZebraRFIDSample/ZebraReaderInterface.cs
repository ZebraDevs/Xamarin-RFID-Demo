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
using static Com.Zebra.Rfid.Api3.TagAccess;
using System.Threading;
using System.Xml;

namespace XamarinZebraRFIDSample
{
    public class ZebraReaderInterface : Java.Lang.Object, IRfidEventsListener
    {

        private static Readers readers;
        private static IList<ReaderDevice> availableRFIDReaderList;
        private static ReaderDevice readerDevice;
        private static RFIDReader Reader;
        private EventHandler<string> readerOutputEvent;

        public ZebraReaderInterface(EventHandler<string> readerOutputEvent)
        {
            this.readerOutputEvent = readerOutputEvent;
        }

        public void InitReader()
        {
            if (readers == null)
            {
                readers = new Readers(Android.App.Application.Context, ENUM_TRANSPORT.ServiceSerial);
            }
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    if (readers != null && readers.AvailableRFIDReaderList != null)
                    {
                        availableRFIDReaderList = readers.AvailableRFIDReaderList;
                        if (availableRFIDReaderList.Count > 0)
                        {
                            if (Reader == null)
                            {
                                // get first reader from list
                                readerDevice = availableRFIDReaderList[0];
                                Reader = readerDevice.RFIDReader;
                                // Establish connection to the RFID Reader
                                Reader.Connect();
                                if (Reader.IsConnected)
                                {
                                    readerOutputEvent.Invoke(this, "Reader connected");
                                    ConfigureReader();
                                }
                            }
                        }
                    }
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                    readerOutputEvent.Invoke(this, "InvalidUsageException " + e.VendorMessage);
                }
                catch (OperationFailureException e)
                {
                    e.PrintStackTrace();
                    readerOutputEvent.Invoke(this, "OperationFailureException " + e.VendorMessage);
                }
            });
        }

        private void ConfigureReader()
        {
            if (Reader.IsConnected)
            {
                TriggerInfo triggerInfo = new TriggerInfo();
                triggerInfo.StartTrigger.TriggerType = START_TRIGGER_TYPE.StartTriggerTypeImmediate;
                triggerInfo.StopTrigger.TriggerType = STOP_TRIGGER_TYPE.StopTriggerTypeImmediate;
                try
                {
                    // receive events from reader
                    /*if (eventHandler == null)
                    {
                        eventHandler = new EventHandler(Reader, this);
                    }
                    Reader.Events.AddEventsListener(eventHandler);*/
                    Reader.Events.AddEventsListener(this);
                    // HH event
                    Reader.Events.SetHandheldEvent(true);
                    // tag event with tag data
                    Reader.Events.SetTagReadEvent(true);
                    Reader.Events.SetAttachTagDataWithReadEvent(false);
                    // set trigger mode as rfid so scanner beam will not come
                    Reader.Config.SetTriggerMode(ENUM_TRIGGER_MODE.RfidMode, true);
                    // set start and stop triggers
                    Reader.Config.StartTrigger = triggerInfo.StartTrigger;
                    Reader.Config.StopTrigger = triggerInfo.StopTrigger;
                }
                catch (InvalidUsageException e)
                {
                    e.PrintStackTrace();
                }
                catch (OperationFailureException e)
                {
                    e.PrintStackTrace();
                }
            }
        }

        // Read Event Notification	
        public void EventReadNotify(RfidReadEvents e)
        {
            TagData[] myTags = Reader.Actions.GetReadTags(1);
            if (myTags != null)
            {
                for (int index = 0; index < myTags.Length; index++)
                {
                    if (myTags[index].OpCode == ACCESS_OPERATION_CODE.AccessOperationRead &&
                    myTags[index].OpStatus == ACCESS_OPERATION_STATUS.AccessSuccess)
                    {

                        if (myTags[index].MemoryBankData.Length > 0)
                        {
                            readerOutputEvent.Invoke(this, "Mem Bank Data " + myTags[index].MemoryBankData);
                        }
                    }
                }
            }
            Reader.Actions.TagAccess.OperationSequence.StopSequence();

        }
        // Status Event Notification
        public void EventStatusNotify(RfidStatusEvents rfidStatusEvents)
        {
            readerOutputEvent.Invoke(this, "Status Notification: " + rfidStatusEvents.StatusEventData.StatusEventType);
            if (rfidStatusEvents.StatusEventData.StatusEventType == STATUS_EVENT_TYPE.HandheldTriggerEvent)
            {
                if (rfidStatusEvents.StatusEventData.HandheldTriggerEventData.HandheldEvent ==
                        HANDHELD_TRIGGER_EVENT_TYPE.HandheldTriggerPressed)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            try
                            {
                                Reader.Config.DPOState = DYNAMIC_POWER_OPTIMIZATION.Disable;
                                MEMORY_BANK[] banks = new MEMORY_BANK[] {
                                        MEMORY_BANK.MemoryBankEpc,
                                        MEMORY_BANK.MemoryBankTid,
                                        MEMORY_BANK.MemoryBankUser
                                };
                                foreach (MEMORY_BANK bank in banks)
                                {
                                    TagAccess ta = new TagAccess();
                                    TagAccess.Sequence sequence = new TagAccess.Sequence(ta, ta);
                                    TagAccess.Sequence.Operation op = new TagAccess.Sequence.Operation(sequence);
                                    op.AccessOperationCode = ACCESS_OPERATION_CODE.AccessOperationRead;
                                    op.ReadAccessParams.MemoryBank = bank;
                                    Reader.Actions.TagAccess.OperationSequence.Add(op);
                                }

                                Reader.Actions.TagAccess.OperationSequence.PerformSequence();
                            }
                            catch (InvalidUsageException e)
                            {
                                e.PrintStackTrace();
                                readerOutputEvent.Invoke(this, "InvalidUsageException " + e.VendorMessage);
                            }
                            catch (OperationFailureException e)
                            {
                                e.PrintStackTrace();
                                readerOutputEvent.Invoke(this, "OperationFailureException " + e.VendorMessage);
                            }
                        });
                    });

                }
                if (rfidStatusEvents.StatusEventData.HandheldTriggerEventData.HandheldEvent ==
                        HANDHELD_TRIGGER_EVENT_TYPE.HandheldTriggerReleased)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            Reader.Actions.TagAccess.OperationSequence.StopSequence();
                        }
                        catch (InvalidUsageException e)
                        {
                            e.PrintStackTrace();
                            readerOutputEvent.Invoke(this, "InvalidUsageException " + e.VendorMessage);
                        }
                        catch (OperationFailureException e)
                        {
                            e.PrintStackTrace();
                            readerOutputEvent.Invoke(this, "OperationFailureException " + e.VendorMessage);
                        }
                    });
                }
            }
        }
    }
}
