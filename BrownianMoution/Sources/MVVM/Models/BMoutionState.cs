using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Xml;
using BrownianMoution.Sources.figures;
using BrownianMoution.Sources.MVVM.Util;
using Microsoft.Practices.Prism.Mvvm;

namespace BrownianMoution.Sources.MVVM.Models
{
    public class BMoutionState : BindableBase
    {
        private int _height = 500;
        private int _width = 670;
        private readonly ObservableCollection<PhysicCircle> _figureCollection;

        private StreamWriter _sr;
        private bool _isLogOn;


        #region Properties

        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                NormolizeHeightState();
            }
        }

        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                NormolizeWightState();
            }
        }

        public ObservableCollection<PhysicCircle> FigureCollection => _figureCollection;

        public bool IsLogOn
        {
            get => _isLogOn;
            set
            {
                _isLogOn = value;
                OnOffReader();
            }
        }

        #endregion


        #region Constructor

        public BMoutionState()
        {
            _figureCollection = new ObservableCollection<PhysicCircle>();
        }

        #endregion


        #region Public methods

        public void AddValue(PhysicCircle figure)
        {
            var normolizedFigure = NormolizeOne(figure);
            _figureCollection.Add(normolizedFigure);
        }

        public void RemoveValue(PhysicCircle circle)
        {
            _figureCollection.Remove(circle);
        }

        public void Tick(object sender, EventArgs args)
        {
            for (var i = 0; i < _figureCollection.Count; i++)
            {
                var circle = _figureCollection[i];
                circle.Move();
                //_figureCollection[i] = circle;

                DetectAndRunCollision(i);

                OnPropertyChanged("Figures");
            }
        }

        public void SaveState(string filepatch)
        {
            var doc = new XmlDocument();

            var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            doc.AppendChild(xmlDeclaration);

            var root = doc.CreateElement("collection");

            var weightAttr = doc.CreateAttribute("Weidth");
            weightAttr.InnerText = _width.ToString();
            root.Attributes.Append(weightAttr);

            var heightAttr = doc.CreateAttribute("Height");
            heightAttr.InnerText = _height.ToString();
            root.Attributes.Append(heightAttr);

            foreach (var circle in _figureCollection)
            {
                var circleNode = doc.CreateElement("Circle");

                AddChildNode("Radius", circle.Radius.ToString(), circleNode, doc);
                AddChildNode("Mass", circle.Mass.ToString(), circleNode, doc);
                AddChildNode("VX", circle.SpeedX.ToString(CultureInfo.CurrentCulture), circleNode, doc);
                AddChildNode("VY", circle.SpeedY.ToString(CultureInfo.CurrentCulture), circleNode, doc);
                AddChildNode("X", circle.X.ToString(CultureInfo.CurrentCulture), circleNode, doc);
                AddChildNode("Y", circle.Y.ToString(CultureInfo.CurrentCulture), circleNode, doc);

                root.AppendChild(circleNode);
            }

            doc.AppendChild(root);

            doc.Save(filepatch);
        }

        public void LoadState(string filepatch)
        {
            _figureCollection.Clear();
            var doc = new XmlDocument();

            doc.Load(filepatch);

            var root = doc.DocumentElement;

            if (root == null)
            {
                throw new FileLoadException("incorrect file structure");
            }

            var weidth = Convert.ToInt32(root.Attributes["Weidth"].InnerText);
            var height = Convert.ToInt32(root.Attributes["Height"].InnerText);

            Height = height;
            Width = weidth;

            foreach (var child in root.ChildNodes)
            {
                if (child is XmlNode node)
                {
                    var radius = Convert.ToInt32(node["Radius"]?.InnerText);
                    var mass = Convert.ToInt32(node["Mass"]?.InnerText);
                    var vx = Convert.ToDouble(node["VX"]?.InnerText);
                    var vy = Convert.ToDouble(node["VY"]?.InnerText);
                    var x = Convert.ToDouble(node["X"]?.InnerText);
                    var y = Convert.ToDouble(node["Y"]?.InnerText);

                    var loadedFigure = new PhysicCircle(x, y, mass, radius, vx, vy);
                    _figureCollection.Add(loadedFigure);
                }
            }
        }

        public void DragMove(MouseDragArgs args)
        {
            var e = args.EArgs as DragDeltaEventArgs;

            var circle = (PhysicCircle)((FrameworkElement)args.Sender)?.DataContext;


            if (circle != null && e != null)
            {
                if (circle.Left + e.HorizontalChange > 0 &&
                    circle.Left + e.HorizontalChange < Width - circle.Radius * 2)
                    circle.X += e.HorizontalChange;

                if (circle.Top + e.VerticalChange > 0 && circle.Top + e.VerticalChange < Height - circle.Radius * 2)
                    circle.Y += e.VerticalChange;
            }
        }

        #endregion


        #region Private Methods

        private static void AddChildNode(string childName, string childText, XmlElement parentNode, XmlDocument doc)
        {
            var child = doc.CreateElement(childName);
            child.InnerText = childText;
            parentNode.AppendChild(child);
        }

        private void DetectAndRunCollision(int index)
        {
            var verifiableCircle = _figureCollection[index];

            if (verifiableCircle.X > _width - verifiableCircle.Radius)
            {
                if (verifiableCircle.Speed.X > 0)
                {
                    if(IsLogOn)
                        LogCollisionBefore("#" + index + verifiableCircle, "Right Border");

                    verifiableCircle.Speed = new Vector(-verifiableCircle.Speed.X, verifiableCircle.Speed.Y);

                    if (IsLogOn)
                        LogCollisionAfter("#" + index + verifiableCircle);
                }

            }
            else if (verifiableCircle.X < verifiableCircle.Radius)
            {
                if (verifiableCircle.Speed.X < 0)
                {
                    if (IsLogOn)
                        LogCollisionBefore("#" + index + verifiableCircle, "Left Border");

                    verifiableCircle.Speed = new Vector(-verifiableCircle.Speed.X, verifiableCircle.Speed.Y);

                    if (IsLogOn)
                        LogCollisionAfter("#" + index + verifiableCircle);
                }

            }
            else if (verifiableCircle.Y > _height - verifiableCircle.Radius)
            {
                if (verifiableCircle.Speed.Y > 0)
                {
                    if (IsLogOn)
                        LogCollisionBefore("#" + index + verifiableCircle, "Bottom Border");

                    verifiableCircle.Speed = new Vector(verifiableCircle.Speed.X, -verifiableCircle.Speed.Y);

                    if (IsLogOn)
                        LogCollisionAfter("#" + index + verifiableCircle);
                }
            }
            else if (verifiableCircle.Y < verifiableCircle.Radius)
            {
                if (verifiableCircle.Speed.Y < 0)
                {
                    if (IsLogOn)
                        LogCollisionBefore("#" + index + verifiableCircle, "Top Border");

                    verifiableCircle.Speed = new Vector(verifiableCircle.Speed.X, -verifiableCircle.Speed.Y);

                    if (IsLogOn)
                        LogCollisionAfter("#" + index + verifiableCircle);


                }
            }
            else
            {
                foreach (var physicCircleB in _figureCollection)
                {
                    if (physicCircleB != verifiableCircle)
                    {
                        var distance = verifiableCircle.GetDistance(physicCircleB);
                        var absolutteDistance = distance - verifiableCircle.Radius - physicCircleB.Radius;
                        if (absolutteDistance <= 0)
                        {
                            var circleBIndex = _figureCollection.IndexOf(physicCircleB);

                            if (IsLogOn)
                                LogCollisionBefore("#" + index + verifiableCircle, "#" + circleBIndex + physicCircleB);

                            CollisionProc(verifiableCircle, physicCircleB);

                            if (IsLogOn)
                                LogCollisionAfter("#" + index + verifiableCircle, "#" + circleBIndex + verifiableCircle);

                        }
                    }
                }

            }

            NormolizeOne(verifiableCircle);
        }

        private static void CollisionProc(PhysicCircle circle1, PhysicCircle circle2)
        {
            var d = ClosestPointOnLine(circle1.X, circle1.Y,
                circle1.X + circle1.X, circle1.Y + circle1.Speed.Y,
                circle2.X, circle2.Y);

            var closestdistsq = Math.Pow(circle2.X - d.X, 2) + Math.Pow((circle2.Y - d.Y), 2);

            if (closestdistsq <= Math.Pow(circle1.Radius + circle2.Radius, 2))
            {
                var backdist = Math.Sqrt(Math.Pow(circle1.Radius + circle2.Radius, 2) - closestdistsq);
                var movementvectorlength = Math.Sqrt(Math.Pow(circle1.Speed.X, 2) + Math.Pow(circle1.Speed.Y, 2));
                if (movementvectorlength.Equals(0)) return;

                var cX = d.X - backdist * (circle1.Speed.X / movementvectorlength);
                var cY = d.Y - backdist * (circle1.Speed.Y / movementvectorlength);

                var collisiondist = Math.Sqrt(Math.Pow(circle2.X - cX, 2) + Math.Pow(circle2.Y - cY, 2));
                var nX = (circle2.X - cX) / collisiondist;
                var nY = (circle2.Y - cY) / collisiondist;
                var p = 2 * (circle1.Speed.X * nX + circle1.Speed.Y * nY) / (circle1.Mass + circle2.Mass);

                var vx1 = circle1.Speed.X - p * circle2.Mass * nX * 1;
                var vy1 = circle1.Speed.Y - p * circle2.Mass * nY * 1;
                var vx2 = circle2.Speed.X + p * circle1.Mass * nX * 1;
                var vy2 = circle2.Speed.Y + p * circle1.Mass * nY * 1;

                circle1.Speed = new Vector(vx1, vy1);
                circle2.Speed = new Vector(vx2, vy2);

                circle1.X = circle1.X + vx1;
                circle1.Y = circle1.Y + vy1;
                circle2.X = circle2.X + vx2;
                circle2.Y = circle2.Y + vy2;
            }
        }

        private static Point ClosestPointOnLine(double lx1, double ly1, double lx2, double ly2, double x0, double y0)
        {
            var a1 = ly2 - ly1;
            var b1 = lx1 - lx2;
            var c1 = (ly2 - ly1) * lx1 + (lx1 - lx2) * ly1;
            var c2 = -b1 * x0 + a1 * y0;
            var det = a1 * a1 - -b1 * b1;
            double cx;
            double cy;

            if (Math.Abs(det) > 0)
            {
                cx = (a1 * c1 - b1 * c2) / det;
                cy = (a1 * c2 - -b1 * c1) / det;
            }
            else
            {
                cx = x0;
                cy = y0;
            }
            return new Point(cx, cy);
        }


        private PhysicCircle NormolizeOne(PhysicCircle circle)
        {
            var isLeftOut = circle.X < circle.Radius;
            if (isLeftOut)
            {
                circle.X = circle.Radius;
            }
            else
            {
                var isRightOut = circle.X > Width - circle.Radius;
                if (isRightOut)
                    circle.X = Width - circle.Radius;
            }

            var isTopOut = circle.Y < circle.Radius;
            if (isTopOut)
            {
                circle.Y = circle.Radius;
            }
            else
            {
                var isBottomOut = circle.Y > Height - circle.Radius;
                if (isBottomOut)
                    circle.Y = Height - circle.Radius;
            }

            return circle;
        }

        private void NormolizeWightState()
        {
            foreach (var circle in _figureCollection)
            {
                var isAfterBorder = circle.X > Width;
                if (isAfterBorder)
                {
                    circle.X = Width - circle.Radius;
                }

            }
        }

        private void NormolizeHeightState()
        {
            foreach (var circle in _figureCollection)
            {
                var isAfterBorder = circle.Y > Height;
                if (isAfterBorder)
                {
                    circle.Y = Height - circle.Radius;
                }
            }
        }

        private void LogCollisionBefore(string infoFirst, string infoSecond)
        {
            if (_sr != null)
            {
                var result = new StringBuilder();

                result.Append("-------------------- Collision time: " + DateTime.Now.ToLongTimeString() +
                              "-------------------->\r\n");

                result.Append("***Before:  \r\n");
                result.Append(infoFirst + "\r\n----Collapsed with:\r\n" + infoSecond);
                result.Append("\r\n\r\n");
                    
                _sr.Write(result.ToString());
            }
        }

        private void LogCollisionAfter(params string[] objectsInfo)
        {
            if (_sr != null)
            {
                var result = new StringBuilder();

                result.Append("***After:  \r\n");
                foreach (var s in objectsInfo)
                {
                    result.Append(s + "\r\n");
                }

                result.Append("<------------------------------------------------------------>\r\n\r\n");
                _sr.Write(result.ToString());
                _sr.Flush();
            }
        }

        private void OnOffReader()
        {
            if (IsLogOn && _sr == null)
            {
                var patch = Directory.GetCurrentDirectory();
                _sr = new StreamWriter(patch + @"\log.txt");
            }

            if (IsLogOn == false && _sr != null)
            {
                _sr.Dispose();
                _sr = null;
            }

        }
        #endregion  
    }
}
