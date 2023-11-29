using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Win32;

namespace StapelAppWPF.Models
{
    class MainModel
    {
        #region Переменные

        // Объект для блокировки разделяемых ресурсов
        private readonly object locker;

        // Бесконечные циклы в потоках
        private bool queue;

        #region Настройки UDP
        // Объект работы с UDP
        private UdpClient receiver;
        // Адрес удаленного устройства
        private readonly string remoteAddress;
        // Номер удаленного порта
        private readonly int remotePort;
        // Номер локального порта
        private readonly int localPort;
        #endregion

        // Номер последнего принятого пакета
        private int lastNumber;

        // Значение оборотов
        private int rpm;

        // Флаг наличия подключения к удаленному порту
        private bool connect;
        public bool Connect { get => connect; set => connect = value; }

        // Флаг перерисовки данных (возможно мне не нужен будет в WPF)
        private bool flagShow;

        #region Для сохранения данных
        // Массив для сохранения данных
        private readonly List<Unit> colection = new List<Unit>();
        // Флаг начала/конца записи данных (возможно надо перенести во viewModel)
        private static bool writeCollection = false;
        #endregion

        // Поток по приему и обработке данных
        private static Thread? receiveThread;

        #endregion

        public MainModel()
        {
            #region Инициализация всех переменных
            // Инициализация объекта-заглушки для доступа к данным из потока
            locker = new();
            // Инициализация допуска к бесконечным циклам в потоках
            queue = true;
            // Инициализация объекта UDP
            receiver = new UdpClient();
            // Инициализация адреса удаленного устройства
            remoteAddress = "192.168.4.22";
            // Инициализация удаленного порта
            remotePort = 4210;
            // Инициализация локального порта
            localPort = 4000;
            // Инициализация последнего номера принятого пакета
            lastNumber = 0;
            // Инициализация значения оборотов
            rpm = 0;
            // Инициализация флага подключения к удаленному порту
            connect = false;
            // Инициализация флага перерисовки данных
            flagShow = true;
            // Инициализация потока обработки данных
            receiveThread = null;


            #endregion
        }

        void ConnectionMethod()
        {
            try
            {
                // Если подключено, отключить и наоборот
                if (connect)
                {
                    // Сбрасываем флаг соединения
                    connect = false;
                    // Завершаем поток
                    receiveThread?.Abort();
                    // Объединяем его с основным потоком и ждем его завершения
                    receiveThread?.Join();
                    // Закрываем поток данных (получаемый по UDP)
                    receiver?.Close();


                }
                else if (!connect)
                {
                    receiveThread?.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
