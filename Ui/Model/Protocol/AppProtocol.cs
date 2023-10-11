﻿using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using _1RM.Model.Protocol.Base;
using _1RM.Utils;
using Shawn.Utils;
using Shawn.Utils.Wpf;
using Shawn.Utils.Wpf.FileSystem;
using _1RM.View;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace _1RM.Model.Protocol
{
    // TODO 改为 ProtocolBaseWithAddressPortUserPwd 并且用 %PASSWORD% 替代默认密码
    public class LocalApp : ProtocolBaseWithAddressPortUserPwd
    {
        public LocalApp() : base("APP", "APP.V1", "APP")
        {
            base.Address = "";
            base.Port = "";
            base.UserName = "";
            base.Password = "";
            base.IsPingBeforeConnect = false;
        }

        public override bool IsOnlyOneInstance()
        {
            return false;
        }


        private string _appSubTitle = "";
        public string AppSubTitle
        {
            get => _appSubTitle;
            set => SetAndNotifyIfChanged(ref _appSubTitle, value);
        }

        private string _exePath = "";
        public string ExePath
        {
            get => _exePath;
            set => SetAndNotifyIfChanged(ref _exePath, value);
        }

        private string _appProtocolDisplayName = "";
        public string AppProtocolDisplayName
        {
            get => _appProtocolDisplayName;
            set => SetAndNotifyIfChanged(ref _appProtocolDisplayName, value);
        }

        public override string GetProtocolDisplayName()
        {
            if (string.IsNullOrEmpty(_appProtocolDisplayName))
                return base.GetProtocolDisplayName();
            return _appProtocolDisplayName;
        }

        [Obsolete]
        private string _arguments = "";
        [Obsolete]
        public string Arguments
        {
            get => _arguments;
            set => SetAndNotifyIfChanged(ref _arguments, value);
        }

        private ObservableCollection<AppArgument> _argumentList = new ObservableCollection<AppArgument>();
        public ObservableCollection<AppArgument> ArgumentList
        {
            get => _argumentList;
            set => SetAndNotifyIfChanged(ref _argumentList, value);
        }


        private bool _runWithHosting = false;
        public bool RunWithHosting
        {
            get => _runWithHosting;
            set => SetAndNotifyIfChanged(ref _runWithHosting, value);
        }


        public override ProtocolBase? CreateFromJsonString(string jsonString)
        {
            try
            {
                var app = JsonConvert.DeserializeObject<LocalApp>(jsonString);
                return app;
            }
            catch (Exception e)
            {
                SimpleLogHelper.Debug(e);
                return null;
            }
        }


        protected override string GetSubTitle()
        {
            return string.IsNullOrEmpty(AppSubTitle) ? $"{this.ExePath}" : AppSubTitle;
        }

        public override double GetListOrder()
        {
            return 100;
        }

        public string GetArguments()
        {
            return AppArgument.GetArgumentsString(ArgumentList, false);
        }


        public string GetDemoArguments()
        {
            return AppArgument.GetArgumentsString(ArgumentList, true);
        }


        private RelayCommand? _cmdSelectExePath;
        [JsonIgnore]
        public RelayCommand CmdSelectExePath
        {
            get
            {
                return _cmdSelectExePath ??= new RelayCommand((o) =>
                {
                    string initPath;
                    try
                    {
                        initPath = new FileInfo(ExePath).DirectoryName!;
                    }
                    catch (Exception)
                    {
                        initPath = Environment.CurrentDirectory;
                    }

                    var path = SelectFileHelper.OpenFile(filter: "Exe|*.exe", initialDirectory: initPath, currentDirectoryForShowingRelativePath: Environment.CurrentDirectory);
                    if (path == null) return;
                    ExePath = path;
                });
            }
        }

        private RelayCommand? _cmdSelectArgumentFile;
        [JsonIgnore]
        public RelayCommand CmdSelectArgumentFile
        {
            get
            {
                return _cmdSelectArgumentFile ??= new RelayCommand((o) =>
                {
                    string initPath;
                    try
                    {
                        initPath = new FileInfo(o?.ToString() ?? "").DirectoryName!;
                    }
                    catch (Exception)
                    {
                        initPath = Environment.CurrentDirectory;
                    }
                    var path = SelectFileHelper.OpenFile(initialDirectory: initPath, currentDirectoryForShowingRelativePath: Environment.CurrentDirectory);
                    if (path == null) return;
                });
            }
        }

        private RelayCommand? _cmdPreview;
        [JsonIgnore]
        public RelayCommand CmdPreview
        {
            get
            {
                return _cmdPreview ??= new RelayCommand((o) =>
                {
                    MessageBoxHelper.Info(ExePath + " " + GetDemoArguments(), ownerViewModel: IoC.Get<MainWindowViewModel>());
                });
            }
        }

        private RelayCommand? _cmdTest;
        [JsonIgnore]
        public RelayCommand CmdTest
        {
            get
            {
                return _cmdTest ??= new RelayCommand((o) =>
                {
                    try
                    {
                        Process.Start(ExePath, GetArguments());
                    }
                    catch (Exception e)
                    {
                        MessageBoxHelper.ErrorAlert(e.Message);
                    }
                });
            }
        }

        public override bool Verify()
        {
            return ArgumentList.All(argument => string.IsNullOrEmpty(argument[nameof(AppArgument.Value)]));
        }

        public override ProtocolBase Clone()
        {
            var clone = base.Clone() as LocalApp;
            clone!.ArgumentList = new ObservableCollection<AppArgument>(ArgumentList.Select(x => x.Clone() as AppArgument)!);
            return clone;
        }
    }
}