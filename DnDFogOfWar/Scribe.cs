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

namespace WpfApp1
{
    class Scribe
    {
        private List<Point> currentStrip = new List<Point>();
        //private List<List<Point>> stripList = new List<List<Point>>();
        private List<Polygon> polygonList = new List<Polygon>();

        private Line lastActiveLine = null;

        //sets the range when a position 'snap to' occurs.
        private int snapTolerance = 5;
        private bool showBorders = true;
        private bool forceBorders = false;

        public int xRegister;
        public int yRegister;

        public bool ForceBorders
        {
            get { return forceBorders; }
        }

        public List<Polygon> Polygons
        {
            get { return polygonList; }
        }

        public Scribe()
        { }

        //check to see if the current point is close enough to complete the polygon.
        public bool CheckSnap()
        {
            bool check = false;

            Point checkPosition = currentStrip.First();

            if (((this.xRegister + snapTolerance) > checkPosition.X) && ((this.xRegister - snapTolerance) < checkPosition.X))
            {
                if (((this.yRegister + snapTolerance) > checkPosition.Y) && ((this.yRegister - snapTolerance) < checkPosition.Y))
                {
                    //snap
                    check = true;
                }
            }

            return check;
        }

        public void EndDrawing(Canvas canvas)
        {
            if(lastActiveLine != null)
            {
                canvas.Children.Remove(lastActiveLine);
                lastActiveLine = null;
            }

            this.currentStrip.Clear();
        }

        //place first point in the chain
        public void PlaceInitialPoint()
        {
            List<Point> newStrip = new List<Point>();

            Point firstPoint = new Point(this.xRegister, this.yRegister);
            newStrip.Add(firstPoint);
            this.currentStrip = newStrip;
        }

        //place intermediate points in chain
        public void PlacePoint()
        {
            Point addPoint = new Point(this.xRegister, this.yRegister);
            this.currentStrip.Add(addPoint);
        }

        //finalize the chain/polygon
        public void PlaceFinalPoint(Canvas canvas)
        {
            //this.stripList.Add(this.currentStrip);

            Polygon polygon = new Polygon();
            PointCollection points = new PointCollection(this.currentStrip);

            polygon.Points = points;
            if (showBorders)
            {
                polygon.Stroke = System.Windows.Media.Brushes.Red;
            }
            else
            {
                polygon.Stroke = System.Windows.Media.Brushes.Black;
            }
            polygon.StrokeThickness = 1;
            polygon.Fill = System.Windows.Media.Brushes.Black;

            canvas.Children.Add(polygon);
            this.polygonList.Add(polygon);

            this.lastActiveLine = null;
        }

        public void DrawActiveLine(Canvas canvas)
        {
            if (lastActiveLine != null)
            {
                canvas.Children.Remove(lastActiveLine);
            }

            Point startPoint = this.currentStrip.Last();
            Point endPoint = new Point(this.xRegister, this.yRegister);

            //place them in contaniner for drawing
            Point[] points = { startPoint, endPoint };

            Line activeLine = new Line();
            activeLine.Name = "Active";

            activeLine.Stroke = System.Windows.Media.Brushes.DarkMagenta;
            activeLine.X1 = startPoint.X;
            activeLine.X2 = endPoint.X;
            activeLine.Y1 = startPoint.Y;
            activeLine.Y2 = endPoint.Y;
            //activeLine.HorizontalAlignment = HorizontalAlignment.Left;
            //activeLine.VerticalAlignment = VerticalAlignment.Center;
            activeLine.StrokeThickness = 1;
            canvas.Children.Add(activeLine);

            //save for later
            this.lastActiveLine = activeLine;
        }

        public void DrawCurrentStrip(Canvas canvas)
        {
            //with only one point, we only have a point-mouse cursor line.
            if (this.currentStrip.Count <= 1)
            {
                return;
            }

            Point startPoint;
            Point endPoint;
            int k = 0;

            for (int i = 0; i < currentStrip.Count - 1; i++)
            {
                k = i + 1;

                startPoint = this.currentStrip[i];
                endPoint = this.currentStrip[k];

                //place them in contaniner for drawing
                Point[] points = { startPoint, endPoint };

                Line drawLine = new Line();

                drawLine.Stroke = System.Windows.Media.Brushes.Red;
                drawLine.X1 = startPoint.X;
                drawLine.X2 = endPoint.X;
                drawLine.Y1 = startPoint.Y;
                drawLine.Y2 = endPoint.Y;
                drawLine.StrokeThickness = 1;
                canvas.Children.Add(drawLine);
            }
        }

