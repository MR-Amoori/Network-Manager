using System;
using System.Diagnostics;

namespace NetworkManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please select your operating system:");
            Console.WriteLine("1: Windows 10");
            Console.WriteLine("2: Windows 11");
            Console.WriteLine("Enter your choice:");

            string osSelection = Console.ReadLine();
            string operatingSystem = "";

            switch (osSelection)
            {
                case "1":
                    operatingSystem = "Windows 10";
                    break;
                case "2":
                    operatingSystem = "Windows 11";
                    break;
                default:
                    Console.WriteLine("Invalid choice. Exiting the program...");
                    return;
            }

            Console.WriteLine("Please select an option:");
            Console.WriteLine("1: Enable network and set static IP addresses");
            Console.WriteLine("2: Disable network and reset settings to default");
            Console.WriteLine("0: Exit");

            while (true)
            {
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ConfigureNetwork(operatingSystem, true);
                        break;
                    case "2":
                        ConfigureNetwork(operatingSystem, false);
                        break;
                    case "0":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        /// <summary>
        /// فعال یا غیرفعال کردن شبکه بین دو سیستم و تنظیم IP مناسب بر اساس سیستم عامل انتخاب‌شده
        /// </summary>
        /// <param name="os">سیستم عاملی که کاربر انتخاب کرده (ویندوز 10 یا 11)</param>
        /// <param name="enable">True برای فعال کردن و False برای غیرفعال کردن</param>
        private static void ConfigureNetwork(string os, bool enable)
        {
            if (enable)
            {
                if (os == "Windows 10")
                {
                    // تنظیم آی‌پی استاتیک برای ویندوز 10
                    SetIPAddress("Ethernet", "192.168.0.10", "255.255.255.0");
                }
                else if (os == "Windows 11")
                {
                    // تنظیم آی‌پی استاتیک برای ویندوز 11
                    SetIPAddress("Wi-Fi", "192.168.0.11", "255.255.255.0");
                }

                // فعال کردن تنظیمات اشتراک‌گذاری
                EnableSharingOptions();

                // خاموش کردن فایروال
                RunCommand("netsh advfirewall set allprofiles state off");

                Console.WriteLine("Network has been enabled, static IP addresses set, and sharing options are enabled.");
            }
            else
            {
                // بازگرداندن تنظیمات آی‌پی به حالت DHCP
                if (os == "Windows 10")
                {
                    ResetIPAddress("Ethernet");
                }
                else if (os == "Windows 11")
                {
                    ResetIPAddress("Wi-Fi");
                }

                // غیرفعال کردن تنظیمات اشتراک‌گذاری
                DisableSharingOptions();

                // روشن کردن فایروال
                RunCommand("netsh advfirewall set allprofiles state on");

                Console.WriteLine("Network has been disabled, all settings reset to default.");
            }
        }

        /// <summary>
        /// تنظیم آی‌پی استاتیک برای رابط شبکه
        /// </summary>
        /// <param name="interfaceName">نام رابط شبکه (مانند Ethernet یا Wi-Fi)</param>
        /// <param name="ipAddress">آی‌پی استاتیک</param>
        /// <param name="subnetMask">ساب‌نت ماسک</param>
        private static void SetIPAddress(string interfaceName, string ipAddress, string subnetMask)
        {
            string command = $"netsh interface ip set address \"{interfaceName}\" static {ipAddress} {subnetMask}";
            RunCommand(command);
        }

        /// <summary>
        /// بازگرداندن آی‌پی به حالت DHCP برای رابط شبکه
        /// </summary>
        /// <param name="interfaceName">نام رابط شبکه (مانند Ethernet یا Wi-Fi)</param>
        private static void ResetIPAddress(string interfaceName)
        {
            string command = $"netsh interface ip set address \"{interfaceName}\" dhcp";
            RunCommand(command);
        }

        /// <summary>
        /// فعال کردن تنظیمات اشتراک‌گذاری (Private، Public، All Networks)
        /// </summary>
        private static void EnableSharingOptions()
        {
            RunCommand("netsh advfirewall firewall set rule group=\"File and Printer Sharing\" new enable=yes");
            RunCommand("netsh advfirewall firewall set rule group=\"Network Discovery\" new enable=yes");
        }

        /// <summary>
        /// غیرفعال کردن تنظیمات اشتراک‌گذاری (Private، Public، All Networks)
        /// </summary>
        private static void DisableSharingOptions()
        {
            RunCommand("netsh advfirewall firewall set rule group=\"File and Printer Sharing\" new enable=no");
            RunCommand("netsh advfirewall firewall set rule group=\"Network Discovery\" new enable=no");
        }

        /// <summary>
        /// اجرای دستور در Command Prompt
        /// </summary>
        /// <param name="command">دستوری که باید اجرا شود</param>
        private static void RunCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C " + command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                string result = process.StandardOutput.ReadToEnd();
                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine(result);
                }

                string error = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("Error: " + error);
                }
            }
        }
    }
}