using System;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace MongooseOsBleConfigService
{
    public interface IConfigService
    {
        Task ScanForDevice(CancellationToken ctoken);
        Task ConnectDeviceAsync(IDevice device);
        Task DisconnectDeviceAsync();
        Task<bool> WriteConfig(string property, string val, string save);
        Task<byte[]> ReadConfig(string property);
    }
}
