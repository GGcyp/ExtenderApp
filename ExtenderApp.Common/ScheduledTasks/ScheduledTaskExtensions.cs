namespace ExtenderApp.Common
{
    /// <summary>
    /// 扩展方法类，用于扩展 ScheduledTask 接口的功能。
    /// </summary>
    public static class ScheduledTaskExtensions
    {
        /// <summary>
        /// 启动一个计划任务，指定延迟时间和周期时间（以毫秒为单位）。
        /// </summary>
        /// <param name="taskService">计划任务实例。</param>
        /// <param name="action">要执行的操作。</param>
        /// <param name="dueTime">任务启动前的延迟时间（毫秒）。</param>
        /// <param name="period">任务执行的周期时间（毫秒）。</param>
        /// <returns>返回 void 对象，用于控制任务的执行。</returns>
        public static void Start(this ScheduledTask taskService, Action<object> action, long dueTime, long period)
        {
            taskService.Start(action, null, dueTime, period);
        }

        /// <summary>
        /// 启动一个计划任务，指定状态对象、延迟时间和周期时间（以毫秒为单位）。
        /// </summary>
        /// <param name="taskService">计划任务实例。</param>
        /// <param name="action">要执行的操作。</param>
        /// <param name="state">传递给操作的状态对象。</param>
        /// <param name="dueTime">任务启动前的延迟时间（毫秒）。</param>
        /// <param name="period">任务执行的周期时间（毫秒）。</param>
        /// <returns>返回 void 对象，用于控制任务的执行。</returns>
        public static void Start(this ScheduledTask taskService, Action<object> action, object state, long dueTime, long period)
        {
            taskService.Start(action, state, TimeSpan.FromMilliseconds(dueTime), TimeSpan.FromMilliseconds(period));
        }

        /// <summary>
        /// 启动一个延迟执行的任务，指定延迟时间（以毫秒为单位）。
        /// </summary>
        /// <param name="taskService">计划任务实例。</param>
        /// <param name="action">要执行的操作。</param>
        /// <param name="dueTime">任务启动前的延迟时间（毫秒）。</param>
        /// <returns>返回 void 对象，用于控制任务的执行。</returns>
        public static void StartDelay(this ScheduledTask taskService, Action<object> action, long dueTime)
        {
            taskService.StartDelay(action, null, TimeSpan.FromMilliseconds(dueTime));
        }

        /// <summary>
        /// 启动一个延迟执行的任务，指定状态对象和延迟时间（以毫秒为单位）。
        /// </summary>
        /// <param name="taskService">计划任务实例。</param>
        /// <param name="action">要执行的操作。</param>
        /// <param name="state">传递给操作的状态对象。</param>
        /// <param name="dueTime">任务启动前的延迟时间（毫秒）。</param>
        /// <returns>返回 void 对象，用于控制任务的执行。</returns>
        public static void StartDelay(this ScheduledTask taskService, Action<object> action, object state, long dueTime)
        {
            taskService.Start(action, state, TimeSpan.FromMilliseconds(dueTime), TimeSpan.Zero);
        }

        /// <summary>
        /// 启动一个周期性执行的任务，指定周期时间（以毫秒为单位）。
        /// </summary>
        /// <param name="taskService">计划任务实例。</param>
        /// <param name="action">要执行的操作。</param>
        /// <param name="period">任务执行的周期时间（毫秒）。</param>
        /// <returns>返回 void 对象，用于控制任务的执行。</returns>
        public static void StartCycle(this ScheduledTask taskService, Action<object> action, long period)
        {
            taskService.StartCycle(action, null, period);
        }

        /// <summary>
        /// 启动一个周期性执行的任务，指定状态对象和周期时间（以毫秒为单位）。
        /// </summary>
        /// <param name="taskService">计划任务实例。</param>
        /// <param name="action">要执行的操作。</param>
        /// <param name="state">传递给操作的状态对象。</param>
        /// <param name="period">任务执行的周期时间（毫秒）。</param>
        /// <returns>返回 void 对象，用于控制任务的执行。</returns>
        public static void StartCycle(this ScheduledTask taskService, Action<object> action, object state, long period)
        {
            taskService.Start(action, state, TimeSpan.Zero, TimeSpan.FromMilliseconds(period));
        }

        /// <summary>
        /// 启动一个计划任务，该任务在指定的延迟时间后执行一次，之后每隔指定的周期时间重复执行。
        /// </summary>
        /// <param name="taskService">计划任务服务接口实例。</param>
        /// <param name="action">要执行的动作。</param>
        /// <param name="dueTime">任务首次执行的延迟时间。</param>
        /// <param name="period">任务重复执行的周期时间。</param>
        /// <returns>返回一个void对象，用于控制任务的执行和取消。</returns>
        public static void Start(this ScheduledTask taskService, Action<object> action, TimeSpan dueTime, TimeSpan period)
        {
            taskService.Start(action, null, dueTime, period);
        }

        /// <summary>
        /// 启动一个计划任务，该任务在指定的延迟时间后执行一次。
        /// </summary>
        /// <param name="taskService">计划任务服务接口实例。</param>
        /// <param name="action">要执行的动作。</param>
        /// <param name="dueTime">任务执行的延迟时间。</param>
        /// <returns>返回一个void对象，用于控制任务的执行和取消。</returns>
        public static void StartDelay(this ScheduledTask taskService, Action<object> action, TimeSpan dueTime)
        {
            taskService.StartDelay(action, null, dueTime);
        }

        /// <summary>
        /// 延迟启动一个定时任务。
        /// </summary>
        /// <param name="taskService">定时任务服务实例。</param>
        /// <param name="action">任务执行时调用的委托。</param>
        /// <param name="state">传递给委托的状态对象。</param>
        /// <param name="dueTime">任务开始前的延迟时间。</param>
        /// <returns>返回一个扩展的CancellationToken，可用于取消任务。</returns>
        public static void StartDelay(this ScheduledTask taskService, Action<object> action, object state, TimeSpan dueTime)
        {
            taskService.Start(action, state, dueTime, TimeSpan.Zero);
        }

        /// <summary>
        /// 启动一个计划任务，该任务每隔指定的周期时间重复执行。
        /// </summary>
        /// <param name="taskService">计划任务服务接口实例。</param>
        /// <param name="action">要执行的动作。</param>
        /// <param name="period">任务重复执行的周期时间。</param>
        /// <returns>返回一个void对象，用于控制任务的执行和取消。</returns>
        public static void StartCycle(this ScheduledTask taskService, Action<object> action, TimeSpan period)
        {
            taskService.StartCycle(action, null, period);
        }

        /// <summary>
        /// 开始一个周期性的任务
        /// </summary>
        /// <param name="taskService">任务服务接口</param>
        /// <param name="action">要执行的操作</param>
        /// <param name="state">操作的状态对象</param>
        /// <param name="period">任务的周期时间间隔</param>
        /// <returns>一个void对象，用于控制任务的取消</returns>
        public static void StartCycle(this ScheduledTask taskService, Action<object> action, object state, TimeSpan period)
        {
            taskService.Start(action, state, TimeSpan.Zero, period);
        }
    }
}
