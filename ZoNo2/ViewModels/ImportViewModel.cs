﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace ZoNo2.ViewModels
{
  public class ImportViewModel : ObservableRecipient
  {
    private static int _id = 0;
    public int Id { get; } = _id++;

    public ImportViewModel()
    {

    }
  }
}