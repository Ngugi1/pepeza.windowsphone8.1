using ByteSizeLib;
using Coding4Fun.Toolkit.Controls;
using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.DbHelpers.Notice;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Requests;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Pepeza.Validation;
using Pepeza.Views.Boards;
using Shared.Db.Models.Notices;
using Shared.Models.NoticeModels;
using Shared.Server.ServerModels.Notices;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Notices
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class AddNoticePage : Page
    {
        CoreApplicationView view = CoreApplication.GetCurrentView();
        int  boardID;
        StorageFile file = null;
        public AddNoticePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override  void OnNavigatedTo(NavigationEventArgs e)
        {
            
            if (e.Parameter != null)
            {
                boardID = (int)e.Parameter;
            }
            else
            {
                this.Frame.GoBack();
            }
       
        }

       
        private async void AppBarButton_Send(object sender, RoutedEventArgs e)
        {
            string title, desc;
            Dictionary<string, string> toPost = null;
            //just check if the title and description are not empty
            if (TextNoticeValidation.isTitleDescValid(txtBoxDesc.Text.Trim()))
            {
                desc = txtBoxDesc.Text.Trim();
                if (TextNoticeValidation.isTitleDescValid(txtBoxTitle.Text.Trim()))
                {
                    StackPanelLoading.Visibility = Visibility.Visible;
                    title = txtBoxTitle.Text.Trim();
                    toPost = new Dictionary<string, string>()
                    {
                        {"boardId" , boardID.ToString()} , {"title" , title } ,{"content",desc}
                    };
                    if (file != null)
                    {
                        await postAttachmentNotice(toPost);
                        //this.Frame.Navigate(typeof(BoardProfileAndNotices), boardID);// Pass a parameter that is only 
                        return;
                    }
                    //Check if attachment is empty 
                   
                        Dictionary<string, string> results = await NoticeService.postNotice(toPost);
                        if (results.ContainsKey(Constants.SUCCESS))
                        {
                            //insert the notice to the local database
                            try
                            {
                                JObject obj = JObject.Parse(results[Constants.SUCCESS]);
                                await NoticeHelper.add(new TNotice()
                                {
                                    noticeId = (int)obj["id"],
                                    boardId = boardID,
                                    title = title,
                                    hasAttachment = 0,
                                    content = desc,
                                    dateCreated =(long)obj["dateCreated"],
                                    dateUpdated = (long)obj["dateUpdated"],
                                   
                                });
                                this.Frame.Navigate(typeof(MainPage) , boardID);// Pass a parameter that is only 
                                
                            }
                            catch
                            {
                                showErrorToast("some error occoured");
                            }

                        }
                        else if (results.ContainsKey(Constants.UNAUTHORIZED))
                        {
                            //Show a popup message 
                            App.displayMessageDialog(Constants.UNAUTHORIZED);
                            this.Frame.Navigate(typeof(LoginPage));
                        }
                        else
                        {
                            //show a toast that it failed
                            showErrorToast(results[Constants.ERROR]);
                        }
                    }
                    else
                    {
                        showErrorToast("Invalid title");
                    }
                }
                else
                {
                    showErrorToast("Invalid description!");
                }
                StackPanelLoading.Visibility = Visibility.Collapsed;
                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.CREATENOTICE);
                    
        }
        private async Task postAttachmentNotice(Dictionary<string, string> content)
        {
            
            //Create a new file notice 
            FileNoticeModel toPost = new FileNoticeModel()
            {
                 title = content["title"],
                 content = content["content"],
                 file = file,
                 boardId = boardID
            };
            Dictionary<string, string> results = null;
            try
            {
                 results= await NoticeService.postItem(toPost, file);
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.ATTACHFILE);
                    JObject jobject = JObject.Parse(results[Constants.SUCCESS]);
                    //Get the FileNotice details
                    TNotice fileNotice = new TNotice()
                    {
                         boardId = toPost.boardId,
                         title = toPost.title,
                         content = toPost.content,
                         noticeId  = (int)jobject["id"],
                         hasAttachment = 1,
                         dateCreated = (long)jobject["dateCreated"]
                       };
                    if (jobject["dateUpdated"]!=null) fileNotice.dateUpdated = (long)jobject["dateUpdated"]; //We have results , get attachment details  

                    //Get the attachment 
                    TAttachment attachment = new TAttachment()
                    {
                        id = (int)jobject["attachment"]["id"],
                        type = (string)jobject["attachment"]["type"],
                        noticeId = (int)jobject["id"],
                        dateCreated = (long)jobject["attachment"]["dateCreated"],
                        link = string.Format(NoticeAddresses.LINK_FORMAT, (int)jobject["id"])
                    };

                    if (jobject["attachment"]["dateUpdated"] != null) attachment.dateUpdated = (long)jobject["attachment"]["dateUpdated"];



                    //Getting the file Item 

                    TFile tfile = new TFile();

                    tfile.id = (int)jobject["file"]["id"];
                        tfile.attachmentId = (int)jobject["file"]["attachmentId"];
                        tfile.size = (long)jobject["file"]["fileSize"];
                        tfile.fileName = (string)jobject["file"]["fileName"];
                        tfile.link = (string)jobject["file"]["link"];
                       tfile.mimeType = (string)jobject["file"]["mime_type"];
                        tfile.dateCreated = (long)jobject["file"]["dateCreated"];
                     tfile.link = string.Format(NoticeAddresses.LINK_FORMAT,(int)jobject["file"]["id"]);
                    
                    var fileNameParts = tfile.fileName.Split('.');
                    int elements = fileNameParts.Length;
                    tfile.uniqueFileName = string.Format(@"{0}.{1}", Guid.NewGuid(), fileNameParts[elements - 1]);
                    if (jobject["file"]["dateUpdated"] != null)
                    {
                        tfile.dateUpdated = (long)jobject["file"]["dateUpdated"];
                    }
                    await DBHelperBase.add(tfile);
                    await DBHelperBase.add(attachment);
                    await DBHelperBase.add(fileNotice);
                    StorageFolder folder = null;
                    //Save the attachment to another folder 
                    try
                    {
                       folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Pepeza");
                    }
                    catch
                    {
                        folder = null;
                    }
                    
                    string fileName = tfile.uniqueFileName;
                    if (folder != null)
                    {
                        if (await folder.GetFileAsync(fileName) == null)
                        {
                            await folder.CreateFileAsync(fileName);
                            using (var stream = await file.OpenStreamForWriteAsync())
                            {
                                using (StreamWriter writer = new StreamWriter(stream))
                                {
                                    await writer.FlushAsync();
                                }

                            }
                        }
                    }
                    else
                    {
                       
                        folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Pepeza");
                        if (folder!= null)
                            {
                                await folder.CreateFileAsync(fileName);
                                using (var stream = await file.OpenStreamForWriteAsync())
                                {
                                    using (StreamWriter writer = new StreamWriter(stream))
                                    {
                                        await writer.FlushAsync();
                                    }

                                }
                            }
                           
                        }
                    }
                else if (results.ContainsKey(Constants.UNAUTHORIZED))
                {
                    //Show a popup message 
                    App.displayMessageDialog(Constants.UNAUTHORIZED);
                    this.Frame.Navigate(typeof(LoginPage));
                }
                else
                {
                    showErrorToast(results[Constants.ERROR]);
                }
            }
            catch(Exception ex)
            {
                if(results != null)
                {
                    showErrorToast(ex.Message);
                }
               
            }
            StackPanelLoading.Visibility = Visibility.Collapsed;
            this.Frame.Navigate(typeof(MainPage) );
        }
        private void showErrorToast(string message)
        {
            toastError.Message = message;
        }
        private async void View_Activated(CoreApplicationView sender, Windows.ApplicationModel.Activation.IActivatedEventArgs args)
        {
            if (args != null)
            {
                FileOpenPickerContinuationEventArgs fileArgs = args as FileOpenPickerContinuationEventArgs;
                if (fileArgs != null)
                {
                    if (fileArgs.Files.Count == 0) return;
                    file = fileArgs.Files[0];
                    var fileProperties = await file.GetBasicPropertiesAsync();
                    var filesize = ByteSize.FromBytes(fileProperties.Size);
                   
                    if(filesize.MegaBytes <= 5)
                    {
                        if (fileProperties.Size > (1024 * 1024) || (fileProperties.Size == (1024 * 1024)))
                        {
                            filesize.ToString("MB");
                        }
                        else if ((fileProperties.Size == 1024 || fileProperties.Size > 1024) && fileProperties.Size < 1024 * 1024)
                        {
                            filesize.ToString("KB");
                        }
                        else if (fileProperties.Size < 1024)
                        {
                            filesize.ToString("B");
                        }

                        txtBlockFileSize.Text = file.DisplayType+ " , "+  filesize.ToString();
                        txtBlockFileName.Text = file.Name;
                        GridAttachment.Visibility = Visibility.Visible;
                    }else
                    {
                        toastError.Message = "File is too large, select a file less than 5MB in size";
                    }
                   
                  
                }
            }
        }
        private void ApBtnAttachment_Click(object sender, RoutedEventArgs e)
        {
            view.Activated += View_Activated;
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Clear();
            filePicker.FileTypeFilter.Add("*");
            filePicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.PickSingleFileAndContinue();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
        private void SymbolIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            file = null;
            GridAttachment.Visibility = Visibility.Collapsed;
        }
        private  void RichEditBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
