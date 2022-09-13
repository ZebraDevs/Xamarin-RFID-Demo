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
    public class ZebraReaderInterface: IDisposable
    {


        private Readers _availableReaderContainer;
        private RFIDReader _reader;
        private List<MEMORY_BANK> memoryBanksToRead = new List<MEMORY_BANK>();

        public ZebraReaderInterface()
        {
        }

        public bool IsConnected { get { return _reader?.IsConnected ?? false; } }

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

            if (_reader != null && _reader.IsConnected)
                return success;

            if (_availableReaderContainer == null)
                _availableReaderContainer = new Readers(Android.App.Application.Context, ENUM_TRANSPORT.All);

            try
            {
                if (_availableReaderContainer != null)
                {
                    if (_availableReaderContainer.AvailableRFIDReaderList?.Count > 0)
                    {
                        if (_reader == null)
                        {
                            // Get first reader from list
                            var readerDevice = _availableReaderContainer.AvailableRFIDReaderList[0];
                            _reader = readerDevice.RFIDReader;

                            // Connect
                            _reader.Connect();
                            if (_reader.IsConnected)
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
                if (_reader != null)
                    _reader.Disconnect();
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
            _reader.Config.StartTrigger = triggerInfo.StartTrigger;
            _reader.Config.StopTrigger = triggerInfo.StopTrigger;

            // Add event handler
            var readerEventsHandler = new ReaderEventsHandler(_reader, memoryBanksToRead);
            readerEventsHandler.ReaderOutputNotification = ReaderOutputNotification; // Notify the user
            readerEventsHandler.ReaderTagDataEventOutput = ReaderTagDataEventOutput;

            _reader.Events.AddEventsListener(readerEventsHandler);
            _reader.Events.SetHandheldEvent(true);
            _reader.Events.SetTagReadEvent(true);
            _reader.Events.SetAttachTagDataWithReadEvent(false);

            // Force RFID mode

            _reader.Config.SetTriggerMode(ENUM_TRIGGER_MODE.RfidMode, true);
        }

        public void Dispose()
        {
            if(_reader != null)
            {
                _reader.Disconnect();
                _reader.Dispose();
            }
        }
    }
}
