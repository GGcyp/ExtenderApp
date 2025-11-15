namespace ExtenderApp.Common.Networks.SNMP
{
    /// <summary>
    /// SNMP 协议中 PDU 的 error-status 字段常见值（适用于 v1/v2c）。
    /// error-status 用于指示请求处理结果的协议级错误（0 表示无错误）。
    /// </summary>
    public enum SnmpErrorStatus : int
    {
        /// <summary>
        /// 没有错误（NoError，值 = 0）。
        /// 表示请求已成功处理（agent 在 varbind 中返回相应值）。
        /// </summary>
        NoError = 0,

        /// <summary>
        /// TooBig（值 = 1）。
        /// 响应的大小超过了管理站允许的最大报文长度（通常由 manager 指定），
        /// agent 可以在响应中返回此错误，管理站应考虑降低请求大小或使用更小的 max-repetitions。
        /// </summary>
        TooBig = 1,

        /// <summary>
        /// NoSuchName（值 = 2）。
        /// SNMPv1 特有：表示请求中指定的对象名不存在或不可访问（v2c 中已弃用，v2 使用 noSuchObject/noSuchInstance/noSuchEntry）。
        /// </summary>
        NoSuchName = 2,

        /// <summary>
        /// BadValue（值 = 3）。
        /// 请求中提供的值不合法（通常在 Set 请求时出现），agent 拒绝应用该值并返回此错误。
        /// </summary>
        BadValue = 3,

        /// <summary>
        /// ReadOnly（值 = 4）。
        /// 尝试写入只读对象时返回（Set 请求），表示目标对象不允许写操作。
        /// </summary>
        ReadOnly = 4,

        /// <summary>
        /// GenErr（Generic Error，值 = 5）。
        /// 通用错误：用于无法用更精确错误码表示的内部错误或未指定错误。
        /// </summary>
        GenErr = 5,

        /// <summary>
        /// NoAccess（值 = 6）。
        /// 表示访问被拒绝（权限不足或 VACM/访问控制规则阻止访问）。
        /// </summary>
        NoAccess = 6,

        /// <summary>
        /// WrongType（值 = 7）。
        /// 提供的值类型与对象期望的类型不匹配（例如为整数字段传入字符串表示）。
        /// </summary>
        WrongType = 7,

        /// <summary>
        /// WrongLength（值 = 8）。
        /// 提供的值长度不符合对象的要求（例如 OCTET STRING 长度越界）。
        /// </summary>
        WrongLength = 8,

        /// <summary>
        /// WrongEncoding（值 = 9）。
        /// 值的编码格式不正确，无法正确解析或应用。
        /// </summary>
        WrongEncoding = 9,

        /// <summary>
        /// WrongValue（值 = 10）。
        /// 值在语义上不被接受（超出允许范围或违反约束），但类型与长度均正确。
        /// </summary>
        WrongValue = 10,

        /// <summary>
        /// NoCreation（值 = 11）。
        /// 在尝试创建一个表项时无法创建（例如缺少必要的前置条件或索引），因此拒绝创建。
        /// </summary>
        NoCreation = 11,

        /// <summary>
        /// InconsistentValue（值 = 12）。
        /// 提供的值与代理上其他相关对象的状态不一致，导致无法提交更改（事务失败类情况）。
        /// </summary>
        InconsistentValue = 12,

        /// <summary>
        /// ResourceUnavailable（值 = 13）。
        /// 资源不可用（例如内存/存储不足或所需资源被占用），因此无法完成请求。
        /// </summary>
        ResourceUnavailable = 13,

        /// <summary>
        /// CommitFailed（值 = 14）。
        /// 在多阶段操作的提交阶段失败（如在 SET 事务的 commit 阶段），需要回滚或重试。
        /// </summary>
        CommitFailed = 14,

        /// <summary>
        /// UndoFailed（值 = 15）。
        /// 在回滚（undo）阶段发生错误，表明对之前更改无法安全回退。
        /// </summary>
        UndoFailed = 15,

        /// <summary>
        /// AuthorizationError（值 = 16）。
        /// 授权失败（用户/凭证无权执行该操作），通常与访问控制或安全政策有关。
        /// </summary>
        AuthorizationError = 16,

        /// <summary>
        /// NotWritable（值 = 17）。
        /// 目标对象不可写（与 ReadOnly 类似，但可用于更明确的语义区分）。
        /// </summary>
        NotWritable = 17,

        /// <summary>
        /// InconsistentName（值 = 18）。
        /// 指定的对象名称与期望/上下文不一致（例如索引或命名冲突），导致操作被拒绝。
        /// </summary>
        InconsistentName = 18
    }
}
