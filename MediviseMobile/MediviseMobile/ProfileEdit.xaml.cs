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
    public partial class ProfileEdit : PhoneApplicationPage
    {
        string[] gender = {"Male", "Female"};

        public ProfileEdit()
        {
            InitializeComponent();
            this.ListPicker.ItemsSource = gender;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            DataContext = App.PatientViewModel.Patients.First();
            base.OnNavigatedTo(e);
        }

        private void AppBarSave_Click(object sender, EventArgs e)
        {
                App.PatientViewModel.SaveChanges();
        }
    }
}