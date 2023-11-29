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

using LiveCharts;
using LiveCharts.Wpf;

using ManagedNativeWifi;

using Microsoft.Win32;

using ScottPlot;

using StapelAppWPF.Infrastructure.Commands;
using StapelAppWPF.Models;
using StapelAppWPF.ViewModels.Base;


namespace StapelAppWPF.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
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
            set
            {
                if (connectText == "Подключиться")
                {
                    ConnectColor = Brushes.Orange;
                    Set(ref connectText, "Отключиться");
                }
                else if (connectText == "Отключиться")
                {
                    ConnectColor = Brushes.Green;
                    Set(ref connectText, "Подключиться");
                }
                else
                {
                    ConnectColor = Brushes.Red;
                    Set(ref connectText, "ОШИБКА");
                }
            }
        }
        #endregion КНОПКА ПОДКЛЮЧЕНИЯ

        #region ПЕРЕКЛЮЧЕНИЕ ОТОБРАЖЕНИЯ ГРАФИКОВ
        // Отображение различных графиков
        private Visibility showOscilChart;
        private Visibility showHarmChart;

        public Visibility ShowOscilChart { get => showOscilChart; set => Set(ref showOscilChart, value); }
        public Visibility ShowHarmChart { get => showHarmChart; set => Set(ref showHarmChart, value); }

        // Для checkBox - ов
        private bool checkOscilChart;
        private bool checkHarmChart;

        public bool CheckOscilChart
        {
            get => checkOscilChart;
            set
            {
                Set(ref checkOscilChart, value);
                if(checkOscilChart)
                {
                    ShowOscilChart = Visibility.Visible;
                }
                else if (!checkOscilChart)
                {
                    ShowOscilChart = Visibility.Collapsed;
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
                    ShowHarmChart = Visibility.Visible;
                }
                else if (!checkHarmChart)
                {
                    ShowHarmChart = Visibility.Collapsed;
                }
            }
        }
        #endregion ПЕРЕКЛЮЧЕНИЕ ОТОБРАЖЕНИЯ ГРАФИКОВ

        #region ДЛЯ СКРОЛЛА ГРАФИКА

        #endregion ДЛЯ СКРОЛЛА ГРАФИКА

        // Коллекция значений для отображения графика
        private SeriesCollection rpmShowCollection;
        public SeriesCollection RpmShowCollection { get => rpmShowCollection; set => Set(ref rpmShowCollection, value); }



        #endregion ВИЗУАЛЬНОЕ ОФОРМЛЕНИЕ

        #region Для отображения данных
        // Массив отображаемых данных
        private ObservableCollection<Unit> show;
        // Макисмальное количество данных, отображаемых на графике
        private readonly int lengthShow;
        // Макисмальное количество данных, отображаемых на графике спектрального анализа
        private readonly int spectrumAnalisisCount;
        // Изменение масштаба графика (нужно вынести во viewModel)
        private int lengthShowSpectrumAnalisisCount;
        public int LengthShowSpectrumAnalisisCount
        {
            get => lengthShowSpectrumAnalisisCount;
            set => Set(ref lengthShowSpectrumAnalisisCount, Convert.ToInt32(value));
        }
        #endregion


        // Диаграмма спектрального анализа
        public ObservableCollection<int>? spectrum;

        // Высоты графиков
        private static int height1 = 150;   // Высота 1 графика Динамическая
        private static int height2 = 100;   // Высота 2 графика
        private static int height3 = 200;   // Высота 3 графика

        private static int NUM_READ = 5;  // порядок медианы
        private static int[] buffer = new int[5];  // статический буфер
        private static byte count = 0;

        // Диалоговые окна (Возможно объекты лучше перенести в команды-обработчики)
        private SaveFileDialog saveDialog = new SaveFileDialog();
        private OpenFileDialog openDialog = new OpenFileDialog();

        #endregion

        #region КОМАНДЫ

        #region КОМАНДА ПЕРЕКЛЮЧЕНИЯ ПОДКЛЮЧЕНИЯ
        public ICommand SwitchConnectionCmd { get; }

        private void OnSwitchConnectionCmdExecuted(object param)
        {
            var connections = NativeWifi.EnumerateInterfaceConnections();
            if (connections.Any())
            {
                var first = connections.Last();
                MessageBox.Show(first.ProfileName);
            }
            else if (!connections.Any())
            {
                MessageBox.Show("Не удалось обнаружить подключенную сеть WiFi");
            }
            else
            {
                MessageBox.Show("Непредвиденная ошибка");
            }
        }

        private bool CanSwitchConnectionCmdExecute(object param)
        {
            //try
            //{
            // Проверка наличия WiFi
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
                    if (connectionName.ProfileName == "Redmi Note 8 Pro")
                    {
                        ConnectColor = Brushes.Green;
                        return true;
                    }
                }
            }
            //}
            //catch (Exception) { }
            ConnectColor = Brushes.DarkGray;
            return false;
        }
        #endregion КОМАНДА ПЕРЕКЛЮЧЕНИЯ ПОДКЛЮЧЕНИЯ

        #region КОМАНДА СКРОЛЛА ГРАФИКА RPM

        ICommand ScrollRpmCmd { get; }

        private void OnScrollRpmCmdExecuted(object param)
        {

        }

        // Можно опустить, поскольку по умолчанию в Lambda Command строит return true
        //private bool CanScrollRpmCmdExecute(object param)
        //{
        //    return true;
        //}

        #endregion КОМАНДА СКРОЛЛА ГРАФИКА RPM

        #endregion КОМАНДЫ

        public MainWindowViewModel()
        {
            #region ИНИЦИАЛИЗАЦИЯ ПОЛЕЙ 
            #region ИНИЦИАЛИЗАЦИЯ ГЛАВНОЙ МОДЕЛИ
            mainModel = new();
            #endregion ИНИЦИАЛИЗАЦИЯ ГЛАВНОЙ МОДЕЛИ

            #region ИНИЦИАЛИЗАЦИЯ ПОЛЕЙ ВИЗУАЛЬНОГО ОФОРМЛЕНИЯ
            connectText = "Подключиться";
            connectColor = Brushes.Green;
            rpmShowCollection = new()
            {
                new LineSeries
                {
                    LineSmoothness = 0,
                    Values = new ChartValues<double>{1,200,300,-100,450,750,-200,30,400,1500,400,300,200,100},
                }
            };

            showOscilChart = Visibility.Collapsed;
            showHarmChart = Visibility.Collapsed;
            #endregion ИНИЦИАЛИЗАЦИЯ ПОЛЕЙ ВИЗУАЛЬНОГО ОФОРМЛЕНИЯ

            // Инициализация списка отображаемых данных 
            show = new();
            // Инициализация длины отображаемых данных
            lengthShow = 1024 * 4;
            // Инициализация максимального количества данных на графике спектрального анализа
            spectrumAnalisisCount = 1024 * 8;
            // Инициализация диаграммы спектрального анализа
            spectrum = new();
            #endregion ИНИЦИАЛИЗАЦИЯ ПОЛЕЙ

            #region ИНИЦИАЛИЗАЦИЯ КОМАНД
            SwitchConnectionCmd = new LambdaCommand(OnSwitchConnectionCmdExecuted, CanSwitchConnectionCmdExecute);
            #endregion ИНИЦИАЛИЗАЦИЯ КОМАНД
        }
    }
}
