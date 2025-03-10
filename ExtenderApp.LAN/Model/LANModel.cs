﻿using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using ExtenderApp.Common;


namespace ExtenderApp.LAN
{
    public class LANModel
    {
        public ObservableCollection<LANInteraceInfo> LANInteraces { get; private set; }

        public LANModel()
        {
            LANInteraces = new();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                var item = interfaces[i];

                var info = new LANInteraceInfo(item);
                info.ScanLocalNetwork();
                LANInteraces.Add(info);
            }
        }
    }
}
