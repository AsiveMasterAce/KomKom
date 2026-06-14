using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomKom.Services
{
    public class NotificationService
    {
        private readonly string _appId = "KomKom.Desktop";


        // Show a simple toast
        public void ShowToast(string title, string body)
        {
            var toastBuilder = new ToastContentBuilder()
            .AddText(title)
            .AddText(body);

            toastBuilder.GetType().GetMethod("Show", Type.EmptyTypes)?.Invoke(toastBuilder, null);
        }
    }
}
