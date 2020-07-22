using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using System.Threading;
using System.IO.Ports;

namespace MidiToSerialbridge
{
    public partial class Form1 : Form
    {
        InputDevice inputDevice = null;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            foreach(var midid in InputDevice.GetAll())
            {
                comboBox1.Items.Add(midid.Name+"("+midid.DriverManufacturer+")");
            }
            foreach (var midid in InputDevice.GetAll())
            {
                comboBox2.Items.Add(midid.Name + "(" + midid.DriverManufacturer + ")");
            }
            foreach(var name in SerialPort.GetPortNames())
            {
                comboBox3.Items.Add(name);
            }
            comboBox1.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;

        }       
        private void OnEventReceived1(object sender, MidiEventReceivedEventArgs e)
        {
            rx.BackColor = Color.Red;
            var midiDevice = (MidiDevice)sender;
            MidiEventToBytesConverter x = new MidiEventToBytesConverter();
            byte[] data = x.Convert(e.Event);
            /*
            List<byte> n = new List<byte>();                       
            //n.Add(0x01);
            //n.Add(Convert.ToByte(data.Length + 1));
            foreach(byte a in data)
            {
                n.Add(0x01);
                n.Add(a);
            }
            //n.AddRange(data);
            //n.Add(calculateCRC(data));
            serialPort1.Write(n.ToArray(), 0, n.Count);*/
            tx.BackColor = Color.LimeGreen;
            serialPort1.Write(data, 0, data.Length);
            
                //textBox1.AppendText($"Event received from '{midiDevice.Name}' at {DateTime.Now}: {e.Event}" + Environment.NewLine);

            
            
        }
        private byte calculateCRC(byte[] data)
        {
            int sum = 0;
            foreach(byte d in data)
            {
                sum = sum + d;
            }
            return (byte)(sum % 255);
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = checkBox1.Checked;
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            textBox1.AppendText(serialPort1.ReadLine() + Environment.NewLine) ;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comboBox3.Text;
                serialPort1.BaudRate = int.Parse(comboBox4.Text);
                serialPort1.Open();
                inputDevice = InputDevice.GetById(comboBox1.SelectedIndex);
                inputDevice.EventReceived += OnEventReceived1;
                inputDevice.StartEventsListening();
                button1.Enabled = false;
                button2.Enabled = true;
                comboBox1.Enabled = false;
                comboBox3.Enabled = false;
                comboBox4.Enabled = false;
                toolStripStatusLabel1.Text ="Sending data from " + inputDevice.Name + " to " + serialPort1.PortName;
                textBox1.AppendText("Midi device opened: "+inputDevice.Name+Environment.NewLine);
                textBox1.AppendText("Serial device opened: " + serialPort1.PortName + " Baud Rate: "+serialPort1.BaudRate.ToString()
                    + Environment.NewLine);
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("COM 포트 열기 실패", "Error");
            }
            catch (MidiDeviceException)
            {
                MessageBox.Show("Midi 장치 열기 실패", "Error");
            }
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {               
                //var inputDevice = InputDevice.GetById(comboBox1.SelectedIndex);
                //inputDevice.EventReceived -= OnEventReceived1;
                inputDevice.StopEventsListening();
                inputDevice.Dispose();
                serialPort1.Close();
                button2.Enabled = false;
                button1.Enabled = true;
                comboBox1.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                textBox1.AppendText("All devices has been closed." + Environment.NewLine);
                toolStripStatusLabel1.Text = "No device opened";
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tx.BackColor = Color.White;
            rx.BackColor = Color.White;
        }
    }
}
