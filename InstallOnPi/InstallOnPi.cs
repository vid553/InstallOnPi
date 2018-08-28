/*
 * Install On Pi Application
 * 
 * Copyright (C) 2018 Vid Rajtmajer <vid.rajtmajer@gmail.com>
 * 
 * Icon made by geotatah from www.flaticon.com
 * Background photo by Juan from www.pexels.com
 */

using System;
using System.Net;
using System.Security;
using System.Windows.Forms;
using Renci.SshNet;

namespace InstallOnPi
{
    public partial class InstallOnPi : Form
    {
        private SshClient sshClient = null;

        string configCommands = 
            "sudo apt-get install libopus0 libasound2 libudev0 libavahi-client3 libcurl3 libevdev2 ; " +              
            "echo 'deb http://archive.itimmer.nl/raspbian/moonlight OS main' | sudo tee -a  /etc/apt/sources.list && " + 
            "wget http://archive.itimmer.nl/itimmer.gpg && " +  
            "sudo apt-key add itimmer.gpg && " +  
            "sudo apt-get update && " +    
            "sudo apt-get install moonlight-embedded";

        public InstallOnPi()
        {
            InitializeComponent();
            textBox4.Text = "jessie";
        }

        private void InstallBtn_Click(object sender, EventArgs e)   // vzpostavimo povezavo z napravo
        {
            infoLabel.Text = "Nalagam ...";
            try
            {
                configCommands = configCommands.Replace("OS", textBox4.Text);
                string deviceIp = textBox3.Text;
                string deviceUsername = textBox1.Text;
                SecureString devicePassword = new NetworkCredential("", textBox2.Text).SecurePassword;
                if (deviceIp != null && deviceUsername != null && devicePassword != null)
                {
                    ConnectionInfo connectionInfo = new ConnectionInfo(deviceIp, deviceUsername, 
                        new PasswordAuthenticationMethod(deviceUsername, new NetworkCredential("", devicePassword).Password));
                    sshClient = new SshClient(connectionInfo);
                    sshClient.Connect();
                    if (sshClient.IsConnected == true)
                    {
                        RunCommands();
                    }
                    else
                    {
                        infoLabel.Text = "Napaka! Preverite povezavo z napravo...";
                        infoLabel.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Napaka pri vzpostavitvi povezave z napravo.");
                infoLabel.Text = "Ni povezave :(";
                infoLabel.Visible = true;
                sshClient.Dispose();
                sshClient = null;
            }
        }

        private void RunCommands()  // izvedemo ukaze za namestitev vseh potrebnih programov
        {
            infoLabel.Text = "Nameščam programe...";
            if (configCommands != "")
            {
                SshCommand sshCommand = sshClient.CreateCommand(configCommands);
                string result = sshCommand.Execute();
                if (result.Contains("error") || result.Contains("ni mogoče"))
                {
                    MessageBox.Show("Prišlo je do napake pri nameščanju! Sporočilo napake:\n" + result.ToString());
                    infoLabel.Text = "Nameščanje ni uspelo!";
                }
                else
                {
                    infoLabel.Text = "Končano!";
                }
            }
            else
            {
                MessageBox.Show("Napaka pri pošiljanju ukazov na napravo!");
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)    // Klik na gumb prekliči
        {
            Close();
        }

        private void InstallOnPi_FormClosing(object sender, FormClosingEventArgs e) // sprostimo vire ko se aplikacija zapre
        {
            if (sshClient != null)
            {
                if (sshClient.IsConnected == true)
                {
                    sshClient.Disconnect();
                }
                sshClient.Dispose();
                this.Dispose();
            }
        }
    }
}
