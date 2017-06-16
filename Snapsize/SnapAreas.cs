using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Snapsize
{
    class SnapAreas
    {
        // todo: allow customization of these
        private readonly List<Rectangle> _snapAreasPercentage = new List<Rectangle>()
        {
            // horizontal halves
            new Rectangle(0,0,50,100),
            new Rectangle(50,0,50,100),

            // horizontal thirds
            new Rectangle(0,0,33,100),
            new Rectangle(33,0,34,100),
            new Rectangle(67,0,33,100),

            // horizontal 1/3 + 2/3
            new Rectangle(0,0,33,100),
            new Rectangle(33,0,67,100),
            
            // horizontal 2/3 + 1/3
            new Rectangle(0,0,67,100),
            new Rectangle(67,0,33,100),
            
            // quandrants
            new Rectangle(0,0,50,50),
            new Rectangle(50,0,50,50),
            new Rectangle(0,50,50,50),
            new Rectangle(50,50,50,50),
        };

        private readonly List<Rectangle> _snapAreasPixels;

        public SnapAreas()
        {
            _snapAreasPixels = (
                from rectPercent in _snapAreasPercentage
                let leftPx = Screen.PrimaryScreen.WorkingArea.Width * rectPercent.Left / 100
                let topPx = Screen.PrimaryScreen.WorkingArea.Height * rectPercent.Top / 100
                let rightPx = Screen.PrimaryScreen.WorkingArea.Width * rectPercent.Right / 100
                let bottomPx = Screen.PrimaryScreen.WorkingArea.Height * rectPercent.Bottom / 100
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
