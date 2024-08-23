using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMID
{
    class 验证码
    {

        #region 生成一个四位数的随机号码
        public static int GenerateFourDigitRandomNumber()
        {
            Random random = new Random();
            return random.Next(1000, 10000); // 生成 [1000, 10000) 之间的随机整数，即四位数
        }
        #endregion
    }
}
