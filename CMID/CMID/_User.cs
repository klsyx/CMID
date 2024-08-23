using System;

namespace CMID
{
    public static class _User
    {
        // 登录帐号是否禁用
        public static bool IsDisabled { get; set; } = false;

        // 登录人ID（系统生成）
        public static string ID { get; set; } = string.Empty;
        public static string Pass = ""; // 登录人加密后的初始密码(ID+123)


        // 登录人加密后的初始密码（ID+123）
        public static string EncryptedPassword { get; set; } = string.Empty;

        // 登录人姓名
        public static string Name { get; set; } = string.Empty;

        // 登录人显示名称
        public static string DisplayName { get; set; } = string.Empty;

        // 登录人性别
        public static string Sex { get; set; } = string.Empty;

        // 登录人中文代码（打印时使用）
        public static string CN { get; set; } = string.Empty;

        // 登录人英文代码（打印时使用）
        public static string EN { get; set; } = string.Empty;

        // 登录人手机号码
        public static string Phone { get; set; } = string.Empty;

        // 登录人工作组
        public static string Group { get; set; } = string.Empty;
    }
}
