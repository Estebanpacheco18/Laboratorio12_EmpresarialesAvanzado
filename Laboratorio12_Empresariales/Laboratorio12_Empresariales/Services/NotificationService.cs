using System;

namespace Laboratorio12_Empresariales.Services
{
    public class NotificationService
    {
        public void SendNotification(string user)
        {
            Console.WriteLine($"Attempting to send notification to {user} at {DateTime.Now}");

            // Simulate an intentional failure
            if (user == "user2")
            {
                throw new Exception("Simulated error: Failed to send the notification.");
            }

            Console.WriteLine($"Notification sent to {user} at {DateTime.Now}");
        }
    }
}