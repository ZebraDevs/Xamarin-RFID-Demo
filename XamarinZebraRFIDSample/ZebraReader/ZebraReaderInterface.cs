using Com.Zebra.Rfid.Api3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XamarinZebraRFIDSample.Model;
using XamarinZebraRFIDSample.ZebraReader;

namespace XamarinZebraRFIDSample.ZebraReader
{
    public class ZebraReaderInterface : IDisposable
    {


        private Readers _availableReaderContainer;
        private RFIDReader reader;
        private List<MEMORY_BANK> memoryBanksToRead = new List<MEMORY_BANK>();

        public ZebraReaderInterface()
        {
        }

        public bool IsConnected { get { return reader?.IsConnected ?? false; } }

        public EventHandler<string> ReaderOutputNotification { get; set; }
        public EventHandler<TagReadData> ReaderTagDataEventOutput { get; set; }

        public void SetMemoryBankRead(MEMORY_BANK memoryBank, bool active)
        {
            if (active)
                memoryBanksToRead.Add(memoryBank);
            else
                memoryBanksToRead.Remove(memoryBank);
        }

        public bool ConnectReader()
        {
            bool success = true;

            if (reader != null && reader.IsConnected)
                return success;

            _availableReaderContainer = new Readers(Android.App.Application.Context, ENUM_TRANSPORT.All);

            try
            {
                if (_availableReaderContainer != null)
                {
                    if (_availableReaderContainer.AvailableRFIDReaderList?.Count > 0)
                    {
                        if (reader == null)
                        {
                            // Get first reader from list
                            var readerDevice = _availableReaderContainer.AvailableRFIDReaderList[0];
                            reader = readerDevice.RFIDReader;

                            // Connect
                            reader.Connect();
                            if (reader.IsConnected)
                            {
                                ReaderOutputNotification?.Invoke(this, "Reader connected");
                                ConfigureReader();
                            }
                        }
                    }
                }
            }
            catch (InvalidUsageException e)
            {
                ReaderOutputNotification?.Invoke(this, "InvalidUsageException " + e.VendorMessage);
                success = false;
            }
            catch (OperationFailureException e)
            {
                ReaderOutputNotification?.Invoke(this, "OperationFailureException " + e.VendorMessage);
                success = false;
            }
            catch (Exception ex)
            {
                ReaderOutputNotification?.Invoke(this, "Generic Exception " + ex.Message);
                success = false;
            }

            return success;
        }

        public bool DisconnectReader()
        {
            bool success = true;
            try
            {
                if (reader != null)
                {
                    reader.Disconnect();
                    reader = null;
                }
            }
            catch (Exception ex)
            {
                ReaderOutputNotification?.Invoke(this, "Generic Exception " + ex.Message);
                success = false;
            }

            return success;
        }

        private void ConfigureReader()
        {
            // Setup triggers
            TriggerInfo triggerInfo = new TriggerInfo();
            triggerInfo.StartTrigger.TriggerType = START_TRIGGER_TYPE.StartTriggerTypeImmediate;
            triggerInfo.StopTrigger.TriggerType = STOP_TRIGGER_TYPE.StopTriggerTypeImmediate;
            reader.Config.StartTrigger = triggerInfo.StartTrigger;
            reader.Config.StopTrigger = triggerInfo.StopTrigger;

            // Add event handler
            var readerEventsHandler = new ReaderEventsHandler(reader, memoryBanksToRead);
            readerEventsHandler.ReaderOutputNotification = ReaderOutputNotification; // Notify the user
            readerEventsHandler.ReaderTagDataEventOutput = ReaderTagDataEventOutput;

            reader.Events.AddEventsListener(readerEventsHandler);
            reader.Events.SetHandheldEvent(true);
            reader.Events.SetTagReadEvent(true);
            reader.Events.SetAttachTagDataWithReadEvent(false);

            // Force RFID mode

            reader.Config.SetTriggerMode(ENUM_TRIGGER_MODE.RfidMode, true);
        }

        public void Dispose()
        {
            if (reader != null)
            {
                this.DisconnectReader();
            }
        }
    }
}
