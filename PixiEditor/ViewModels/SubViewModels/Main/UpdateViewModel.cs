﻿using PixiEditor.Helpers;
using PixiEditor.Models.Processes;
using PixiEditor.UpdateModule;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class UpdateViewModel : SubViewModel<ViewModelMain>
    {

        private bool _updateReadyToInstall = false;

        public UpdateChecker UpdateChecker { get; set; }

        public RelayCommand RestartApplicationCommand { get; set; }

        private string _versionText;

        public string VersionText
        {
            get => _versionText;
            set
            {
                _versionText = value;
                RaisePropertyChanged(nameof(VersionText));
            }
        }


        public bool UpdateReadyToInstall
        {
            get => _updateReadyToInstall;
            set
            {
                _updateReadyToInstall = value;
                RaisePropertyChanged(nameof(UpdateReadyToInstall));
            }
        }

        public UpdateViewModel(ViewModelMain owner) : base(owner)
        {
            Owner.OnStartupEvent += Owner_OnStartupEvent;
            RestartApplicationCommand = new RelayCommand(RestartApplication);
            InitUpdateChecker();
        }

        private async void Owner_OnStartupEvent(object sender, EventArgs e)
        {
            await CheckForUpdate();
        }

        private void RestartApplication(object parameter)
        {
            try
            {
                ProcessHelper.RunAsAdmin(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "PixiEditor.UpdateInstaller.exe"));
                Application.Current.Shutdown();
            }
            catch (Win32Exception)
            {
                MessageBox.Show("Couldn't update without administrator rights.", "Insufficient permissions",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<bool> CheckForUpdate()
        {
            return await Task.Run(async () =>
            {
                bool updateAvailable = await UpdateChecker.CheckUpdateAvailable();
                bool updateFileDoesNotExists = !File.Exists(
                    Path.Join(UpdateDownloader.DownloadLocation, $"update-{UpdateChecker.LatestReleaseInfo.TagName}.zip"));
                if (updateAvailable && updateFileDoesNotExists)
                {
                    VersionText = "Downloading update...";
                    await UpdateDownloader.DownloadReleaseZip(UpdateChecker.LatestReleaseInfo);
                    VersionText = "to install update"; //Button shows "Restart" before this text
                    UpdateReadyToInstall = true;
                    return true;
                }
                return false;
            });
        }

        private void InitUpdateChecker()
        {
            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
            UpdateChecker = new UpdateChecker(info.FileVersion);
            VersionText = $"Version {info.FileVersion}";
        }
    }
}
