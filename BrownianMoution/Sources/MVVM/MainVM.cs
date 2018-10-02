using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using BrownianMoution.Sources.figures;
using BrownianMoution.Sources.Interfaces;
using BrownianMoution.Sources.MVVM.Models;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace BrownianMoution.Sources.MVVM
{
    public class MainVM : BindableBase
    {
        private readonly BMoutionState _moutionModel;
        private DispatcherTimer _timer;

        private Ellipse _selectedEllipse;

        private PhysicCircle _selectedCircle;


        #region Properties

        public ObservableCollection<PhysicCircle> FigureCollection => _moutionModel.FigureCollection;

        public int Weidth => _moutionModel.Weight;
        public int Height => _moutionModel.Height;

        public PhysicCircle SelectedCircle
        {
            get => _selectedEllipse.DataContext as PhysicCircle;

            set => _selectedEllipse.DataContext = value;
        }

        public bool IsEnabledSelect => _selectedEllipse != null; 

        #endregion


        #region Constructors

        public MainVM()
        {
            _moutionModel = new BMoutionState();
            _moutionModel.PropertyChanged += (s, e) => { OnPropertyChanged(e.PropertyName); };


            _timer = new DispatcherTimer { Interval = new TimeSpan(10000) };
            _timer.Tick += _moutionModel.Tick;

            _selectedEllipse = new Ellipse();
            _selectedEllipse.DataContext = new PhysicCircle();


            AddCommand = new DelegateCommand(() =>
           {
               var mousePos = Mouse.GetPosition(Application.Current.MainWindow);
               var figure = new PhysicCircle((int)mousePos.X, (int)mousePos.Y);

               _moutionModel.AddValue(figure);
           });

            RemoveCommand = new DelegateCommand<int?>(i =>
            {
                if (i.HasValue)
                    _moutionModel.RemoveValue(i.Value);
            });

            EnableTimer = new DelegateCommand(() =>
            {

                _timer.IsEnabled = !_timer.IsEnabled;
            });

            SelectCommand = new DelegateCommand<MouseButtonEventArgs>(Select);
        }

        #endregion
        
        
        #region Commands

        public DelegateCommand AddCommand { get; }
        public DelegateCommand<int?> RemoveCommand { get; }
        public DelegateCommand EnableTimer { get; }
        public DelegateCommand<MouseButtonEventArgs> SelectCommand { get; }

        #endregion


        #region Private Methods

        private void Select(MouseButtonEventArgs args)
        {
            var currentEllipse = args.OriginalSource as Ellipse;


            if (currentEllipse != null)
            {
                currentEllipse.Fill = Brushes.Chartreuse;

                if (_selectedEllipse != null)
                    _selectedEllipse.Fill = Brushes.Beige;

                _selectedEllipse = currentEllipse;
                OnPropertyChanged("SelectedCircle");
            }

        }

        #endregion
    }
}
