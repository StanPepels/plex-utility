using System;
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

namespace Plex_Util
{
  /// <summary>
  /// Interaction logic for ProgressControl.xaml
  /// </summary>
  public partial class ProgressControl : UserControl
  {
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( nameof(Value), typeof(double),typeof(ProgressControl));
    public double Value
    {
      get => (double)GetValue(ValueProperty);
      set {
        SetValue(ValueProperty, value);
      }
    }

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(ProgressControl));
    public double Maximum
    {
      get => (double)GetValue(MaximumProperty);
      set
      {
        SetValue(MaximumProperty, value);
      }
    }

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(ProgressControl));
    public double Minimum
    {
      get => (double)GetValue(MinimumProperty);
      set
      {
        SetValue(MinimumProperty, value);
      }
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(ProgressControl));
    public string Text
    {
      get => (string)GetValue(TextProperty);
      set
      {
        SetValue(TextProperty, value);
      }
    }


    public ProgressControl()
    {
      InitializeComponent();
    }
    private void PART_Indicator_Loaded(object sender, RoutedEventArgs e)
    {
      System.Windows.Shapes.Rectangle indicatorRectangle = sender as System.Windows.Shapes.Rectangle;
      if (indicatorRectangle == null) return;

      // Assign the new brush to the rectangle
      indicatorRectangle.Fill = HorizontalGradientAnimation.Create(Colors.LightGreen, Colors.Green, 0.1f);
    }

  }
}