        //public void DrawExistingStrips(Canvas canvas)
        //{
        //    //saftey check
        //    if (this.stripList.Count < 1)
        //    {
        //        return;
        //    }

        //    Point startPoint;
        //    Point endPoint;
        //    int k = 0;

        //    for (int i = 0; i < stripList.Count; i++)
        //    {
        //        List<Point> strip = stripList[i];

        //        for(int x = 0; x < strip.Count; x++)
        //        {
        //            k = x + 1;

        //            if(k == strip.Count)
        //            {
        //                k = 0; //wrap around and grab first point to complete polygon
        //            }

        //            startPoint = strip[x];
        //            endPoint = strip[k];

        //            //place them in contaniner for drawing
        //            Point[] points = { startPoint, endPoint };

        //            Line drawLine = new Line();

        //            drawLine.Stroke = System.Windows.Media.Brushes.Black;
        //            drawLine.X1 = startPoint.X;
        //            drawLine.X2 = endPoint.X;
        //            drawLine.Y1 = startPoint.Y;
        //            drawLine.Y2 = endPoint.Y;
        //            drawLine.StrokeThickness = 1;
        //            canvas.Children.Add(drawLine);

        //        }
        //    }
        //}

        public void DrawAllPolygons(Canvas canvas)
        {
            foreach(Polygon p in this.polygonList)
            {
                canvas.Children.Add(p);
            }
        }
        
        public void ClearAllArtifacts()
        {
            this.polygonList.Clear();
            this.currentStrip.Clear();
        }

        public void ToggleBorders()
        {
            if(showBorders)
            {
                showBorders = false;

                foreach(Polygon p in this.polygonList)
                {
                    p.Stroke = System.Windows.Media.Brushes.Black;
                }

            }
            else
            {
                showBorders = true;

                foreach (Polygon p in this.polygonList)
                {
                    p.Stroke = System.Windows.Media.Brushes.Red;
                }
            }
        }

        public void CheckPolygons(Point checkPoint)
        {
            foreach(Polygon p in this.polygonList)
            {
                PointCollection pc = p.Points;
                Point[] points = pc.ToArray();

                if(IsPointInPolygon(points, checkPoint))
                {
                    if(p.Fill == System.Windows.Media.Brushes.Black) //visible polygon
                    {
                        p.Fill = System.Windows.Media.Brushes.Transparent;

                        if (forceBorders)
                        {
                            p.Stroke = System.Windows.Media.Brushes.Red;
                        }
                        else
                        {
                            p.Stroke = System.Windows.Media.Brushes.Transparent;
                        }
                    }
                    else //invisible polygon
                    {
                        p.Fill = System.Windows.Media.Brushes.Black;

                        if (showBorders || forceBorders)
                        {
                            p.Stroke = System.Windows.Media.Brushes.Red;
                        }
                        else
                        {
                            p.Stroke = System.Windows.Media.Brushes.Black;
                        }
                    }
                }
            }
        }

        public void CheckPolygonsForDelete(Point checkPoint)
        {
            foreach (Polygon p in this.polygonList)
            {
                PointCollection pc = p.Points;
                Point[] points = pc.ToArray();

                if (IsPointInPolygon(points, checkPoint))
                {
                    this.polygonList.Remove(p);
                    break;
                }
            }
        }

        public void ToggleForceBorders()
        {
            if(forceBorders)
            {
                this.forceBorders = false;

                foreach(Polygon p in this.polygonList)
                {
                    if (p.Fill == System.Windows.Media.Brushes.Transparent) //its invisible anyhow
                    {
                        p.Stroke = System.Windows.Media.Brushes.Red;
                    }
                    else
                    {
                        if (showBorders)
                        {
                            p.Stroke = System.Windows.Media.Brushes.Red;
                        }
                        else
                        {
                            p.Stroke = System.Windows.Media.Brushes.Black;
                        }
                    }
                }
            }
            else
            {
                this.forceBorders = true;

                foreach(Polygon p in this.polygonList)
                {
                    p.Stroke = System.Windows.Media.Brushes.Red;
                }
            }
        }

        private bool IsPointInPolygon(Point[] polygon, Point point)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }
    }
}
