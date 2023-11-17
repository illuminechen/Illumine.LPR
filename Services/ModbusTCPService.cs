using System;
using NModbus;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using Illumine.LPR;
using System.Threading.Tasks;

public class ModbusTCPService : IDisposable
{
    IPEndPoint _ipep;
    TcpClient _tcpClient;
    IModbusMaster _master;
    bool _connecting = false;
    bool _connected = false;

    enum State { S00, S01, S10, S11 }

    public event EventHandler ActuralEnter;
    public event EventHandler ActuralLeave;
    public void OnActuralEnter()
    {
        ActuralEnter?.Invoke(this, EventArgs.Empty);
    }
    public void OnActuralLeave()
    {
        ActuralLeave?.Invoke(this, EventArgs.Empty);
    }

    private State enterState = 0;
    private State leaveState = 0;

    private bool _SensorEnter0;
    public bool SensorEnter0
    {
        get => _SensorEnter0;
        set
        {
            if (_SensorEnter0 != value)
            {
                if (value && enterState == State.S00)
                {
                    enterState = State.S10;
                    Console.WriteLine(_ipep.Address.ToString() + "Enter,S00-S10");
                }
                if (!value && enterState == State.S11)
                {
                    enterState = State.S01;
                    Console.WriteLine(_ipep.Address.ToString() + "Enter,S11-S01");
                }
                if (!value && enterState == State.S10)
                {
                    enterState = State.S00;
                    Console.WriteLine(_ipep.Address.ToString() + "Enter,S10-S00");
                }
                if (value && enterState == State.S01)
                {
                }
                _SensorEnter0 = value;
            }
        }
    }

    private bool _SensorEnter1;
    public bool SensorEnter1
    {
        get => _SensorEnter1;
        set
        {
            if (_SensorEnter1 != value)
            {
                if (value && enterState == State.S00)
                {
                }
                if (!value && enterState == State.S11)
                {
                    enterState = State.S10;
                    Console.WriteLine(_ipep.Address.ToString() + "Enter,S11-S10");
                }
                if (value && enterState == State.S10)
                {
                    enterState = State.S11;
                    Console.WriteLine(_ipep.Address.ToString() + "Enter,S10-S11");
                }
                if (!value && enterState == State.S01)
                {
                    enterState = State.S00;
                    Console.WriteLine("S01-S00");
                    Console.WriteLine(_ipep.Address.ToString() + "Enter,S01-S00");
                    ActuralEnter?.Invoke(this, EventArgs.Empty);
                }
                _SensorEnter1 = value;
            }
        }
    }

    private bool _SensorLeave0;
    public bool SensorLeave0
    {
        get => _SensorLeave0; set
        {
            if (_SensorLeave0 != value)
            {
                if (value && leaveState == State.S00)
                {
                    leaveState = State.S10;
                    Console.WriteLine(_ipep.Address.ToString() + "Leave,S00-S10");
                }
                if (!value && leaveState == State.S11)
                {
                    leaveState = State.S01;
                    Console.WriteLine(_ipep.Address.ToString() + "Leave,S11-S01");
                }
                if (!value && leaveState == State.S10)
                {
                    leaveState = State.S00;
                    Console.WriteLine(_ipep.Address.ToString() + "Leave,S10-S00");
                }
                if (value && leaveState == State.S01)
                {
                }
                _SensorLeave0 = value;
            }
        }
    }

    private bool _SensorLeave1;
    public bool SensorLeave1
    {
        get => _SensorLeave1;
        set
        {
            if (_SensorLeave1 != value)
            {
                if (value && leaveState == State.S00)
                {
                }
                if (!value && leaveState == State.S11)
                {
                    leaveState = State.S10;
                    Console.WriteLine(_ipep.Address.ToString() + "Leave,S11-S10");
                }
                if (value && leaveState == State.S10)
                {
                    leaveState = State.S11;
                    Console.WriteLine(_ipep.Address.ToString() + "Leave,S10-S11");
                }
                if (!value && leaveState == State.S01)
                {
                    leaveState = State.S00;
                    Console.WriteLine(_ipep.Address.ToString() + "Leave,S01-S00");
                    ActuralLeave?.Invoke(this, EventArgs.Empty);
                }
                _SensorLeave1 = value;
            }
        }
    }

