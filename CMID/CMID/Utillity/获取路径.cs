using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMID.Utillity
{
    class 获取路径
    {

        public static string duqustring()
        {
            // 获取exe文件的路径
            string exePath = AppDomain.CurrentDomain.BaseDirectory;

            // 拼接出文本文件的完整路径
            string filePath = Path.Combine(exePath, "link_info.txt");

            // 检查文件是否存在
            if (File.Exists(filePath))
            {
                try
                {
                    // 读取文件中的连接字符串
                    string initialConnectionString = File.ReadAllText(filePath);

                    // 可以将这个连接字符串用于数据库连接
                    var dbHelper = new DatabaseHelper(initialConnectionString);

                    return initialConnectionString;
                }
                catch (Exception ex)
                {
                    // 捕获异常并显示错误信息
                    string jinggao = "请检查您的连接字符串配置并确保数据库服务器可用。";
                    MessageBox.Show($"连接失败: {ex.Message}\n{jinggao}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // 当文件不存在时，显示警告信息并返回空字符串
                MessageBox.Show("配置文件 'link_info.txt' 不存在！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // 在发生异常或文件不存在的情况下返回空字符串
            return string.Empty;
        }

    }
}
