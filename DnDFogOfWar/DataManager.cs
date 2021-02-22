using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Shapes;
using System.Windows;
using System.Media;
using System.Diagnostics;
using System.Windows.Media;

namespace WpfApp1
{
    class DataManager
    {
        public DataManager()
        { }

        /// <summary>
        /// Just going to use the basic Xmlwriter for this simple doc. Primitive, but easy to use
        /// </summary>
        /// <param name="scribe"></param>
        /// <param name="fileName"></param>
        /// <param name="imageSource"></param>
        public void SaveData(Scribe scribe, string fileName, string imageSource)
        {
            try
            {
                int index = imageSource.LastIndexOf("\\");
                string nameWithExtension = imageSource.Remove(0, index);
                //index = nameWithExtension.LastIndexOf(".");
                //string nameOnly = nameWithExtension.Substring(0, index);

                string checkPath = AppDomain.CurrentDomain.BaseDirectory + "SaveFiles" + nameWithExtension;

                //first, create data file
                FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);

                XmlWriter writer = XmlWriter.Create(fileStream);

                writer.WriteStartDocument();
                writer.WriteStartElement("SaveData");
                //image used
                writer.WriteStartElement("ImageFile");
                writer.WriteString(imageSource);
                writer.WriteEndElement();

                writer.WriteStartElement("Polygon_List");
                foreach (Polygon p in scribe.Polygons)
                {
                    writer.WriteStartElement("Polygon");

                    foreach (Point point in p.Points)
                    {
                        writer.WriteStartElement("Point");

                        writer.WriteStartElement("X");
                        writer.WriteString(point.X.ToString());
                        writer.WriteEndElement();

                        writer.WriteStartElement("Y");
                        writer.WriteString(point.Y.ToString());
                        writer.WriteEndElement();

                        writer.WriteEndElement(); //end point block
                    }

                    writer.WriteStartElement("Visible");
                    if (p.Fill == System.Windows.Media.Brushes.Black)
                    {
                        writer.WriteString("True");
                    }
                    else
                    {
                        writer.WriteString("False");
                    }
                    writer.WriteEndElement(); //end visibility

                    writer.WriteEndElement(); //end polygon
                }

                writer.WriteEndElement(); //end polygon block

                writer.WriteEndElement();//end "SaveData"

                writer.WriteEndDocument();

                writer.Close();

                //check for the presence of the image file in the save directory.
                //in not there, copy it in
                if (!File.Exists(checkPath))
                {
                    string destPath;
                    destPath = AppDomain.CurrentDomain.BaseDirectory + "SaveFiles";

                    int imgIndex = imageSource.LastIndexOf("\\");
                    string name = imageSource.Remove(0, imgIndex);

                    destPath += name;

                    File.Copy(imageSource, destPath);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Now to load, we'll use something more modern; XDocument
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="scribe"></param>
        /// <returns></returns>
        public bool LoadData(string fileName, Scribe scribe, ref string imageFile)
        {
            try
            {
                //first, create data file
                FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                XmlWriterSettings setting = new XmlWriterSettings();
                setting.WriteEndDocumentOnClose = true;

                XDocument loadData = XDocument.Load(fileStream);

                List<Point> pointList;
                string rawVal;
                int x;
                int y;

                var root = loadData.Element("SaveData");
                var imageElement = root.Element("ImageFile");
                imageFile = imageElement.Value;

                var polyList = root.Elements("Polygon_List");

                var polygons = from polys in polyList.Descendants("Polygon") select polys;

                foreach (XElement poly in polygons)
                {
                    var points = from point in poly.Descendants("Point") select point;

                    pointList = new List<Point>();
                    foreach (XElement pnt in points)
                    {
                        rawVal = pnt.Element("X").Value;
                        x = int.Parse(rawVal);
                        rawVal = pnt.Element("Y").Value;
                        y = int.Parse(rawVal);

                        Point newPoint = new Point(x, y);
                        pointList.Add(newPoint);
                    }

                    //show or hide?
                    string visible = poly.Element("Visible").Value;
                        
                    Polygon newPolygon = new Polygon();
                    PointCollection pc = new PointCollection(pointList);
                    newPolygon.Points = pc;
                    newPolygon.Stroke = System.Windows.Media.Brushes.Black;
                    if (visible == "True")
                    {
                            newPolygon.Fill = System.Windows.Media.Brushes.Black;
                    }
                    else
                    {
                            newPolygon.Fill = System.Windows.Media.Brushes.Transparent;
                    }

                    scribe.Polygons.Add(newPolygon);
                    
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
