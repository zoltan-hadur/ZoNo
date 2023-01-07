using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using ZoNo.Helpers;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class ImportPage : Page
  {
    public ImportViewModel ViewModel { get; }

    public ImportPage()
    {
      ViewModel = App.GetService<ImportViewModel>();
      InitializeComponent();
    }

    private void Border_DragOver(object sender, DragEventArgs e)
    {
      e.AcceptedOperation = DataPackageOperation.Copy;
      e.DragUIOverride.Caption = "Import_Add".GetLocalized();
    }

    private async void Border_Drop(object sender, DragEventArgs e)
    {
      if (e.DataView.Contains(StandardDataFormats.StorageItems))
      {
        var items = await e.DataView.GetStorageItemsAsync();
        foreach (var path in items.Select(item => item.Path))
        {
          await ViewModel.LoadExcelDocument(path);
        }
      }
    }
  }
}