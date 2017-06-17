using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Snapsize
{

    class SnapAreas
    {
        private static readonly Fraction Half = new Fraction(1, 2);
        private static readonly Fraction OneThird = new Fraction(1, 3);
        private static readonly Fraction TwoThirds = new Fraction(2, 3);
        private static readonly Fraction OneQuarter = new Fraction(1, 4);
        private static readonly Fraction ThreeQuarters = new Fraction(3, 4);

        // todo: allow customization of these
        private readonly List<Area> _snapAreaFractions = new List<Area>()
        {
            // horizontal halves
            new Area(0, 0, Half, 1),
            new Area(Half, 0, 1, 1),
            
            // horizontal thirds
            new Area(0, 0, OneThird, 1),
            new Area(OneThird, 0, TwoThirds, 1),
            new Area(TwoThirds, 0, 1, 1),

            // horizontal 1/3 + 2/3
            new Area(0, 0, OneThird, 1),
            new Area(OneThird, 0, 1, 1),
            
            // horizontal 2/3 + 1/3
            new Area(0, 0, TwoThirds, 1),
            new Area(TwoThirds, 0, 1, 1),
            
            // quandrants
            new Area(0, 0, Half, Half),
            new Area(Half, 0, 1, Half),
            new Area(0, Half, Half, 1),
            new Area(Half, Half, 1, 1),
        };

        private readonly List<Rectangle> _snapAreasPixels;

        public SnapAreas()
        {
            _snapAreasPixels = (
                from area in _snapAreaFractions
                let leftPx = area.Left.Of(Screen.PrimaryScreen.WorkingArea.Width)
                let topPx = area.Top.Of(Screen.PrimaryScreen.WorkingArea.Height)
                let rightPx = area.Right.Of(Screen.PrimaryScreen.WorkingArea.Width)
                let bottomPx = area.Bottom.Of(Screen.PrimaryScreen.WorkingArea.Height)
                select new Rectangle(leftPx, topPx, rightPx - leftPx, bottomPx - topPx)
                ).ToList();
        }

        public Rectangle GetClosestSnapAreaPixels(Point position)
        {
            var areasWithDistances =
                from area in _snapAreasPixels
                select new { area, dist = DistanceOfPointFromCentreOfRectangle(position, area) };

            return areasWithDistances.OrderBy(t => t.dist).Select(t => t.area).First();
        }

        private int DistanceOfPointFromCentreOfRectangle(Point point, Rectangle rectangle)
        {
            var centre = new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
            var xdist = point.X - centre.X;
            var ydist = point.Y - centre.Y;
            var dist = //Math.Sqrt  // comparing these to each other only, don't need to bother Sqrting them
                (
                    xdist * xdist + ydist * ydist
                );
            return dist;
        }


    }
}
