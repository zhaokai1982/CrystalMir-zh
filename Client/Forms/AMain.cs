﻿using System.Diagnostics;
using System.Net;
using Client;
using Microsoft.Web.WebView2.Core;
using System.Net.Http.Headers;
using System.Net.Http.Handlers;
using Client.Utils;

namespace Launcher
{
    public partial class AMain : Form
    {
        long _totalBytes, _completedBytes;
        private int _fileCount, _currentCount;

        public bool Completed, Checked, CleanFiles, LabelSwitch, ErrorFound;

        public List<FileInformation> OldList;
        public Queue<FileInformation> DownloadList = new Queue<FileInformation>();
        public List<Download> ActiveDownloads = new List<Download>();

        private Stopwatch _stopwatch = Stopwatch.StartNew();

        public Thread _workThread;

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private Config ConfigForm = new Config();

        private bool Restart = false;

        public AMain()
        {
            InitializeComponent();

            BackColor = Color.FromArgb(1, 0, 0);
            TransparencyKey = Color.FromArgb(1, 0, 0);
        }

        public static void SaveError(string ex)
        {
            try
            {
                if (Settings.RemainingErrorLogs-- > 0)
                {
                    File.AppendAllText(@".\Error.txt",
                                       string.Format("[{0}] {1}{2}", DateTime.Now, ex, Environment.NewLine));
                }
            }
            catch
            {
            }
        }

