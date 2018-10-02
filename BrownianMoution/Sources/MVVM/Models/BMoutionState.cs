using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Xml;
using BrownianMoution.Sources.figures;
using Microsoft.Practices.Prism.Mvvm;

namespace BrownianMoution.Sources.MVVM.Models
{
    public class BMoutionState : BindableBase
    {
        private int _height = 400;
        private int _weidth = 670;
        private readonly ObservableCollection<PhysicCircle> _figureCollection;

        public int Height
        {
            get => _height;
            set
            {
                _height = value; 
                NormolizeHeightState();
            }
        }

        public int Weidth
        {
            get => _weidth;
            set
            {
                _weidth = value; 
                NormolizeWightState();
            }
        }

        public ObservableCollection<PhysicCircle> FigureCollection => _figureCollection;


        public BMoutionState()
        {
            _figureCollection = new ObservableCollection<PhysicCircle>();
        }


        public void AddValue(PhysicCircle figure)
        {
            _figureCollection.Add(figure);
            OnPropertyChanged("Add");
        }

        public void RemoveValue(int index)
        {
            if (index >= 0 && index < _figureCollection.Count) _figureCollection.RemoveAt(index);
            OnPropertyChanged("Remove");
        }

        public void Tick(object sender, EventArgs args)
        {
            foreach (var circleA in _figureCollection)
            {
                circleA.Move();
                DetectAndRunCollision(circleA);

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
            weightAttr.InnerText = _weidth.ToString();
            root.Attributes.Append(weightAttr);

            var heightAttr = doc.CreateAttribute("Height");
            heightAttr.InnerText = _height.ToString();
            root.Attributes.Append(heightAttr);

            foreach (var circle in _figureCollection)
            {
                var circleNode = doc.CreateElement("Circle");

                AddChildNode("Radius", circle.Radius.ToString(), circleNode, doc);
                AddChildNode("Mass",   circle.Mass.ToString(), circleNode, doc);
                AddChildNode("VX", circle.SpeedX.ToString(CultureInfo.InvariantCulture), circleNode, doc);
                AddChildNode("VY", circle.SpeedY.ToString(CultureInfo.InvariantCulture), circleNode, doc);
                AddChildNode("X", circle.X.ToString(CultureInfo.InvariantCulture), circleNode, doc);
                AddChildNode("Y", circle.Y.ToString(CultureInfo.InvariantCulture), circleNode, doc);

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

                return;
            }

            var weidth = Convert.ToInt32(root.Attributes["Weidth"].InnerText);
            var height = Convert.ToInt32(root.Attributes["Height"].InnerText);

            Height = height;
            Weidth = weidth;

            foreach (var child in root.ChildNodes)
            {
                var node = child as XmlNode;
                if (node != null)
                {
                    var radius = Convert.ToInt32(node["Radius"].InnerText);
                    var mass = Convert.ToInt32(node["Mass"].InnerText);
                    var vx = Convert.ToInt32(node["VX"].InnerText);
                    var vy = Convert.ToInt32(node["VY"].InnerText);
                    var x = Convert.ToInt32(node["X"].InnerText);
                    var y = Convert.ToInt32(node["Y"].InnerText);

                    var loadedFigure = new PhysicCircle(x, y, mass, radius, vx, vy);
                    _figureCollection.Add(loadedFigure);
                }
            }
        }



        private static void AddChildNode(string childName, string childText, XmlElement parentNode, XmlDocument doc)
        {
            var child = doc.CreateElement(childName);
            child.InnerText = childText;
            parentNode.AppendChild(child);
        }

        private void DetectAndRunCollision(PhysicCircle verifiableCircle)
        {
            if (verifiableCircle.X > _weidth - verifiableCircle.Radius)
            {
                if (verifiableCircle.Speed.X > 0)
                {
                    verifiableCircle.Speed = new Vector(-verifiableCircle.Speed.X, verifiableCircle.Speed.Y);
                }

            }
            else if (verifiableCircle.X < verifiableCircle.Radius)
            {
                if (verifiableCircle.Speed.X < 0)
                { 
                    verifiableCircle.Speed = new Vector(-verifiableCircle.Speed.X, verifiableCircle.Speed.Y);
                }

            }
            else if (verifiableCircle.Y > _height - verifiableCircle.Radius)
            {
                if (verifiableCircle.Speed.Y > 0)
                {
                    verifiableCircle.Speed = new Vector(verifiableCircle.Speed.X, -verifiableCircle.Speed.Y);
                }
            }
            else if (verifiableCircle.Y < verifiableCircle.Radius)
            {
                if (verifiableCircle.Speed.Y < 0)
                {
                    verifiableCircle.Speed = new Vector(verifiableCircle.Speed.X, -verifiableCircle.Speed.Y);
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
                        if ( absolutteDistance <= 0)
                        {
                            CollisionProc(verifiableCircle, physicCircleB);

                        }
                    }
                }
               
            }
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

        private void NormolizeWightState()
        {
            foreach (var circle in _figureCollection)
            {
                var isAfterBorder = circle.X > Weidth;
                if (isAfterBorder)
                {
                    circle.X = Weidth - circle.Radius;
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
    }
}
