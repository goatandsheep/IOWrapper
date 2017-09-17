﻿using Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    class TestApp
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("DBGVIEWCLEAR");
            var t = new Tester();
            Console.ReadLine();
            t.Dispose();
        }
    }
}

class Tester
{
    private OutputSubscriptionRequest vJoyOutputSubReq;
    private OutputSubscriptionRequest interceptionKeyboardOutputSubReq;
    private OutputSubscriptionRequest interceptionMouseOutputSubReq;
    bool defaultProfileState = false;
    Guid defaultProfileGuid = Guid.NewGuid();
    IOWrapper.IOController iow;

    public Tester()
    {
        iow = new IOWrapper.IOController();
        var inputList = iow.GetInputList();
        var outputList = iow.GetOutputList();
        string inputHandle = null;
        bool ret;

        // Get handle to 1st DirectInput device
        string outputHandle = null;
        try { inputHandle = inputList["SharpDX_DirectInput"].Devices.FirstOrDefault().Key; }
        catch { return; }
        //inputHandle = "VID_1234&PID_BEAD/0";    // vJoy
        //inputHandle = "VID_0C45&PID_7403/0";   // XBox
        //inputHandle = "VID_054C&PID_09CC/0";   // DS4
        //inputHandle = "VID_044F&PID_B10A/0";   // T.16000M

        // Get handle to 1st vJoy device
        try { outputHandle = outputList["Core_vJoyInterfaceWrap"].Devices.FirstOrDefault().Key; }
        catch { return; }

        // Get handle to Keyboard
        string keyboardHandle = null;
        try { keyboardHandle = inputList["Core_Interception"].Devices.FirstOrDefault().Key; }
        catch { return; }
        //keyboardHandle = @"Keyboard\HID\VID_04F2&PID_0112&REV_0103&MI_00";

        string mouseHandle = null;
        mouseHandle = @"Mouse\HID\VID_046D&PID_C531&REV_2100&MI_00";

        ToggleDefaultProfileState();

        // Acquire vJoy stick
        vJoyOutputSubReq = new OutputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "Core_vJoyInterfaceWrap",
            DeviceHandle = outputHandle
        };
        iow.SubscribeOutput(vJoyOutputSubReq);

        #region DirectInput
        Console.WriteLine("Binding input to handle " + inputHandle);
        // Subscribe to the found stick
        var diSub1 = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_DirectInput",
            Type = BindingType.BUTTON,
            DeviceHandle = inputHandle,
            Index = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, BindingType.BUTTON, 0, value);
                //iow.SetOutputstate(interceptionKeyboardOutputSubReq, InputType.BUTTON, 311, value); // Right Alt
                //iow.SetOutputstate(interceptionMouseOutputSubReq, InputType.BUTTON, 1, value); // RMB
            })
        };
        iow.SubscribeInput(diSub1);

        Console.WriteLine("Binding input to handle " + inputHandle);
        // Subscribe to the found stick
        var diSub2 = new InputSubscriptionRequest()
        {
            ProfileGuid = Guid.NewGuid(),
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_DirectInput",
            Type = BindingType.BUTTON,
            DeviceHandle = inputHandle,
            Index = 1,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 1 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, BindingType.BUTTON, 0, value);
                if (value == 1)
                {
                    ToggleDefaultProfileState();
                }
            })
        };
        iow.SubscribeInput(diSub2);
        iow.SetProfileState(diSub2.ProfileGuid, true);

        var sub2 = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = defaultProfileGuid,
            ProviderName = "SharpDX_DirectInput",
            Type = BindingType.AXIS,
            DeviceHandle = inputHandle,
            Index = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Axis 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, BindingType.AXIS, 0, value);
            })
        };
        iow.SubscribeInput(sub2);
        //iow.UnsubscribeInput(sub2);
        //iow.SubscribeInput(sub2);
        #endregion

        #region XInput
        var xinputAxis = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_XInput",
            Type = BindingType.AXIS,
            DeviceHandle = "0",
            Index = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Axis 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, BindingType.AXIS, 0, value);
            })
        };
        iow.SubscribeInput(xinputAxis);


        var xinputButton = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_XInput",
            Type = BindingType.BUTTON,
            DeviceHandle = "0",
            Index = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Button 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, BindingType.BUTTON, 1, value);
            })
        };
        ret = iow.SubscribeInput(xinputButton);
        #endregion

        #region Interception
        interceptionKeyboardOutputSubReq = new OutputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "Core_Interception",
            DeviceHandle = keyboardHandle
        };
        iow.SubscribeOutput(interceptionKeyboardOutputSubReq);
        //iow.UnsubscribeOutput(interceptionKeyboardOutputSubReq);

        interceptionMouseOutputSubReq = new OutputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "Core_Interception",
            DeviceHandle = mouseHandle
        };
        iow.SubscribeOutput(interceptionKeyboardOutputSubReq);

        /*
        var subInterception = new InputSubscriptionRequest()
        {
            ProfileGuid = Guid.NewGuid(),
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "Core_Interception",
            InputType = InputType.BUTTON,
            //DeviceHandle = keyboardHandle,
            DeviceHandle = mouseHandle,
            //InputIndex = 1, // 1 key on keyboard
            //InputIndex = 311, // Right ALT key on keyboard
            InputIndex = 0, // LMB
            Callback = new Action<int>((value) =>
            {
                //iow.SetOutputstate(interceptionOutputSubReq, InputType.BUTTON, 17, value);
                //iow.SetOutputstate(vJoyOutputSubReq, InputType.BUTTON, 0, value);
                Console.WriteLine("Keyboard Key Value: " + value);
            })
        };
        iow.SubscribeInput(subInterception);
        */
        #endregion

        #region Tobii Eye Tracker
        /*
        var tobiiGazePointSubReq = new InputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "Core_Tobii_Interaction",
            SubProviderName = "GazePoint",
            InputType = InputType.AXIS,
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Tobii Eye Gaxe X: {0}", value);
            })
        };
        iow.SubscribeInput(tobiiGazePointSubReq);

        var tobiiHeadPoseSubReq = new InputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "Core_Tobii_Interaction",
            SubProviderName = "HeadPose",
            InputType = InputType.AXIS,
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Tobii Head Pose X: {0}", value);
            })
        };
        iow.SubscribeInput(tobiiHeadPoseSubReq);
        */
        #endregion
    }

    void ToggleDefaultProfileState()
    {
        defaultProfileState = !defaultProfileState;
        iow.SetProfileState(defaultProfileGuid, defaultProfileState);
    }

    public void Dispose()
    {
        iow.Dispose();
    }
}