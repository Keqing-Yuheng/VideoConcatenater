using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace VideoConcatenater
{
    public static partial class Constant
    {
        public const string FFmpeg_LIST = "List.txt";
        public const string Color_Default_Blue = "#FF3399FF";
        public const string Color_WarnDraw_Yellow = "#FFFFDD00";
        //public const string Color_WarnText_Yellow = "#FFFFBB00";
    }
    
    public static partial class Varible
    {
        public static bool CN = true;
        public static string Command;
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Varible.Command = "ffmpeg -f concat -safe 0 -i " + Constant.FFmpeg_LIST + " -c copy " + textbox_SelectOut.Text;
            try
            {
                textblk_Command.Text = Varible.Command; //初始化命令显示
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            Varible.CN = System.Globalization.CultureInfo.InstalledUICulture.Name == "zh-CN" ? true : false;
            if (!Varible.CN)    //国际化
            {
                text_Title.Text = "Video Concatenater";
                text_SelectIn.Text = "Source Folder";
                text_SelectOut.Text = "Output File";
                text_Command.Text = "Command";
                chkbox_GenerateOnly.Content = "Only Generate List";
                btn_CopyCommand.Content = "Copy";
                btn_Run.Content = "Run";
                textblk_NameWarn.Text = "[WARN] Output File Already Exists. Overwrite?";
                textblk_OutNotDefined.Text = "[ERROR] Output File Not Defined!";
                textblk_InNotFound.Text = "[ERROR] Input Folder Not Exist!";
                textblk_Working.Text = "Video Generating, Please Wait...";
                textblk_Fail.Text = "[ERROR] Video Generation Failed!";
                expander_About.Header = "About";
            }
            if(File.Exists(textbox_SelectOut.Text))
            {
                textblk_NameWarn.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 调用FFmpeg
        /// </summary>
        public void FFmpeg_Concat(string list, string output)
        {
            textblk_Working.Visibility = Visibility.Visible;
            Process proc = new Process();
            proc.StartInfo.FileName = "ffmpeg.exe";
            proc.StartInfo.Arguments = "-f concat -safe 0 -i " + list + " -c copy " + output;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            BackgroundWorker worker = new BackgroundWorker();   //异步以保证界面可刷新
            worker.DoWork += (s, e) =>
            {
                try
                {
                    proc.Start();
                    StreamWriter sw = proc.StandardInput;
                    sw.WriteLine("y");  //遇到FFmpeg覆盖文件之类的提示时自动确认以免阻塞
                    sw.Close();
                    proc.WaitForExit(); //等待FFmpeg结束
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                textblk_Working.Visibility = Visibility.Hidden;
            };
            worker.RunWorkerAsync();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            textblk_OutNotDefined.Visibility = Visibility.Hidden;   //1.复位警告信息
            textblk_InNotFound.Visibility = Visibility.Hidden;
            textblk_Fail.Visibility = Visibility.Hidden;
            progressbar_run.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Constant.Color_Default_Blue));
            if (textbox_SelectOut.Text == "")   //2.检测是否输出设置是否为空
            {
                textblk_OutNotDefined.Visibility = Visibility.Visible;
                return; //为空则返回
            }
            string DirPath = textbox_SelectIn.Text;
            if (DirPath.Length >= 2 && DirPath[DirPath.Length - 1] == '\\') //3.检测输入目录是否以\结束
            {
                DirPath = DirPath.Remove(DirPath.Length - 1, 1);
            }
            if (DirPath == "")   //4.检测是否输入设置是否为空
            {
                DirPath = ".";  //为空则设置为当前目录
            }
            DirectoryInfo DirInfo = new DirectoryInfo(DirPath);
            FileInfo[] DirFile;
            try //用try-catch避免目录不存在的问题
            {
                DirFile = DirInfo.GetFiles("*", SearchOption.TopDirectoryOnly);    //GetFiles返回目录的文件列表 AllDirectories查找子目录 TopDirectoryOnly不查找子目录
            }
            catch
            {
                textblk_InNotFound.Visibility = Visibility.Visible;
                return;
            }
            StreamWriter sw_cover = new StreamWriter(Constant.FFmpeg_LIST, false);  //清空文件
            sw_cover.Write("");
            sw_cover.Close();
            StreamWriter sw_append = new StreamWriter(Constant.FFmpeg_LIST, true);  //以附加的方式写入
            string[] FileList = new string[DirFile.Length]; //在开始和结束时保存数据的数组
            int FileNum = DirFile.Length;
            int FileLenMin = 255;
            int FileLenMax = 0;
            for (int i = 0; i < FileNum; i++)
            {
                FileList[i] = DirFile[i].Name;
                int FileLen = FileList[i].Length;   //获取文件名长度最值 便于按长度分类
                if (FileLen > FileLenMax)
                {
                    FileLenMax = FileLen;
                }
                if (FileLen < FileLenMin)
                {
                    FileLenMin = FileLen;
                }
            }
            string[][] FileGroup = new string[FileLenMax - FileLenMin + 1][];   //分类数组定义第1个维度
            for (int i = 0; i < FileLenMax - FileLenMin + 1; i++)   //分类数组定义第2个维度
            {
                FileGroup[i] = new string[FileNum];
            }
            for (int i = FileLenMin; i <= FileLenMax; i++)  //分类 i是该轮要查找的文件名长度值
            {
                for (int j = 0; j < FileNum; j++)    //检测第j个的长度
                {
                    if (FileList[j].Length == i)    //将检测到长度符合的存入分类数组
                    {
                        int k;
                        for (k = 0; k < FileNum; k++)    //存入在数组中找到的第1个null的位置
                        {
                            if (FileGroup[i - FileLenMin][k] == null)
                            {
                                FileGroup[i - FileLenMin][k] = FileList[j];
                                break;
                            }
                        }
                    }
                }
            }
            Array.Clear(FileList, 0, FileList.Length);  //清除原数组
            for (int i = FileLenMin; i <= FileLenMax; i++)  //读取分类结果 i是该轮要操作的文件名长度值
            {
                Array.Sort(FileGroup[i - FileLenMin]);  //排序某个长度的文件名数组
                for (int j = 0; j < FileGroup[i - FileLenMin].Length; j++) //将分类数组的内容复制到结果数组
                {
                    int k;
                    for (k = 0; k < FileNum; k++)    //存入结果数组中第1个null的位置
                    {
                        if (FileList[k] == null)
                        {
                            FileList[k] = FileGroup[i - FileLenMin][j];
                            break;
                        }
                    }
                }
            }
            foreach (string str in FileList)     //输出数组到文件
            {
                sw_append.WriteLine("file \'" + DirPath + "\\" + str + "\'"); //FFmpeg要求的格式
            }
            sw_append.Close();
            if (!(bool)chkbox_GenerateOnly.IsChecked)    //未勾选 即需要执行合并操作
            {
                FFmpeg_Concat(Constant.FFmpeg_LIST, textbox_SelectOut.Text);
                if (!File.Exists(textbox_SelectOut.Text))    //生成文件不存在则判定为执行失败
                {
                    progressbar_run.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Constant.Color_WarnDraw_Yellow));
                    textblk_Fail.Visibility = Visibility.Visible;
                }
            }
            BackgroundWorker worker_100 = new BackgroundWorker();   //异步
            worker_100.DoWork += (s, ex) =>
            {

            };
            worker_100.RunWorkerCompleted += (s, ex) =>
            {
                progressbar_run.Value = 100;  //进度条
            };
            worker_100.RunWorkerAsync();
        }
        
        private void textblk_SelectOut_TextChanged(object sender, TextChangedEventArgs e)
        {   //使用try-catch可缓解错误
            Varible.Command = "ffmpeg -f concat -safe 0 -i " + Constant.FFmpeg_LIST + " -c copy " + textbox_SelectOut.Text; //显示命令
            try
            {
                textblk_Command.Text = Varible.Command;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            try
            {
                if (File.Exists(textbox_SelectOut.Text))    //提示文件名是否重复
                {
                    textblk_NameWarn.Visibility = Visibility.Visible;   //更改后输出文件名仍然重复
                }
                else
                {
                    textblk_NameWarn.Visibility = Visibility.Hidden;    //更改后输出文件名不重复
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void btn_CopyCommand_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(Varible.Command, true); //复制命令到剪贴板
        }
    }
}
