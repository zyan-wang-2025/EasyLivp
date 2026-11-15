using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace EasyLivp; // 已改为 EasyLivp

public class MainForm : Form
{
    // ==================== UI 控件 ====================
    private Label _fileNameLabel = null!;
    private SplitContainer _splitContainer = null!;
    private ListView _fileList = null!;
    private Panel _previewPanel = null!;
    private PictureBox _pictureBox = null!;
    private VideoView _videoView = null!;
    private Label _placeholderLabel = null!;
    
    private Label _statusLabel = null!;
    private TrackBar _trackBar = null!;
    private Button _btnPrevFrame = null!;
    private Button _btnPlayPause = null!;
    private Button _btnStop = null!;
    private Button _btnNextFrame = null!;
    private Label _timeLabel = null!;
    
    private Button _btnMute = null!;
    private TrackBar _volumeTrackBar = null!;

    // ==================== LibVLC 相关 ====================
    private LibVLC _libVlc = null!;
    private MediaPlayer _mediaPlayer = null!;
    private Media? _currentMedia;
    private string? _currentMovPath;
    
    private bool _isPlaying = false;
    private bool _isDraggingTrackBar = false;
    private int _pendingStep = 0; 

    public MainForm(string[] args)
    {
        Text = "EasyLivp By Zyan"; // 修改窗口标题
        Width = 1200;
        Height = 850;
        StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 240, 240);
        this.KeyPreview = true;

        InitializeVlc();
        InitializeUI();
        BindMediaEvents();

