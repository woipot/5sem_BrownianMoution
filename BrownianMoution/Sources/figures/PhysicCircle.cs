using System;
using BrownianMoution.Sources.Interfaces;

namespace BrownianMoution.Sources.figures
{
    public class PhysicCircle : IPhysicFigure
    {
        private int _mass;
        private int _radius;

        public int Mass
        {
            get => _mass;
            set => _mass = Math.Max(1, value);
        }
        public int Radius
        {
            get => _radius;
            set => _radius = Math.Max(1, value);
        }
        public int X { get; set; }
        public int Y { get; set; }
        public int Speed { get; set; }
        public int Angle { get; set; }


        public PhysicCircle(int mass = 1, int radius = 1, int x = 0, int y = 0, int speed = 0, int angle = 0)
        {
            Mass = mass;
            Radius = radius;
            X = x;
            Y = y;
            Speed = speed;
            Angle = angle;
        }


    }
}
