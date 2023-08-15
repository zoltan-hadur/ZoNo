﻿using Microsoft.UI.Xaml.Controls;

namespace ZoNo.Helpers
{
  public static class FrameExtensions
  {
    public static object? GetPageViewModel(this Frame frame)
    {
      return frame?.Content?.GetType().GetProperty("ViewModel")?.GetValue(frame.Content, null);
    }
  }
}