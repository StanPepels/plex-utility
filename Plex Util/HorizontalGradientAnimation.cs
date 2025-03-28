using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Plex_Util
{
  public static class HorizontalGradientAnimation
  {
    /// <summary>
    /// Utility that creates a brush to create a horizontal gradient.
    /// </summary>
    public static LinearGradientBrush Create(Color fadeColor, Color fillColor, float fillColorAspect)
    {
      var brush = new LinearGradientBrush
      {
        StartPoint = new Point(0, 0),
        EndPoint = new Point(1, 0),
        MappingMode = BrushMappingMode.RelativeToBoundingBox
      };

      float fadeLength = (1.0f - fillColorAspect) * 0.5f;
      // Add GradientStops

      float gradientOffset0 = -1.0f;
      float gradientOffset1 = -1.0f + fadeLength;
      float gradientOffset2 = 0.0f - fadeLength;
      float gradientOffset3 = 0.0f;
      float gradientOffset4 = 0.0f + fadeLength;
      float gradientOffset5 = 1 - fadeLength;
      float gradientOffset6 = 1.0f;
      var gradientStop0 = new GradientStop(fadeColor, gradientOffset0);
      var gradientStop1 = new GradientStop(fillColor, gradientOffset1);
      var gradientStop2 = new GradientStop(fillColor, gradientOffset2);
      var gradientStop3 = new GradientStop(fadeColor, gradientOffset3);
      var gradientStop4 = new GradientStop(fillColor, gradientOffset4);
      var gradientStop5 = new GradientStop(fillColor, gradientOffset5);
      var gradientStop6 = new GradientStop(fadeColor, gradientOffset6);

      brush.GradientStops.Add(gradientStop0);
      brush.GradientStops.Add(gradientStop1);
      brush.GradientStops.Add(gradientStop2);
      brush.GradientStops.Add(gradientStop3);
      brush.GradientStops.Add(gradientStop4);
      brush.GradientStops.Add(gradientStop5);
      brush.GradientStops.Add(gradientStop6);


      // Create animations for the GradientStops
      var animation0 = new DoubleAnimation(gradientOffset0, gradientOffset0 + 1, TimeSpan.FromSeconds(2))
      {
        RepeatBehavior = RepeatBehavior.Forever
      };
      var animation1 = new DoubleAnimation(gradientOffset1, gradientOffset1 + 1,TimeSpan.FromSeconds(2))
      {
        RepeatBehavior = RepeatBehavior.Forever
      };
      var animation2 = new DoubleAnimation(gradientOffset2, gradientOffset2 + 1, TimeSpan.FromSeconds(2))
      {
        RepeatBehavior = RepeatBehavior.Forever
      };
      var animation3 = new DoubleAnimation(gradientOffset3, gradientOffset3 + 1, TimeSpan.FromSeconds(2))
      {
        RepeatBehavior = RepeatBehavior.Forever
      };
      var animation4 = new DoubleAnimation(gradientOffset4, gradientOffset4 + 1, TimeSpan.FromSeconds(2))
      {
        RepeatBehavior = RepeatBehavior.Forever
      };
      var animation5 = new DoubleAnimation(gradientOffset5, gradientOffset5 + 1, TimeSpan.FromSeconds(2))
      {
        RepeatBehavior = RepeatBehavior.Forever
      };
      var animation6 = new DoubleAnimation(gradientOffset6, gradientOffset6 + 1, TimeSpan.FromSeconds(2))
      {
        RepeatBehavior = RepeatBehavior.Forever
      };
      // Apply animations to the GradientStops
      brush.GradientStops[0].BeginAnimation(GradientStop.OffsetProperty, animation0);
      brush.GradientStops[1].BeginAnimation(GradientStop.OffsetProperty, animation1);
      brush.GradientStops[2].BeginAnimation(GradientStop.OffsetProperty, animation2);
      brush.GradientStops[3].BeginAnimation(GradientStop.OffsetProperty, animation3);
      brush.GradientStops[4].BeginAnimation(GradientStop.OffsetProperty, animation4);
      brush.GradientStops[5].BeginAnimation(GradientStop.OffsetProperty, animation5);
      brush.GradientStops[6].BeginAnimation(GradientStop.OffsetProperty, animation6);
      return brush;
    }
  }
}
