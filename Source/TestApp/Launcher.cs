﻿using HidWizards.IOWrapper.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestApp.Wrappers;
using HidWizards.IOWrapper.DataTransferObjects;
using TestApp.Testers;

namespace TestApp
{
    class Launcher
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("DBGVIEWCLEAR");
            var inputList = IOW.Instance.GetInputList();
            var outputList = IOW.Instance.GetOutputList();

            //var bindModeTester = new BindModeTester();

            //var vigemDs4OutputTester = new VigemDs4OutputTester();

            #region DI
            //var vj1 = new VJoyTester(1, false);
            //var vj2 = new VJoyTester(2, false);
            //var genericStick_1 = new GenericDiTester("T16K", Library.Devices.DirectInput.T16000M);
            #endregion

            //var xInputPad_1 = new XiTester(1);

            #region Interception

            //var interceptionKeyboardInputTester = new InterceptionKeyboardInputTester();
            //var interceptionMouseInputTester = new InterceptionMouseInputTester();

            //var interceptionMouseOutputTester = new InterceptionMouseOutputTester();
            //var interceptionKeyboardOutputTester = new InterceptionKeyboardOutputTester();

            #endregion

            Console.WriteLine("Load Complete");
            Console.ReadLine();
            IOW.Instance.Dispose();
        }

    }
}

