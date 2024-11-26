using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using EasyUploaderClient;
using System.Threading;
using System.Threading.Tasks;

namespace EasyUploader_Editor
{
    public class EasyUploader : EditorWindow
    {
        public LogBox ServerLog;

        #region player prefs keys
        readonly static string _pp_connectUrl = "DNUploader_connect_address";
        readonly static string _pp_playerName = "DNUploader_playerName";
        readonly static string _pp_playerDestination = "DNUploader_playerDestination";
        readonly static string _pp_target = "DNUploader_target";
        readonly static string _pp_launchCommands = "DNUploader_launchCommands";
        readonly static string _pp_developmentBuild = "DNUploader_developmentBuild";
        #endregion

        ConnectionPanel _connectionPanel;
        ServerBuildPanel _serverBuildPanel;
        ServerBuildLauncher _serverBuildLauncher;

        public ScrollView RootVerticalScrollElement;

        #region user preferences
        string userPath;
        #endregion

        public static int MaxLinesInLogs = 90;

        public Button ConnectButton;

        public static int unityThread;
        static public Queue<Action> runInUpdate = new Queue<Action>();

        EasyUploaderAPI _easyUploaderAPI;
        public string BuildName;

        public void Awake()
        {
            unityThread = Thread.CurrentThread.ManagedThreadId;
        }

        private void Update()
        {
            while (runInUpdate.Count > 0)
            {
                Action action = null;
                lock (runInUpdate)
                {
                    if (runInUpdate.Count > 0)
                        action = runInUpdate.Dequeue();
                }
                action?.Invoke();
            }
        }

        public static void RunOnUnityThread(Action action)
        {
            if (unityThread == Thread.CurrentThread.ManagedThreadId)
            {
                action();
            }
            else
            {
                lock (runInUpdate)
                {
                    runInUpdate.Enqueue(action);
                }
            }
        }

        #region Main window managament

        [MenuItem("DNTools/EasyUploader")]
        public static void ShowExample()
        {
            EasyUploader wnd = GetWindow<EasyUploader>();
            wnd.titleContent = new GUIContent("EasyUploader");

            wnd.minSize = new Vector2(450, 600);
            wnd.maxSize = new Vector2(600, 1440);

            UnityEngine.Debug.Log("Opened EasyUploader");
        }

        public void CreateGUI()
        {
            _easyUploaderAPI = new EasyUploaderAPI();
            _easyUploaderAPI.Init();

            _easyUploaderAPI.Callback_Log += (string log) => RunOnUnityThread(() => UnityEngine.Debug.Log(log));

            _easyUploaderAPI.Callback_OnReceivedPlayerStatus += (UploadStatus status, Target target) => {
                OnReceivedPlayerStatus(status);
                _serverBuildPanel.SetTarget(target);
            };

            _easyUploaderAPI.Callback_OnPlayerUploadTotalProgressUpdated += DNU_ProgressBar;
            _easyUploaderAPI.Callback_PlayerUploadedConfirmation += (int code) => {
                DNU_ProgressBar("", -1f);
                UnityEngine.Debug.Log($"uploaded {code}");
                OnReceivedPlayerStatus(UploadStatus.Uploaded);
            };

            _easyUploaderAPI.Callback_OnReceivedPlayerOutput += (string output) => { RunOnUnityThread(() => { ServerLog.AddLogLine(output); }); };
            _easyUploaderAPI.Callback_PlayerStoppedConfirmation += (int code) => {

                _serverBuildLauncher.DNU_OnReceivedPlayerState(UploadStatus.Uploaded);
                _serverBuildPanel.SetBuildAndUploadButton(true);
            };

            RootVerticalScrollElement = new ScrollView(ScrollViewMode.Vertical);
            //initialize all panels when window opens, then we will show and hide them as needed
            _connectionPanel = new ConnectionPanel(this);
            _serverBuildPanel = new ServerBuildPanel(this);
            _serverBuildLauncher = new ServerBuildLauncher(this);

            ServerLog = new LogBox("Server log");

            ResetWindow();
            RootVerticalScrollElement.Add(_connectionPanel.panel);
            RootVerticalScrollElement.Add(_serverBuildPanel.panel);
            RootVerticalScrollElement.Add(_serverBuildLauncher.panel);
            RootVerticalScrollElement.Add(ServerLog.panel);

            PositionLogs();

            //draw every tile disable exept for url panel at first
            _serverBuildPanel.SetTarget(Target.Unknown);
            OnReceivedPlayerStatus(UploadStatus.NotConnected);
        }

