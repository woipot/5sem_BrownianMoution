using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Serialization;
using BrownianMoution.Sources.figures;
using BrownianMoution.Sources.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace BrownianMoution.Sources.MVVM.Models
{
    public class BMoutionState : BindableBase
    {
        private int _height = 400;
        private int _weight = 670;
        private readonly ObservableCollection<PhysicCircle> _figureCollection;

        public int Height
        {
            get => _height;
            set => _height = value;
        }
        public int Weight
        {
            get => _weight;
            set => _weight = value;
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



        private void DetectAndRunCollision(PhysicCircle verifiableCircle)
        {
            if (verifiableCircle.X > _weight - verifiableCircle.Radius)
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
    }
}
