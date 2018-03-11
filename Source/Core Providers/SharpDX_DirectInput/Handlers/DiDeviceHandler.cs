﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX.DirectInput;
using SharpDX_DirectInput.Helpers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Helpers;

namespace SharpDX_DirectInput.Handlers
{
    internal class DiDeviceHandler : DeviceHandler
    {
        //private Joystick _joystick;
        private readonly Guid _instanceGuid = Guid.Empty;

        public DiDeviceHandler(DeviceDescriptor deviceDescriptor) : base(deviceDescriptor)
        {
            //Guid instanceGuid = Guid.Empty;
            var instances = Lookups.GetDeviceOrders(deviceDescriptor.DeviceHandle);
            if (instances.Count >= deviceDescriptor.DeviceInstance)
            {
                _instanceGuid = instances[deviceDescriptor.DeviceInstance];
            }

            if (_instanceGuid == Guid.Empty)
            {
                throw new Exception($"DeviceHandle '{deviceDescriptor.DeviceHandle}' was not found");
            }
        }

        protected override int GetBindingKey(InputSubscriptionRequest subReq)
        {
            return (int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index];
        }

        protected override BindingHandler CreateBindingHandler(InputSubscriptionRequest subReq)
        {
            switch (subReq.BindingDescriptor.Type)
            {
                case BindingType.Axis:
                    return new DiAxisBindingHandler(subReq);
                case BindingType.Button:
                    return new DiButtonBindingHandler(subReq);
                case BindingType.POV:
                    return new DiPovBindingHandler(subReq);
                default:
                    throw new NotImplementedException();
            }
        }

        protected override DevicePoller CreateDevicePoller(DetectionMode mode)
        {
            switch (mode)
            {
                case DetectionMode.Bind:
                    return new DiDevicePoller(_deviceDescriptor, ProcessBindModePoll);
                case DetectionMode.Subscription:
                    return new DiDevicePoller(_deviceDescriptor, ProcessSubscriptionModePoll);
                default:
                    throw new NotImplementedException();
            }
        }

        public override void ProcessSubscriptionModePoll(DevicePollUpdate update)
        {
            var bindingType = update.Type;
            var offset = update.Index;
            if (BindingDictionary.ContainsKey(bindingType) && BindingDictionary[bindingType].ContainsKey(offset))
            {
                BindingDictionary[bindingType][offset].Poll(update.State);
            }
        }

        public override void ProcessBindModePoll(DevicePollUpdate update)
        {
            if (update.Type == BindingType.POV)
            {
                if (update.State == -1) return;
                for (var i = 0; i < POVHelper.PovDirections.Count; i++)
                {
                    if (!Lookups.ValueMatchesAngle(POVHelper.PovDirections[i], update.State)) continue;

                    _bindModeCallback(_deviceDescriptor, new BindingDescriptor { Type = update.Type, Index = update.Index, SubIndex = i }, 1);
                    _bindModeCallback(_deviceDescriptor, new BindingDescriptor { Type = update.Type, Index = update.Index, SubIndex = i }, 0);
                }
            }
            else
            {
                var value = Lookups.InputConversionFuncs[update.Type](update.State);
                _bindModeCallback(_deviceDescriptor, new BindingDescriptor { Type = update.Type, Index = update.Index, SubIndex = update.SubIndex }, value);
            }
        }

        public override void Poll()
        {
            //// ToDo: Pollthread should not be spamming here if joystick is not attached


            //JoystickUpdate[] data;
            //// ToDo: Find better way of detecting unplug. DiHandler.DiInstance.IsDeviceAttached(instanceGuid) kills performance
            //try
            //{
            //    // Try / catch seems the only way for now to ensure no crashes on replug
            //    data = _joystick.GetBufferedData();
            //}
            //catch
            //{
            //    return;
            //}
            //foreach (var state in data)
            //{
            //    int offset = (int)state.Offset;
            //    var bindingType = Lookups.OffsetToType(state.Offset);
            //    if (BindingDictionary.ContainsKey(bindingType) && BindingDictionary[bindingType].ContainsKey(offset))
            //    {
            //        BindingDictionary[bindingType][offset].Poll(state.Value);
            //    }
            //}
        }

        public override void Dispose()
        {
            _devicePoller.SetPollThreadState(false);
            //try
            //{
            //    _joystick.Unacquire();
            //    _joystick.Dispose();
            //}
            //catch
            //{

            //}
            //_joystick = null;
            base.Dispose();
        }
    }
}