        private void OnReceivedPlayerStatus(UploadStatus status)
        {
            RunOnUnityThread(() =>
            {
                _serverBuildLauncher.DNU_OnReceivedPlayerState(status);
                _serverBuildPanel.SetBuildAndUploadButton(true);
                if (status == UploadStatus.NotConnected)
                {
                    _serverBuildLauncher.panel.SetEnabled(false);
                    _serverBuildPanel.panel.SetEnabled(false);
                    _connectionPanel.SetConnectButton(false);
                    return;
                }

                if (status == UploadStatus.Uploaded)
                {

                }

                _connectionPanel.SetConnectButton(false);
                _serverBuildLauncher.panel.SetEnabled(true);
                _serverBuildPanel.panel.SetEnabled(true);
            });
        }

        public void ShowConnectionMenu()
        {
            ResetWindow();
            DNU_ProgressBar("DN", -1f); //hide progress bar in case it was shown during panel reset

            wantsMouseMove = true;
            wantsLessLayoutEvents = false;

            //insert connection panel
            RootVerticalScrollElement.Insert(0, _connectionPanel.panel);
            RootVerticalScrollElement.Insert(2, ServerLog.panel);
            _connectionPanel.SetConnectButton(false);
        }

        public void ResetWindow()
        {
            rootVisualElement.Clear();
            RootVerticalScrollElement.Clear();

            var spriteImage = new Image();
            //spriteImage.scaleMode = ScaleMode.ScaleToFit;
            //spriteImage.scaleMode = ScaleMode.StretchToFill;
            spriteImage.style.alignItems = Align.Stretch;
            spriteImage.style.display = DisplayStyle.Flex;
            spriteImage.style.justifyContent = Justify.FlexStart;
            spriteImage.style.overflow = Overflow.Visible;
            spriteImage.style.position = Position.Relative;
            spriteImage.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;
            spriteImage.style.backgroundColor = Color.black;
            spriteImage.sprite = Resources.Load<Sprite>("sprite_easyUploader_cover");


            Button cover = new Button(() => Process.Start("http://zbigniew.dev/projects/easyuploader"));

            cover.tooltip = "Developer website";

            cover.style.backgroundColor = Color.black; //give more or less matching background color for nonstandard stretching
            cover.style.position = Position.Relative;
            cover.style.justifyContent = Justify.SpaceAround;
            cover.style.alignItems = Align.FlexStart;
            cover.style.marginBottom = 0;
            cover.style.marginTop = 0;
            cover.style.marginLeft = 0;
            cover.style.marginRight = 0;
            cover.style.paddingBottom = 0;
            cover.style.paddingTop = 0;
            cover.style.paddingLeft = 0;
            cover.style.paddingRight = 0;

            cover.Add(spriteImage);

            rootVisualElement.Insert(0, cover);
            rootVisualElement.Insert(1, RootVerticalScrollElement);
        }

        public void PositionLogs()
        {
            ServerLog.panel.BringToFront();
        }
        #endregion
        #region Unity progress bar

        void DNU_ProgressBar(string info, float progress)
        {
            RunOnUnityThread(Action);

            void Action()
            {
                if (progress < 0)
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                EditorUtility.DisplayProgressBar("Easy Uploader", info, progress);
            }
        }
        #endregion
        #region Panels
        public class LogBox
        {
            public Box panel;

            Label _logLabel;

