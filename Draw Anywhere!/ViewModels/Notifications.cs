using System;
using System.Windows.Threading;
using Notifications.Wpf;

namespace DrawAnywhere.ViewModels
{
    internal class Notifications
    {
        static Notifications()
        {
            _notificationManager = new NotificationManager(Dispatcher.CurrentDispatcher);
        }

        private static NotificationManager _notificationManager;

        public static void ShowSuccess(string title, string message, Action callback = null)
        {
            _notificationManager.Show(new NotificationContent()
            {
                Title = title, Message = message, Type = NotificationType.Success
            }, onClick: callback);
        }

        public static void ShowError(string title, string message, Action callback = null)
        {
            _notificationManager.Show(new NotificationContent()
            {
                Title = title, Message = message, Type = NotificationType.Error
            }, onClick: callback);
        }
    }
}