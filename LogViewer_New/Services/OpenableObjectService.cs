using LogViewer.Helpers;
using LogViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Services
{
    public static class OpenableObjectService
    {
        #region Public methods
        public static async void SaveOpenableObject(OpenableObject openableObject)
        {
            if (openableObject is LogView)
            {
                await SettingsService.AddOrUpdateLogView((LogView)openableObject, true);
            }
            else if (openableObject is Database)
            {
                await SettingsService.AddOrUpdateDatabase((Database)openableObject, true);
            }
            else if (openableObject is LogFile)
            {
                await SettingsService.AddOrUpdateLogFile((LogFile)openableObject, true);
            }
        }
        #endregion
    }
}
