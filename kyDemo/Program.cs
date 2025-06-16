using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace kyDemo
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            int processorCount = Environment.ProcessorCount;
            ThreadPoolManager.Initialize(processorCount);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!File.Exists("userdata.json"))
            {
                ParamModel.SaveUserData();
            }
            else
            {
                ParamModel.LoadUserData();
            }
            MainPage mainPage = new MainPage();
            mainPage.FormClosed += (sender, args) =>
            {
                ThreadPoolManager.Dispose();
            };
            Application.Run(mainPage);
            
        }
    }
}
