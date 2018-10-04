﻿using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface.Devices
{
    public interface IDeviceLibrary<out TDeviceIdentifier>
    {
        TDeviceIdentifier GetDeviceIdentifier(DeviceDescriptor deviceDescriptor);
        void RefreshConnectedDevices();
    }

    public interface IInputDeviceLibrary<out TDeviceIdentifier> : IDeviceLibrary<TDeviceIdentifier>
    {
        ProviderReport GetInputList();
        DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor);
    }
}
