using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Classes.Disc;
using Archiver.Shared.Classes;
using Archiver.Shared.Models;
using Archiver.Shared.Utilities;
using Archiver.Utilities.Shared;
using TerminalUI;
using TerminalUI.Elements;

namespace Archiver.Utilities.Disc
{
    public class DiscVerifier
    {
        private List<DiscDetail> _pendingDiscs => _discsToVerify.Where(x => x.DaysSinceLastVerify > 7)
                                                                .ToList();

        private List<DiscDetail> _completedDiscs => _discsToVerify.Where(x => x.DaysSinceLastVerify <= 7)
                                                                  .ToList();

        private int _discCount = 0;
        private List<DiscDetail> _discsToVerify;
        private List<int> _pendingDiscIds;
        private List<int> _completedDiscIds;

        private string _drive;
        private volatile bool _cancel = false;

        #region GUI Objects
        private KeyValueText kvtStatus;
        private KeyValueText kvtDiscs;
        private KeyValueText kvtElapsedTime;
        private KeyValueText kvtDiscName;
        private KeyValueText kvtVerified;
        private KeyValueText kvtCurrentRate;
        private KeyValueText kvtAvgRate;
        private ProgressBar progressBar;
        private Text pendingLabel;
        private DataTable dataTable;
        private NotificationBox box;
        #endregion

        public DiscVerifier(string DriveLetter, List<DiscDetail> discsToVerify)
        {
            _discsToVerify = discsToVerify;
            _drive = DriveLetter;
            _discCount = _discsToVerify.Count();

            _pendingDiscIds = _pendingDiscs.Select(x => x.DiscNumber).ToList();
            _completedDiscIds = _completedDiscs.Select(x => x.DiscNumber).ToList();

            TerminalPoint prevPoint = TerminalPoint.GetCurrent();
            Terminal.RootPoint.MoveTo();

            Terminal.Clear();

            kvtStatus = new KeyValueText("Status", null, -16);
            Terminal.NextLine();
            kvtDiscs = new KeyValueText("Discs Verified", $"{this._completedDiscIds.Count}/{this._discCount}", -16);
            Terminal.NextLine();
            Terminal.NextLine();
            kvtElapsedTime = new KeyValueText("Elapsed Time", null, -16);
            Terminal.NextLine();
            kvtDiscName = new KeyValueText("Disc Name", null, -16);
            Terminal.NextLine();

            box = new NotificationBox(5, 0);
            box.SetTextJustify(0, TextJustify.Center);
            box.SetTextJustify(1, TextJustify.Center);

            kvtVerified = new KeyValueText("Verified", Formatting.GetFriendlySize(0), -16);
            Terminal.NextLine();

            kvtCurrentRate = new KeyValueText("Current Rate", Formatting.GetFriendlyTransferRate(0), -16);
            Terminal.NextLine();

            kvtAvgRate = new KeyValueText("Average Rate", Formatting.GetFriendlyTransferRate(0), -16);
            Terminal.NextLine();
            Terminal.NextLine();

            progressBar = new ProgressBar();
            Terminal.NextLine();
            Terminal.NextLine();

            pendingLabel = new Text(ConsoleColor.Magenta, "Discs Pending Verification...");
            Terminal.NextLine();

            dataTable = new DataTable();
            dataTable.ShowHeader = false;
            dataTable.Columns = new List<DataTableColumn>()
            {
                new DataTableColumn("DiscName", "Disc Name", 12),
                new DataTableColumn(null, "Status", 20)
                {
                    Format = (value) => 
                    {
                        DiscDetail detail = (DiscDetail)value;
                        int lastVerifyDays = detail.DaysSinceLastVerify;

                        if (lastVerifyDays > 7)
                            return "Pending";
                        else
                        {
                            if (detail.LastVerifySuccess == true)
                                return "Verification Passed";
                            else
                                return "Verification FAILED";
                        }
                        
                    }
                },
                new DataTableColumn("DaysSinceLastVerify", null, 23)
                {
                    Format = (value) => $"Days Since Verify: {value.ToString().PadLeft(4)}"
                },
                new DataTableColumn("DataSize", "Data Size", 25)
                {
                    Format = (value) => "Data Size: " + Formatting.GetFriendlySize((long)value)
                }
            };

            dataTable.DataStore = this._pendingDiscs;

            SetStatus("Starting...");
        }

        private async Task ShowVerifySuccessAsync(DiscDetail disc)
        {
            box.BorderColor = ConsoleColor.Green;
            box.SetLineColor(0, ConsoleColor.Green);
            box.UpdateText(0, "Verification Successful");
            box.UpdateText(1, $"Disc `{disc.DiscName}` was successfully verified!");

            box.Show();

            await Task.Delay(5000);

            box.Hide();
        }

        private async Task ShowVerifyFailedAsync(DiscDetail disc)
        {
            box.BorderColor = ConsoleColor.Red;
            box.SetLineColor(0, ConsoleColor.Red);
            box.UpdateText(0, "Verification FAILED");
            box.UpdateText(1, $"Disc `{disc.DiscName}` failed verification!");

            box.Show();

            await Task.Delay(5000);

            box.Hide();
        }

