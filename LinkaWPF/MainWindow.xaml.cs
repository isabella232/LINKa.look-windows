﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tobii.Interaction;
using Tobii.Interaction.Wpf;

namespace LinkaWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Models.Card> _cards;
        private List<CardButton> _buttons;
        private int _currentPage;
        private int _countPages;
        private int _gridSize;
        private int _rows;
        private int _columns;
        private CircularProgressBar _progress;
        private Storyboard _sb;

        private Host _host;
        private WpfInteractorAgent _agent;
        private GazePointDataStream _lightlyFilteredGazePointDataStream;

        private EyePositionStream _eyePositionStream;
        private GazePointDataStream _gazePointDataStream;
        private FixationDataStream _fixationDataStream;
        private GazePointData _gazePointData;
        private FixationData _fixationData;

        public MainWindow()
        {
            InitializeComponent();

            InitTobii();

            Closed += MainWindow_Closed;
            MouseMove += MainWindow_MouseMove;

            Init();
            Render();
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            //ellipse.Margin = new Thickness(0 - (mainGrid.ActualWidth / 2), 250 - (mainGrid.ActualHeight - 10), 0, 0);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // we will close the coonection to the Tobii Engine before exit.
            _host.DisableConnection();
        }

        private void InitTobii()
        {
            // Everything starts with initializing Host, which manages connection to the 
            // Tobii Engine and provides all Tobii Core SDK functionality.
            // NOTE: Make sure that Tobii.EyeX.exe is running
            _host = new Host();

            // We need to instantiate InteractorAgent so it could control lifetime of the interactors.
            _agent = _host.InitializeWpfAgent();

            _lightlyFilteredGazePointDataStream = _host.Streams.CreateGazePointDataStream();

            // Наблюдение за состоянием глаз
            _eyePositionStream = _host.Streams.CreateEyePositionStream();
            _eyePositionStream.Next += _eyePositionStream_Next;

            _gazePointDataStream = _host.Streams.CreateGazePointDataStream();
            _gazePointDataStream.Next += _gazePointDataStream_Next;

            _fixationDataStream = _host.Streams.CreateFixationDataStream();
            _fixationDataStream.Next += _fixationDataStream_Next;
            
        }

        private void _eyePositionStream_Next(object sender, StreamData<EyePositionData> e)
        {
            if (e.Data.HasLeftEyePosition == false && e.Data.HasRightEyePosition == false)
            {
                Dispatcher.Invoke(() =>
                {
                    stopClick();
                });
            }
        }

        private void _gazePointDataStream_Next(object sender, StreamData<GazePointData> e)
        {
            _gazePointData = e.Data;

            RenderData();
        }

        private void _fixationDataStream_Next(object sender, StreamData<FixationData> e)
        {
            _fixationData = e.Data;

            RenderData();
        }

        private void RenderData()
        {
            Dispatcher.Invoke(() =>
            {
                text.Text = "GazePointDataX: " + _gazePointData.X + " GazePointDataY: " + _gazePointData.Y + " PointDataX: " + _fixationData.X + " PointDataY: " + _fixationData.Y;
            });
        }

        private void Init()
        {
            this._currentPage = 0;
            this._rows = 6;
            this._columns = 6;
            this._gridSize = this._rows * this._columns;

            for (var i = 0; i < this._rows; i++)
            {
                var rowDefinition = new RowDefinition();
                gridCard.RowDefinitions.Add(rowDefinition);
            }

            for (var i = 0; i < this._columns; i++)
            {
                var columnDefinition = new ColumnDefinition();
                gridCard.ColumnDefinitions.Add(columnDefinition);
            }

            this._cards = new List<Models.Card>() {
                // Page one
                new Models.Card(0, "One", "1.png"),
                new Models.Card(1, "Two", "2.png"),
                new Models.Card(2, "Three", "3.png"),
                new Models.Card(3, "Four", "4.png"),
                new Models.Card(4, "Five", "5.png"),
                new Models.Card(5, "Six", "6.png"),
                new Models.Card(6, "Seven", "7.png"),
                new Models.Card(7, "Eight", "8.png"),
                new Models.Card(8, "Nine", "9.png"),
                new Models.Card(9, "Nine", "9.png"),
                new Models.Card(10, "Sleep", "sleep.gif"),
                new Models.Card(11, "Sleep", "eat.gif")
            };

            // Рассчитываем максимальное количество страниц
            this._countPages = Convert.ToInt32(Math.Round(Convert.ToDouble(this._cards.Count) / this._gridSize, 0));

            this._buttons = new List<CardButton>();

            // Создаем кнопки и раскладываем их по клеткам таблицы
            for (var i = 0; i < this._gridSize; i++)
            {
                var button = new CardButton();
                button.Click += new RoutedEventHandler(cardButton_Click);
                button.HazGazeChanged += new RoutedEventHandler(cardButton_HazGazeChanged);
                button.MouseEnter += new MouseEventHandler(cardButton_MouseEnter);
                button.MouseLeave += new MouseEventHandler(cardButton_MouseLeave);

                var row = Convert.ToInt32(Math.Round(Convert.ToDouble(i / this._rows), 0));
                int column = i - (this._rows * row);

                this.gridCard.Children.Add(button);
                Grid.SetRow(button, row);
                Grid.SetColumn(button, column);

                this._buttons.Add(button);
            }

            _progress = new CircularProgressBar();
            _progress.StrokeThickness = 6;
            _progress.HorizontalAlignment = HorizontalAlignment.Center;
            _progress.VerticalAlignment = VerticalAlignment.Center;
            _progress.Visibility = Visibility.Hidden;

            var animation = new DoubleAnimation(0, 100, TimeSpan.FromSeconds(3));
            animation.Completed += new EventHandler((o, args) => {
                stopClick();
            });
            Storyboard.SetTarget(animation, _progress);
            Storyboard.SetTargetProperty(animation, new PropertyPath(CircularProgressBar.PercentageProperty));

            _sb = new Storyboard();
            _sb.Children.Add(animation);
        }

        private void Render()
        {
            for (var i = this._currentPage * this._gridSize; i < this._currentPage * this._gridSize + this._gridSize; i++)
            {
                Models.Card card = null;
                if (i >= 0 && i < this._cards.Count)
                {
                    card = this._cards[i];
                }
                var count = i - this._currentPage * this._gridSize;
                this._buttons[count].Card = card;
            }
        }

        private void NextPage()
        {
            if (this._currentPage == this._countPages - 1)
            {
                this._currentPage = 0;
            }
            else
            {
                this._currentPage++;
            }
            Render();
        }

        private void PrevPage()
        {
            if (this._currentPage - 1 < 0)
            {
                this._currentPage = this._countPages - 1;
            }
            else
            {
                this._currentPage--;
            }
            Render();
        }

        private void cardButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as CardButton;
            text.Text = button.Card.Title;
        }

        private void cardButton_HazGazeChanged(object sender, RoutedEventArgs e)
        {
            var button = sender as CardButton;
            startClick(button);
        }

        private void startClick(CardButton button)
        {
            if (_progress.Parent != null)
            {
                (_progress.Parent as Grid).Children.Remove(_progress);
            }

            // Добавляем прогресс на карточку
            button.grid.Children.Add(_progress);

            _progress.Radius = Convert.ToInt32((button.ActualHeight - 20) / 2);
            _progress.Visibility = Visibility.Visible;

            _sb.Stop();
            _sb.Begin();
        }

        private void stopClick()
        {
            _progress.Visibility = Visibility.Hidden;
            _sb.Stop();
        }

        private void cardButton_MouseEnter(object sender, RoutedEventArgs e)
        {
            var button = sender as CardButton;

            startClick(button);
        }

        private void cardButton_MouseLeave(object sender, RoutedEventArgs e)
        {
            stopClick();
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            // Prev
            this.PrevPage();

            GC.Collect();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            // Next
            this.NextPage();

            GC.Collect();
        }
    }
}
