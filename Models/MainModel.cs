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

        #region ПОЛЯ
        #region ДЛЯ РАБОТЫ С UDP
        // IP адрес удаленного устройства
        private readonly string remoteIP = ""; 
        // Поле для работы с передачей данных по UDP
        UdpClient udp;
        #endregion ДЛЯ РАБОТЫ С UDP
        #endregion ПОЛЯ

        // Установить соединение
        void SetConnect()
        {

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
            // Инициализация поля для работы с UDP
            udp = new();


        }
    }
}
