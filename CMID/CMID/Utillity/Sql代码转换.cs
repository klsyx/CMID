using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMID.Utillity
{
    class Sql代码转换
    {

        public static string ConvertOleDbToSqlConnectionString(string oleDbConnectionString)
        {
            // 使用 `;` 分隔连接字符串中的各个部分
            var parts = oleDbConnectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var sqlConnectionString = new List<string>();

            foreach (var part in parts)
            {
                // 忽略 `Provider` 关键字
                if (part.TrimStart().StartsWith("Provider=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                // 添加其他部分到新的连接字符串列表中
                sqlConnectionString.Add(part);
            }

            // 将新的连接字符串部分组合成一个完整的字符串
            return string.Join(";", sqlConnectionString);
        }
    }
}
