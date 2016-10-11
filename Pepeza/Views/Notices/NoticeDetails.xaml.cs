﻿using Newtonsoft.Json.Linq;
using Pepeza.Db.Models.Notices;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Connectivity;
using Pepeza.Server.Requests;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Shared.Db.DbHelpers;
using Shared.Db.Models.Notices;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Notices
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NoticeDetails : Page
    {
        string PEPEZA = "Pepeza";
        string fileName;
        private List<DownloadOperation> activeDownloads = new List<DownloadOperation>();
        private CancellationTokenSource cts;
        TFile file = new TFile();
        TNotice notice = null;
        public NoticeDetails()
        {
            cts = new CancellationTokenSource();
            this.InitializeComponent();
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null && e.Parameter.GetType() == typeof(TNotice))
            {
                notice = e.Parameter as TNotice;
                // Now do a sumission if we are connected to the internet
                if (notice.hasAttachment==1)
                {
                    
                   //Check if the file is downloaded
                    try
                    {
                        file = await FileHelper.getByAttachmentId(notice.attachmentId);
                        if (file!=null)
                        {
                            file.fileTypeAndSize = "(" + file.mimeType + " ," + ByteSizeLib.ByteSize.FromBytes(file.size) + ")";
                            if (await folderExists(PEPEZA))
                            {
                                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(PEPEZA);
                                fileName = file.id + file.fileName;
                                var localFile = await folder.GetFileAsync(file.id + file.fileName);
                                if (localFile != null)
                                {
                                    HyperLinkOpen.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    HLBDownloadAttachment.Visibility = Visibility.Visible;
                                }
                            }
                            else
                            {
                                HLBDownloadAttachment.Visibility = Visibility.Visible;
                            }
                        }
                       
                       
                    }
                    catch(Exception)
                    {
                        HLBDownloadAttachment.Visibility = Visibility.Visible;
                    }
                    StackPanelDownload.Visibility = Visibility.Visible;
                    this.StackPanelDownload.DataContext = file;
                }
                else
                {
                    StackPanelDownload.Visibility = Visibility.Collapsed;
                }
                this.RootGrid.DataContext = notice;
                await NoticeService.submitReadNoticeItems();
              
            }else if(e.Parameter!=null && e.Parameter.GetType() == typeof(int))
            {
                //Now load the notice itself 
                int noticeId = (int)e.Parameter;
                try
                {
                    Dictionary<string, string> noticeresults = await NoticeService.getNotice(noticeId);
                    if (noticeresults.ContainsKey(Constants.SUCCESS))
                    {
                        //Get the notice details 
                        JObject json = JObject.Parse(noticeresults[Constants.SUCCESS]);
                        JToken attachment = (JToken)json["attachment"];
                        TNotice notice = new TNotice();
                        if (attachment.Type != JTokenType.Null)
                        {
                            file = new TFile();
                            file.id = (int)json["file"]["id"];
                            file.mimeType = (string)json["file"]["mimeType"];
                            file.fileName = (string)json["file"]["fileName"];
                            file.size = (long)json["file"]["size"];
                            file.dateCreated = DateTimeFormatter.format((long)json["file"]["dateCreated"]);

                            file.link = string.Format(NoticeAddresses.LINK_FORMAT, file.id);
                            file.fileTypeAndSize = "(" + file.mimeType + " ," + ByteSizeLib.ByteSize.FromBytes(file.size) + ")";
                            try
                            {
                                if (await folderExists(PEPEZA))
                                {
                                    var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(PEPEZA);
                                    fileName = file.id + file.fileName;
                                    var localFile = await folder.GetFileAsync(file.id + file.fileName);
                                    if (localFile != null)
                                    {
                                        HyperLinkOpen.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        HLBDownloadAttachment.Visibility = Visibility.Visible;
                                    }
                                }
                                else
                                {
                                    HLBDownloadAttachment.Visibility = Visibility.Visible;
                                }

                            }
                            catch (Exception)
                            {
                                HLBDownloadAttachment.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            StackPanelDownload.Visibility = Visibility.Collapsed;
                        }
                       
                        notice.title = (string)json["notice"]["title"];
                        notice.hasAttachment = (int)json["notice"]["hasAttachment"];
                        notice.content = (string)json["notice"]["content"];
                        notice.dateCreated = (DateTimeFormatter.format((long)json["notice"]["dateCreated"]));
                        RootGrid.DataContext = notice;
                        StackPanelDownload.DataContext = file;
                    }
                    else
                    {
                        //Throw error 
                        ToastStatus.Message = noticeresults[Constants.ERROR];
                    }
                }
                catch(Exception ex)
                {
                    string exep = ex.ToString();
                    ToastStatus.Message = Constants.UNKNOWNERROR;
                }

            }
        }
        private void HLBDownloadAttachment_Click(object sender, RoutedEventArgs e)
        {
            //We need to download the file
            if (SymbolOperation.Symbol == Symbol.Cancel)
            {
                //Cancel download
                cts.Cancel();
                cts.Dispose();
                // Re-create the CancellationTokenSource and activeDownloads for future downloads.
                cts = new CancellationTokenSource();
                activeDownloads = new List<DownloadOperation>();
                displayProgress("",true);
            }
            else
            {
                //Start the download 
                StartDownload(file);
            }
        }
        private  async void StartDownload(TFile file)
        {
            Network network = new Network();

            if (network.HasInternetConnection)
            {
                #region Create Destination Folder and file
                //Create a root destination folder
                //Create a download URI
                Uri source = new Uri(file.link);
                //Create a destination URI , make sure there will be no collission by adding file ID at the end
                string destinationUri = file.id + file.fileName;
                //Create the destination folder
                StorageFolder destinationFolder = null;
                StorageFile storageFile = null;
                if (!await folderExists(PEPEZA))
                {
                    //Create the folder 
                    destinationFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(PEPEZA);
                }
                else
                {
                    destinationFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(PEPEZA);
                }
                storageFile = await destinationFolder.CreateFileAsync(destinationUri);
                #endregion
                #region Bsckground Downloader
                BackgroundDownloader downloader = new BackgroundDownloader();
                if (storageFile != null)
                {
                    downloader.SetRequestHeader(Constants.APITOKEN, (string)Settings.getValue(Constants.APITOKEN));
                    DownloadOperation downloadOperation = downloader.CreateDownload(source, storageFile);
                    downloadOperation.Priority = BackgroundTransferPriority.High;
                    //Now handle the download 
                    await handleDownloadAsync(downloadOperation, true);
                }
                else
                {
                    new MessageDialog("We could not download");
                    return;
                }


                #endregion
            }
            else
            {
                //Show toast that we do not have internet connection 
                ToastStatus.Message = Constants.NO_INTERNET_CONNECTION;
            }
        }
        /// <summary>
        /// check if the pepeza foder exists in windows phone 
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="targetFolder"></param>
        /// <returns></returns>
        private  async Task<bool> folderExists(string folderName)
        {
            try
            {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(folderName);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task  handleDownloadAsync(DownloadOperation download , bool start)
        {
            try
            {
                activeDownloads.Add(download);
                Progress<DownloadOperation> progress = new Progress<DownloadOperation>(DownloadProgress);
                if(start)
                {
                    //We are starting now , attach the progress bar
                    await download.StartAsync().AsTask(cts.Token,progress);
                }else
                {
                    //Reattach the progress 
                    await download.AttachAsync().AsTask(cts.Token, progress);
                }
            }
            catch
            {
                new MessageDialog("Could not download").ShowAsync();
            }
            finally
            {
                activeDownloads.Remove(download);
            }

        }
        private void DownloadProgress(DownloadOperation download)
        {
            double percentage = 0;
            if (download.Progress.BytesReceived > 0)
            {
                percentage = (double)(download.Progress.BytesReceived * 100) / (double)download.Progress.TotalBytesToReceive;
                displayProgress(string.Format("downloading ... {0}%", percentage));
                if (percentage == 100)
                {
                    HLBDownloadAttachment.Visibility = Visibility.Collapsed;
                    HyperLinkOpen.Visibility = Visibility.Visible;
                }
            }
            else
            {
                displayProgress(string.Format("downloading ... {0}%", percentage));
            }
        }
        private void displayProgress(string status , bool canceled = false)
        {
            if (!canceled)
            {
                txtBlockDownload.Text = status;
                SymbolOperation.Symbol = Symbol.Cancel;
            }
            else
            {
                txtBlockDownload.Text = "download";
                SymbolOperation.Symbol = Symbol.Download;
            }
           
        }
        private async void HyperLinkOpen_Click(object sender, RoutedEventArgs e)
        {
            //Launch the file 
            try
            {
                var currentFolder  = await ApplicationData.Current.LocalFolder.GetFolderAsync(PEPEZA);
                var file = await currentFolder.GetFileAsync(fileName);
                await Launcher.LaunchFileAsync(file);
            }
            catch
            {
                HyperLinkOpen.Visibility = Visibility.Collapsed;
                HLBDownloadAttachment.Visibility = Visibility.Visible;
                new MessageDialog("File could not be located. Redownload the file").ShowAsync();
            }
        }
       
    }
}
