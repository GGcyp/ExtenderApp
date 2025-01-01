using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtenderApp.Abstract;
using ExtenderApp.Views;
using LLama.Common;
using LLama;

namespace ExtenderApp.ML
{
    /// <summary>
    /// MachineLearningMainView.xaml 的交互逻辑
    /// </summary>
    public partial class MachineLearningMainView : ExtenderAppView
    {
        public MachineLearningMainView()
        {
            InitializeComponent();
            //Temp();
        }


        //private async void Temp()
        //{
        //    // 请更改为你自己的模型路径
        //    string modelPath = @"<Your model path>";
        //    var prompt = "Transcript of a dialog, where the User interacts with an Assistant named Bob. Bob is helpful, kind, honest, good at writing, and never fails to answer the User's requests immediately and with precision.\r\n\r\nUser: Hello, Bob.\r\nBob: Hello. How may I help you today?\r\nUser: Please tell me the largest city in Europe.\r\nBob: Sure. The largest city in Europe is Moscow, the capital of Russia.\r\nUser:";

        //    // 加载模型
        //    var parameters = new ModelParams(modelPath) { ContextSize = 1024, GpuLayerCount = 5 };
        //    using var model = LLamaWeights.LoadFromFile(parameters);

        //    // 初始化一个聊天会话
        //    using var context = model.CreateContext(parameters);
        //    var ex = new InteractiveExecutor(context);
        //    ChatSession session = new ChatSession(ex);

        //    // 展示提示内容
        //    Console.WriteLine();
        //    Console.Write(prompt);

        //    // 循环运行推理，与 LLM 聊天
        //    while (prompt != "stop")
        //    {
        //        await foreach (var text in session.ChatAsync(new ChatHistory.Message(AuthorRole.User, prompt), new InferenceParams { AntiPrompts = new List<string> { "User:" } }))
        //        {
        //            Console.Write(text);
        //        }
        //        prompt = Console.ReadLine() ?? "";
        //    }

        //    // 保存会话
        //    session.SaveSession("SavedSessionPath");
        //}
    }
}
