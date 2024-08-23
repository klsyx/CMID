using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMID.Utillity
{
    class Stringoverride
    {

        #region 将字符串的奇数位变成星号
        public static string MaskOddPositions(string input)
        {
            //改成369变星号
            if (string.IsNullOrEmpty(input))
            {
                return input; // 如果输入为空或null，直接返回
            }

            char[] chars = input.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i % 3 == 0) // 奇数位索引（即 0, 2, 4,...）要显示为星号
                {
                    chars[i] = '*';
                }
            }

            return new string(chars);
        }

        #endregion
    }
}