        public async Task StartVerificationAsync()
        {
            kvtStatus.Show();
            kvtDiscs.Show();
            pendingLabel.Show();
            dataTable.Show();

            CancellationTokenSource cts = new CancellationTokenSource();

            Terminal.InitStatusBar(
                new StatusBarItem(
                    "Cancel",
                    (key) => {
                        cts.Cancel();
                        return Task.Delay(0);
                    },
                    Key.MakeKey(ConsoleKey.C, ConsoleModifiers.Control)
                )
            );

            HideProgress();

            while (_pendingDiscs.Count() > 0 && !cts.Token.IsCancellationRequested)
            {
                DiscDetail disc = await WaitForDiscAsync(cts.Token);

                if (disc == null)
                    break;

                kvtDiscName.UpdateValue(OpticalDriveUtils.GetDriveLabel(_drive));
                kvtDiscName.Show();
                kvtElapsedTime.Show();
                kvtVerified.Show();
                kvtCurrentRate.Show();
                kvtAvgRate.Show();
                progressBar.Show();

                Stopwatch sw = Stopwatch.StartNew();

                using (Stream reader = OpticalDriveUtils.GetDriveRawStream(_drive))
                {
                    Md5StreamGenerator generator = new Md5StreamGenerator(reader);
                    generator.OnProgressChanged += (progress) =>
                    {
                        kvtVerified.UpdateValue($"{Formatting.GetFriendlySize(progress.TotalCopiedBytes)} / {Formatting.GetFriendlySize(progress.TotalBytes)}");
                        kvtAvgRate.UpdateValue(Formatting.GetFriendlyTransferRate(progress.AverageTransferRate));
                        kvtCurrentRate.UpdateValue(Formatting.GetFriendlyTransferRate(progress.InstantTransferRate));

                        progressBar.UpdateProgress(progress.PercentCopied / 100.0);
                        kvtElapsedTime.UpdateValue(sw.Elapsed.ToString());
                    };

                    generator.OnComplete += async (hash) =>
                    {
                        sw.Stop();

                        bool discValid = (disc.Hash.ToLower() == hash.ToLower());

                        disc.RecordVerification(DateTime.UtcNow, discValid);
                        Helpers.SaveDestinationDisc(disc);

                        _pendingDiscIds.Remove(disc.DiscNumber);
                        _completedDiscIds.Add(disc.DiscNumber);

                        this.HideProgress();

                        if (discValid)
                            await ShowVerifySuccessAsync(disc);
                        else
                            await ShowVerifyFailedAsync(disc);
                    };

                    await generator.GenerateAsync(cts.Token);
                }

                SetStatus($"Ejecting disc `{disc.DiscName}`...");
                // await Task.Delay(2000);
                OpticalDriveUtils.EjectDrive(_drive);

                dataTable.DataStore = this._pendingDiscs;
                dataTable.Redraw();
            }

            if (cts.Token.IsCancellationRequested)
                SetStatus("Process canceled");
            else
                SetStatus("All Discs have been verified");
                
            await Task.Delay(0);
        }

        private void SetStatus(string status)
            => kvtStatus.UpdateValue(status);

        private void HideProgress()
        {
            kvtElapsedTime.Hide();
            kvtDiscName.Hide();
            kvtVerified.Hide();
            kvtCurrentRate.Hide();
            kvtAvgRate.Hide();
            progressBar.Hide();
        }

        private async Task<DiscDetail> WaitForDiscAsync(CancellationToken cToken)
        {
            DiscDetail insertedDisc = default(DiscDetail);

            OpticalDrive driveInfo = OpticalDriveUtils.GetDrives().FirstOrDefault(x => x.Name == _drive);

            // DriveInfo driveInfo = DriveInfo.GetDrives().Where(x => x.Name.TrimEnd('\\').ToUpper() == _drive.ToUpper()).FirstOrDefault();

            if (driveInfo != null)
            {
                string append = "";

                if (_pendingDiscIds.Count() == 1)
                    append = " " + _pendingDiscIds[0].ToString();

                while (!cToken.IsCancellationRequested)
                {
                    driveInfo = OpticalDriveUtils.GetDrives().FirstOrDefault(x => x.Name == _drive);

                    if (driveInfo.IsReady == false)
                        SetStatus($"Please insert archive disc{append}...");

                    else if (driveInfo.VolumeLabel != null)
                    {
                        if (driveInfo.VolumeLabel.ToLower().StartsWith("archive ") == false)
                            SetStatus("Unknown disc inserted...");

                        else
                        {
                            string discIdStr = driveInfo.VolumeLabel.Substring(8);
                            int discId = Int32.Parse(discIdStr);

                            if (_pendingDiscIds.Contains(discId))
                            {
                                SetStatus($"Verifying disc {discId}");
                                return _discsToVerify.FirstOrDefault(x => x.DiscNumber == discId);
                            }
                            else if (_completedDiscIds.Contains(discId))
                            {
                                if (_pendingDiscIds.Count() == 1)
                                    SetStatus($"Archive disc {discId} has already been verified, please insert disc {_pendingDiscIds[0].ToString()}");
                                else
                                    SetStatus($"Archive disc {discId} has already been verified, please insert another disc");
                            }
                            else
                            {
                                if (_pendingDiscIds.Count() == 1)
                                    SetStatus($"Archive disc {discId} does not need to be verified, please insert disc {_pendingDiscIds[0].ToString()}");
                                else
                                    SetStatus($"Archive disc {discId} does not need to be verified, please insert another disc");
                            }
                        }
                    }

                    await Task.Delay(500);
                }
            }

            return insertedDisc;
        }
    }
}