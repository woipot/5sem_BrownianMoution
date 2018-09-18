namespace BrownianMoution.Sources.figures
{
    public interface IPhysicsObject
    {

        int Mass
        {
            get;
            set;
        }

        int Speed
        {
            get;
            set;
        }

        int Angle
        {
            get;
            set;
        }
    }
}
