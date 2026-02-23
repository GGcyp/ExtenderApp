using System.Diagnostics;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.Options;

namespace ExtenderApp.Test.Tests
{
    /// <summary>
    /// IOptions接口的单元测试。
    /// </summary>
    public static class IOptionsTests
    {
        private static IOptions options;

        private class TestOptions : OptionsObject
        {
            //// Expose protected/internal members for testing
            //public void SetOptionProtectedPublic<T>(OptionIdentifier<T> identifier, T value) => SetOptionProtected(identifier, value);

            //public bool TrySetOptionProtectedPublic<T>(OptionIdentifier<T> identifier, T value) => TrySetOptionProtected(identifier, value);

            //public T GetOptionProtectedPublic<T>(OptionIdentifier<T> identifier) => GetOptionProtectedValue(identifier);

            //public bool TryGetOptionProtectedPublic<T>(OptionIdentifier<T> identifier, out T value) => TryGetOptionProtected(identifier, out value);

            //public void UnRegisterOptionProtectedPublic<T>(OptionIdentifier<T> identifier) => UnRegisterOptionProtected(identifier);

            //public bool TryUnRegisterOptionProtectedPublic(OptionIdentifier identifier) => TryUnRegisterOptionProtected(identifier);

            //public void RegisterOptionChangeProtectedPublic<T>(OptionIdentifier<T> identifier, EventHandler<T> handler) => RegisterOptionChangeProtected(identifier, handler);
        }

        public static void RunAll()
        {
            options = new TestOptions();
            RegisterAndGetOptionValue_ShouldReturnValue();
            SetOptionValue_ShouldUpdateValue();
            UnRegisterOption_ShouldRemoveOption();
            RegisterOptionChange_ShouldTriggerEvent();
            UnregisterOptionChange_ShouldNotTriggerEvent();
        }

        /// <summary>
        /// 测试注册和获取选项。
        /// </summary>
        public static void RegisterAndGetOptionValue_ShouldReturnValue()
        {
            var identifier = new OptionIdentifier<int>("TestInt", OptionVisibility.Public);
            options.RegisterOption(identifier, 123);
            var value = options.GetOptionValue(identifier);
            Debug.Print(value.ToString());
        }

        /// <summary>
        /// 测试设置选项值。
        /// </summary>

        public static void SetOptionValue_ShouldUpdateValue()
        {
            var identifier = new OptionIdentifier<string>("TestString", OptionVisibility.Public);
            options.RegisterOption(identifier, "abc");
            options.SetOptionValue(identifier, "def");
            var value = options.GetOptionValue(identifier);
            Debug.Print(value.ToString());
        }

        /// <summary>
        /// 测试注销选项。
        /// </summary>

        public static void UnRegisterOption_ShouldRemoveOption()
        {
            var identifier = new OptionIdentifier<double>("TestDouble", OptionVisibility.Public);
            options.RegisterOption(identifier, 1.23);
            options.UnRegisterOption(identifier);
            var result = options.TryGetOptionValue(identifier, out double value);
            Debug.Print(result.ToString());
        }

        /// <summary>
        /// 测试注册选项值变更事件。
        /// </summary>

        public static void RegisterOptionChange_ShouldTriggerEvent()
        {
            var identifier = new OptionIdentifier<int>("TestEvent", OptionVisibility.Public);
            options.RegisterOption(identifier, 10);
            bool eventTriggered = false;
            options.RegisterOptionChange(identifier, (s, e) => { eventTriggered = true; });
            options.SetOptionValue(identifier, 20);
            Debug.Print(eventTriggered.ToString());
        }

        /// <summary>
        /// 测试注销选项值变更事件。
        /// </summary>

        public static void UnregisterOptionChange_ShouldNotTriggerEvent()
        {
            var identifier = new OptionIdentifier<int>("TestEvent2", OptionVisibility.Public);
            options.RegisterOption(identifier, 10);
            bool eventTriggered = false;
            EventHandler<(OptionIdentifier, int)> handler = (s, item) => { eventTriggered = true; };
            options.RegisterOptionChange(identifier, handler);
            options.UnregisterOptionChange(identifier, handler);
            options.SetOptionValue(identifier, 20);
            Debug.Print(eventTriggered.ToString());
        }
    }
}