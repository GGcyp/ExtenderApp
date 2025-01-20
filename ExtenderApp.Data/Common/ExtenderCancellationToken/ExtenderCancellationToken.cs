namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个可取消操作的令牌，用于与 <see cref="ExtenderCancellationTokenSource"/> 配合使用。
    /// </summary>
    public struct ExtenderCancellationToken : IEquatable<ExtenderCancellationToken>
    {
        /// <summary>
        /// 取消令牌的源。
        /// </summary>
        private ExtenderCancellationTokenSource source;

        /// <summary>
        /// 获取一个值，该值指示是否可以操作。
        /// </summary>
        public bool CanOperate => source is not null && source.CanOperate;

        /// <summary>
        /// 使用指定的 <see cref="ExtenderCancellationTokenSource"/> 初始化 <see cref="ExtenderCancellationToken"/> 实例。
        /// </summary>
        /// <param name="source">取消令牌的源。</param>
        internal ExtenderCancellationToken(ExtenderCancellationTokenSource source)
        {
            this.source = source;
        }

        public ExtenderCancellationToken()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 注册一个回调函数，以便在请求取消时执行。
        /// </summary>
        /// <param name="callback">要注册的回调函数。</param>
        public void Register(Action callback)
        {
            source.Register(callback);
        }

        #region Pause

        /// <summary>
        /// 暂停指定的毫秒数
        /// </summary>
        /// <param name="millisecondsDelay">暂停的毫秒数</param>
        public void Pause(long millisecondsDelay)
        {
            if (!CanOperate) return;

            source.Pause(millisecondsDelay);
        }

        /// <summary>
        /// 暂停指定的时间间隔
        /// </summary>
        /// <param name="delay">暂停的时间间隔</param>
        public void Pause(TimeSpan delay)
        {
            if (!CanOperate) return;

            source.Pause(delay);
        }

        /// <summary>
        /// 立即暂停
        /// </summary>
        public void Pause()
        {
            if (!CanOperate) return;

            source.Pause();
        }

        #endregion

        #region Resume

        /// <summary>
        /// 恢复播放并延迟指定的毫秒数
        /// </summary>
        /// <param name="millisecondsDelay">延迟的毫秒数</param>
        public void Resume(long millisecondsDelay)
        {
            if (!CanOperate) return;

            source.Resume(millisecondsDelay);
        }

        /// <summary>
        /// 恢复播放并延迟指定的时间间隔
        /// </summary>
        /// <param name="delay">延迟的时间间隔</param>
        public void Resume(TimeSpan delay)
        {
            if (!CanOperate) return;

            source.Resume(delay);
        }

        /// <summary>
        /// 立即恢复播放
        /// </summary>
        public void Resume()
        {
            if (!CanOperate) return;

            source.Resume();
        }

        #endregion

        #region Stop

        /// <summary>
        /// 停止播放并延迟指定的毫秒数
        /// </summary>
        /// <param name="millisecondsDelay">延迟的毫秒数</param>
        public void Stop(long millisecondsDelay)
        {
            if (!CanOperate) return;

            source.Stop(millisecondsDelay);
            source = null;
        }

        /// <summary>
        /// 停止播放并延迟指定的时间间隔
        /// </summary>
        /// <param name="delay">延迟的时间间隔</param>
        public void Stop(TimeSpan delay)
        {
            if (!CanOperate) return;

            source.Stop(delay);
            source = null;
        }

        /// <summary>
        /// 立即停止播放
        /// </summary>
        public void Stop()
        {
            if (!CanOperate) return;

            source.Stop();
            source = null;
        }

        #endregion

        #region Override

        /// <summary>
        /// 确定指定的对象是否等于当前对象。
        /// </summary>
        /// <param name="other">要与当前对象进行比较的对象。</param>
        /// <returns>如果指定的对象等于当前对象，则为 true；否则为 false。</returns>
        public bool Equals(ExtenderCancellationToken other)
        {
            return source == other.source;
        }

        /// <summary>
        /// 指示两个指定的 <see cref="ExtenderCancellationToken"/> 实例是否相等。
        /// </summary>
        /// <param name="left">要比较的第一个 <see cref="ExtenderCancellationToken"/> 实例。</param>
        /// <param name="right">要比较的第二个 <see cref="ExtenderCancellationToken"/> 实例。</param>
        /// <returns>如果两个实例相等，则为 true；否则为 false。</returns>
        public static bool operator ==(ExtenderCancellationToken left, ExtenderCancellationToken right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 指示两个指定的 <see cref="ExtenderCancellationToken"/> 实例是否不相等。
        /// </summary>
        /// <param name="left">要比较的第一个 <see cref="ExtenderCancellationToken"/> 实例。</param>
        /// <param name="right">要比较的第二个 <see cref="ExtenderCancellationToken"/> 实例。</param>
        /// <returns>如果两个实例不相等，则为 true；否则为 false。</returns>
        public static bool operator !=(ExtenderCancellationToken left, ExtenderCancellationToken right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 确定指定的对象是否等于当前对象。（重写 <see cref="object.Equals(object)"/>。）
        /// </summary>
        /// <param name="obj">要与当前对象进行比较的对象。</param>
        /// <returns>如果指定的对象等于当前对象，则为 true；否则为 false。</returns>
        public override bool Equals(object obj)
        {
            return obj is ExtenderCancellationToken && Equals((ExtenderCancellationToken)obj);
        }

        /// <summary>
        /// 作为默认哈希函数。
        /// </summary>
        /// <returns>当前对象的哈希代码。</returns>
        public override int GetHashCode()
        {
            return source.GetHashCode();
        }

        #endregion
    }
}
