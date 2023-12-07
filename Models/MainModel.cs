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

        // Максимальное количество данных для отображения
        private readonly int maxCountForShow;
        #region ПОЛЯ ДЛЯ РАБОТЫ С ДАННЫМИ
        // Объект-заглушка для синхронизации потоков
        private readonly object locker;
        // Номер последнего пришедшего пакета
        private int lastMesNumber;
        // Для хранения данных
        private List<Unit> storageCollection;
        // Поток обработки данных
        Thread receiveThread;
        #endregion ПОЛЯ ДЛЯ РАБОТЫ С ДАННЫМИ
        #endregion ПОЛЯ - ВНЕШНИЙ РЕГИОН

        #region СВОЙСТВА
        // Флаг приема данных
        private bool receiveData;
        public bool ReceiveData { get => receiveData; set => Set(ref receiveData, value); }
        // Флаг фильтрации медианным методом
        private bool median;
        public bool Median { get => median; set => Set(ref median, value); }
        // Флаг линейного сглаживания
        private bool linear;
        public bool Linear { get => linear; set => Set(ref linear, value); }
        // Количество оборотов
        private int rpm;
        public int Rpm { get => rpm; set => Set(ref rpm, value); }
        // Включение записи в файл
        private bool writeDataInFile;
        public bool WriteDataInFile { get => writeDataInFile; set => Set(ref writeDataInFile, value); }
        // Для отображения данных
        public ObservableCollection<Unit> showCollection { get; set; }
        // Для диаграммы спектрального анализа
        public ObservableCollection<int> showSpectrum { get; set; }
        #endregion

        // Установить соединение
        private void SendInfoForController()
        {
            try
            {
                // Создание объекта для работы с Udp
                using UdpClient sender = new(remoteIP, port);
                // Преобразование сообщение в массив байт
                byte[] data = Encoding.ASCII.GetBytes("*");
                // Отправка сообщения для устанговки соединения
                sender.Send(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Неудалось отправить сообщение контроллеру");
                MessageBox.Show(ex.Message);
            }
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
            while (receiveData)
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
                            // Отбрасываем все повторяющиеся пакеты
                            if (mesNumber != lastMesNumber)
                            {
                                // Запоминаем номер последнего пакета
                                lastMesNumber = mesNumber;
                                // Сохраняем количество оборотов
                                if (message[mesLen - 1] != 0)
                                {
                                    Rpm = 60000 / ((message[mesLen - 3] << 8) | message[mesLen - 2]);
                                    noRpmTime = 0;
                                }
                                else
                                {
                                    // Засекание времени без оборотов
                                    noRpmTime += ((message[mesLen - 3] << 8) | message[mesLen - 2]);
                                    // Если спустя 3.5 сек нет сигнала - нет оборотов
                                    if (noRpmTime > 3500)
                                    {
                                        rpm = 0;
                                        noRpmTime = 0;
                                    }
                                }


                                for (int i = 0; i < mesLen / 2 - 3; i++)
                                {
                                    // Получаем значение колебаний
                                    value = (message[i * 2] << 8) | message[i * 2 + 1];
                                    // 512 - для ототбражения направления колебаний в + или -
                                    int curValue = (value & 0b0111111111111111) - 512;

                                    // Блокировка данных на время использования
                                    lock (locker)
                                    {
                                        // Данные для отображения на графике
                                        // Обработка данных
                                        if (showCollection.Count > 10)
                                        {
                                            // Если количество данных для отображения больше 10 использовать сглаживание

                                            // Если включена фильтрация медианным методом
                                            if (median)
                                            {
                                                //
                                                // TODO: Вызвать метдо медианной фильтрации
                                                //
                                            }

                                            // Если установлен влаг линейного сглаживания
                                            if (linear)
                                            {
                                                curValue += showCollection[showCollection.Count - 1].Value + showCollection[showCollection.Count - 2].Value + showCollection[showCollection.Count - 3].Value;
                                                curValue /= 4;
                                            }
                                        }

                                        // Добавление данных для отображения
                                        showCollection.Add(new Unit
                                        {
                                            Sing = 1024 * (value >> 15),
                                            Value = curValue,
                                        });

                                        // Если превышено ограничение на отображение, удалять первые значения
                                        if (showCollection.Count > maxCountForShow)
                                            showCollection.Remove(showCollection.First());
                                    }
                                    // Если включена запись файла
                                    if (writeDataInFile)
                                        storageCollection.Add(showCollection[showCollection.Count - 1]);
                                    Thread.Yield();
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

        public void StartReceiveThread()
        {
            receiveData = true;
            receiveThread.Start();
        }

        public void StopReceiveThread()
        {
            receiveData = false;
            receiveThread.Join();
        }

        public void SetDisconnect()
        {

        }

        public MainModel()
        {
            remoteIP = "192.168.4.22";
            port = 4210;

            locker = new();
            lastMesNumber = 0;
            storageCollection = new();

            showCollection = new();

            SendInfoForController();
            receiveThread = new(new ThreadStart(ProcessingPackage))
            {
                IsBackground = true
            };
        }
    }
}
