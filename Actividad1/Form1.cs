using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Actividad1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle; // Desactivar la capacidad de redimensionar
            this.MaximizeBox = false; // Desactivar la capacidad de maximizar
            this.MinimizeBox = false; // Desactivar la capacidad de minimizar

            // Check IP Host
            string hostName = Dns.GetHostName();
            IPAddress[] ips = Dns.GetHostAddresses(hostName);
            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    labelIP.Text = ip.ToString();
                    break;
                }
            }

            // Display gateway
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                GatewayIPAddressInformationCollection gateways = properties.GatewayAddresses;
                foreach (GatewayIPAddressInformation gateway in gateways)
                {
                    if (gateway.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        labelGateway.Text = gateway.Address.ToString();
                        break;
                    }
                }
            }

            // Display current user
            labelUser.Text = Environment.UserName;

            // Display hostname
            labelHostname.Text = Dns.GetHostName();

            // Display SSID name and status
            string output = RunCommand("netsh", "wlan show interfaces");

            // Obtener el SSID
            Match match = Regex.Match(output, @"SSID\s+:\s(.+)\r\n");
            string ssid = match.Groups[1].Value.Trim();

            // Obtener el estado de conexión
            match = Regex.Match(output, @"Estado\s+:\s(.+)\r\n");
            string status = match.Groups[1].Value.Trim();

            // Actualizar las etiquetas
            labelSSID.Text = ssid;
            labelStatus.Text = status;

            // Display MAC address
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                if (adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback && adapter.OperationalStatus == OperationalStatus.Up)
                {
                    PhysicalAddress address = adapter.GetPhysicalAddress();
                    byte[] bytes = address.GetAddressBytes();
                    string macAddress = "";
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        macAddress += bytes[i].ToString("X2");
                        if (i != bytes.Length - 1)
                        {
                            macAddress += "-";
                        }
                    }
                    labelMAC.Text = macAddress;
                    break;
                }
            }

            // Internet connection status
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send("cloudflare.com", 1000);
                if (reply.Status == IPStatus.Success)
                {
                    labelInternet.Text = "Established";
                }
                else
                {
                    // Count the number of lost packets
                    int lostPackets = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        reply = ping.Send("cloudflare.com", 1000);
                        if (reply.Status != IPStatus.Success)
                        {
                            lostPackets++;
                        }
                    }

                    if (lostPackets == 0)
                    {
                        Console.WriteLine("Established");
                    }
                    else if (lostPackets < 4)
                    {
                        Console.WriteLine("Unstable");
                    }
                    else
                    {
                        Console.WriteLine("Disconnected");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // Check if VirtualBox is installed and show version
            // Buscar la clave de registro de VirtualBox
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Oracle\VirtualBox");

            if (key != null)
            {
                // VirtualBox está instalado
                labelVirtualbox.Text = "Yes";

                // Obtener la versión instalada
                string version = key.GetValue("Version").ToString();
                labelVersion.Text = version;

                // Verificar si es la última versión
                string latestVersion = "6.1.30"; // Supongamos que esta es la última versión
                int comparison = String.Compare(version, latestVersion);

                if (comparison < 0)
                {
                    labelVersion.Text += " (Not the latest version)";
                }
                else if (comparison > 0)
                {
                    labelVersion.Text += " (Newer version than latest)";
                }
                else
                {
                    labelVersion.Text += " (Latest version)";
                }
            }
            else
            {
                // VirtualBox no está instalado
                labelVirtualbox.Text = "N/A";
            }

        }

        private string RunCommand(string command, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                return process.StandardOutput.ReadToEnd();
            }
        }


    }
}
