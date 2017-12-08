using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;

namespace MongooseOsBleConfigService
{

    public class ConfigService : IConfigService
    {
        public IBluetoothLE BLE { get; set; }
        public IAdapter ADAPTER { get; set; }
        Guid ConfigServiceGuid, KeyCharacteristicGuid, ValueCharacteristicGuid, SaveCharacteristicGuid;
        public IDevice ConnectedDevice { get; set; }

        static ConfigService instance;
        public static ConfigService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConfigService();
                }
                return instance;
            }
        }

        private ConfigService()
        {
            BLE = CrossBluetoothLE.Current;
            ADAPTER = CrossBluetoothLE.Current.Adapter;
            ConfigServiceGuid = Guid.Parse("5f6d4f53-5f43-4647-5f53-56435f49445f");
            KeyCharacteristicGuid = Guid.Parse("306d4f53-5f43-4647-5f6b-65795f5f5f30");
            ValueCharacteristicGuid = Guid.Parse("316d4f53-5f43-4647-5f76-616c75655f31");
            SaveCharacteristicGuid = Guid.Parse("326d4f53-5f43-4647-5f73-6176655f5f32");
        }

        public async Task ScanForDevice(CancellationToken ctoken)
        {
            ADAPTER.ScanMode = ScanMode.LowPower;
            await ADAPTER.StartScanningForDevicesAsync(serviceUuids: new Guid[]{ ConfigServiceGuid }, cancellationToken: ctoken);
        }

        public async Task ConnectDeviceAsync(IDevice device)
        {
            await ADAPTER.ConnectToDeviceAsync(device);
            if (device.State == Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                ConnectedDevice = device;
            }
        }

        public async Task DisconnectDeviceAsync()
        {
            await ADAPTER.DisconnectDeviceAsync(ConnectedDevice);
        }

        async Task<bool> IConfigService.WriteConfig(string property, string val, string save)
        {
            var ConfigService = await ConnectedDevice.GetServiceAsync(ConfigServiceGuid);
            var KeyCharacteristic = await ConfigService.GetCharacteristicAsync(KeyCharacteristicGuid);
            var ValueCharacteristic = await ConfigService.GetCharacteristicAsync(ValueCharacteristicGuid);
            var SaveCharacteristic = await ConfigService.GetCharacteristicAsync(SaveCharacteristicGuid);

            var isKeyWritten = await KeyCharacteristic.WriteAsync(Encoding.UTF8.GetBytes(property));
            var isValueWritten = await ValueCharacteristic.WriteAsync(Encoding.UTF8.GetBytes(val));
            var isSaved = await SaveCharacteristic.WriteAsync(Encoding.UTF8.GetBytes(save));

            return isKeyWritten && isValueWritten && isSaved;
        }

        async Task<byte[]> IConfigService.ReadConfig(string property)
        {
            var ConfigService = await ConnectedDevice.GetServiceAsync(ConfigServiceGuid);
            var KeyCharacteristic = await ConfigService.GetCharacteristicAsync(KeyCharacteristicGuid);
            var ValueCharacteristic = await ConfigService.GetCharacteristicAsync(ValueCharacteristicGuid);

            await KeyCharacteristic.WriteAsync(Encoding.UTF8.GetBytes(property));
            return await ValueCharacteristic.ReadAsync();
        }
    }

}