            public void Clear()
            {
                _logLabel.text = string.Empty;
            }
            public void AddLogLine(string line)
            {
                string logLine = string.IsNullOrEmpty(_logLabel.text) ? string.Empty : "\n";
                logLine += $"{DateTime.Now} - {line}";

                string content = _logLabel.text += logLine;
                int extraLines = GetLineCount(content) - MaxLinesInLogs;
                if (extraLines > 0)
                {
                    content = DeleteLines(content, extraLines);
                }
                _logLabel.text = content;
            }
            public LogBox(string logName)
            {
                Box box = DNUBox();

                Foldout foldout = new Foldout();
                foldout.text = logName;

                _logLabel = new Label(string.Empty);
                _logLabel.enableRichText = true;

                ScrollView scroll = new ScrollView(ScrollViewMode.Vertical);

                foldout.Add(scroll);

                scroll.Add(_logLabel);

                panel = box;
                panel.Add(foldout);
            }

            public static string DeleteLines(string s, int linesToRemove)
            {
                return s.Split(Environment.NewLine.ToCharArray(),
                               linesToRemove + 1
                    ).Skip(linesToRemove)
                    .FirstOrDefault();
            }
            public static int GetLineCount(string input)
            {
                int lineCount = 0;

                for (int i = 0; i < input.Length; i++)
                {
                    switch (input[i])
                    {
                        case '\r':
                            {
                                if (i + 1 < input.Length)
                                {
                                    i++;
                                    if (input[i] == '\r')
                                    {
                                        lineCount += 2;
                                    }
                                    else
                                    {
                                        lineCount++;
                                    }
                                }
                                else
                                {

                                    lineCount++;
                                }
                            }
                            break;
                        case '\n':
                            lineCount++;
                            break;
                        default:
                            break;
                    }
                }
                return lineCount;
            }
        }

        public class ConnectionPanel
        {
            EasyUploader _root;
            public Box panel;
            TextField address;
            bool _connecting = true;

            public string Address() { return address.value; }

            public ConnectionPanel(EasyUploader root)
            {
                _root = root;
                panel = Panel();
            }

            Box Panel()
            {
                Box connectionBox = new Box();

                connectionBox.style.position = Position.Relative;
                connectionBox.style.marginRight = 30;
                connectionBox.style.marginLeft = 30;
                connectionBox.style.top = 30;
                connectionBox.style.marginBottom = 30;

                Label connectLabel = new Label("Connect");

                address = new TextField("Address: ", 40, false, false, '*');

                string pp_address = PlayerPrefs.GetString(_pp_connectUrl);
                address.value = !string.IsNullOrEmpty(pp_address) ? pp_address : "localhost";

                _root._easyUploaderAPI.SetUrl(address.value);

                address.RegisterValueChangedCallback(OnAddressChanged);

                //setup connect button
                _root.ConnectButton = new Button(Connect);
                SetConnectButton(false);


                connectionBox.Insert(0, address);
                connectionBox.Insert(1, _root.ConnectButton);

                return connectionBox;
            }

            public void Connect()
            {
                if (_connecting) return;

                SetConnectButton(true);
                _root._easyUploaderAPI.GetServerStatus();
            }

            public void SetConnectButton(bool connecting)
            {
                if (_connecting == connecting) return; //dont set same state twice
                _connecting = connecting;

                _root.ConnectButton.style.position = Position.Relative;

                _root.ConnectButton.style.backgroundColor = connecting ? Color.gray : Color.yellow;
                _root.ConnectButton.style.color = connecting ? Color.white : Color.black;
                _root.ConnectButton.SetEnabled(!connecting);

                _root.ConnectButton.text = connecting ? "Connecting..." : "Connect";

                _root.ConnectButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            void OnAddressChanged(ChangeEvent<string> evt)
            {
                PlayerPrefs.SetString(_pp_connectUrl, address.value);
                _root._easyUploaderAPI.SetUrl(address.value);
            }
        }

