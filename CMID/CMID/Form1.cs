using DevExpress.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using CMID.Utillity;
using System.Linq;
using System.Drawing;

namespace CMID
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private readonly DatabaseHelper dbHelper; // 用于处理数据库操作的帮助类实例
        private bool messageShown账号 = false;     // 用于跟踪账号输入框的弹窗是否已经显示
        private bool messageShown企业是否存在 = false;     // 用于跟踪企业是否存在的弹窗是否已经显示
        private bool messageShown是否已经加入企业 = false;     // 用于跟踪是否已经加入企业的弹窗是否已经显示

        private bool messageShown申请人账号 = false;    // 用于跟踪申请人账号输入框的弹窗是否已经显示
        private bool 申请人是否注册 = false;
        private bool 点击登录 = false;
        private int countdownSeconds = 60; // 倒计时秒数
        private const string AllCompanies = "所有企业"; // 定义“所有企业”常量
        private List<string> companies = new List<string>();



        public Form1()
        {
            InitializeComponent(); // 初始化组件
                                   // 获取硬件信息
            _主机.获取硬件信息();
            this.Text = "CMID ERP 9"; // 设置窗体标题
            this.StartPosition = FormStartPosition.CenterScreen; // 设置窗体居中显示
            this.Icon = Properties.Resources.CMID; // 设置窗体图标

            // 获取初始的数据库连接字符串并解密
            string initialConnectionString = 获取路径.duqustring();
            initialConnectionString = JiaMi.DecryptText(initialConnectionString, _CMIDall.miyao); // 使用密钥解密连接字符串
            dbHelper = new DatabaseHelper(initialConnectionString); // 实例化数据库帮助类

            simpleButton2.Enabled = false; // 初始禁用发送验证码按钮
            simpleButton3.Enabled = false;
            LoadLoginInfo(); // 加载上次登录信息
        }

        #region 登录界面用户账号输入处理

        private void textEdit账号_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(textEdit账号.Text))
            {

                if (!messageShown账号)
                {
                    HandleLoginInput(textEdit账号.Text); // 处理用户登录输入
                    messageShown账号 = true; // 标记弹窗已显示
                }
            }
        }

        private void textEdit账号_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textEdit账号.Text) && !messageShown账号)
            {
                HandleLoginInput(textEdit账号.Text); // 处理用户离开输入框后的操作
                messageShown账号 = true; // 标记弹窗已显示
            }

            // 离开时不会重置标志位，只会在值改变时重置
        }

        private void textEdit账号_EditValueChanged(object sender, EventArgs e)
        {
            messageShown账号 = false; // 当文本值改变时，重置弹窗标志
            if (textEdit账号.Text == "")
            {
                ResetAllDetails();
            }
        }

        #endregion

        #region 加入企业文本框事件处理

        private void textEdit加入企业_KeyDown(object sender, KeyEventArgs e)
        {
            // 当用户按下回车键并且输入框内容不为空时，处理公司输入
            if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(textEdit加入企业.Text))
            {
                if (!messageShown是否已经加入企业 && !messageShown企业是否存在)
                {
                    HandleCompanyInput(); // 处理公司输入
                    //messageShown是否已经加入企业 = true; // 标记"是否已经加入企业"弹窗已显示
                    //messageShown企业是否存在 = true;
                }
            }
        }

        private void textEdit加入企业_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textEdit加入企业.Text) && !messageShown是否已经加入企业 && !messageShown企业是否存在)
            {
                HandleCompanyInput(); // 处理公司输入
            }

            // 离开时不会重置标志位，只会在值改变时重置
        }

        private void textEdit加入企业_EditValueChanged(object sender, EventArgs e)
        {
            messageShown是否已经加入企业 = false; // 当文本值改变时，重置弹窗标志
            messageShown企业是否存在 = false; // 重置企业是否存在的标志
            点击登录 = false;
        }

        #endregion

        #region 注册企业时的用户账号输入处理

        private void textEdit申请人账号_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(textEdit申请人账号.Text))
            {
                if (!messageShown申请人账号)
                {
                    UpdateApplicantDetails(textEdit申请人账号.Text); // 更新申请人详细信息
                }
            }
        }

        private void textEdit申请人账号_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textEdit申请人账号.Text) && !messageShown申请人账号)
            {
                UpdateApplicantDetails(textEdit申请人账号.Text); // 更新申请人离开输入框后的操作
            }

            // 离开时不会重置标志位，只会在值改变时重置
        }

        private void textEdit申请人账号_EditValueChanged(object sender, EventArgs e)
        {
            messageShown申请人账号 = false; // 当文本值改变时，重置弹窗标志
            if (textEdit申请人账号.Text == "")
            {
                ResetAllDetails();
            }
        }

        #endregion

        #region 登录按钮事件处理

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            // 检查用户输入的账号、密码和企业是否有效
            if (!IsInputValid(textEdit账号, textEdit密码, comboBoxEdit企业))
            {
                return; // 如果无效，直接返回
            }

            // 对输入的密码进行加密处理
            string encryptedPassword = JiaMi.EncryptText(textEdit密码.Text.Trim(), _CMIDall.miyao);

            // 验证加密后的密码是否正确
            if (!IsPasswordCorrect(encryptedPassword))
            {
                ShowError("密码错误！"); // 如果密码错误，显示错误信息
                return;
            }

            // 从数据库中查询是否禁用
            var result1 = QueryDatabase("VIP_Arct", $"VIP_ID = '{_CMIDall.VIP_ID}" + "'");

            // 如果查询结果不为空
            if (dbHelper.HasResults(result1))
            {
                // 首先检查是否存在 "IS_taboo" 字段并且其值不为空
                if (result1[0]["IS_taboo"] != null && !string.IsNullOrEmpty(result1[0]["IS_taboo"].ToString()))
                {
                    // 将 "IS_taboo" 的值解析为布尔类型并赋值给 _User.IsDisabled
                    _User.IsDisabled = bool.Parse(result1[0]["IS_taboo"].ToString());
                    if (_User.IsDisabled == true)
                    {

                        ShowMessage("该用户已经被禁用，无法登录！");
                        Application.Exit();
                    }
                }
            }


            // 从数据库中查询是否终端禁用
            var result2 = QueryDatabase("PC_Info", $"终端ID = '{_主机.终端ID}'");

            // 如果查询结果不为空
            if (dbHelper.HasResults(result2))
            {


                dbHelper.Update("PC_Info", $"终端ID = '{_主机.终端ID}'", "最近登录日期", DateTime.Now);

                string shiyongrenliebiao = _CMIDall.VIP_Acc;
                if (result2[0]["使用人列表"] != null && !string.IsNullOrEmpty(result2[0]["使用人列表"].ToString()))
                {

                    string shiyongrenliebiao1 = "|" + result2[0]["使用人列表"].ToString() + "|";
                    if (!shiyongrenliebiao1.Contains("|" + shiyongrenliebiao + "|"))
                    {
                        shiyongrenliebiao = result2[0]["使用人列表"].ToString() + "|" + _CMIDall.VIP_Acc;

                    }
                    else
                    {
                        shiyongrenliebiao = result2[0]["使用人列表"].ToString();
                    }


                }
                dbHelper.Update("PC_Info", $"终端ID = '{_主机.终端ID}'", "使用人列表", shiyongrenliebiao);





                string denglurizhi;
                denglurizhi = (_CMIDall.VIP_Acc + "," + _CMIDall.qiye + "," + DateTime.Now.ToString() + "\n");
                if (result2[0]["登录日志"] != null && !string.IsNullOrEmpty(result2[0]["登录日志"].ToString()))
                {
                    denglurizhi = result2[0]["登录日志"].ToString();
                    denglurizhi += (_CMIDall.VIP_Acc + "," + _CMIDall.qiye + "," + DateTime.Now.ToString() + "\n");


                }

                dbHelper.Update("PC_Info", $"终端ID = '{_主机.终端ID}'", "登录日志", denglurizhi);







                int denglucishu = 0;

                if (result2[0]["累计登录次数"] != null && !string.IsNullOrEmpty(result2[0]["累计登录次数"].ToString()))
                {
                    //登录次数加1
                    denglucishu = int.Parse(result2[0]["累计登录次数"].ToString());
                    denglucishu++;


                }
                else
                {
                    denglucishu = 1;
                }
                dbHelper.Update("PC_Info", $"终端ID = '{_主机.终端ID}'", "累计登录次数", denglucishu);


                // 首先检查是否存在 "IS_taboo" 字段并且其值不为空
                if (result2[0]["禁用"] != null && !string.IsNullOrEmpty(result2[0]["禁用"].ToString()))
                {
                    //MessageBox.Show("asdad");
                    // 将 "IS_taboo" 的值解析为布尔类型并赋值给 _User.IsDisabled
                    _主机.禁用 = bool.Parse(result2[0]["禁用"].ToString());
                    if (_主机.禁用 == true)
                    {

                        ShowMessage("该终端已经被禁用，无法登录！");
                        Application.Exit();
                    }
                }
            }
            else
            {
                //ShowMessage("终端未注册!");

                // 假设我们有一个新的表或操作，其列与参数字典匹配  
                var columnNames = new List<string>
{
    "终端ID", "IP地址", "主板ID", "BOSSID", "CPUID", "WINDOWS版本", "进程64位", "操作系统64位", "显示分辨率", "缩放","申请人","申请日期","企业简称", "_New_Date"
};

                var values = new List<object>
{
    _主机.终端ID, _主机.IP地址, _主机.主板ID, _主机.BossID, _主机.CPUID, _主机.WINDOWS版本,
    _主机.进程64位, _主机.操作系统64位, _主机.显示分辨率, _主机.缩放,_CMIDall.VIP_name,DateTime.Now,_CMIDall.qiye,DateTime.Now
            };
                // 向数据库插入验证码发送信息
                int rowsAffected = dbHelper.Insert("PC_Info", columnNames, values);

                // MessageBox.Show(rowsAffected + "");

                Application.Exit();


            }



            _CMIDall.Companyselected = comboBoxEdit企业.Text; // 设置选中的企业名称

            if (_CMIDall.Companyselected == AllCompanies)
            {
                // 如果选择了“所有企业”，跳过企业匹配，直接允许登录
                ShowMessage("登录成功！");
                return;
            }
            // 从数据库中查询与选中企业对应的连接信息
            var result = QueryDatabase("Connection", $"CompanyAbbr = '{_CMIDall.Companyselected}' AND ServerType = 'PC9'");

            // 如果查询结果不为空，设置连接字符串并显示登录成功信息
            if (dbHelper.HasResults(result))
            {
                _CMIDall.connectionPC9 = result[0]["Connection_PC9"].ToString();
                ShowMessage("登录成功！");
                // 将当前选择的企业加入到企业列表中
                if (!companies.Contains(comboBoxEdit企业.Text))
                {
                    companies.Add(comboBoxEdit企业.Text);
                }

                // 保存登录信息
                SaveLoginInfo(textEdit账号.Text, textEdit姓名.Text, comboBoxEdit企业.Text);

               // 创建新窗体并显示zhuce控件
        Form newForm = new Form();
        newForm.Text = "新窗口"; // 可以设置新窗体的标题
        newForm.StartPosition = FormStartPosition.CenterScreen; // 设置窗体居中显示
                newForm.WindowState = FormWindowState.Maximized; // 将窗体设置为最大化状态
                zhuce userControl = new zhuce(); // 创建zhuce控件实例
        userControl.Dock = DockStyle.Fill; // 让控件充满整个窗体
        newForm.Controls.Add(userControl); // 将zhuce控件添加到新窗体
        newForm.Show(); // 显示新窗体

        this.Hide(); // 隐藏当前窗体，而不是关闭它



            }
            else
            {
                ShowError("未找到匹配的记录。"); // 否则显示错误信息
            }
        }

        #endregion

        #region 输入验证和辅助方法

        // 验证输入框是否为空
        private bool IsInputValid(params Control[] controls)
        {
            foreach (var control in controls)
            {
                if (string.IsNullOrWhiteSpace(control.Text)) // 如果任意输入框为空
                {
                    ShowError("账号、密码和企业不能为空!"); // 显示错误信息
                    return false;
                }
            }
            return true; // 输入有效
        }

        // 验证密码是否正确
        private bool IsPasswordCorrect(string encryptedPassword)
        {
            return _CMIDall.VIPpasswordJiami == encryptedPassword; // 比较输入密码与存储的加密密码
        }

        // 从数据库查询数据
        private List<Dictionary<string, object>> QueryDatabase(string tableName, string whereClause, List<string> fields = null)
        {
            try
            {
                return dbHelper.Query(tableName, whereClause, fields); // 使用帮助类查询数据库
            }
            catch (Exception ex)
            {
                ShowError("数据库查询失败：" + ex.Message); // 如果查询失败，显示错误信息
                return null;
            }
        }

        // 显示错误信息
        private void ShowError(string message)
        {
            MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        // 显示普通信息
        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region 设置用户详细信息

        private void HandleLoginInput(string zhanghao)
        {
            comboBoxEdit企业.Text = "";
            // 从数据库查询用户信息
            var result = QueryDatabase("VIP_Arct", $"VIP_ID = '{zhanghao}' OR VIP_MOBILE = '{zhanghao}'");

            // 如果用户存在，设置用户信息
            if (dbHelper.HasResults(result))
            {
                SetUserDetails(result[0]);
            }
            else
            {
                ShowError("用户未注册，请先注册用户！"); // 如果用户不存在，显示错误信息
                ResetAllDetails(); // 重置所有相关控件
            }
        }

        // 设置用户详细信息
        private void SetUserDetails(Dictionary<string, object> userRecord)
        {
            textEdit姓名.Text = userRecord["Vip_Acc"].ToString(); // 只显示Vip_Acc，不拼接VIP_ID
            string DataSysTemp = userRecord["DataSysTemp"].ToString();
            comboBoxEdit企业.Properties.Items.Clear(); // 清空企业下拉列表
            if (DataSysTemp == "所有企业")
            {
                var result1 = dbHelper.Query("App_Register");
                foreach (var re in result1)
                {

                    comboBoxEdit企业.Properties.Items.Add(re["CompanyAbbr"].ToString()); // 添加企业列表

                }

            }
            else
            {
                comboBoxEdit企业.Properties.Items.AddRange(DataSysTemp.Split('|')); // 添加企业列表

            }



            _CMIDall.VIP_ID = userRecord["VIP_ID"].ToString(); // 设置VIP ID
            _CMIDall.VIP_Acc = userRecord["Vip_Acc"].ToString(); // 设置VIP账号
                                                                 // MessageBox.Show(userRecord["Password_PC9"].ToString());
                                                                 // _CMIDall.VIPpassword = userRecord["Password_PC9"].ToString(); // 设置VIP密码

            // 如果密码为空，设置默认密码并加密
            if (string.IsNullOrEmpty(_CMIDall.VIPpassword))
            {
                _CMIDall.VIPpassword = _CMIDall.VIP_ID + "123";
                _CMIDall.VIPpasswordJiami = JiaMi.EncryptText(_CMIDall.VIPpassword, _CMIDall.miyao);
                dbHelper.Update("VIP_Arct", $"VIP_ID = '{_CMIDall.VIP_ID}'", "Password_PC9", _CMIDall.VIPpasswordJiami); // 更新数据库中的密码
            }
            else
            {
                _CMIDall.VIPpassword = userRecord["Password_PC9"].ToString(); // 设置VIP密码

                _CMIDall.VIPpasswordJiami = _CMIDall.VIPpassword; // 否则直接使用现有的加密密码
            }
        }

        #endregion

        #region 重置用户详细信息

        // 重置所有详细信息
        private void ResetAllDetails()
        {
            textEdit姓名.Text = ""; // 清空姓名输入框
            comboBoxEdit企业.Properties.Items.Clear(); // 清空企业下拉列表
            comboBoxEdit企业.Text = ""; // 清空企业选择
            textEdit申请人姓名.Text = ""; // 清空申请人姓名输入框
            textEdit加入企业.Text = ""; // 清空加入企业输入框
            comboBoxEdit审批人列表.Properties.Items.Clear(); // 清空审批人列表
            comboBoxEdit审批人列表.Text = ""; // 清空审批人选择
            textEdit审批人手机.Text = ""; // 清空审批人手机输入框
            textEdit验证码.Text = ""; // 清空验证码输入框

            _CMIDall.shenqingrenxingming = ""; // 重置申请人姓名
            _CMIDall.qiye = ""; // 重置企业信息
            _CMIDall.shenpirenshouji = ""; // 重置审批人手机
            _CMIDall.yanzhengma = 0; // 重置验证码

            simpleButton2.Enabled = false; // 禁用发送验证码按钮
            simpleButton3.Enabled = false;
        }

        #endregion

        #region 检查是否可以启用发送验证码按钮

        // 检查是否满足条件以启用发送验证码按钮
        private void CheckEnableSendButton()
        {
            // 如果所有必要条件满足，则启用按钮
            if (!string.IsNullOrEmpty(textEdit申请人姓名.Text) &&
                !string.IsNullOrEmpty(_CMIDall.qiye) &&
                !string.IsNullOrEmpty(textEdit审批人手机.Text))
            {
                simpleButton2.Enabled = true; // 启用发送验证码按钮
            }
            else
            {
                simpleButton2.Enabled = false; // 禁用发送验证码按钮
            }
        }

        #endregion

        #region 加入企业文本框处理逻辑

        // 处理公司输入的逻辑
        private void HandleCompanyInput()
        {
            if (string.IsNullOrEmpty(textEdit申请人账号.Text))
            {
                MessageBox.Show("请先输入申请人账号！");
                return;
            }

            if (!申请人是否注册)
            {
                MessageBox.Show("申请人尚未注册");
                return;
            }

            if (string.IsNullOrEmpty(textEdit加入企业.Text))
            {
                ClearApprovalDetails(); // 清空审批人列表和手机号
                return;
            }

            _CMIDall.qiye = textEdit加入企业.Text; // 设置公司名称

            // 从数据库查询企业信息
            var result2 = QueryDatabase("App_Register", $"CompanyAbbr = '{_CMIDall.qiye}'", new List<string> { "CellNumList" });
            // 如果查询结果存在，更新审批人列表
            if (dbHelper.HasResults(result2))
            {

                // 从数据库查询申请人信息
                var result1 = QueryDatabase("VIP_Arct", $"VIP_Acc = '{_CMIDall.shenqingrenxingming}'", new List<string> { "DataSysTemp" });

                // 检查申请人是否已加入企业
                if (dbHelper.HasResults(result1))
                {
                    string companies1 = result1[0]["DataSysTemp"].ToString();
                    companies = companies1.Split('|').ToList();
                    bool exists = companies1.Split('|').Contains(textEdit加入企业.Text);

                    if (exists)
                    {

                        if (!messageShown是否已经加入企业)
                        {
                            string messge = $"您已在【{_CMIDall.qiye}】列表中，不需要再次加入";
                            ShowMessage(messge);
                            messageShown是否已经加入企业 = true;
                        }
                        simpleButton2.Enabled = false; // 禁用发送验证码按钮
                        return; // 提醒后直接返回，不执行后续操作
                    }
                }




                string DataSysTemp = result2[0]["CellNumList"].ToString();
                comboBoxEdit审批人列表.Properties.Items.Clear(); // 清空审批人下拉列表
                comboBoxEdit审批人列表.Properties.Items.AddRange(DataSysTemp.Split('|')); // 添加审批人列表

                // 更新手机号
                comboBoxEdit审批人列表.SelectedIndex = 0; // 选择第一个审批人
                UpdateApprovalPhoneNumber(comboBoxEdit审批人列表.Text); // 更新审批人手机号码
            }
            else
            {
                if (!messageShown企业是否存在)  // 确保"企业是否存在"弹窗只显示一次
                {
                    ClearApprovalDetails(); // 清空审批人详细信息
                    ShowError("企业未注册"); // 如果企业未注册，显示错误信息
                    messageShown企业是否存在 = true; // 标记弹窗已显示
                }
            }











            // 重置标志位，这样确保下次输入时可以再次弹窗
            messageShown是否已经加入企业 = false;
            CheckEnableSendButton();  // 检查是否可以启用发送验证码按钮
        }

        #endregion





        #region 注册企业时的用户账号输入处理逻辑

        // 更新申请人详细信息
        private void UpdateApplicantDetails(string accountId)
        {
            // 从数据库查询申请人信息
            var result = QueryDatabase("VIP_Arct", $"VIP_ID = '{accountId}'");

            // 如果申请人信息存在，更新控件内容
            if (dbHelper.HasResults(result))
            {
                申请人是否注册 = true;
                textEdit申请人姓名.Text = result[0]["Vip_Acc"].ToString(); // 只显示Vip_Acc，不拼接VIP_ID
                _CMIDall.shenqingrenxingming = textEdit申请人姓名.Text; // 设置申请人姓名
            }
            else
            {

                ShowError("用户未注册，请先注册用户！"); // 如果用户未注册，显示错误信息
                messageShown申请人账号 = true; // 标记弹窗已显示

                ResetAllDetails(); // 重置所有相关控件
            }

            CheckEnableSendButton();  // 检查是否可以启用发送验证码按钮
        }

        #endregion

        #region 审批人手机号码更新

        // 更新审批人手机号码
        private void UpdateApprovalPhoneNumber(string selectedApproval)
        {
            // 从数据库查询审批人手机信息
            var result = QueryDatabase("VIP_Arct", $"VIP_ACC = '{selectedApproval}'", new List<string> { "VIP_MOBILE" });

            // 如果查询结果存在，更新手机输入框内容
            if (dbHelper.HasResults(result))
            {
                string phone = result[0]["VIP_MOBILE"].ToString();
                _CMIDall.shenpirenshouji = phone; // 设置审批人手机
                textEdit审批人手机.Text = Stringoverride.MaskOddPositions(phone); // 显示掩码后的手机号码
            }
            else
            {
                textEdit审批人手机.Text = ""; // 如果手机号码不存在，清空输入框
            }

            CheckEnableSendButton();  // 检查是否可以启用发送验证码按钮
        }

        #endregion

        #region 清空审批人详细信息

        // 清空审批人详细信息
        private void ClearApprovalDetails()
        {
            comboBoxEdit审批人列表.Properties.Items.Clear(); // 清空审批人下拉列表
            comboBoxEdit审批人列表.Text = ""; // 清空审批人选择
            textEdit审批人手机.Text = ""; // 清空审批人手机输入框
        }

        #endregion

        #region 显示/隐藏密码按钮处理

        private void simpleButton显示密码_Click(object sender, EventArgs e)
        {
            // 如果当前密码是隐藏状态（显示为星号）
            if (textEdit密码.Properties.PasswordChar == '*')
            {
                // 取消掩码，显示实际密码
                textEdit密码.Properties.PasswordChar = '\0';
            }
            else
            {
                // 恢复掩码，隐藏密码
                textEdit密码.Properties.PasswordChar = '*';
            }

            // 确保使用设置的密码掩码属性
            textEdit密码.Properties.UseSystemPasswordChar = false;
        }

        #endregion

        // 启动倒计时
        private void StartCountdown()
        {
            simpleButton2.Enabled = false; // 禁用发送按钮
            countdownSeconds = 60; // 重置倒计时秒数
            timer1.Start(); // 启动计时器
        }
        // 倒计时每秒执行的方法

        #region 发送验证码按钮处理逻辑

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            // 启动倒计时
            StartCountdown();
            // 如果审批人手机为空，禁用发送按钮
            if (string.IsNullOrEmpty(textEdit审批人手机.Text))
            {
                simpleButton2.Enabled = false;
                return;
            }

            // 生成四位随机验证码
            _CMIDall.yanzhengma = 验证码.GenerateFourDigitRandomNumber();
            string newAcc = textEdit申请人姓名.Text; // 获取申请人姓名

            // 向数据库插入验证码发送信息
            //int rowsAffected = dbHelper.Insert("Call_Mess", new List<string> { "PlanID", "ToValue", "ToMov", "State", "_New_Date", "_New_Acc" },
            //    new List<object> { "229106", $"#code#={_CMIDall.yanzhengma}", _CMIDall.shenpirenshouji, -1, DateTime.Now, newAcc });
           int rowsAffected = 1;
            MessageBox.Show(""+_CMIDall.yanzhengma);
            // 根据插入结果显示相应的信息
            ShowMessage(rowsAffected > 0 ? "验证码已发送" : "验证码发送失败");
        }

        #endregion

        #region 申请加入企业按钮处理逻辑

        private void simpleButton3_Click(object sender, EventArgs e)
        {


            // 如果验证码输入框为空，显示错误信息
            if (string.IsNullOrEmpty(textEdit验证码.Text))
            {
                ShowError("请输入验证码！");
                return;
            }

            // 验证输入的验证码是否正确
            if (textEdit验证码.Text.Trim() != _CMIDall.yanzhengma.ToString())
            {
                ShowError("验证码错误，请重新输入！");
                return;
            }

            // 从数据库查询申请人企业信息
            var result = QueryDatabase("VIP_Arct", $"VIP_Acc = '{_CMIDall.shenqingrenxingming}'", new List<string> { "DataSysTemp" });
            if (点击登录 == true)
            {
                ShowMessage("您已经加入该企业无需重复加入！");
                return;
            }
            // 如果查询结果存在，更新企业信息并保存到数据库
            if (dbHelper.HasResults(result))
            {
                int rowsAffected;
                if (result[0]["DataSysTemp"].ToString() != null && !string.IsNullOrEmpty(result[0]["DataSysTemp"].ToString()))
                {
                    string DataSysTemp = result[0]["DataSysTemp"].ToString() + "|" + textEdit加入企业.Text;
                     rowsAffected = dbHelper.Update("VIP_Arct", $"VIP_Acc = '{_CMIDall.shenqingrenxingming}'", "DataSysTemp", DataSysTemp);
                }
                else
                {
                    rowsAffected = dbHelper.Update("VIP_Arct", $"VIP_Acc = '{_CMIDall.shenqingrenxingming}'", "DataSysTemp", textEdit加入企业.Text);

                }

                // 根据更新结果显示相应的信息
                ShowMessage(rowsAffected > 0 ? "加入企业成功！" : "加入企业失败！");
                点击登录 = true;

            }
        }

        #endregion

        private void textEdit1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(textEdit申请人账号.Text))
            {
                if (!messageShown申请人账号)
                {
                    UpdateApplicantDetails(textEdit申请人账号.Text); // 更新申请人详细信息

                }
            }
        }

        #region 审批人列表选择更改事件处理程序

        private void comboBoxEdit审批人列表_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedValue = comboBoxEdit审批人列表.Text; // 获取选择的审批人名称

            // 从数据库查询审批人手机信息
            var result = QueryDatabase("VIP_Arct", $"VIP_ACC = '{selectedValue}'", new List<string> { "VIP_MOBILE" });

            // 如果审批人信息存在，更新手机输入框内容
            if (dbHelper.HasResults(result))
            {
                string phone = result[0]["VIP_MOBILE"].ToString();
                _CMIDall.shenpirenshouji = phone; // 设置审批人手机
                textEdit审批人手机.Text = Stringoverride.MaskOddPositions(phone); // 显示掩码后的手机号码
            }

            CheckEnableSendButton();  // 检查是否可以启用发送验证码按钮
        }

        #endregion

        private void textEdit验证码_EditValueChanged(object sender, EventArgs e)
        {

            simpleButton3.Enabled = true;

        }
        private void SaveLoginInfo(string account, string name, string selectedCompany)
        {
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LastLoginInfo.txt");

            // 准备要写入文件的内容
            var linesToWrite = new List<string>
    {
        account,      // 账号
        name,         // 姓名
        selectedCompany // 当前选中的企业
    };

            // 添加所有企业信息到写入内容




            // 将内容写入文件，覆盖之前的内容
            try
            {
                System.IO.File.WriteAllLines(filePath, linesToWrite);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存企业信息时出错：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLoginInfo()
        {
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LastLoginInfo.txt");

            if (System.IO.File.Exists(filePath))
            {
                var lines = System.IO.File.ReadAllLines(filePath);

                if (lines.Length >= 3)
                {
                    // 加载账号和姓名
                    textEdit账号.Text = lines[0];
                    textEdit姓名.Text = lines[1];

                    // 加载所有企业到下拉框
                    //companies = lines.Skip(2).ToList(); // 将第三行及以后的内容作为企业列表
                    //comboBoxEdit企业.Properties.Items.Clear();
                    //comboBoxEdit企业.Properties.Items.AddRange(companies.ToArray());

                    // 设置默认选择为上次登录的企业
                    comboBoxEdit企业.Text = lines[2];
                }
            }
        }








        private void timer1_Tick(object sender, EventArgs e)
        {
            countdownSeconds--; // 每秒减一

            if (countdownSeconds <= 0)
            {
                timer1.Stop(); // 停止计时器
                simpleButton2.Invoke(new Action(() =>
                {
                    simpleButton2.Text = "发送验证码"; // 重新设置按钮文本
                    simpleButton2.Enabled = true; // 重新启用按钮
                }));
            }
            else
            {
                simpleButton2.Invoke(new Action(() => simpleButton2.Text = $"请等待 {countdownSeconds} 秒")); // 更新按钮文本，显示倒计时
            }
        }

        private void textEdit密码_PropertiesChanged(object sender, EventArgs e)
        {

        }
    }
}
