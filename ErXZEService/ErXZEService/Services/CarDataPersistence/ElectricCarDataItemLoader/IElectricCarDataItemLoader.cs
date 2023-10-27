using System;
using ErXZEService.Models;
using System.Collections.Generic;

namespace ErXZEService.Services.CarDataPersistence.ElectricCarDataItemLoader
{
    public interface IElectricCarDataItemLoader
    {
        Action<string> OnLoaderProgressChange { get; set; }

        List<ElectricCarDataItem> Load();
    }
}
