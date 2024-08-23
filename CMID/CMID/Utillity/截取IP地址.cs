using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMID.Utillity
{
    class 截取IP地址
    {

        public static string chazhao(string connectionString)
        {
            // 查找 "Data Source=" 在连接字符串中的位置
            int dataSourceIndex = connectionString.IndexOf("Data Source=");
            // 从 "Data Source=" 开始提取并找到结束位置
            int startIndex = dataSourceIndex + "Data Source=".Length;
            int endIndex = connectionString.IndexOf(';', startIndex);

            // 如果找不到结束的分号，则提取到字符串的末尾
            if (endIndex == -1)
            {
                endIndex = connectionString.Length;
            }

            // 截取服务器地址
            string serverAddress = connectionString.Substring(startIndex, endIndex - startIndex);
            Console.WriteLine("服务器地址为：" + serverAddress);

            // 你也可以在 MessageBox 中显示这个地址
            //MessageBox.Show("服务器地址为：" + serverAddress, "服务器地址", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return serverAddress;
        }
    }
}