        public void Start()
        {
            try
            {
                GetOldFileList();

                if (OldList.Count == 0)
                {
                    MessageBox.Show(GameLanguage.PatchErr);
                    Completed = true;
                    return;
                }

                _fileCount = OldList.Count;
                for (int i = 0; i < OldList.Count; i++)
                    CheckFile(OldList[i]);

                Checked = true;
                _fileCount = 0;
                _currentCount = 0;

                _fileCount = DownloadList.Count;

                ServicePointManager.DefaultConnectionLimit = Settings.P_Concurrency;

                _stopwatch = Stopwatch.StartNew();
                for (var i = 0; i < Settings.P_Concurrency; i++)
                    BeginDownload();


            }
            catch (EndOfStreamException ex)
            {
                MessageBox.Show("已找到数据源.主机可能使用1.1.0.0之前的补丁系统");
                Completed = true;
                SaveError(ex.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接自动更新服务器失败.但不影响游戏登录.", "提示！");  //修复连接失败错误提示
                Completed = true;
                SaveError(ex.ToString());
            }

            _stopwatch.Stop();
        }

        private void BeginDownload()
        {
            if (DownloadList.Count == 0)
            {
                Completed = true;

                CleanUp();
                return;
            }

            var download = new Download();
            download.Info = DownloadList.Dequeue();
            DownloadFile(download);
            
        }

        private void CleanUp()
        {
            if (!CleanFiles) return;

            string[] fileNames = Directory.GetFiles(@".\", "*.*", SearchOption.AllDirectories);
            string fileName;
            for (int i = 0; i < fileNames.Length; i++)
            {
                if (fileNames[i].StartsWith(".\\Screenshots\\")) continue;  //截图保存目录

                fileName = Path.GetFileName(fileNames[i]);

                if (fileName == "Mir2Config.ini" || fileName == System.AppDomain.CurrentDomain.FriendlyName) continue;

                try
                {
                    if (!NeedFile(fileNames[i]))
                        File.Delete(fileNames[i]);
                }
                catch { }
            }
        }
        public bool NeedFile(string fileName)
        {
            for (int i = 0; i < OldList.Count; i++)
            {
                if (fileName.EndsWith(OldList[i].FileName))
                    return true;
            }

            return false;
        }

        private void GetOldFileList()
        {
            OldList = new List<FileInformation>();

            byte[] data = Download(Settings.P_PatchFileName);
            if (data != null)
            {
                using MemoryStream stream = new MemoryStream(data);
                using BinaryReader reader = new BinaryReader(stream);

                if (reader.ReadByte() == 60)
                {
                    //假设反馈了一个带有错误代码的html页面，并非是补丁列表连接报错
                    return;
                }
                reader.BaseStream.Seek(0,SeekOrigin.Begin);
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    OldList.Add(new FileInformation(reader));
                }
            }
        }


        public void ParseOld(BinaryReader reader)
        {
            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
                OldList.Add(new FileInformation(reader));
        }

        public void CheckFile(FileInformation old)
        {
            FileInformation info = GetFileInformation(Settings.P_Client + old.FileName);
            _currentCount++;

            if (info == null || old.Length != info.Length || old.Creation != info.Creation)
            {
                DownloadList.Enqueue(old);
                _totalBytes += old.Length;
            }
        }

        private int errorcount = 0;

        public void DownloadFile(Download dl)
        {
            var info = dl.Info;
            string fileName = info.FileName.Replace(@"\", "/");

            if (fileName != "PList.gz" && (info.Compressed != info.Length || info.Compressed == 0))
            {
                fileName += ".gz";
            }

            try
            {
                HttpClientHandler httpClientHandler = new() { AllowAutoRedirect = true };
                ProgressMessageHandler progressMessageHandler = new(httpClientHandler);

                progressMessageHandler.HttpReceiveProgress += (_, args) =>
                {

                    dl.CurrentBytes = args.BytesTransferred;

                };

                using (HttpClient client = new(progressMessageHandler))
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.AcceptCharset.Clear();
                    client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));

                    if (Settings.P_NeedLogin)
                    {
                        string authInfo = Settings.P_Login + ":" + Settings.P_Password;
                        authInfo = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(authInfo));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                    }

                    ActiveDownloads.Add(dl);

                    var task = Task.Run(() => client.GetAsync(new Uri($"{Settings.P_Host}{fileName}"), HttpCompletionOption.ResponseHeadersRead));
                    var response = task.Result;

                    var task2 = Task.Run(() => response.Content.ReadAsByteArrayAsync());
                    byte[] data = task2.Result;

                    _currentCount++;
                    _completedBytes += dl.CurrentBytes;
                    dl.CurrentBytes = 0;
                    dl.Completed = true;

                    if (info.Compressed > 0 && info.Compressed != info.Length)
                    {
                        data = Functions.DecompressBytes(data);
                    }

                    var fileNameOut = Settings.P_Client + info.FileName;
                    var dirName = Path.GetDirectoryName(fileNameOut);
                    if (!Directory.Exists(dirName))
                        Directory.CreateDirectory(dirName);

                    //如果需要，请先删除原始文件
                    string[] specialfiles = { ".dll", ".exe", ".pdb" };
                    if (File.Exists(fileNameOut) && ( specialfiles.Contains( Path.GetExtension(fileNameOut).ToLower() )))
                    {
                        string oldFilename = Path.Combine(Path.GetDirectoryName(fileNameOut), ("Old__" + Path.GetFileName(fileNameOut)));

                        try
                        {
                            //如果有另一个以前版本的备份：请先删除它
                            if (File.Exists(oldFilename))
                            {
                                File.Delete(oldFilename);   
                            }
                            File.Move(fileNameOut, oldFilename);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            SaveError(ex.ToString());
                            errorcount++;
                            if (errorcount == 5)
                                MessageBox.Show("出现了太多问题，将不再显示以后的错误");
                            if (errorcount < 5)
                                MessageBox.Show("保存此文件时发生问题: " + fileNameOut);
                        }
                        catch (Exception ex)
                        {
                            SaveError(ex.ToString());
                            errorcount++;
                            if (errorcount == 5)
                                MessageBox.Show("出现了太多问题，将不再显示以后的错误");
                            if (errorcount < 5)
                                MessageBox.Show("保存此文件时发生问题: " + fileNameOut);
                        }
                        finally
                        {
                            //如果长时间无法访问，可能会导致无限循环，卡在检测
                            Restart = true;
                        }
                    }

                    File.WriteAllBytes(fileNameOut, data);
                    File.SetLastWriteTime(fileNameOut, info.Creation);
                }
            }
            catch (HttpRequestException e)
            {
                File.AppendAllText(@".\Error.txt",
                                       $"[{DateTime.Now}] {info.FileName} 无法下载 ({e.Message}) {Environment.NewLine}");
                ErrorFound = true;
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
                errorcount++;
                if (errorcount == 5)
                    MessageBox.Show("出现了太多问题，将不再显示以后的错误");
                if (errorcount < 5)
                    MessageBox.Show("保存此文件时发生问题: " + dl.Info.FileName);
            }
            finally
            {
                if (ErrorFound)
                {
                MessageBox.Show(string.Format("下载文件失败: {0}", fileName));
                }
            }

