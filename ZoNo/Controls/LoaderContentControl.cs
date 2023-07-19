﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ZoNo.Controls
{
  [ContentProperty(Name = nameof(Content))]
  public class LoaderContentControl : ContentControl
  {
    public static new readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(object), typeof(LoaderContentControl), new PropertyMetadata(null, OnContentChanged));
    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LoaderContentControl), new PropertyMetadata(null, OnIsLoadingChanged));

    public new object Content
    {
      get => GetValue(ContentProperty);
      set => SetValue(ContentProperty, value);
    }

    public bool IsLoading
    {
      get => (bool)GetValue(IsLoadingProperty);
      set => SetValue(IsLoadingProperty, value);
    }

    private ContentControl _contentControl = new ContentControl()
    {
      HorizontalContentAlignment = HorizontalAlignment.Stretch,
      VerticalContentAlignment = VerticalAlignment.Stretch,
    };

    private ProgressRing _progressRing = new ProgressRing()
    {
      Width = 100,
      Height = 100,
      IsActive = false
    };

    public LoaderContentControl()
    {
      base.Content = new Grid() { Children = { _contentControl, _progressRing } };
    }

    public static void OnContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      if (sender is LoaderContentControl loaderContentControl)
      {
        loaderContentControl._contentControl.Content = e.NewValue;
      }
    }

    public static void OnIsLoadingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      if (sender is LoaderContentControl loaderContentControl && e.NewValue is bool isLoading)
      {
        loaderContentControl._contentControl.IsEnabled = !isLoading;
        loaderContentControl._progressRing.IsActive = isLoading;
      }
    }
  }
}
