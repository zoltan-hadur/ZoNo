﻿using Microsoft.UI.Xaml.Controls;

using ZoNo2.ViewModels;

namespace ZoNo2.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
  public SettingsViewModel ViewModel { get; }

  public SettingsPage()
  {
    ViewModel = App.GetService<SettingsViewModel>();
    InitializeComponent();
  }
}
