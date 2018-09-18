using System.Collections.ObjectModel;
using BrownianMoution.Sources.figures;
using BrownianMoution.Sources.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace BrownianMoution.Sources.MVVM.Models
{
    public class BrownianMoution : BindableBase
    {
        private readonly ObservableCollection<IPhysicFigure> _figureCollection;

        public BrownianMoution()
        {
            _figureCollection = new ObservableCollection<IPhysicFigure>();
        }


        public void AddValue(IPhysicFigure figure)
        {
            _figureCollection.Add(figure);
            //RaisePropertyChanged("Sum");
        }

        public void RemoveValue(int index)
        {
            if (index >= 0 && index < _figureCollection.Count) _figureCollection.RemoveAt(index);
            //RaisePropertyChanged("Sum");
        }

    }
}
