using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using BrownianMoution.Sources.figures;
using BrownianMoution.Sources.MVVM.Models;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace BrownianMoution.Sources.MVVM
{
    public class MainVM : BindableBase
    {
        private readonly BMoutionState _moutionModel;
        private Timer _timer;

        private Ellipse _selectedEllipse;


        #region Properties

        public IEnumerable<PhysicCircle> FigureCollection => _moutionModel.FigureCollection;

        public int Weidth
        {
            get => _moutionModel.Weidth;
            set => _moutionModel.Weidth = Math.Max(1, value);
        }

        public int Height
        {
            get => _moutionModel.Height;
            set => _moutionModel.Height = Math.Max(1, value);
        }

        public PhysicCircle SelectedCircle
        {
            get => _selectedEllipse.DataContext as PhysicCircle;

            set => _selectedEllipse.DataContext = value;
        }

        public bool IsEnabledSelect => _selectedEllipse != null;

        public int CircleCount => _moutionModel.FigureCollection.Count;

        public PhysicCircle CirclePrefab { get; set; }

        public bool UseMouseCords { get; set; } = true;

        public bool IsLogOn
        {
            get => _moutionModel.IsLogOn;
            set => _moutionModel.IsLogOn = value;
        }

        #endregion


        #region Constructors

        public MainVM()
        {
            _moutionModel = new BMoutionState();
            _moutionModel.PropertyChanged += (s, e) => { OnPropertyChanged(e.PropertyName); };


            _timer = new Timer {Interval = 10};
            _timer.Tick += _moutionModel.Tick;

            _selectedEllipse = new Ellipse();
            _selectedEllipse.DataContext = new PhysicCircle();

            CirclePrefab = new PhysicCircle();


            AddCommand = new DelegateCommand(() =>
            {
                if (UseMouseCords)
                {
                    var mousePos = Mouse.GetPosition(Application.Current.MainWindow);
                    CirclePrefab.X = mousePos.X;
                    CirclePrefab.Y = mousePos.Y;
                }

                _moutionModel.AddValue((PhysicCircle)CirclePrefab.Clone());

                OnPropertyChanged("CircleCount");
            });

            RemoveCommand = new DelegateCommand(() =>
            {
                _moutionModel.RemoveValue(SelectedCircle);
                SelectedCircle = new PhysicCircle();
                OnPropertyChanged("CircleCount");
            });

            EnableTimer = new DelegateCommand(() =>
            {
                _timer.Enabled = !_timer.Enabled;
            });

            SelectCommand = new DelegateCommand<MouseButtonEventArgs>(Select);

            SaveCommand = new DelegateCommand(SaveInFile);
            LoadCommand = new DelegateCommand(LoadFromFile);
        }

        #endregion
        
        
        #region Commands

        public DelegateCommand AddCommand { get; }
        public DelegateCommand RemoveCommand { get; }
        public DelegateCommand EnableTimer { get; }
        public DelegateCommand<MouseButtonEventArgs> SelectCommand { get; }

        public DelegateCommand SaveCommand { get; }
        public DelegateCommand LoadCommand { get; }

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

        private void SaveInFile()
        {
            var saveFileDialog = new SaveFileDialog {Filter = @"brounMoution ini (*.xml)|*.xml"};

            if (saveFileDialog.ShowDialog() == true)
            {
                var patch = saveFileDialog.FileName;
                _moutionModel.SaveState(patch);
            }
        }

        private void LoadFromFile()
        {
            var openFileDialog = new OpenFileDialog { Filter = @"brounMoution ini (*.xml)|*.xml" };

            if (openFileDialog.ShowDialog() == true)
            {
                var patch = openFileDialog.FileName;
                try
                {
                    _moutionModel.LoadState(patch);
                    OnPropertyChanged("CircleCount");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Eror in file : " + e.Message);
                }
            }
        }
        #endregion
    }
}
