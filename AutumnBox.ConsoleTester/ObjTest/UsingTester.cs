﻿/* =============================================================================*\
*
* Filename: UsingTester
* Description: 
*
* Version: 1.0
* Created: 2017/11/1 18:56:00 (UTC+8:00)
* Compiler: Visual Studio 2017
* 
* Author: zsh2401
* Company: I am free man
*
\* =============================================================================*/
using AutumnBox.Shared.CstmDebug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutumnBox.ConsoleTester.ObjTest
{
    public class UsingTester : IDisposable
    {
        public void Dispose()
        {
            Logger.D("Dispose!");
        }
        public static void StartTest()
        {
            using (UsingTester t = new UsingTester())
            {
                return;
            }
        }
    }
}