        public class ServerBuildPanel
        {
            EasyUploader _root;
            public Box panel;
            TextField _buildName;
            DropdownField _targetDropDown;
            Toggle _developmentBuild;
            Button _buildAndUploadButton;

            List<string> _targetOptions = new List<string>();
            Target _selectedtarget;

            public ServerBuildPanel(EasyUploader root)
            {
                _root = root;
                panel = Panel();

                _selectedtarget = (Target)PlayerPrefs.GetInt(_pp_target);
                _targetDropDown.value = _targetOptions[(int)_selectedtarget];
            }

            private void OnTargetSelected(ChangeEvent<string> evt)
            {
                for (int i = 0; i < _targetOptions.Count; i++)
                {
                    if (_targetOptions[i] == _targetDropDown.value)
                    {
                        _selectedtarget = (Target)i;
                        PlayerPrefs.SetInt(_pp_target, i);
                        return;
                    }
                }
            }

            public void SetTarget(Target target)
            {
                if (target >= 0)
                    _targetDropDown.SetValueWithoutNotify(_targetOptions[(int)target]);
                else
                    _targetDropDown.SetValueWithoutNotify("unknown");

                _targetDropDown.SetEnabled(false);
                _selectedtarget = target;
            }

            private void OnPlayerBuildNameChanged(ChangeEvent<string> evt)
            {
                PlayerPrefs.SetString(_pp_playerName, _buildName.value);
            }
            void OpenBuildFolder()
            {
                _root.userPath = PlayerPrefs.GetString(_pp_playerDestination);

                if (string.IsNullOrEmpty(_root.userPath))
                {
                    UnityEngine.Debug.Log("There is no build destination selected yet");
                    return;
                }

                Process.Start(_root.userPath);
            }

            void ChangeBuildFolder()
            {
                _root.userPath = _root.GetBuildPlayerLocation();
                PlayerPrefs.SetString(_pp_playerDestination, _root.userPath);
            }

            void BuildPlayer()
            {
                if (string.IsNullOrEmpty(_buildName.value)) _buildName.value = "game";

                _root.BuildName = _buildName.value;

                _root._serverBuildLauncher.DNU_OnReceivedPlayerState(UploadStatus.Uploading); //change ui state

                //replace illegal characters if they were used in build name
                string illegalCharacters = $"\"/:*?<>|" + (char)92;

                for (int i = 0; i < illegalCharacters.Length; i++)
                {
                    if (!_buildName.value.Contains(illegalCharacters[i])) continue;

                    _buildName.value = _buildName.value.Replace(illegalCharacters[i], 'X');
                }

                SetBuildAndUploadButton(false, "Uploading...");

                string path;
                _root.userPath = PlayerPrefs.GetString(_pp_playerDestination);

                if (string.IsNullOrEmpty(_root.userPath))
                {
                    path = _root.GetBuildPlayerLocation();
                    PlayerPrefs.SetString(_pp_playerDestination, path);
                }
                else
                    path = _root.userPath;

                string[] levels;

                List<string> includedLevels = new List<string>();

                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    if (EditorBuildSettings.scenes[i].enabled)
                        includedLevels.Add(EditorBuildSettings.scenes[i].path);
                }

                levels = includedLevels.ToArray();

                string buildPath = path;

                string executableName = _buildName.value;

                if (_selectedtarget == Target.Linux)
                    executableName += ".x86_64";
                else
                    executableName += ".exe";

                buildPath += $"/{executableName}";

                BuildPlayerOptions buildOptions = new BuildPlayerOptions
                {
                    locationPathName = buildPath,
                    target = _selectedtarget == Target.Linux ? BuildTarget.StandaloneLinux64 : BuildTarget.StandaloneWindows,
                    subtarget = (int)StandaloneBuildSubtarget.Server,
                    scenes = levels,
                };

                if (_developmentBuild.value)
                    buildOptions.options = BuildOptions.Development;

                BuildPipeline.BuildPlayer(buildOptions);

                _root._easyUploaderAPI.SendFile(path, _buildName.value, executableName);
            }

