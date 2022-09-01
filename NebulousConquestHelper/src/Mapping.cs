using System;
using System.Xml.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace NebulousConquestHelper
{
    public class Mapping
    {
        private const int CANVAS_LENGTH = 1024;
        private const int CANVAS_HEIGHT = 1024;
        private const int PIXELS_PER_AU = 128;
        private const int DIAMETER_STAR = 32;
        private const int DIAMETER_PLANET = 16;

        private static Point Centre()
        {
            return new Point(CANVAS_LENGTH / 2, CANVAS_HEIGHT / 2);
        }

        private static Point Centre(int length, int height)
        {
            return new Point(length / 2, height / 2);
        }

        private static Rectangle RectangleAround(Point point, int size)
        {
            return new Rectangle(point.X - (size / 2), point.Y - (size / 2), size, size);
        }

        private static Rectangle RectangleAround(Point point, int length, int height)
        {
            return new Rectangle(point.X - (length / 2), point.Y - (height / 2), length, height);
        }

        private static void DrawCaption(Graphics map, string caption, Point textPos, int offset = 0)
        {
            int textX = (int)(textPos.X + offset + 2);
            int textY = textPos.Y - 8;
            /*
            map.FillRectangle(
                Brushes.Navy,
                new Rectangle(textX, textY - 1, caption.Length * 6, 18)
            );
            */
            map.DrawString(
                caption,
                SystemFonts.IconTitleFont,
                Brushes.White,
                new Point(textX, textY)
            );
        }

        public static Point GetLocationCoordinates(LocationInfo loc, int daysFromStart)
        {
            double radians = loc.GetCurrentDegrees(daysFromStart) * (Math.PI / 180);
            double planetX = Math.Sin(radians) * loc.OrbitalDistanceAU * PIXELS_PER_AU;
            double planetY = Math.Cos(radians) * loc.OrbitalDistanceAU * PIXELS_PER_AU * -1;
            return new Point((int)(CANVAS_LENGTH / 2 + planetX), (int)(CANVAS_HEIGHT / 2 + planetY));
        }

        public static void CreateSystemMap(SystemInfo star, int daysFromStart = 0)
        {
            Image img = Image.FromFile(Helper.DATA_FOLDER_PATH + "canvas.png");
            Graphics map = Graphics.FromImage(img);

            map.Clear(Color.Navy);

            List<BeltInfo> belts = star.SurroundingBelts;
            belts.Sort();

            foreach (BeltInfo belt in belts)
            {
                Brush beltColour = belt.NearEdgeDistanceAU <= 2.00f ? Brushes.DarkGoldenrod : Brushes.LightSkyBlue;
                map.FillEllipse(beltColour, RectangleAround(Centre(), (int)(belt.FarEdgeDistanceAU * PIXELS_PER_AU * 2)));
                map.FillEllipse(Brushes.Navy, RectangleAround(Centre(), (int)(belt.NearEdgeDistanceAU * PIXELS_PER_AU * 2)));
            }

            map.FillEllipse(Brushes.LightGoldenrodYellow, RectangleAround(Centre(), DIAMETER_STAR));
            
            foreach (LocationInfo planet in star.OrbitingLocations)
            {
                map.DrawEllipse(Pens.Blue, RectangleAround(Centre(), (int)(planet.OrbitalDistanceAU * PIXELS_PER_AU * 2)));
                Point planetPos = GetLocationCoordinates(planet, daysFromStart);
                Brush planetColour = planet.ControllingTeam == GameInfo.ConquestTeam.GreenTeam ? Brushes.Green : Brushes.OrangeRed;
                map.FillEllipse(planetColour, RectangleAround(planetPos, DIAMETER_PLANET));
            }

            foreach (BeltInfo belt in belts)
            {
                DrawCaption(map, belt.Name, new Point((int)(belt.FarEdgeDistanceAU * PIXELS_PER_AU + (CANVAS_LENGTH / 2)), CANVAS_HEIGHT / 2));
            }

            foreach (LocationInfo planet in star.OrbitingLocations)
            {
                Point planetPos = GetLocationCoordinates(planet, daysFromStart);
                DrawCaption(map, planet.Name, planetPos, DIAMETER_PLANET / 2);
            }

            img.Save(Helper.DATA_FOLDER_PATH + "SystemMap.png", ImageFormat.Png);
        }
    }
}