using System;
using System.Drawing;
using System.Management;
using System.Windows.Forms;

namespace CMID
{
    class _主机
    {
        public static bool 禁用 = false;
        public static string 终端ID = "";
        public static string 终端名称 = "";
        public static string 企业简称 = ""; // 可自定义企业简称
        public static string IP地址 = "";
        public static string CPUID = "";
        public static string 主板ID = "";
        public static string 硬盘ID = "";
        public static string BossID = ""; // 可自定义BossID
        public static string WINDOWS版本 = Environment.OSVersion.ToString();
        public static string 显示分辨率 = $"{Screen.PrimaryScreen.Bounds.Width}x{Screen.PrimaryScreen.Bounds.Height}";
        public static string 缩放;
        public static string 进程64位 = Environment.Is64BitProcess ? "是" : "否";
        public static string 操作系统64位 = Environment.Is64BitOperatingSystem ? "是" : "否";

        // 提供获取硬件信息的方法
        public static void 获取硬件信息()
        {
            CPUID = GetCpuId();
            硬盘ID = GetHardDiskId();
            主板ID = GetMotherboardId();
            IP地址 = GetIpAddress();
            终端ID = 主板ID + 硬盘ID + CPUID; // 可根据需要调整终端ID的组合方式
        }

        static _主机()
        {
            缩放 = GetScreenScaling();
        }
        private static string GetScreenScaling()
        {
            using (System.Drawing.Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr desktop = g.GetHdc();
                int logicalScreenHeight = GetDeviceCaps(desktop, 10); // HORZRES
                int physicalScreenHeight = GetDeviceCaps(desktop, 117); // DESKTOPHORZRES
                g.ReleaseHdc(desktop);

                float screenScalingFactor = (float)physicalScreenHeight / logicalScreenHeight;

                return $"{(int)(screenScalingFactor * 100)}%";
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        private static string GetCpuId()
        {
            string cpuId = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject mo in searcher.Get())
            {
                cpuId = mo["ProcessorId"].ToString().Trim();
                break;
            }
            return cpuId;
        }

        private static string GetHardDiskId()
        {
            string diskId = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                diskId = wmi_HD["SerialNumber"].ToString().Trim();
                break;
            }
            return diskId;
        }

        private static string GetMotherboardId()
        {
            string motherboardId = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            foreach (ManagementObject mo in searcher.Get())
            {
                motherboardId = mo["SerialNumber"].ToString().Trim();
                break;
            }
            return motherboardId;
        }

        private static string GetIpAddress()
        {
            string ipAddress = "";
            System.Net.IPAddress[] addresses = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
            foreach (System.Net.IPAddress address in addresses)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = address.ToString();
                    break;
                }
            }
            return ipAddress;
        }
    }
}