            public void SetBuildAndUploadButton(bool enabled, string label = "")
            {
                _buildAndUploadButton.text = enabled ? "Build and upload" : label;
                _buildAndUploadButton.SetEnabled(enabled);
            }

            void OnDevelopmentBuildChecked(ChangeEvent<bool> evt)
            {
                PlayerPrefs.SetInt(_pp_developmentBuild, System.Convert.ToInt32(_developmentBuild.value));
            }

            public Box Panel()
            {
                //set build name input field
                _buildName = new TextField("Server build name");
                _buildName.RegisterValueChangedCallback(OnPlayerBuildNameChanged);
                string savedPlayerName = PlayerPrefs.GetString(_pp_playerName);
                _buildName.value = string.IsNullOrEmpty(savedPlayerName) ? "game" : savedPlayerName;

                //set target dropdown
                _targetOptions = Enum.GetNames(typeof(Target)).ToList();
                _targetDropDown = new DropdownField("Target", _targetOptions, 0);
                _targetDropDown.RegisterValueChangedCallback(OnTargetSelected);
                _targetDropDown.SetEnabled(false);

                //set build button
                _buildAndUploadButton = new Button(BuildPlayer);
                _buildAndUploadButton.style.height = 30;
                _buildAndUploadButton.style.marginBottom = 15;
                _buildAndUploadButton.style.marginTop = 5;
                SetBuildAndUploadButton(true);

                //set open folder button
                Button openBuildFolderBtn = new Button(OpenBuildFolder);
                openBuildFolderBtn.text = "Open build player location";
                //openBuildFolderBtn.style.marginBottom = 5;

                //sett new build location button
                Button newBuildFolderLocation = new Button(ChangeBuildFolder);
                newBuildFolderLocation.text = "Change build player location";

                Box serverPanel = DNUBox("Build Settings");

                _developmentBuild = new Toggle("DevelopmentBuild");
                _developmentBuild.value = PlayerPrefs.GetInt(_pp_developmentBuild) > 0 ? true : false;
                _developmentBuild.RegisterValueChangedCallback(OnDevelopmentBuildChecked);


                Button _showBuildPlayerWindow = new Button(BuildPlayerWindow.ShowBuildPlayerWindow);
                _showBuildPlayerWindow.text = "Show build player window";
                //_showBuildPlayerWindow.style.marginTop = 5;

                serverPanel.Insert(1, _buildName);
                serverPanel.Insert(2, _developmentBuild);
                serverPanel.Insert(3, _targetDropDown);
                serverPanel.Insert(4, _buildAndUploadButton);
                serverPanel.Insert(5, openBuildFolderBtn);
                serverPanel.Insert(6, newBuildFolderLocation);
                serverPanel.Insert(7, newBuildFolderLocation);
                serverPanel.Add(_showBuildPlayerWindow);

                return serverPanel;
            }
        }

        public class ServerBuildLauncher
        {
            EasyUploader _root;

            public Box panel;

            TextField _launchCommands;
            Button _runBuild;
            Button _stopPlayer;
            Label _buildNameLabel;

            public ServerBuildLauncher(EasyUploader root)
            {
                _root = root;
                panel = BuildPresentOnServerPanel();

                WriteServerBuildName(UploadStatus.NotConnected);
            }

            Box BuildPresentOnServerPanel()
            {
                Box box = DNUBox("Server build launcher");

                _buildNameLabel = new Label("label");
                _buildNameLabel.style.fontSize = 15;

                _launchCommands = new TextField("Launch commands: ", 500, false, false, '*');
                _launchCommands.RegisterValueChangedCallback(OnLaunchCommandsChanged);
                _launchCommands.value = "";

                _runBuild = new Button(RunServerPlayer);

                _runBuild.text = "Run server player";
                _runBuild.SetEnabled(false);

                _stopPlayer = new Button(StopServerPlayer);

                box.Insert(1, _buildNameLabel);
                box.Insert(2, _launchCommands);
                box.Insert(3, _runBuild);

                _launchCommands.value = PlayerPrefs.GetString(_pp_launchCommands);

                return box;
            }

