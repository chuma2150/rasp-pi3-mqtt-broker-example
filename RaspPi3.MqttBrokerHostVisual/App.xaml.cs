﻿using Okra;
using System;

[assembly: CLSCompliant(false)]
namespace RaspPi3.MqttBrokerHostVisual
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : OkraApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
            : base(new AppBootstrapper())
        {
            this.InitializeComponent();
        }
    }
}