    public Dictionary<EntryMode, bool> Monitoring = new Dictionary<EntryMode, bool>() { { EntryMode.In, false }, { EntryMode.Out, false } };

    public ModbusTCPService(string ip, int port = 502)
    {
        _ipep = new IPEndPoint(IPAddress.Parse(ip), port);
        Connect();
    }

    public void Connect()
    {
        ConnectTimer = new Timer(ConnectHandler, "", 0, 10000);
    }

    public async void ConnectHandler(object o)
    {
        Console.WriteLine(_ipep.Address.ToString() + "Check");

        if (_connecting)
            return;

        if (!_connected)
        {
            _connecting = true;
            try
            {
                var action = new Action(() =>
                {
                    _tcpClient = new TcpClient();
                    _tcpClient.Connect(_ipep);
                    var factory = new ModbusFactory();
                    _master = factory.CreateMaster(_tcpClient);
                    _connected = true;
                });

                Task delaytask = Task.Delay(3000);
                if (await Task.WhenAny(delaytask, Task.Run(action)) == delaytask)
                    throw new Exception("failed");
                delaytask = null;
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex.Message);
            }
            finally
            {
                _connecting = false;
            }
        }
    }

    public void Dispose()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        ConnectTimer?.Dispose();
        _master?.Dispose();
        _tcpClient?.Close();
    }


    public ushort[] Read(ushort address, ushort len)
    {
        // 讀取保持寄存器的資料
        var data = _master.ReadHoldingRegisters(1, address, len);
        // 輸出資料
        return data;
    }

    public void Write(ushort address, ushort[] data)
    {
        if (data.Length == 1)
            _master.WriteSingleRegister(1, address, data[0]);
        else
            _master.WriteMultipleRegisters(1, address, data);
    }

    public void StartMonitor(EntryMode state)
    {
        if (!Monitoring[state])
        {
            Monitoring[state] = true;
            StartMonitoring(state);
        }
    }

    public void StopMonitor(EntryMode state)
    {
        if (Monitoring[state])
        {
            Monitoring[state] = false;
            MonitorTimer[state]?.Dispose();
        }
    }

    private Timer ConnectTimer;
    private Dictionary<EntryMode, Timer> MonitorTimer = new Dictionary<EntryMode, Timer>();

    private void StartMonitoring(EntryMode state)
    {
        // 設定定期讀取的時間間隔，例如每 500 毫秒
        MonitorTimer[state] = new Timer(ReadRegisterValue, state, 0, 500);
    }

    private void ReadRegisterValue(object state)
    {
        Console.WriteLine(_ipep.Address.ToString() + " " + state.ToString());

        if (!(state is EntryMode mode))
            return;
        if (!App.Initialized)
            return;

        if (!_connected || _connecting)
        {
            if (mode == EntryMode.In)
                OnActuralEnter();
            if (mode == EntryMode.Out)
                OnActuralLeave();
            return;
        }
        try
        {
            ushort[] data = Read(41, 1);
            ushort value = data[0];

            // 處理讀取到的註冊器值
            HandleRegisterValueChange(value, mode);
        }
        catch
        {
            _connected = false;
        }
    }

    private void HandleRegisterValueChange(ushort value, EntryMode mode)
    {
        if (mode == EntryMode.In)
        {
            SensorEnter0 = (value & 0x01) == 0x01;
            SensorEnter1 = ((value >> 1) & 0x01) == 0x01;
        }
        if (mode == EntryMode.Out)
        {
            SensorLeave0 = ((value >> 2) & 0x01) == 0x01;
            SensorLeave1 = ((value >> 3) & 0x01) == 0x01;
        }
    }
}