            void RunServerPlayer()
            {
                _runBuild.SetEnabled(false);
                _runBuild.text = "Stop server player";

                DNU_OnReceivedPlayerState(UploadStatus.Running);
                _root._easyUploaderAPI.SetServerPlayerLaunchCommands(_launchCommands.value);
                Task.Factory.StartNew(_root._easyUploaderAPI.RunServerPlayer);
            }
            void StopServerPlayer()
            {
                _stopPlayer.SetEnabled(false);
                _stopPlayer.text = "Stopping server player...";
                _root._easyUploaderAPI.StopServerPlayer();
            }


            public void DNU_OnReceivedPlayerState(UploadStatus playerState)
            {
                //disable build button if server player is already running on server
                _root._serverBuildPanel.SetBuildAndUploadButton(playerState != UploadStatus.Uploaded && playerState != UploadStatus.Running, playerState == UploadStatus.Running ? "Server player running" : playerState == UploadStatus.Uploading ? "Uploading..." : null);

                WriteServerBuildName(playerState);

                if (playerState == UploadStatus.NotUploaded)
                {
                    panel.RemoveAt(3);
                    panel.Insert(3, _runBuild);

                    _runBuild.text = "Run server player";

                    _runBuild.SetEnabled(false);
                }
                else if (playerState == UploadStatus.Uploaded) //player uploaded but not running, setup button for launching process on server
                {
                    panel.RemoveAt(3);
                    panel.Insert(3, _runBuild);

                    _runBuild.text = "Run server player";
                    _runBuild.style.backgroundColor = Color.green;
                    _runBuild.style.color = Color.black;

                    _runBuild.SetEnabled(true);
                }
                else if (playerState == UploadStatus.Running) //player uploaded and running, setup button for killing process on server
                {
                    panel.RemoveAt(3);
                    panel.Insert(3, _stopPlayer);

                    _stopPlayer.text = "Stop server player";
                    _stopPlayer.style.backgroundColor = Color.red;
                    _stopPlayer.style.color = Color.white;

                    _stopPlayer.SetEnabled(true);
                }
                else if (playerState == UploadStatus.Uploading) //server build is being uploaded
                {
                    _runBuild.SetEnabled(false);
                    _launchCommands.SetEnabled(false);

                    _buildNameLabel.style.color = Color.cyan;
                    _buildNameLabel.text = "Server build is being uploaded...";
                }
            }

            public void WriteServerBuildName(UploadStatus uploadStatus)
            {
                if (uploadStatus == UploadStatus.Uploading || uploadStatus == UploadStatus.NotUploaded || uploadStatus == UploadStatus.NotConnected)
                {
                    _launchCommands.SetEnabled(false);
                    _buildNameLabel.style.color = Color.yellow;
                    _buildNameLabel.text = "Server build is not uploaded yet";
                }
                else
                {
                    _launchCommands.SetEnabled(true);
                    _buildNameLabel.style.color = Color.green;
                    _buildNameLabel.text = $"Uploaded build: {_root.BuildName}";
                }
            }

            private void OnLaunchCommandsChanged(ChangeEvent<string> evt)
            {
                PlayerPrefs.SetString(_pp_launchCommands, _launchCommands.value);
            }
        }

        #endregion
        #region Styles
        public static Box DNUBox(string labelName = "")
        {
            Box box = new Box();

            box.style.position = Position.Relative;
            box.style.marginRight = 30;
            box.style.marginLeft = 30;
            box.style.marginTop = 15;
            box.style.marginBottom = 15;

            if (string.IsNullOrEmpty(labelName))
                return box;

            Label label = new Label(labelName);
            label.style.fontSize = 15;
            box.Add(label);

            return box;
        }
        #endregion

        string GetBuildPlayerLocation()
        {
            return EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
        }
    }
}