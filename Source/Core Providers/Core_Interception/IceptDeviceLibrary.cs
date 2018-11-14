﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core_Interception.Lib;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using Hidwizards.IOWrapper.Libraries.HidDeviceHelper;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception
{
    public class IceptDeviceLibrary : IInputOutputDeviceLibrary<int>
    {
        private readonly ProviderDescriptor _providerDescriptor;
        private readonly IntPtr _deviceContext;
        private Dictionary<string, List<int>> _deviceHandleToId;
        private List<DeviceReport> _deviceReports;
        private static DeviceReportNode _keyboardList;
        private static DeviceReportNode _mouseButtonList;
        private static readonly DeviceReportNode MouseAxisList = new DeviceReportNode
        {
            Title = "Axes",
            Bindings = new List<BindingReport>
            {
                new BindingReport
                {
                    Title = "X",
                    Category = BindingCategory.Delta,
                    BindingDescriptor =   new BindingDescriptor
                    {
                        Index = 0,
                        Type = BindingType.Axis
                    }
                },
                new BindingReport
                {
                    Title = "Y",
                    Category = BindingCategory.Delta,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = 1,
                        Type = BindingType.Axis
                    }
                }
            }
        };
        private static readonly List<string> MouseButtonNames = new List<string> { "Left Mouse", "Right Mouse", "Middle Mouse", "Side Button 1", "Side Button 2", "Wheel Up", "Wheel Down", "Wheel Left", "Wheel Right" };
        private ProviderReport _providerReport;

        public IceptDeviceLibrary(ProviderDescriptor providerDescriptor)
        {
            _providerDescriptor = providerDescriptor;
            _deviceContext = ManagedWrapper.CreateContext();
            RefreshConnectedDevices();
        }

        private int GetDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            if (_deviceHandleToId.ContainsKey(deviceDescriptor.DeviceHandle))
            {
                if (_deviceHandleToId[deviceDescriptor.DeviceHandle].Count >= deviceDescriptor.DeviceInstance)
                {
                    return _deviceHandleToId[deviceDescriptor.DeviceHandle][deviceDescriptor.DeviceInstance];
                }
            }

            throw new ArgumentOutOfRangeException($"Unknown Device: {deviceDescriptor}");
        }

        public int GetInputDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            return GetDeviceIdentifier(deviceDescriptor);
        }

        public ProviderReport GetInputList()
        {
            return _providerReport;
        }

        private DeviceReport GetDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            foreach (var deviceReport in _deviceReports)
            {
                if (deviceReport.DeviceDescriptor.DeviceHandle == deviceDescriptor.DeviceHandle && deviceReport.DeviceDescriptor.DeviceInstance == deviceDescriptor.DeviceInstance)
                {
                    return deviceReport;
                }
            }
            return null;
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return GetDeviceReport(deviceDescriptor);
        }

        public void RefreshConnectedDevices()
        {
            _deviceHandleToId = new Dictionary<string, List<int>>();
            _deviceReports = new List<DeviceReport>();

            UpdateKeyList();
            UpdateMouseButtonList();
            string handle;

            for (var i = 1; i < 11; i++)
            {
                if (ManagedWrapper.IsKeyboard(i) != 1) continue;
                handle = ManagedWrapper.GetHardwareStr(_deviceContext, i, 1000);
                if (handle == "") continue;
                int vid = 0, pid = 0;
                GetVidPid(handle, ref vid, ref pid);
                var name = "";
                if (vid != 0 && pid != 0)
                {
                    name = DeviceHelper.GetDeviceName(vid, pid);
                }

                if (name == "")
                {
                    name = handle;
                }

                handle = $@"Keyboard\{handle}";

                if (!_deviceHandleToId.ContainsKey(handle))
                {
                    _deviceHandleToId.Add(handle, new List<int>());
                }

                var instance = _deviceHandleToId[handle].Count;
                _deviceHandleToId[handle].Add(i - 1);

                name = $"K: {name}";
                if (instance > 0) name += $" #{instance + 1}";

                _deviceReports.Add(new DeviceReport
                {
                    DeviceName = name,
                    DeviceDescriptor = new DeviceDescriptor
                    {
                        DeviceHandle = handle,
                        DeviceInstance = instance
                    },
                    Nodes = new List<DeviceReportNode>
                    {
                        _keyboardList
                    }
                });
                //Log(String.Format("{0} (Keyboard) = VID: {1}, PID: {2}, Name: {3}", i, vid, pid, name));

                _providerReport = new ProviderReport
                {
                    Title = "Interception (Core)",
                    Description = "Supports per-device Keyboard and Mouse Input/Output, with blocking\nRequires custom driver from http://oblita.com/interception",
                    API = "Interception",
                    ProviderDescriptor = _providerDescriptor,
                    Devices = _deviceReports
                };

            }

            for (var i = 11; i < 21; i++)
            {
                if (ManagedWrapper.IsMouse(i) != 1) continue;
                handle = ManagedWrapper.GetHardwareStr(_deviceContext, i, 1000);
                if (handle == "") continue;
                int vid = 0, pid = 0;
                GetVidPid(handle, ref vid, ref pid);
                var name = "";
                if (vid != 0 && pid != 0)
                {
                    name = DeviceHelper.GetDeviceName(vid, pid);
                }

                if (name == "")
                {
                    name = handle;
                }

                handle = $@"Mouse\{handle}";

                if (!_deviceHandleToId.ContainsKey(handle))
                {
                    _deviceHandleToId.Add(handle, new List<int>());
                }

                var instance = _deviceHandleToId[handle].Count;
                _deviceHandleToId[handle].Add(i - 1);

                name = $"M: {name}";
                if (instance > 0) name += $" #{instance + 1}";

                _deviceReports.Add(new DeviceReport
                {
                    DeviceName = name,
                    DeviceDescriptor = new DeviceDescriptor
                    {
                        DeviceHandle = handle,
                        DeviceInstance = instance
                    },
                    Nodes = new List<DeviceReportNode>
                    {
                        _mouseButtonList,
                        MouseAxisList
                    }
                });
                //Log(String.Format("{0} (Mouse) = VID/PID: {1}", i, handle));
                //Log(String.Format("{0} (Mouse) = VID: {1}, PID: {2}, Name: {3}", i, vid, pid, name));
            }

        }

        private static void UpdateMouseButtonList()
        {
            _mouseButtonList = new DeviceReportNode
            {
                Title = "Buttons"
            };
            for (var i = 0; i < 5; i++)
            {
                _mouseButtonList.Bindings.Add(new BindingReport
                {
                    Title = MouseButtonNames[i],
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        Type = BindingType.Button
                    }
                });
            }

            for (var i = 5; i < 9; i++)
            {
                _mouseButtonList.Bindings.Add(new BindingReport
                {
                    Title = MouseButtonNames[i],
                    Category = BindingCategory.Event,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        Type = BindingType.Button
                    }
                });
            }

        }

        private void UpdateKeyList()
        {
            _keyboardList = new DeviceReportNode
            {
                Title = "Keys"
            };
            //buttonNames = new Dictionary<int, string>();
            var sb = new StringBuilder(260);

            for (var i = 0; i < 256; i++)
            {
                var lParam = (uint)(i + 1) << 16;
                if (ManagedWrapper.GetKeyNameTextW(lParam, sb, 260) == 0)
                {
                    continue;
                }
                var keyName = sb.ToString().Trim();
                if (keyName == "")
                    continue;
                //Log("Button Index: {0}, name: '{1}'", i, keyName);
                _keyboardList.Bindings.Add(new BindingReport
                {
                    Title = keyName,
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        Type = BindingType.Button
                    }
                });
                //buttonNames.Add(i, keyName);

                // Check if this button has an extended (Right) variant
                lParam = (0x100 | ((uint)i + 1 & 0xff)) << 16;
                if (ManagedWrapper.GetKeyNameTextW(lParam, sb, 260) == 0)
                {
                    continue;
                }
                var altKeyName = sb.ToString().Trim();
                if (altKeyName == "" || altKeyName == keyName)
                    continue;
                //Log("ALT Button Index: {0}, name: '{1}'", i + 256, altKeyName);
                _keyboardList.Bindings.Add(new BindingReport
                {
                    Title = altKeyName,
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i + 256,
                        Type = BindingType.Button
                    }
                });
                //Log("Button Index: {0}, name: '{1}'", i + 256, altKeyName);
                //buttonNames.Add(i + 256, altKeyName);
            }
            _keyboardList.Bindings.Sort((x, y) => string.Compare(x.Title, y.Title, StringComparison.Ordinal));
        }

        private static void GetVidPid(string str, ref int vid, ref int pid)
        {
            var matches = Regex.Matches(str, @"VID_(\w{4})&PID_(\w{4})");
            if ((matches.Count <= 0) || (matches[0].Groups.Count <= 1)) return;
            vid = Convert.ToInt32(matches[0].Groups[1].Value, 16);
            pid = Convert.ToInt32(matches[0].Groups[2].Value, 16);
        }

        public int GetOutputDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            return GetDeviceIdentifier(deviceDescriptor);
        }

        public ProviderReport GetOutputList()
        {
            return _providerReport;
        }

        public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return GetDeviceReport(deviceDescriptor);
        }
    }
}
