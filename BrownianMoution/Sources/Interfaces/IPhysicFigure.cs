using System;

namespace BrownianMoution.Sources.Interfaces
{
    public interface IPhysicFigure : IFigure, IPhysicsObject
    {
        void Move();

        double GetDistance(IPhysicFigure figureB);
    }
}
