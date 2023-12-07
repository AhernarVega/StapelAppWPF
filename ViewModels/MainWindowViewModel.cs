using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.Painting;

using ManagedNativeWifi;

using Microsoft.Win32;

using SkiaSharp;

using StapelAppWPF.Infrastructure.Commands;
using StapelAppWPF.Models;
using StapelAppWPF.ViewModels.Base;
using ScottPlot.Styles;

namespace StapelAppWPF.ViewModels
{
    internal class MainWindowViewModel : ImplementedINPC
    {
        #region КОД АА
        #region Для отображения данных
        //// Массив отображаемых данных
        //private ObservableCollection<Unit> show;
        //// Макисмальное количество данных, отображаемых на графике
        //private readonly int lengthShow;
        //// Макисмальное количество данных, отображаемых на графике спектрального анализа
        //private readonly int spectrumAnalisisCount;
        //// Изменение масштаба графика (нужно вынести во viewModel)
        //private int lengthShowSpectrumAnalisisCount;
        //public int LengthShowSpectrumAnalisisCount
        //{
        //    get => lengthShowSpectrumAnalisisCount;
        //    set => Set(ref lengthShowSpectrumAnalisisCount, Convert.ToInt32(value));
        //}
        #endregion
        #endregion

        #region DEBUG
        //ObservableCollection<double> ser;
        //Random rand = new();
        #endregion DEBUG

        #region Свойства

        // !!! СВОЙСТВО ГЛАВНОЙ МОДЕЛИ !!! //

        private MainModel mainModel;

        // !!! СВОЙСТВО ГЛАВНОЙ МОДЕЛИ !!! //

        #region ВИЗУАЛЬНОЕ ОФОРМЛЕНИЕ

        #region КНОПКА ПОДКЛЮЧЕНИЯ
        private Brush connectColor;
        public Brush ConnectColor { get => connectColor; set => Set(ref connectColor, value); }

        private string connectText;
        public string ConnectText
        {
            get => connectText;
            set => Set(ref connectText, value);
        }
        #endregion КНОПКА ПОДКЛЮЧЕНИЯ

        #region ПЕРЕКЛЮЧЕНИЕ ОТОБРАЖЕНИЯ ГРАФИКОВ
        private readonly int smallHeight = 200;
        private readonly int middleHeight = 300;
        private readonly int largeHeight = 400;
        #region VISIBILITY
        // Отображение различных графиков
        private Visibility showRpmChart;
        private Visibility showHarmChart;

        public Visibility ShowRpmChart { get => showRpmChart; set => Set(ref showRpmChart, value); }
        public Visibility ShowHarmChart { get => showHarmChart; set => Set(ref showHarmChart, value); }
        #endregion VISIBILITY
        #region checkBoxes ОТОБРАЖЕНИЯ ГРАФИКОВ
        // Для checkBox - ов
        private bool checkRpmChart;
        private bool checkHarmChart;

        public bool CheckRpmChart
        {
            get => checkRpmChart;
            set
            {
                Set(ref checkRpmChart, value);
                if (checkRpmChart)
                {
                    SetHeights();
                    ShowRpmChart = Visibility.Visible;
                }
                else if (!checkRpmChart)
                {
                    SetHeights();
                    ShowRpmChart = Visibility.Collapsed;
                }
            }
        }
        public bool CheckHarmChart
        {
            get => checkHarmChart;
            set
            {
                Set(ref checkHarmChart, value);
                if (checkHarmChart)
                {
                    SetHeights();
                    ShowHarmChart = Visibility.Visible;
                }
                else if (!checkHarmChart)
                {
                    SetHeights();
                    ShowHarmChart = Visibility.Collapsed;
                }
            }
        }
        #endregion checkBoxes ОТОБРАЖЕНИЯ ГРАФИКОВ
        #region ВЫСОТЫ ГРАФИКОВ ДЛЯ ПОЯВЛЕНИЯ/СКРЫТИЯ
        // Высоты графиков для их скрытия
        private int heightRpmOscilChart;
        private int heightRpmChart;
        private int heightHarmChart;

