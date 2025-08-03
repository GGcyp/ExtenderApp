using System.Windows;
using System.Windows.Media.Animation;

namespace ExtenderApp.Views.Animation
{
    public abstract class GenericAnimation : AnimationTimeline
    {
        public static GenericAnimation<T> Create<T>(T from, T to, Duration duration, IEasingFunction easingFunction = null)
        {
            return new GenericAnimation<T>
            {
                From = from,
                To = to,
                Duration = duration,
                EasingFunction = easingFunction
            };
        }
    }

    /// <summary>
    /// 表示一个泛型动画类，继承自 AnimationTimeline 类。
    /// </summary>
    /// <typeparam name="T">动画值的数据类型。</typeparam>
    public class GenericAnimation<T> : GenericAnimation
    {
        /// <summary>
        /// 获取动画的目标属性值类型。
        /// </summary>
        /// <returns>返回动画的目标属性值类型。</returns>
        public override Type TargetPropertyType => typeof(T);

        /// <summary>
        /// 表示动画起始值的依赖属性。
        /// </summary>
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From",
                typeof(T),
                typeof(GenericAnimation<T>));

        /// <summary>
        /// 获取或设置动画的起始值。
        /// </summary>
        public T From
        {
            get => (T)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        /// <summary>
        /// 表示动画结束值的依赖属性。
        /// </summary>
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To",
                typeof(T),
                typeof(GenericAnimation<T>));

        /// <summary>
        /// 获取或设置动画的结束值。
        /// </summary>
        public T To
        {
            get => (T)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        /// <summary>
        /// 表示动画缓动函数的依赖属性。
        /// </summary>
        public static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(GenericAnimation<T>));

        /// <summary>
        /// 获取或设置动画的缓动函数。
        /// </summary>
        public IEasingFunction EasingFunction
        {
            get => (IEasingFunction)GetValue(EasingFunctionProperty);
            set => SetValue(EasingFunctionProperty, value);
        }

        /// <summary>
        /// 根据给定的默认起始值、默认结束值和动画时钟，获取当前动画值。
        /// </summary>
        /// <param name="defaultOriginValue">动画的默认起始值。</param>
        /// <param name="defaultDestinationValue">动画的默认结束值。</param>
        /// <param name="animationClock">动画时钟。</param>
        /// <returns>返回当前动画值。</returns>
        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            dynamic fromValue = From;
            dynamic toValue = To;

            double progress = animationClock.CurrentProgress.Value;
            if (EasingFunction != null)
            {
                progress = EasingFunction.Ease(progress);
            }

            return (T)((1 - progress) * fromValue + progress * toValue);
        }

        /// <summary>
        /// 创建当前类的实例。
        /// </summary>
        /// <returns>返回当前类的实例。</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new GenericAnimation<T>();
        }
    }
}