        if (args.Length > 0 && File.Exists(args[0]))
        {
            string path = args[0];
            bool isExtract = args.Contains("--extract");
            if (isExtract) Task.Run(() => ExtractAndInform(path));
            else LoadLivp(path);
        }
    }

    private void InitializeVlc()
    {
        // 已修改：指向 Support 文件夹
        string vlcNativeDir = Path.Combine(AppContext.BaseDirectory, "Support", "libvlc", "win-x64");
        if (Directory.Exists(vlcNativeDir)) Core.Initialize(vlcNativeDir);
        else Core.Initialize();

        _libVlc = new LibVLC("--no-osd", "--no-video-title-show"); 
        _mediaPlayer = new MediaPlayer(_libVlc);
    }

    private void InitializeUI()
    {
        // ---- 1. 底部状态栏 ----
        _statusLabel = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 24,
            Text = "就绪。可通过“文件 → 打开Livp文件”开始。",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 0, 0)
        };
        Controls.Add(_statusLabel);

        // ---- 2. 底部控制栏 ----
        var controlPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 100, 
            Padding = new Padding(10, 5, 10, 5),
            BackColor = Color.FromArgb(250, 250, 250)
        };
        
        _trackBar = new TrackBar
        {
            Dock = DockStyle.Top,
            Height = 30,
            TickStyle = TickStyle.None,
            Maximum = 1000
        };
        _trackBar.MouseDown += (_, _) => _isDraggingTrackBar = true;
        _trackBar.Scroll += TrackBar_Scroll;
        _trackBar.MouseUp += (_, _) => 
        {
            _isDraggingTrackBar = false;
            if (_mediaPlayer.Media != null && _mediaPlayer.IsSeekable)
            {
                if (_mediaPlayer.State == VLCState.Ended && !string.IsNullOrEmpty(_currentMovPath))
                {
                    ResetMediaAndPlay(_currentMovPath);
                    _mediaPlayer.Pause();
                }
                _mediaPlayer.Position = (float)_trackBar.Value / _trackBar.Maximum;
            }
        };

        var flowLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 15, 0, 0) 
        };

        _btnPrevFrame = new Button { Text = "上一帧", Width = 75, Height = 30, Margin = new Padding(0, 0, 10, 0) };
        _btnPlayPause = new Button { Text = "播放", Width = 75, Height = 30, Margin = new Padding(0, 0, 10, 0) };
        _btnStop = new Button { Text = "停止", Width = 75, Height = 30, Margin = new Padding(0, 0, 10, 0) };
        _btnNextFrame = new Button { Text = "下一帧", Width = 75, Height = 30, Margin = new Padding(0, 0, 10, 0) };
        
        _timeLabel = new Label { Text = "00:00.000 / 00:00.000", AutoSize = true, Font = new Font("Consolas", 10), Margin = new Padding(0, 7, 20, 0) };
        
        _btnMute = new Button { Text = "🔊", Width = 35, Height = 30, Margin = new Padding(0, 0, 5, 0) };
        _volumeTrackBar = new TrackBar { Width = 100, Height = 30, Minimum = 0, Maximum = 100, Value = 100, TickStyle = TickStyle.None };

        _btnPrevFrame.Click += (_, _) => StepFrame(false);
        _btnPlayPause.Click += (_, _) => TogglePlayPause();
        _btnStop.Click += (_, _) => StopAndClear();
        _btnNextFrame.Click += (_, _) => StepFrame(true);
        
        _btnMute.Click += (_, _) => ToggleMute();
        _volumeTrackBar.Scroll += (_, _) => UpdateVolume(_volumeTrackBar.Value);

        flowLayout.Controls.AddRange(new Control[] { 
            _btnPrevFrame, _btnPlayPause, _btnStop, _btnNextFrame, 
            _timeLabel, _btnMute, _volumeTrackBar 
        });
        
        controlPanel.Controls.Add(flowLayout);
        controlPanel.Controls.Add(_trackBar);
        Controls.Add(controlPanel);

        // ---- 3. 文件名显示栏 ----
        _fileNameLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 30,
            Text = "当前未打开文件",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            Font = new Font("Microsoft YaHei", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(230, 230, 230)
        };
        Controls.Add(_fileNameLabel);

        // ---- 4. 左右分栏区域 ----
        _splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 800,
            SplitterWidth = 5
        };
        
        _previewPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 240, 240) };
        _placeholderLabel = new Label
        {
            Text = "请通过左上角打开livp文件，\n或拖动.livp文件到这里打开",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.Gray,
            Font = new Font("Microsoft YaHei", 14, FontStyle.Regular),
            BackColor = Color.Transparent
        };
        _pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent,
            Visible = false
        };
        _pictureBox.AllowDrop = true;
        _pictureBox.DragEnter += PictureBox_DragEnter;
        _pictureBox.DragDrop += PictureBox_DragDrop;

        _videoView = new VideoView
        {
            Dock = DockStyle.Fill,
            MediaPlayer = _mediaPlayer,
            Visible = false
        };

        _previewPanel.Controls.Add(_placeholderLabel);
        _previewPanel.Controls.Add(_pictureBox);
        _previewPanel.Controls.Add(_videoView);
        _splitContainer.Panel1.Controls.Add(_previewPanel);

        _fileList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            MultiSelect = false,
            HeaderStyle = ColumnHeaderStyle.None,
            Font = new Font("Microsoft YaHei", 10)
        };
        _fileList.Columns.Add("文件名", 350); 
        _fileList.DoubleClick += FileList_DoubleClick;
        _splitContainer.Panel2.Controls.Add(_fileList);
        
        Controls.Add(_splitContainer);

        // ---- 5. 菜单栏 ----
        var menuStrip = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("文件(&F)");
        var openItem = new ToolStripMenuItem("打开.Livp文件...", null, (_, _) => OpenLivpDialog());
        var batchExtractItem = new ToolStripMenuItem("批量解压目录...", null, (_, _) => BatchExtractDialog());
        var exitItem = new ToolStripMenuItem("退出", null, (_, _) => Close());
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { openItem, batchExtractItem, new ToolStripSeparator(), exitItem });

        var toolsMenu = new ToolStripMenuItem("工具(&T)");
        var addContextItem = new ToolStripMenuItem("注册右键菜单", null, (_, _) => RegisterContextMenu());
        var removeContextItem = new ToolStripMenuItem("移除右键菜单", null, (_, _) => RemoveContextMenu());
        var setDefaultItem = new ToolStripMenuItem("引导默认打开方式", null, (_, _) => SetAsDefaultApp());
        toolsMenu.DropDownItems.AddRange(new ToolStripItem[] { addContextItem, removeContextItem, new ToolStripSeparator(), setDefaultItem });

        // 新增：关于菜单
        var helpMenu = new ToolStripMenuItem("关于(&A)");
        helpMenu.Click += (_, _) => 
        {
            using var about = new AboutForm();
            about.ShowDialog(this);
        };

        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, toolsMenu, helpMenu });
        MainMenuStrip = menuStrip;
        Controls.Add(menuStrip);
    }

    // ==================== 音量控制逻辑 ====================
    private void ToggleMute()
    {
        if (_mediaPlayer == null) return;
        _mediaPlayer.Mute = !_mediaPlayer.Mute;
        _btnMute.Text = _mediaPlayer.Mute ? "🔇" : "🔊";
    }

    private void UpdateVolume(int volume)
    {
        if (_mediaPlayer == null) return;
        _mediaPlayer.Volume = volume;
        if (volume == 0)
        {
            _btnMute.Text = "🔇";
            _mediaPlayer.Mute = true;
        }
        else
        {
            _btnMute.Text = "🔊";
            _mediaPlayer.Mute = false;
        }
    }

    private void BindMediaEvents()
    {
        _mediaPlayer.LengthChanged += (_, e) => Invoke(() =>
        {
            UpdateTimeLabel(_mediaPlayer.Time, e.Length);
        });

        _mediaPlayer.TimeChanged += (_, e) => Invoke(() =>
        {
            if (!_isDraggingTrackBar && _mediaPlayer.Length > 0)
            {
                int progress = (int)(e.Time * 1000 / _mediaPlayer.Length);
                if (progress >= 0 && progress <= 1000)
                {
                    _trackBar.Value = progress;
                }
                UpdateTimeLabel(e.Time, _mediaPlayer.Length);
            }
        });

        _mediaPlayer.Playing += (_, _) => Invoke(() =>
        {
            _isPlaying = true;
            _btnPlayPause.Text = "暂停";
            _videoView.Visible = true;
            _pictureBox.Visible = false;

            if (_pendingStep != 0)
            {
                _mediaPlayer.Pause(); 
            }
        });

        _mediaPlayer.Paused += (_, _) => Invoke(() =>
        {
            _isPlaying = false;
            _btnPlayPause.Text = "播放";

            if (_pendingStep != 0)
            {
                ExecuteStep(_pendingStep > 0);
                _pendingStep = 0; 
            }
        });

        _mediaPlayer.EndReached += (_, _) => Invoke(() =>
        {
            _isPlaying = false;
            _btnPlayPause.Text = "播放";
            _videoView.Visible = false;
            if (_pictureBox.Image != null) _pictureBox.Visible = true;
            _statusLabel.Text = "播放结束";
        });
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Space && _btnPlayPause.Enabled)
        {
            TogglePlayPause();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void TogglePlayPause()
    {
        if (_mediaPlayer.Media == null) return;

        if (_isPlaying)
        {
            _mediaPlayer.Pause();
        }
        else
        {
            if (_mediaPlayer.State == VLCState.Ended || (_mediaPlayer.Length > 0 && _mediaPlayer.Time >= _mediaPlayer.Length))
            {
                if (!string.IsNullOrEmpty(_currentMovPath))
                {
                    ResetMediaAndPlay(_currentMovPath); 
                    return;
                }
            }
            _mediaPlayer.Play();
        }
    }

    private void StepFrame(bool next)
    {
        if (_mediaPlayer.Media == null) return;
        
        _pendingStep = next ? 1 : -1;

        if (_mediaPlayer.State == VLCState.Playing)
        {
            _mediaPlayer.Pause();
        }
        else if (_mediaPlayer.State == VLCState.Paused)
        {
            ExecuteStep(next);
            _pendingStep = 0;
        }
        else
        {
            if (_mediaPlayer.State == VLCState.Ended && !string.IsNullOrEmpty(_currentMovPath))
            {
                ResetMediaAndPlay(_currentMovPath);
            }
            else
            {
                _mediaPlayer.Play();
            }
        }
    }

    private void ExecuteStep(bool next)
    {
        if (next)
        {
            _mediaPlayer.NextFrame();
        }
        else
        {
            long targetTime = _mediaPlayer.Time - 33;
            if (targetTime < 0) targetTime = 0;
            _mediaPlayer.Time = targetTime;
        }
    }

    private void TrackBar_Scroll(object? sender, EventArgs e)
    {
        if (_mediaPlayer.Length > 0)
        {
            long simulatedTime = (long)(_mediaPlayer.Length * (double)_trackBar.Value / _trackBar.Maximum);
            UpdateTimeLabel(simulatedTime, _mediaPlayer.Length);
        }
    }

    private void StopAndClear()
    {
        _mediaPlayer.Stop();
        _videoView.Visible = false;
        _pictureBox.Visible = false;
        
        if (_pictureBox.Image != null)
        {
            _pictureBox.Image.Dispose();
            _pictureBox.Image = null;
        }

        _placeholderLabel.Visible = true;
        _trackBar.Value = 0;
        UpdateTimeLabel(0, 0);
        _currentMovPath = null;
        _btnPlayPause.Text = "播放";
        _isPlaying = false;
        _statusLabel.Text = "已停止并清空预览";
    }

    private void UpdateTimeLabel(long currentMs, long totalMs)
    {
        if (currentMs < 0) currentMs = 0;
        if (totalMs < 0) totalMs = 0;
        TimeSpan current = TimeSpan.FromMilliseconds(currentMs);
        TimeSpan total = TimeSpan.FromMilliseconds(totalMs);
        _timeLabel.Text = $"{current:mm\\:ss\\.fff} / {total:mm\\:ss\\.fff}";
    }

    private void PictureBox_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        else e.Effect = DragDropEffects.None;
    }

    private void PictureBox_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data == null) return;
        var files = e.Data.GetData(DataFormats.FileDrop) as string[];
        if (files == null || files.Length == 0) return;

        string filePath = files[0];
        if (Path.GetExtension(filePath).Equals(".livp", StringComparison.OrdinalIgnoreCase))
            LoadLivp(filePath);
        else
            MessageBox.Show(this, "请拖入 .livp 格式的文件。", "文件格式不支持", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void LoadLivp(string livpPath)
    {
        _statusLabel.Text = $"正在处理：{Path.GetFileName(livpPath)}";
        _placeholderLabel.Visible = false;
        _btnPlayPause.Enabled = false;
        _btnStop.Enabled = false;
        
        _fileNameLabel.Text = $"当前文件：{Path.GetFileName(livpPath)}";

        Task.Run(() =>
        {
            try
            {
                // 注意：这里使用的是 LivpExtractor，而不是 EasyLivpExtractor
                var (heic, mov) = LivpExtractor.Extract(livpPath);
                
                Invoke(() =>
                {
                    _videoView.Visible = false;
                    _pictureBox.Visible = false;
                    
                    if (heic != null) ShowHeic(heic);
                    if (mov != null) 
                    {
                        _currentMovPath = mov;
                        ResetMediaAndPlay(mov);
                    }
                    
                    _btnPlayPause.Enabled = true;
                    _btnStop.Enabled = true;
                    _statusLabel.Text = $"已加载：{Path.GetFileName(livpPath)}";
                    
                    UpdateDirectoryList(livpPath);
                });
            }
            catch (Exception ex)
            {
                Invoke(() =>
                {
                    MessageBox.Show(this, $"处理失败：{ex.Message}", "错误");
                    _statusLabel.Text = "加载失败";
                    _placeholderLabel.Visible = true;
                });
            }
        });
    }

    private void UpdateDirectoryList(string livpPath)
    {
        string? dir = Path.GetDirectoryName(livpPath);
        if (string.IsNullOrEmpty(dir)) return;

        _fileList.BeginUpdate();
        _fileList.Items.Clear();

        string[] livpFiles = Directory.GetFiles(dir, "*.livp", SearchOption.TopDirectoryOnly);
        foreach (string file in livpFiles)
        {
            var item = new ListViewItem(Path.GetFileName(file));
            item.Tag = file;
            
            if (file.Equals(livpPath, StringComparison.OrdinalIgnoreCase))
            {
                item.BackColor = Color.LightBlue;
                item.Selected = true;
            }
            _fileList.Items.Add(item);
        }
        _fileList.EndUpdate();
    }

    private void FileList_DoubleClick(object? sender, EventArgs e)
    {
        if (_fileList.SelectedItems.Count > 0)
        {
            string? file = _fileList.SelectedItems[0].Tag as string;
            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                LoadLivp(file);
            }
        }
    }

    private void ShowHeic(string heicPath)
    {
        try
        {
            _pictureBox.Image?.Dispose();
            Bitmap bitmap = HeicHelper.DecodeToBitmap(heicPath);
            _pictureBox.Image = bitmap;
            _pictureBox.Visible = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"HEIC 解码失败：{ex.Message}", "错误");
        }
    }

    private void ResetMediaAndPlay(string movPath)
    {
        try
        {
            var newMedia = new Media(_libVlc, new Uri(movPath));
            _mediaPlayer.Media = newMedia;
            
            _currentMedia?.Dispose();
            _currentMedia = newMedia;

            if (!_mediaPlayer.Mute)
            {
                _mediaPlayer.Volume = _volumeTrackBar.Value;
            }

            _mediaPlayer.Play();
            
            _isPlaying = false; 
            _trackBar.Value = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"MOV 加载失败：{ex.Message}", "错误");
        }
    }

    private void OpenLivpDialog()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Live 图文件 (*.livp)|*.livp|所有文件 (*.*)|*.*",
            Title = "选择 .livp 文件"
        };
        if (dialog.ShowDialog(this) == DialogResult.OK) LoadLivp(dialog.FileName);
    }

    private void BatchExtractDialog()
    {
        using var dialog = new FolderBrowserDialog { Description = "选择包含 .livp 文件的目录" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        string[] livpFiles = Directory.GetFiles(dialog.SelectedPath, "*.livp", SearchOption.TopDirectoryOnly);
        if (livpFiles.Length == 0)
        {
            MessageBox.Show(this, "所选目录下没有 .livp 文件。", "提示");
            return;
        }

        Task.Run(() =>
        {
            int success = 0;
            var errors = new List<string>();
            foreach (string file in livpFiles)
            {
                try { LivpExtractor.Extract(file); success++; }
                catch (Exception ex) { errors.Add($"{Path.GetFileName(file)}: {ex.Message}"); }
            }
            Invoke(() =>
            {
                string msg = $"批量解压完成。\n成功：{success} 个\n失败：{errors.Count} 个";
                if (errors.Count > 0) msg += "\n\n失败详情：\n" + string.Join("\n", errors);
                MessageBox.Show(this, msg, "批量解压结果");
            });
        });
    }

    private string ExePath => Environment.ProcessPath ?? Application.ExecutablePath;

    private void RegisterContextMenu()
    {
        try { ContextMenuManager.AddContextMenu(ExePath); MessageBox.Show(this, "右键菜单已注册。", "完成"); }
        catch (Exception ex) { MessageBox.Show(this, $"注册失败：{ex.Message}", "错误"); }
    }

    private void RemoveContextMenu()
    {
        try { ContextMenuManager.RemoveContextMenu(); MessageBox.Show(this, "右键菜单已移除。", "完成"); }
        catch (Exception ex) { MessageBox.Show(this, $"移除失败：{ex.Message}", "错误"); }
    }

    private void SetAsDefaultApp()
    {
        try
        {
            DefaultAppManager.RegisterProgId(ExePath);
            var result = MessageBox.Show(this, "已写入注册表。\n是否现在打开系统设置以完成最后一步？", "半自动设置", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes) DefaultAppManager.OpenDefaultAppsSettings();
        }
        catch (Exception ex) { MessageBox.Show(this, $"设置失败：{ex.Message}", "错误"); }
    }

    private void ExtractAndInform(string livpPath)
    {
        try
        {
            var (heic, mov) = LivpExtractor.Extract(livpPath);
            Invoke(() =>
            {
                MessageBox.Show(this, $"解压完成：\nHEIC：{(heic ?? "未找到")}\nMOV：{(mov ?? "未找到")}", "完成");
                _statusLabel.Text = $"已解压：{Path.GetFileName(livpPath)}";
            });
        }
        catch (Exception ex) { Invoke(() => MessageBox.Show(this, $"解压失败：{ex.Message}", "错误")); }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _mediaPlayer?.Stop();
        _mediaPlayer?.Dispose();
        _currentMedia?.Dispose();
        _libVlc?.Dispose();
        _pictureBox?.Image?.Dispose();
        base.OnFormClosing(e);
    }
}