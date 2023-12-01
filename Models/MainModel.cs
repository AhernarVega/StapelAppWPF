using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Win32;

using StapelAppWPF.ViewModels.Base;

namespace StapelAppWPF.Models
{
    class MainModel : ImplementedINPC
    {
        #region ПЕРЕНЕСЕННЫЙ КОД АА

        //// Объект для блокировки разделяемых ресурсов
        //private readonly object locker;

        //// Бесконечные циклы в потоках
        //private bool queue;

        //#region НАСТРОЙКИ UDP
        //// Объект работы с UDP
        //private UdpClient receiver;
        //// Адрес удаленного устройства
        //private readonly string remoteAddress;
        //// Номер удаленного порта
        //private readonly int remotePort;
        //// Номер локального порта
        //private readonly int localPort;
        //#endregion НАСТРОЙКИ UDP

        //// Номер последнего принятого пакета
        //private int lastNumber;

        //// Значение оборотов
        //private int rpm;

        //// Флаг наличия подключения к удаленному порту
        //private bool connect;
        //public bool Connect { get => connect; set => connect = value; }

        //// Флаг перерисовки данных (возможно мне не нужен будет в WPF)
        //private bool flagShow;

        //#region ДЛЯ СОХРАНЕНИЯ ДАННЫХ
        //// Массив для сохранения данных
        //private readonly List<Unit> colection = new List<Unit>();
        //// Флаг начала/конца записи данных (возможно надо перенести во viewModel)
        //private static bool writeCollection = false;
        //#endregion ДЛЯ СОХРАНЕНИЯ ДАННЫХ

        //// Поток по приему и обработке данных
        //private static Thread? receiveThread;

        #endregion ПЕРЕНЕСЕННЫЙ КОД АА

        #region ПОЛЯ - ВНЕШНИЙ РЕГИОН
        #region ДЛЯ РАБОТЫ С UDP
        // IP адрес удаленного устройства
        private readonly string remoteIP;
        // Порт для удаленного соединения
        private readonly int port;
        #endregion ДЛЯ РАБОТЫ С UDP

        #region ПОЛЯ ДЛЯ РАБОТЫ С ДАННЫМИ
        // Объект-заглушка для синхронизации потоков
        private object locker;
        // Номер последнего пришедшего пакета
        private int lastMesNumber;
        // Для хранения данных
        private List<int> storageCollection;
        #endregion ПОЛЯ ДЛЯ РАБОТЫ С ДАННЫМИ
        #region ВНЕШНИ ПОЛЯ ДЛЯ ОТОБРАЖЕНИЯ ДАННЫХ
        // Для отображения данных
        ObservableCollection<int> showCollection { get; set; }
        #endregion ВНЕШНИ ПОЛЯ ДЛЯ ОТОБРАЖЕНИЯ ДАННЫХ
        #endregion ПОЛЯ - ВНЕШНИЙ РЕГИОН

        // Установить соединение
        void SetConnect()
        {
            // Создание объекта для работы с Udp
            using UdpClient sender = new(new IPEndPoint(IPAddress.Parse(remoteIP), port));
            // Преобразование сообщение в массив байт
            byte[] data = Encoding.ASCII.GetBytes("*");
            // Отправка сообщения для устанговки соединения
            sender.Send(data);
        }

        async void ProcessingPackage()
        {
            // Создание объекта для работы с Udp
            using UdpClient receiver = new(port);
            // Переменная, для сохранения значения колебаний
            int value = 0;
            // Переменная для сохранения времени, прошедшего без оборотов
            int noRpmTime = 0;
            // Цикл прослушки сообщений
            while (true)
            {
                try
                {
                    // Получение данных в виде массива byte
                    byte[] message = (await receiver.ReceiveAsync()).Buffer;
                    // Если данные действительно пришли
                    if (message.Length > 0)
                    {
                        // Парсинг данных
                        int mesLen = message.Length;
                        // Временная переменная номера пакета
                        // Получение номера пакета
                        int mesNumber = (message[mesLen - 5] << 8) | message[mesLen - 4];
                        // Отбрасываем все устаревшие пакеты
                        if(mesNumber > lastMesNumber)
                        {// Блокировка данных на время использования
                            lock (locker)
                            {
                                // Сохранение данных в массивы

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        void SetDisconnect()
        {

        }

        // Начать прием пакетов
        void ReceiveMessage()
        {

        }

        void SendMessage()
        {
        }

        public MainModel()
        {
            remoteIP = "192.168.4.22";
            port = 4210;

            locker = new();
            lastMesNumber= 0;
            storageCollection = new();

            showCollection = new();
        }
    }
}