        public int HeightRpmOscilChart { get => heightRpmOscilChart; set => Set(ref heightRpmOscilChart, value); }
        public int HeightRpmChart { get => heightRpmChart; set => Set(ref heightRpmChart, value); }
        public int HeightHarmChart { get => heightHarmChart; set => Set(ref heightHarmChart, value); }
        #endregion ВЫСОТЫ ГРАФИКОВ ДЛЯ ПОЯВЛЕНИЯ/СКРЫТИЯ
        #region ОТОБРАЖЕНИЕ РАЗДЕЛИТЕЛЕЙ
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        #endregion ОТОБРАЖЕНИЕ РАЗДЕЛИТЕЛЕЙ
        #endregion ПЕРЕКЛЮЧЕНИЕ ОТОБРАЖЕНИЯ ГРАФИКОВ

        // Коллекция значений для отображения графика
        //private ISeries[] rpmShowCollection;
        //public ISeries[] RpmShowCollection { get => rpmShowCollection; set => Set(ref rpmShowCollection, value); }

        #endregion ВИЗУАЛЬНОЕ ОФОРМЛЕНИЕ




        // Диаграмма спектрального анализа
        public ObservableCollection<int>? spectrum;

        private static int NUM_READ = 5;  // порядок медианы
        private static int[] buffer = new int[5];  // статический буфер
        private static byte count = 0;

        // Диалоговые окна (Возможно объекты лучше перенести в команды-обработчики)
        private SaveFileDialog saveDialog = new SaveFileDialog();
        private OpenFileDialog openDialog = new OpenFileDialog();

        #endregion

        #region ФУНКЦИИ 

        public void SetHeights()
        {
            if (checkRpmChart && !checkHarmChart)
            {
                HeightRpmOscilChart = largeHeight;
                HeightRpmChart = middleHeight;
            }
            else if (checkHarmChart && !checkRpmChart)
            {
                HeightRpmOscilChart = largeHeight;
                HeightHarmChart = middleHeight;
            }
            else if (checkRpmChart && checkHarmChart)
            {
                HeightRpmOscilChart = middleHeight;
                HeightRpmChart = smallHeight - 50;
                HeightHarmChart = smallHeight;
            }
            else
            {
                HeightRpmOscilChart = largeHeight;
                HeightRpmChart = HeightHarmChart = 0;
            }
        }

        #endregion ФУНКЦИИ

        #region КОМАНДЫ

        #region DEBUG
        #region (ДЕБАГ) ДОБАВЛЕНИЕ ТОЧКИ ЧАРТУ

        public ICommand PointAddCmd { get; }

        private void OnPointAddCmdExecuted(object param)
        {
            //ser.Remove(ser.First());
            //ser.Add(rand.Next(-100, 100));
        }
        #endregion (ДЕБАГ) ДОБАВЛЕНИЕ ТОЧКИ ЧАРТУ

        #endregion DEBUG 

        #region КОМАНДА ПЕРЕКЛЮЧЕНИЯ ПОДКЛЮЧЕНИЯ
        public ICommand SwitchConnectionCmd { get; }

        private void OnSwitchConnectionCmdExecuted(object param)
        {
            var connections = NativeWifi.EnumerateInterfaceConnections();
            if (connections.Any())
            {
                if (connectText == "Подключиться" || connectText == "ОШИБКА")
                {
                    ConnectColor = Brushes.Orange;
                    ConnectText = "Отключиться";
                    mainModel.ReceiveData = true;
                }
                else if (connectText == "Отключиться")
                {
                    ConnectColor = Brushes.Green;
                    ConnectText = "Подключиться";
                    mainModel.ReceiveData = false;
                }
                else
                {
                    ConnectColor = Brushes.Red;
                    ConnectText = "ОШИБКА";
                    mainModel.ReceiveData = false;
                }
                return;
            }
            else if (!connections.Any())
            {
                MessageBox.Show("Не удалось обнаружить подключенную сеть WiFi");
            }
            else
            {
                MessageBox.Show("Непредвиденная ошибка");
            }

            ConnectColor = Brushes.Red;
            ConnectText = "ОШИБКА";
        }

