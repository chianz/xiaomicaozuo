﻿/*************************************************
** auth： zsh2401@163.com
** date:  2018/1/29 8:48:54 (UTC +8:00)
** desc： ...
*************************************************/
using AutumnBox.Basic.Device;
using AutumnBox.Basic.Util;
using AutumnBox.Support.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutumnBox.Basic.ACP
{
    [Obsolete("Project-ACP is dead")]
    internal sealed class AcpCommunicator : IDisposable
    {
        private static readonly List<AcpCommunicator> communicators;
        static AcpCommunicator()
        {
            communicators = new List<AcpCommunicator>();
        }
        public static void CloseAll()
        {
            communicators.ForEach((c) =>
            {
                c.Dispose();
            });
        }
        public static AcpCommunicator GetAcpCommunicator(DeviceSerialNumber device)
        {
            var communicator = communicators.Find((c) =>
            {
                return c.device.Equals(device);
            });
            return communicator ?? new AcpCommunicator(device);
        }


        private readonly Object _locker = new object();
        private readonly DeviceSerialNumber device;
        private Socket socket;
        private AcpCommunicator(DeviceSerialNumber device, bool connectAfterCreate = true)
        {
            this.device = device;
            if (connectAfterCreate) { Connect(); }
            communicators.Add(this);
        }
        public void Connect()
        {
            AndroidClientController.AwakeAcpService(device);
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(GetEndPoint(device));
            Logger.Info(this,$"{device} communicator connected");
        }
        public void Close()
        {
            try {
                socket.Close();
                socket.Dispose();
            } catch { }
            AndroidClientController.StopAcpService(device);
            Logger.Info(this,$"{device} communicator disconnected");
        }
        public AcpResponse SendCommand(String baseCommand, params string[] args)
        {
            lock (_locker)
            {
                var builder = new AcpCommand.Builder
                {
                    BaseCommand = baseCommand,
                    Args = args
                };
                return AcpFlow(builder.ToCommand());
            }
        }
        public AcpResponse SendCommand(AcpCommand command)
        {
            lock (_locker)
            {
                return AcpFlow(command);
            }
        }
        private AcpResponse AcpFlow(AcpCommand command)
        {
            AliveCheck();
            byte fCode = Acp.FCODE_NO_RESPONSE;
            byte[] dataBuffer = new byte[0];
            try
            {
                AcpStandardProcess.Send(socket, command.ToBytes());
                AcpStandardProcess.Receive(socket, out fCode, out dataBuffer);
            }
            catch (AcpTimeOutException)
            {
                Logger.Info(this,"timeout..");
                fCode = Acp.FCODE_TIMEOUT;
            }
            catch (SocketException e)
            {
                Logger.Warn(this,$"acp socket exception {e.ToString()}");
                fCode = Acp.FCODE_NO_RESPONSE;
            }
            catch (IOException e)
            {
                Logger.Warn(this,$"acp exception(client reading) {e}");
                fCode = Acp.FCODE_ERR_WITH_EX;
                dataBuffer = Encoding.UTF8.GetBytes(e.ToString() + " " + e.Message);
            }
            return new AcpResponse()
            {
                FirstCode = fCode,
                Data = dataBuffer,
            };

        }
        public void AliveCheck(bool tryReconnnect = true)
        {
            if (IsAlive()) return;
            if (tryReconnnect)
            {
                Logger.Info(this,$"{device} disconnected..reconnecting...");
                Connect();
            };
            if (IsAlive())
            {
                Logger.Info(this,$"{device} reconnected...");
                return;
            }
            Logger.Warn(this,$"{device} connection lost....");
            throw new AcpConnectionLostException();
        }
        public bool IsAlive()
        {
            try
            {
                AcpStandardProcess.Send(socket, Encoding.UTF8.GetBytes(Acp.CMD_TEST));
                AcpStandardProcess.Receive(socket, out byte fCode, out byte[] data, 5000);
                return fCode == Acp.FCODE_SUCCESS;
            }
            catch (SocketException e)
            {
                Logger.Warn(this,"a exception happend on test ACPService on android is running",e);
                return false;
            }
            catch (IOException e)
            {
                Logger.Warn(this,"a exception happend on test ACPService on android is running", e);
                return false;
            }
            catch (ObjectDisposedException e) {
                Logger.Warn("a exception happend on test ACPService on android is running", e);
                return false;
            }
        }
        private static IPEndPoint GetEndPoint(DeviceSerialNumber serial)
        {
            return new IPEndPoint(IPAddress.Parse("127.0.0.1"),
                ForwardManager.GetForwardByRemotePort(serial, Acp.STD_PORT));
        }
        public void Dispose()
        {
            try
            {
                Close();
                socket.Dispose();
                communicators.Remove(this);
            }
            catch { }
        }
        ~AcpCommunicator()
        {
            Dispose();
        }
    }
}
