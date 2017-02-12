using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.DbHelpers.Notice;
using Pepeza.Db.DbHelpers.User;
using Pepeza.Db.Models;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.Db.Models.Orgs;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Connectivity;
using Pepeza.Server.Requests;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Pepeza.Views.Analytics;
using Shared.Db.DbHelpers;
using Shared.Db.DbHelpers.Notice;
using Shared.Db.DbHelpers.Orgs;
using Shared.Db.Models.Notices;
using Shared.Db.Models.Orgs;
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
        string fileName;
        private List<DownloadOperation> activeDownloads = new List<DownloadOperation>();
        private CancellationTokenSource cts;
        TFile file = new TFile();
        StorageFile storageFile = null;
        TNotice notice = null;
        string noticeTitle;
        int noticeId;
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
           
            StackPanelDetails.Visibility = Visibility.Collapsed;
            StackPanelLoading.Visibility = Visibility.Visible;
            if (e.Parameter != null && e.Parameter.GetType() == typeof(TNotice))
            {
                notice = e.Parameter as TNotice;
                notice.date_created_display = DateTimeFormatter.UnixTimestampToDate(notice.dateCreated, false);
                noticeId = notice.noticeId;
                // Now do a sumission if we are connected to the internet
                if (notice.hasAttachment==1)
                {
                    StackPanelDownload.Visibility = Visibility.Visible;  
                   //Check if the file is downloaded
                    try
                    {
                        file = await FileHelper.getByAttachmentId(notice.attachmentId);
                        if (file!=null)
                        {
                            file.fileTypeAndSize = "(" + file.mimeType + " ," + ByteSizeLib.ByteSize.FromBytes(file.size) + ")";
                            if (await folderExists(Constants.PEPEZA))
                            {
                                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(Constants.PEPEZA);
                               
                                if (await fileExists(file.uniqueFileName))
                                {
                                    fileName = file.uniqueFileName;
                                    var localFile = await folder.GetFileAsync(fileName);
                                    if (localFile != null)
                                    {
                                        var fileproperties = await localFile.GetBasicPropertiesAsync();
                                        if (fileproperties.Size != 0)
                                        {
                                            storageFile = localFile;
                                            StackPanelDownload.Visibility = Visibility.Visible;
                                            HyperLinkOpen.Visibility = Visibility.Visible;
                                            HLBDownloadAttachment.Visibility = Visibility.Collapsed;
                                        }
                                        else
                                        {
                                            storageFile = null;
                                            StackPanelDownload.Visibility = Visibility.Visible;
                                            HyperLinkOpen.Visibility = Visibility.Collapsed;
                                            HLBDownloadAttachment.Visibility = Visibility.Visible;
                                        }
                                        
                                    }
                                    else
                                    {
                                        storageFile = null;
                                        StackPanelDownload.Visibility = Visibility.Visible;
                                        HyperLinkOpen.Visibility = Visibility.Collapsed;
                                        HLBDownloadAttachment.Visibility = Visibility.Visible;
                                    }
                                }
                                else
                                {
                                    //We do not have such file, we need to redownload it.
                                    StackPanelDownload.Visibility = Visibility.Visible;
                                    HLBDownloadAttachment.Visibility = Visibility.Visible;
                                    HyperLinkOpen.Visibility = Visibility.Collapsed;
                                }
                               
                            }
                            else
                            {
                              //Folder does not exist 
                              StackPanelDownload.Visibility = Visibility.Visible;
                              HLBDownloadAttachment.Visibility = Visibility.Visible;
                              HyperLinkOpen.Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            StackPanelDownload.Visibility = Visibility.Visible;
                            HyperLinkOpen.Visibility = Visibility.Collapsed;
                            HLBDownloadAttachment.Visibility = Visibility.Visible;
                        }
                       
                       
                    }
                    catch(Exception)
                    {
                        //File is not downloaded yet 
                        HyperLinkOpen.Visibility = Visibility.Collapsed;
                        StackPanelDownload.Visibility = Visibility.Visible;
                        HLBDownloadAttachment.Visibility = Visibility.Visible;
                       
                    } 
                    this.StackPanelDownload.DataContext = file;
                    assignRoles(notice.boardId);
                }
                else
                {
                    assignRoles(notice.boardId);
                    StackPanelDownload.Visibility = Visibility.Collapsed;
                }
                if (string.IsNullOrWhiteSpace(notice.board))
                {
                    StackPanelSentFrom.Visibility = Visibility.Collapsed;
                }
                else
                {
                    StackPanelSentFrom.Visibility = Visibility.Visible;
                }
                //Get the poster of the notice 
               // Dictionary<string,string>
                TUserInfo userInfo = await UserHelper.getUserInfo(notice.userId);
                if (userInfo != null)
                {
                    if (userInfo.firstName != null && userInfo.lastName != null)
                    {
                        notice.poster = userInfo.firstName + " " + userInfo.lastName;
                    }
                    else
                    {
                        notice.poster = userInfo.username;
                    }
                }
                
                this.RootGrid.DataContext = notice;
                noticeTitle = notice.title;
                StackPanelLoading.Visibility = Visibility.Collapsed;
                await NoticeService.submitReadNoticeItems();
            }
            #region Remote notice 
            //else if(e.Parameter!=null && e.Parameter.GetType() == typeof(int))
            //{
            //    //Now load the notice itself 
            //    StackPanelSentFrom.Visibility = Visibility.Collapsed;
            //    noticeId = (int)e.Parameter;
            //    try
            //    {
            //        Dictionary<string, string> noticeresults = await NoticeService.getNotice(noticeId);
            //        if (noticeresults.ContainsKey(Constants.SUCCESS))
            //        {
            //            //Get the notice details 
            //            JObject json = JObject.Parse(noticeresults[Constants.SUCCESS]);
            //            JToken attachment = (JToken)json["attachment"];
            //            TNotice notice = new TNotice();
            //            if (attachment.Type != JTokenType.Null)
            //            {
            //                file = new TFile();
            //                file.id = (int)json["file"]["id"];
            //                file.mimeType = (string)json["file"]["mimeType"];
            //                file.fileName = (string)json["file"]["fileName"];
            //                file.size = (long)json["file"]["size"];
            //                file.dateCreated = (long)json["file"]["dateCreated"];

            //                file.link = string.Format(NoticeAddresses.LINK_FORMAT, file.id);
            //                file.fileTypeAndSize = "(" + file.mimeType + " ," + ByteSizeLib.ByteSize.FromBytes(file.size) + ")";
            //                try
            //                {
            //                    if (await folderExists(PEPEZA))
            //                    {
            //                        var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(PEPEZA);
            //                        fileName = file.id + file.fileName;
            //                        var localFile = await folder.GetFileAsync(file.id + file.fileName);
            //                        if (localFile != null)
            //                        {
            //                            HyperLinkOpen.Visibility = Visibility.Visible;
            //                        }
            //                        else
            //                        {
            //                            HLBDownloadAttachment.Visibility = Visibility.Visible;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        HLBDownloadAttachment.Visibility = Visibility.Visible;
            //                    }

            //                }
            //                catch (Exception)
            //                {
            //                    HLBDownloadAttachment.Visibility = Visibility.Visible;
            //                }
            //            }
            //            else
            //            {
            //                StackPanelDownload.Visibility = Visibility.Collapsed;
            //            }
                       
            //            notice.title = (string)json["notice"]["title"];
            //            notice.noticeId = (int)json["notice"]["id"];
            //            notice.boardId = (int)json["notice"]["boardId"];
            //            notice.hasAttachment = (int)json["notice"]["hasAttachment"];
            //            notice.content = (string)json["notice"]["content"];
            //            notice.dateCreated = (long)json["notice"]["dateCreated"];
            //            noticeTitle = notice.title;
            //            if (notice.hasAttachment == 1)
            //            {
            //                StackPanelDownload.Visibility = Visibility.Visible;
            //            }
            //            else
            //            {
            //                StackPanelDownload.Visibility = Visibility.Collapsed;
            //            }
            //            TUserInfo userInfo = await NoticePosterHelper.get(notice.userId);
            //            if (userInfo != null)
            //            {
            //                if (userInfo.firstName != null && userInfo.lastName != null)
            //                {
            //                    notice.poster = userInfo.firstName + " " + userInfo.lastName;
            //                }
            //                else
            //                {
            //                    notice.poster = userInfo.username;
            //                }
            //            }
            //            else
            //            {
            //                //Get the user details online
            //            }
                        

            //            RootGrid.DataContext = notice;
            //            StackPanelDownload.DataContext = file;
            //            assignRoles(notice.boardId);
            //        }
            //        else
            //        {
            //            //Throw error 
            //            ToastStatus.Message = noticeresults[Constants.ERROR];
            //        }
            //    }
            //    catch(Exception ex)
            //    {
            //        string exep = ex.ToString();
            //        ToastStatus.Message = Constants.UNKNOWNERROR;
            //    }

            //}
            #endregion
            try
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.VIEWNOTICE);
            }
            catch(Exception)
            {
            }
            StackPanelDetails.Visibility = Visibility.Visible;
            StackPanelLoading.Visibility = Visibility.Collapsed;
        }
        private async void assignRoles(int boardId)
        {
            //Get the board with the given ID 
            TBoard notice_board = await BoardHelper.getBoard(boardId);
            if (notice_board != null)
            {
                    //Get the role 
                    TCollaborator collaborator = await CollaboratorHelper.getRole((int)Settings.getValue(Constants.USERID), notice_board.orgID);
                    if (collaborator != null)
                    {
                        if (collaborator.role == Constants.EDITOR || collaborator.role == Constants.ADMIN || collaborator.role == Constants.OWNER)
                        {
                            CommandBarControls.Visibility = Visibility.Visible;

                        }
                    }
            }
            else
            {
                CommandBarControls.Visibility = Visibility.Collapsed;

            }
           
           
        }
        private async void HLBDownloadAttachment_Click(object sender, RoutedEventArgs e)
        {
            //We need to download the file
            if (SymbolOperation.Symbol == Symbol.Cancel)
            {
                displayProgress("", true);
                //Cancel download
                cts.Cancel();
                SymbolOperation.Symbol = Symbol.Download;
                HyperLinkOpen.Visibility = Visibility.Collapsed;
                cts.Dispose();
                // Re-create the CancellationTokenSource and activeDownloads for future downloads.
                cts = new CancellationTokenSource();
                activeDownloads = new List<DownloadOperation>();
               
                if (storageFile != null)
                {
                    try
                    {
                        StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(Constants.PEPEZA);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            var file = await folder.GetFileAsync(fileName);
                            await file.DeleteAsync();
                        }
                    }
                    catch
                    {

                    }
                   
                }
                HyperLinkOpen.Visibility = Visibility.Collapsed;
                HLBDownloadAttachment.Visibility = Visibility.Visible;
                
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
                txtBlockDownload.Text = "downloading attachment... 0%";
                SymbolOperation.Symbol = Symbol.Cancel;
                #region Create Destination Folder and file
                //Create a root destination folder
                //Create a download URI
                Uri source = new Uri(file.link);
                //Create a destination URI
                string destinationUri = file.uniqueFileName;
                //Create the destination folder
                StorageFolder destinationFolder = null;
                if (!await folderExists(Constants.PEPEZA))
                {
                    //Create the folder 
                    destinationFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(Constants.PEPEZA);
                }
                else
                {
                    destinationFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(Constants.PEPEZA);
                }
                if (!await fileExists(file.uniqueFileName))
                {
                    storageFile = await destinationFolder.CreateFileAsync(file.uniqueFileName);
                     #endregion
                #region Background Downloader
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
                        storageFile = await destinationFolder.CreateFileAsync(destinationUri);
                        downloader.SetRequestHeader(Constants.APITOKEN, (string)Settings.getValue(Constants.APITOKEN));
                        DownloadOperation downloadOperation = downloader.CreateDownload(source, storageFile);
                        downloadOperation.Priority = BackgroundTransferPriority.High;
                        //Now handle the download 
                        await handleDownloadAsync(downloadOperation, true);
                    }
                    #endregion
                }
                else
                {
                    storageFile = await destinationFolder.GetFileAsync(file.uniqueFileName);
                    var props =  await storageFile.GetBasicPropertiesAsync();
                    if (props.Size > 0)
                    {
                        HyperLinkOpen.Visibility = Visibility.Visible;
                        StackPanelDownload.Visibility = Visibility.Visible;
                        return;
                    }
                    else
                    {
                        #region Background Downloader
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
                            storageFile = await destinationFolder.CreateFileAsync(destinationUri);
                            downloader.SetRequestHeader(Constants.APITOKEN, (string)Settings.getValue(Constants.APITOKEN));
                            DownloadOperation downloadOperation = downloader.CreateDownload(source, storageFile);
                            downloadOperation.Priority = BackgroundTransferPriority.High;
                            //Now handle the download 
                            await handleDownloadAsync(downloadOperation, true);
                        }
                        #endregion
                    }
                   
                }
               
               
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
        private async Task<bool> fileExists(string fileName)
        {
            try
            {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(Constants.PEPEZA);
                await folder.GetFileAsync(fileName);
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
                displayProgress(string.Format("downloading attachment... {0}%", percentage));
                if (percentage ==100)
                {
                    HLBDownloadAttachment.Visibility = Visibility.Collapsed;
                    HyperLinkOpen.Visibility = Visibility.Visible;
                    var storage = storageFile;
                }
            }
            else
            {
                displayProgress(string.Format("downloading attachment ... {0}%", percentage));
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
                txtBlockDownload.Text = "download attachment";
                SymbolOperation.Symbol = Symbol.Download;
            }
           
        }
        private async void HyperLinkOpen_Click(object sender, RoutedEventArgs e)
        {
            //Launch the file 
            try
            {
                if (storageFile != null)
                {
                    await Launcher.LaunchFileAsync(storageFile);
                }
                else
                {
                    var currentFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(Constants.PEPEZA);
                    StorageFile sfile = await currentFolder.GetFileAsync(fileName);
                    await Launcher.LaunchFileAsync(sfile);
                }
                
               
            }
            catch
            {
                StackPanelDownload.Visibility = Visibility.Visible;
                HyperLinkOpen.Visibility = Visibility.Collapsed;
                txtBlockDownload.Text = "download attachment";
                SymbolOperation.Symbol = Symbol.Download;
                HLBDownloadAttachment.Visibility = Visibility.Visible;
                App.displayMessageDialog("File could not be located. Redownload the file");
            }
        }
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

            this.Frame.Navigate(typeof(NoticeAnalytics), new Dictionary<string, string>() { { "id", noticeId.ToString() }, { "title", noticeTitle} });
        }

        private async void AppBtnDeleteNotice_Click(object sender, RoutedEventArgs e)
        {
            DeletingNoticeProgress.Show();
            Dictionary<string, string> results = await NoticeService.delete(noticeId);
            if (results != null)
            {
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    await NoticeHelper.deleteNotice(noticeId);
                    ToastStatus.Message = "deleted successfully";
                    this.Frame.GoBack();
                }
                else if(results.ContainsKey(Constants.ERROR))
                {
                    //show a toast
                    ToastStatus.Message = "deletion failed, try again later";
                }
            }
            DeletingNoticeProgress.Hide();
        }
       
    }
}
