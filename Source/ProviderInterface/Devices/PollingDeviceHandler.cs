﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Subscriptions;
using HidWizards.IOWrapper.ProviderInterface.Updates;

namespace HidWizards.IOWrapper.ProviderInterface.Devices
{
    public abstract class PollingDeviceHandler<TUpdate, TProcessorKey> : IDisposable
    {
        private Thread _pollThread;
        protected IDeviceUpdateHandler<TUpdate> DeviceUpdateHandler;
        protected SubscriptionHandler SubHandler;
        protected DeviceDescriptor DeviceDescriptor;

        protected PollingDeviceHandler(DeviceDescriptor deviceDescriptor)
        {
            DeviceDescriptor = deviceDescriptor;
        }

        public PollingDeviceHandler<TUpdate, TProcessorKey> Initialize(EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler)
        {
            SubHandler = new SubscriptionHandler(DeviceDescriptor, deviceEmptyHandler);
            DeviceUpdateHandler = CreateUpdateHandler(DeviceDescriptor, SubHandler, bindModeHandler);

            _pollThread = new Thread(PollThread);
            _pollThread.Start();

            return this;
        }

        public bool IsEmpty()
        {
            return SubHandler.Count() == 0;
        }

        public void SetDetectionMode(DetectionMode detectionMode)
        {
            DeviceUpdateHandler.SetDetectionMode(detectionMode);
        }

        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            SubHandler.Subscribe(subReq);
        }

        public void UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            SubHandler.Unsubscribe(subReq);
        }

        protected abstract IDeviceUpdateHandler<TUpdate> CreateUpdateHandler(DeviceDescriptor deviceDescriptor, SubscriptionHandler subscriptionHandler, EventHandler<BindModeUpdate> bindModeHandler);

        protected abstract void PollThread();


        public void Dispose()
        {
            _pollThread.Abort();
            _pollThread.Join();
            _pollThread = null;
        }
    }
}
