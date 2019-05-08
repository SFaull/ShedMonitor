﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SmartMonitorApp.Properties;

namespace SmartMonitorApp
{
    public partial class UserControlHome : UserControl
    {
        public UserControlHome()
        {
            InitializeComponent();
            txtUsername.Text = Settings.Default.MqttUsername;
            txtPassword.Password = Settings.Default.MqttPassword;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            // for now just apply
            Settings.Default.MqttUsername = txtUsername.Text;
            Settings.Default.MqttPassword = txtPassword.Password;
            Settings.Default.Save();
        }
    }
}
