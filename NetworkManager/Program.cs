using System;
using System.Diagnostics;

namespace NetworkManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("لطفا یک گزینه انتخاب کنید:");
            Console.WriteLine("1: فعال کردن شبکه بین دو سیستم با IP های استاتیک");
            Console.WriteLine("2: غیرفعال کردن شبکه و بازگرداندن تنظیمات به حالت پیشفرض");
            Console.WriteLine("0: خروج");

            while (true)
            {
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ConfigureNetwork(true);
                        break;
                    case "2":
                        ConfigureNetwork(false);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("گزینه نامعتبر است، لطفا دوباره تلاش کنید.");
                        break;
                }
            }
        }

        /// <summary>
        /// فعال یا غیرفعال کردن شبکه بین دو سیستم و تنظیم IP مناسب بر اساس سیستم عامل
        /// </summary>
        /// <param name="enable">True برای فعال کردن و False برای غیرفعال کردن</param>
        private static void ConfigureNetwork(bool enable)
        {
            // تشخیص نوع سیستم عامل
            string osVersion = GetOSVersion();

            if (enable)
            {
                if (osVersion == "Windows 10")
                {
                    // تنظیم آی‌پی استاتیک برای کامپیوتر ویندوز 10
                    SetIPAddress("Ethernet", "192.168.0.10", "255.255.255.0");
                }
                else if (osVersion == "Windows 11")
                {
                    // تنظیم آی‌پی استاتیک برای لپ‌تاپ ویندوز 11
                    SetIPAddress("Wi-Fi", "192.168.0.11", "255.255.255.0");
                }
                else
                {
                    Console.WriteLine("سیستم عامل شناسایی نشد. لطفا بررسی کنید.");
                    return;
                }

                // فعال کردن تنظیمات اشتراک‌گذاری (Private، Public، All Networks)
                EnableSharingOptions();

                // خاموش کردن فایروال
                RunCommand("netsh advfirewall set allprofiles state off");

                Console.WriteLine("شبکه فعال شد و تنظیمات آی‌پی و اشتراک‌گذاری اعمال شدند.");
            }
            else
            {
                // بازگرداندن تنظیمات آی‌پی به حالت DHCP
                if (osVersion == "Windows 10")
                {
                    ResetIPAddress("Ethernet");
                }
                else if (osVersion == "Windows 11")
                {
                    ResetIPAddress("Wi-Fi");
                }

                // غیرفعال کردن تنظیمات اشتراک‌گذاری
                DisableSharingOptions();

                // روشن کردن فایروال
                RunCommand("netsh advfirewall set allprofiles state on");

                Console.WriteLine("شبکه غیرفعال شد و تنظیمات به حالت پیشفرض بازگردانده شدند.");
            }
        }

        /// <summary>
        /// شناسایی نسخه سیستم عامل
        /// </summary>
        /// <returns>نام سیستم عامل (ویندوز 10 یا ویندوز 11)</returns>
        private static string GetOSVersion()
        {
            // دریافت نسخه سیستم عامل
            string osVersion = Environment.OSVersion.Version.ToString();
            int majorVersion = Environment.OSVersion.Version.Major;
            int minorVersion = Environment.OSVersion.Version.Minor;

            if (majorVersion == 10 && minorVersion == 0)
            {
                return "Windows 10";
            }
            else if (majorVersion == 10 && minorVersion == 1)
            {
                return "Windows 11";
            }
            else
            {
                return "Unknown";
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
                    Console.WriteLine("خطا: " + error);
                }
            }
        }
    }
}