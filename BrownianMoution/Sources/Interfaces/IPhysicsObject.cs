using System.Windows;

namespace BrownianMoution.Sources.Interfaces
{
    public interface IPhysicsObject
    {
        int Mass
        {
            get;
            set;
        }

        Vector Speed
        {
            get;
            set;
        }

       
    }
}