        private bool CanSwitchConnectionCmdExecute(object param)
        {
            bool checkWifi = false;

            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.NetworkInterfaceType.ToString().Contains("Wireless"))
                    checkWifi = true;
            }

            if (checkWifi)
            {
                foreach (var connectionName in NativeWifi.EnumerateInterfaceConnections())
                {
                    if (connectionName.ProfileName == "Torsiograph_V1.4")
                    {
                        if (connectText == "ОШИБКА")
                            ConnectText = "Подключиться";
                        if (connectText == "Подключиться")
                            ConnectColor = Brushes.Green;
                        return true;
                    }
                }
            }

            ConnectColor = Brushes.DarkGray;
            return false;
        }
        #endregion КОМАНДА ПЕРЕКЛЮЧЕНИЯ ПОДКЛЮЧЕНИЯ



        #endregion КОМАНДЫ

        public MainWindowViewModel()
        {
            #region DEBUG
            //ser = new();

            //for (int i = 0; i < 50; i++)
            //{
            //    ser.Add(rand.Next(-100, 100));
            //}

            //rpmShowCollection = new ISeries[]
            //{
            //    new LineSeries<double>
            //    {
            //        Values = ser,
            //        Fill = null,
            //    }
            //};
            #endregion DEBUG

            #region ИНИЦИАЛИЗАЦИЯ ПОЛЕЙ 
            #region ИНИЦИАЛИЗАЦИЯ ГЛАВНОЙ МОДЕЛИ
            mainModel = new();
            #endregion ИНИЦИАЛИЗАЦИЯ ГЛАВНОЙ МОДЕЛИ

            #region ИНИЦИАЛИЗАЦИЯ ПОЛЕЙ ВИЗУАЛЬНОГО ОФОРМЛЕНИЯ
            #region КНОПКА ПОДКЛЮЧЕНИЯ
            connectText = "Подключиться";
            connectColor = Brushes.Green;
            connectColor = Brushes.Green;
            #endregion КНОПКА ПОДКЛЮЧЕНИЯ
            #region ВИЗАУЛЬНАЯ ЧАСТЬ ГРАФИКОВ
            showRpmChart = Visibility.Hidden;
            showHarmChart = Visibility.Hidden;

            heightRpmOscilChart = largeHeight;
            heightRpmChart = 0;
            heightHarmChart = 0;

            XAxes = new Axis[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    SeparatorsPaint = new SolidColorPaint(SKColors.DarkGray)
                    {
                        StrokeThickness = 2
                    }
                }
            };

            YAxes = new Axis[]
                {
                new Axis
                {
                    TextSize = 14,
                    LabelsPaint = new SolidColorPaint(SKColors.Black),
                    SeparatorsPaint = new SolidColorPaint(SKColors.DarkGray)
                    {
                        StrokeThickness = 2,
                        PathEffect = new DashEffect(new float[] { 3, 3 })
                    }
                }
                };
            #endregion ВИЗАУЛЬНАЯ ЧАСТЬ ГРАФИКОВ

            #endregion ИНИЦИАЛИЗАЦИЯ ПОЛЕЙ ВИЗУАЛЬНОГО ОФОРМЛЕНИЯ

            // Инициализация диаграммы спектрального анализа
            spectrum = new();
            #endregion ИНИЦИАЛИЗАЦИЯ ПОЛЕЙ

            #region ИНИЦИАЛИЗАЦИЯ КОМАНД
            SwitchConnectionCmd = new LambdaCommand(OnSwitchConnectionCmdExecuted, CanSwitchConnectionCmdExecute);
            PointAddCmd = new LambdaCommand(OnPointAddCmdExecuted);
            #endregion ИНИЦИАЛИЗАЦИЯ КОМАНД
        }
    }
}
