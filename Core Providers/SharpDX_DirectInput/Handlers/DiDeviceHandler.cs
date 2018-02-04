﻿using System;
using System.Collections.Concurrent;
using Providers;
using Providers.Handlers;
using SharpDX.DirectInput;

namespace SharpDX_DirectInput
{
    class DiDeviceHandler
    {
        private Joystick joystick;

        private ConcurrentDictionary<BindingType,
            ConcurrentDictionary<int, BindingHandler>> _bindingDictionary
            = new ConcurrentDictionary<BindingType, ConcurrentDictionary<int, BindingHandler>>();

        public DiDeviceHandler(InputSubscriptionRequest subReq)
        {
            joystick = new Joystick(DiHandler.DiInstance, Lookups.DeviceHandleToInstanceGuid("VID_044F&PID_B10A"));
            joystick.Properties.BufferSize = 128;
            joystick.Acquire();

        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            var bindingType = subReq.BindingDescriptor.Type;
            var dict = _bindingDictionary
                .GetOrAdd(subReq.BindingDescriptor.Type,
                    new ConcurrentDictionary<int, BindingHandler>());

            switch (bindingType)
            {
                case BindingType.Axis:
                    return dict
                        .GetOrAdd((int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index], new DiAxisBindingHandler())
                        .Subscribe(subReq);
                case BindingType.Button:
                    return dict
                        .GetOrAdd((int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index], new DiButtonBindingHandler())
                        .Subscribe(subReq);
                case BindingType.POV:
                    return dict
                        .GetOrAdd((int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index], new DiPovBindingHandler())
                        .Subscribe(subReq);
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            return true;
        }

        public void Poll()
        {
            JoystickUpdate[] data = joystick.GetBufferedData();
            foreach (var state in data)
            {
                int offset = (int)state.Offset;
                var bindingType = Lookups.OffsetToType(state.Offset);
                if (_bindingDictionary.ContainsKey(bindingType) && _bindingDictionary[bindingType].ContainsKey(offset))
                {
                    _bindingDictionary[bindingType][offset].Poll(state.Value);
                }
            }
        }
    }
}