using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Snapsize
{
    class SnapAreas
    {
        private readonly List<Rectangle> _snapAreasPixels;
        private readonly List<Area> _snapAreasFractions;
        
        public SnapAreas(IEnumerable<Area> areas)
        {
            _snapAreasFractions = areas.ToList();
            _snapAreasPixels = (
                from area in areas
                let leftPx = area.Left.Of(Screen.PrimaryScreen.WorkingArea.Width)
                let topPx = area.Top.Of(Screen.PrimaryScreen.WorkingArea.Height)
                let rightPx = area.Right.Of(Screen.PrimaryScreen.WorkingArea.Width)
                let bottomPx = area.Bottom.Of(Screen.PrimaryScreen.WorkingArea.Height)
                select new Rectangle(leftPx, topPx, rightPx - leftPx, bottomPx - topPx)
                ).ToList();
        }

        public string Serialize()
        {
            return string.Join(Environment.NewLine, _snapAreasFractions.Select(t => t.Serialize()));
        }

        public static SnapAreas Deserialize(string input)
        {
            var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var areas = lines
                .Select(t => t.Trim())
                .Where(t => !t.StartsWith("#"))
                .Select(Area.Deserialize);

            return new SnapAreas(areas);
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
            var dist =
                // comparing these to each other only, don't need to bother Sqrting them
                //Math.Sqrt  
                (
                    xdist * xdist + ydist * ydist
                );
            return dist;
        }


    }
}
