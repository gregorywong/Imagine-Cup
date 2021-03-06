﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace MediviseMobile
{
    public partial class MessageEdit : PhoneApplicationPage
    {
        public MessageEdit()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string selectedIndex = "";
            if (NavigationContext.QueryString.TryGetValue("selectedIndex", out selectedIndex))
            {
                int index = int.Parse(selectedIndex);
                DataContext = App.PatientViewModel.Messages[index];
            }
        }
    }
}