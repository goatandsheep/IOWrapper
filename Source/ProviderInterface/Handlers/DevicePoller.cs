﻿using System;
using System.Threading;
using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface.Handlers
{
    public abstract class DevicePoller : IDisposable
    {
        protected Thread _pollThread;
        protected readonly DeviceDescriptor _deviceDescriptor;
        private bool _pollThreadState = false;
        protected readonly Action<DevicePollUpdate> _callback;

        protected DevicePoller(DeviceDescriptor deviceDescriptor, Action<DevicePollUpdate> callback)
        {
            _callback = callback;
            _deviceDescriptor = deviceDescriptor;
        }

        public void SetPollThreadState(bool state)
        {
            if (_pollThreadState == state) return;
            if (!_pollThreadState && state)
            {
                _pollThread = new Thread(PollThread);
                _pollThread.Start();
                //Log("Started Poll Thread");
            }
            else if (_pollThreadState && !state)
            {
                _pollThread.Abort();
                _pollThread.Join();
                _pollThread = null;
                //Log("Stopped Poll Thread");
            }

            _pollThreadState = state;
        }

        protected abstract void PollThread();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                SetPollThreadState(false);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class DevicePollUpdate
    {
        public BindingType Type { get; set; }
        public int Index { get; set; }
        public int SubIndex { get; set; }
        public int State { get; set; }
    }
}