﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Harvester.Core;
using Harvester.Forms;

/* Copyright (c) 2012 CBaxter
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
 * IN THE SOFTWARE. 
 */

namespace Harvester
{
    internal static class Program
    {
        [STAThread]
        internal static void Main()
        {
            Thread.CurrentThread.Name = "Main";
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => ShowFatalException((Exception)e.ExceptionObject);
            
            Boolean onlyInstance;
            using (SystemMonitor.CreateSingleInstance(out onlyInstance))
            {
                if (onlyInstance)
                {
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.EnableVisualStyles();
                    Application.Run(new Main());
                }
                else
                {
                    MessageBox.Show(Localization.DebuggerAlreadyActive, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SystemMonitor.ShowExistingInstance();
                    Application.Exit();
                }
            }
        }

        private static void ShowFatalException(Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}