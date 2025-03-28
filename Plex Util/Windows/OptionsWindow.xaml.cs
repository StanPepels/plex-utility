using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace Plex_Util
{
  /// <summary>
  /// Interaction logic for OptionsWindow.xaml
  /// </summary>
  public partial class OptionsWindow : Window, INotifyPropertyChanged
  {
    private string handbrakeCliOverride;
    private string handbrakePathOverride;
    private string winSCPPathOverride;
    private string makeMKVPathOverride;
    private string vLCPathOverride;

    public string MakeMKVPathOverride
    {
      get => makeMKVPathOverride;
      set
      {
        if (makeMKVPathOverride != value)
        {
          makeMKVPathOverride = value;
          OnPropertyChanged(nameof(MakeMKVPathOverride));
        }
      }
    }


    public string VLCPathOverride
    {
      get => vLCPathOverride;
      set
      {
        if (vLCPathOverride != value)
        {
          vLCPathOverride = value;
          OnPropertyChanged(nameof(VLCPathOverride));
        }
      }
    }


    public string WinSCPPathOverride
    {
      get => winSCPPathOverride;
      set
      {
        if (winSCPPathOverride != value)
        {
          winSCPPathOverride = value;
          OnPropertyChanged(nameof(WinSCPPathOverride));
        }
      }
    }


    public string HandbrakePathOverride
    {
      get => handbrakePathOverride;
      set
      {
        if (handbrakePathOverride != value)
        {
          handbrakePathOverride = value;
          OnPropertyChanged(nameof(HandbrakePathOverride));
        }
      }
    }


    public string HandbrakeCliOverride
    {
      get => handbrakeCliOverride;
      set
      {
        if (handbrakeCliOverride != value)
        {
          handbrakeCliOverride = value;
          OnPropertyChanged(nameof(HandbrakeCliOverride));
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public OptionsWindow()
    {
      InitializeComponent();
      DataContext = this;
      if(Settings.TryLoad(out Settings settings))
      {
        HandbrakeCliOverride = settings.HandbrakeCliOverride;
        HandbrakePathOverride = settings.HandbrakePathOverride;
        WinSCPPathOverride = settings.WinSCPPathOverride;
        VLCPathOverride = settings.VLCPathOverride;
        MakeMKVPathOverride = settings.MakeMKVPathOverride;
      }
    }

    protected override void OnClosed(EventArgs e)
    {
      if (!Settings.TryLoad(out Settings settings))
      {
        settings = new Settings();
      }

      settings.HandbrakeCliOverride = HandbrakeCliOverride;
      settings.HandbrakePathOverride = HandbrakePathOverride;
      settings.WinSCPPathOverride = WinSCPPathOverride;
      settings.VLCPathOverride = VLCPathOverride;
      settings.MakeMKVPathOverride = MakeMKVPathOverride;

      Settings.Save(settings);
    }


    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}