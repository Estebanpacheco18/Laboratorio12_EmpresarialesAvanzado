using System;

namespace Laboratorio12_Empresariales.Services
{
    public class CustomTaskService
    {
        public void CleanOldData()
        {
            Console.WriteLine($"[CustomTaskService] Cleaning old data at {DateTime.Now}");
            // Simular la limpieza de datos
            Console.WriteLine("[CustomTaskService] Old data cleaned successfully.");
        }
    }
}