﻿/* =============================================================================*\
*
* Filename: AdvanceResult
* Description: 
*
* Version: 1.0
* Created: 2017/11/25 1:05:07 (UTC+8:00)
* Compiler: Visual Studio 2017
* 
* Author: zsh2401
* Company: I am free man
*
\* =============================================================================*/
using AutumnBox.Basic.Executer;
using AutumnBox.Basic.FlowFramework.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutumnBox.Basic.Flows.Result
{
    public class AdvanceResult : FlowResult
    {
        public ShellOutput ShellOutput { get; internal set; }
    }
}