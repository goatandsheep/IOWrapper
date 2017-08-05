﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginContracts;

namespace IOWrapper
{
    public enum InputTypes { Button, Axis };

    public class IOController
    {
        Dictionary<string, IPlugin> _Plugins;

        public IOController()
        {
            GenericMEFPluginLoader<IPlugin> loader = new GenericMEFPluginLoader<IPlugin>("Plugins");
            _Plugins = new Dictionary<string, IPlugin>();
            IEnumerable<IPlugin> plugins = loader.Plugins;
            foreach (var item in plugins)
            {
                _Plugins[item.PluginName] = item;
            }
        }

        public SortedDictionary<string, ProviderReport> GetInputList()
        {
            var list = new SortedDictionary<string, ProviderReport>();
            foreach (var plugin in _Plugins.Values)
            {
                var report = plugin.GetInputList();
                if (report != null)
                {
                    list.Add(plugin.PluginName, report);
                }
            }
            return list;
        }

        public Guid? SubscribeButton(string pluginName, string deviceHandle, uint buttonId, dynamic callback)
        {
            var subReq = new SubscriptionRequest()
            {
                PluginName = pluginName,
                InputType = InputType.BUTTON,
                DeviceHandle = deviceHandle,
                InputIndex = buttonId,
                Callback = callback
            };
            return GetPlugin(pluginName).SubscribeButton(subReq);
        }

        public bool UnsubscribeButton(string pluginName, Guid subscriptionGuid)
        {
            return GetPlugin(pluginName).UnsubscribeButton(subscriptionGuid);
        }

        public Guid? SubscribeOutputDevice(string pluginName, string deviceHandle)
        {
            var subReq = new SubscriptionRequest()
            {
                PluginName = pluginName,
                InputType = InputType.BUTTON,
                DeviceHandle = deviceHandle
            };
            return GetPlugin(pluginName).SubscribeOutputDevice(subReq);
        }

        //public bool SetOutputButton(string pluginName, string deviceHandle, uint button, bool state)
        public bool SetOutputButton(string pluginName, Guid deviceSubscription, uint button, bool state)
        {
            return GetPlugin(pluginName).SetOutputButton(deviceSubscription, button, state);
        }

        private IPlugin GetPlugin(string pluginName)
        {
            if (_Plugins.ContainsKey(pluginName))
            {
                return _Plugins[pluginName];
            }
            else
            {
                return null;
            }
        }
    }
}