            BeginDownload();
        }

        public byte[] Download(string fileName)
        {
            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.AcceptCharset.Clear();
                client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));

                if (Settings.P_NeedLogin)
                {
                    string authInfo = Settings.P_Login + ":" + Settings.P_Password;
                    authInfo = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(authInfo));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authInfo);
                }

                string uriString = Settings.P_Host + Path.ChangeExtension(fileName, ".gz");

                if (Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
                {
                    var task = Task.Run(() => client.GetAsync(new Uri(uriString), HttpCompletionOption.ResponseHeadersRead));
                    var response = task.Result;
                    using Stream sm = response.Content.ReadAsStream();
                    using MemoryStream ms = new();
                    sm.CopyTo(ms);
                    byte[] data = ms.ToArray();
                    return data;
                }
                else
                {
                    MessageBox.Show(string.Format("请检查启动器的 HOST 设置格式是否正确\n可能是缺少或多余的斜杠或拼写错误造成的。\n如果不需要补丁，可以忽略此错误。"), "HOST 格式错误");
                    return null;
                }
            }
        }

        public FileInformation GetFileInformation(string fileName)
        {
            if (!File.Exists(fileName)) return null;

            FileInfo info = new FileInfo(fileName);
            return new FileInformation
            {
                FileName = fileName.Remove(0, Settings.P_Client.Length),
                Length = (int)info.Length,
                Creation = info.LastWriteTime
            };
        }

        private void AMain_Load(object sender, EventArgs e)
        {
            var envir = CoreWebView2Environment.CreateAsync(null, Settings.ResourcePath).Result;
            Main_browser.EnsureCoreWebView2Async(envir);

            if (Settings.P_BrowserAddress != "")
            {
                if (Uri.IsWellFormedUriString(Settings.P_BrowserAddress, UriKind.Absolute))
                {
                    Main_browser.NavigationCompleted += Main_browser_NavigationCompleted;
                    Main_browser.Source = new Uri(Settings.P_BrowserAddress);
                }
                else
                {
                    MessageBox.Show(string.Format("请检查启动器的 BROWSER 设置格式是否正确。\n可能是缺少或多余的斜杠或拼写错误造成的。\n如果不需要特别注意此设置，可以忽略此错误。"), "BROWSER 格式错误");
                }
            }

            RepairOldFiles();

            Launch_pb.Enabled = false;
            ProgressCurrent_pb.Width = 5;
            TotalProg_pb.Width = 5;
            Version_label.Text = string.Format("版本: {0}.{1}.{2}", Globals.ProductCodename, Settings.UseTestConfig ? "Debug" : "Release", Application.ProductVersion);

            if (Settings.P_ServerName != String.Empty)
            {
                Name_label.Visible = true;
                Name_label.Text = Settings.P_ServerName;
            }

            _workThread = new Thread(Start) { IsBackground = true };
            _workThread.Start();
        }

        private void Main_browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (Main_browser.Source.AbsolutePath != "blank") Main_browser.Visible = true;
        }

        private void Launch_pb_Click(object sender, EventArgs e)
        {
            Launch();
        }

        private void Launch()
        {
            if (ConfigForm.Visible) ConfigForm.Visible = false;

            Program.Launch = true;
            Close();
        }

        private void Close_pb_Click(object sender, EventArgs e)
        {
            if (ConfigForm.Visible) ConfigForm.Visible = false;
            Close();
        }

        private void Movement_panel_MouseClick(object sender, MouseEventArgs e)
        {
            if (ConfigForm.Visible) ConfigForm.Visible = false;
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void Movement_panel_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void Movement_panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void Launch_pb_MouseEnter(object sender, EventArgs e)
        {
            Launch_pb.Image = Client.Resources.Images.Launch_Hover;
        }

        private void Launch_pb_MouseLeave(object sender, EventArgs e)
        {
            Launch_pb.Image = Client.Resources.Images.Launch_Base1;
        }

        private void Close_pb_MouseEnter(object sender, EventArgs e)
        {
            Close_pb.Image = Client.Resources.Images.Cross_Hover;
        }

        private void Close_pb_MouseLeave(object sender, EventArgs e)
        {
            Close_pb.Image = Client.Resources.Images.Cross_Base;
        }

        private void Launch_pb_MouseDown(object sender, MouseEventArgs e)
        {
            Launch_pb.Image = Client.Resources.Images.Launch_Pressed;
        }

        private void Launch_pb_MouseUp(object sender, MouseEventArgs e)
        {
            Launch_pb.Image = Client.Resources.Images.Launch_Base1;
        }

        private void Close_pb_MouseDown(object sender, MouseEventArgs e)
        {
            Close_pb.Image = Client.Resources.Images.Cross_Pressed;
        }

        private void Close_pb_MouseUp(object sender, MouseEventArgs e)
        {
            Close_pb.Image = Client.Resources.Images.Cross_Base;
        }

        private void ProgressCurrent_pb_SizeChanged(object sender, EventArgs e)
        {
            ProgEnd_pb.Location = new Point((ProgressCurrent_pb.Location.X + ProgressCurrent_pb.Width), ProgressCurrent_pb.Location.Y);
            if (ProgressCurrent_pb.Width == 0) ProgEnd_pb.Visible = false;
            else ProgEnd_pb.Visible = true;
        }

        private void Config_pb_MouseDown(object sender, MouseEventArgs e)
        {
            Config_pb.Image = Client.Resources.Images.Config_Pressed;
        }

        private void Config_pb_MouseEnter(object sender, EventArgs e)
        {
            Config_pb.Image = Client.Resources.Images.Config_Hover;
        }

        private void Config_pb_MouseLeave(object sender, EventArgs e)
        {
            Config_pb.Image = Client.Resources.Images.Config_Base;
        }

        private void Config_pb_MouseUp(object sender, MouseEventArgs e)
        {
            Config_pb.Image = Client.Resources.Images.Config_Base;
        }

        private void Config_pb_Click(object sender, EventArgs e)
        {
            if (ConfigForm.Visible) ConfigForm.Hide();
            else ConfigForm.Show(Program.PForm);
            ConfigForm.Location = new Point(Location.X + Config_pb.Location.X - 183, Location.Y + 36);
        }

        private void TotalProg_pb_SizeChanged(object sender, EventArgs e)
        {
            ProgTotalEnd_pb.Location = new Point((TotalProg_pb.Location.X + TotalProg_pb.Width), TotalProg_pb.Location.Y);
            if (TotalProg_pb.Width == 0) ProgTotalEnd_pb.Visible = false;
            else ProgTotalEnd_pb.Visible = true;
        }

        private void InterfaceTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Completed && ActiveDownloads.Count == 0)
                {
                    ActionLabel.Text = "";
                    CurrentFile_label.Text = "数据更新";
                    SpeedLabel.Text = "";
                    ProgressCurrent_pb.Width = 550;
                    TotalProg_pb.Width = 550;
                    CurrentFile_label.Visible = true;
                    CurrentPercent_label.Visible = true;
                    TotalPercent_label.Visible = true;
                    CurrentPercent_label.Text = "100%";
                    TotalPercent_label.Text = "100%";
                    InterfaceTimer.Enabled = false;
                    Launch_pb.Enabled = true;
                    if (ErrorFound) MessageBox.Show("一个或多个文件下载失败，请检查错误", "下载失败");
                    ErrorFound = false;

                    if (CleanFiles)
                    {
                        CleanFiles = false;
                        MessageBox.Show("文件已清理", "清理文件");
                    }

                    if (Restart)
                    {
                        Program.Restart = true;

                        Close();
                    }

                    if (Settings.P_AutoStart)
                    {
                        Launch();
                    }
                    return;
                }

                var currentBytes = 0L;
                FileInformation currentFile = null;

                // 删除已完成的下载..
                for (var i = ActiveDownloads.Count - 1; i >= 0; i--)
                {
                    var dl = ActiveDownloads[i];

                    if (dl.Completed)
                    {
                        ActiveDownloads.RemoveAt(i);
                        continue;
                    }
                }

                for (var i = ActiveDownloads.Count - 1; i >= 0; i--)
                {
                    var dl = ActiveDownloads[i];
                    if (!dl.Completed)
                        currentBytes += dl.CurrentBytes;
                }

                if (Settings.P_Concurrency == 1)
                {
                    // 注意：在完成更好的UI之前，现在只需模仿旧的行为.
                    if (ActiveDownloads.Count > 0)
                        currentFile = ActiveDownloads[0].Info;
                }

                ActionLabel.Visible = true;
                SpeedLabel.Visible = true;
                CurrentFile_label.Visible = true;
                CurrentPercent_label.Visible = true;
                TotalPercent_label.Visible = true;

                if (LabelSwitch) ActionLabel.Text = string.Format("{0} 剩余文件", _fileCount - _currentCount);
                else ActionLabel.Text = string.Format("{0:#,##0}MB 待更文件", ((_totalBytes) - (_completedBytes + currentBytes)) / 1024 / 1024);

                if (Settings.P_Concurrency > 1)
                {
                    CurrentFile_label.Text = string.Format("<Concurrent> {0}", ActiveDownloads.Count);
                    SpeedLabel.Text = ToSize(currentBytes / _stopwatch.Elapsed.TotalSeconds);
                }
                else
                {
                    if (currentFile != null)
                    {
                        CurrentFile_label.Text = string.Format("{0}", currentFile.FileName);
                        SpeedLabel.Text = ToSize(currentBytes / _stopwatch.Elapsed.TotalSeconds);
                        CurrentPercent_label.Text = ((int)(100 * currentBytes / currentFile.Length)).ToString() + "%";
                        ProgressCurrent_pb.Width = (int)(5.5 * (100 * currentBytes / currentFile.Length));
                    }
                }

                if (!(_completedBytes is 0 && currentBytes is 0 && _totalBytes is 0))
                {
                    TotalProg_pb.Width = (int)(5.5 * (100 * (_completedBytes + currentBytes) / _totalBytes));
                    TotalPercent_label.Text = ((int)(100 * (_completedBytes + currentBytes) / _totalBytes)).ToString() + "%";
                }

            }
            catch
            {
                //to-do 
            }

        }

        private void AMain_Click(object sender, EventArgs e)
        {
            if (ConfigForm.Visible) ConfigForm.Visible = false;
        }

        private void ActionLabel_Click(object sender, EventArgs e)
        {
            LabelSwitch = !LabelSwitch;
        }

        private void Credit_label_Click(object sender, EventArgs e)
        {
            if (Credit_label.Text == "Powered by Rxwx M2") Credit_label.Text = "致敬:Breezer";
            else Credit_label.Text = "技术支持水晶传奇:CrystalM2";
        }

        private void AMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            MoveOldFilesToCurrent();

            Launch_pb?.Dispose();
            Close_pb?.Dispose();
        }

        private static string[] suffixes = new[] { " B", " KB", " MB", " GB", " TB", " PB" };

        private string ToSize(double number, int precision = 2)
        {
            // 单位的字节数
            const double unit = 1024;
            // 后缀计数器
            int i = 0;
            // 只要我们比一个单位大，就继续前进
            while (number > unit)
            {
                number /= unit;
                i++;
            }
            // 应用精度和当前后缀
            return Math.Round(number, precision) + suffixes[i];
        }

        private void RepairOldFiles()
        {
            var files = Directory.GetFiles(Settings.P_Client, "*", SearchOption.AllDirectories).Where(x => Path.GetFileName(x).StartsWith("Old__"));

            foreach (var oldFilename in files)
            {
                if (!File.Exists(oldFilename.Replace("Old__", "")))
                {
                    File.Move(oldFilename, oldFilename.Replace("Old__", ""));
                }
                else
                {
                    File.Delete(oldFilename);
                }
            }
        }

        private void MoveOldFilesToCurrent()
        {
            var files = Directory.GetFiles(Settings.P_Client, "*", SearchOption.AllDirectories).Where(x => Path.GetFileName(x).StartsWith("Old__"));

            foreach (var oldFilename in files)
            {
                string originalFilename = Path.Combine(Path.GetDirectoryName(oldFilename), (Path.GetFileName(oldFilename).Replace("Old__", "")));

                if (!File.Exists(originalFilename) && File.Exists(oldFilename))
                    File.Move(oldFilename, originalFilename);
            }
        }
    } 
}
