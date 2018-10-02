using System;
using System.Windows;
using BrownianMoution.Sources.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace BrownianMoution.Sources.figures
{
    public class PhysicCircle : BindableBase, IPhysicFigure
    {
        private int _mass;
        private int _radius;
        private Vector _speed;

        public int Mass
        {
            get => _mass;
            set
            {
                _mass = Math.Max(1, value); 
            }
        }
        public int Radius
        {
            get => _radius;
            set
            {
                _radius = Math.Max(1, value); 
            }
        }

        public double X { get; set; }
        public double Y { get; set; }

        public double Left => X - _radius;
        public double Top => Y - _radius;


        public double Size => Radius * 2;

        public Vector Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged("SpeedX");
                OnPropertyChanged("SpeedY");
            }
        }

        public double SpeedX
        {
            get => _speed.X;
            set => _speed.X = value;
        }

        public double SpeedY
        {
            get => _speed.Y;
            set => _speed.Y = value;
        }

        public void Move()
        {
            X += Speed.X;
            Y += Speed.Y;
        }

        public double GetDistance(IPhysicFigure figureB)
        {
            var x = Math.Pow(X - figureB.X, 2);
            var y = Math.Pow(Y - figureB.Y, 2);

            var result = Math.Sqrt(x + y);
            return result;
        }

        public PhysicCircle(double x = 0, double y = 0, int mass = 1, int radius = 15, int speed = 1)
        {
            Mass = mass;
            Radius = radius;
            X = x;
            Y = y;
            Speed = new Vector(speed, speed);
        }
    }
}
