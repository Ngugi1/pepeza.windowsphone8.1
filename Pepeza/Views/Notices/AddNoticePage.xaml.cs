using ByteSizeLib;
using Coding4Fun.Toolkit.Controls;
using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.DbHelpers.Notice;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Validation;
using Shared.Db.Models.Notices;
using Shared.Models.NoticeModels;
using Shared.Server.ServerModels.Notices;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        protected override async void OnNavigatedTo(NavigationEventArgs e)
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
                        postAttachmentNotice(toPost);
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
                                    hasAttachment = (int)obj["hasAttachment"],
                                    content = desc,
                                    dateCreated = DateTimeFormatter.format((long)obj["dateCreated"]),
                                    dateUpdated = DateTimeFormatter.format((long)obj["dateUpdated"]),
                                   
                                });
                                this.Frame.GoBack();
                            }
                            catch
                            {
                                showErrorToast("some error occoured");
                            }

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
                    
        }

        private async void postAttachmentNotice(Dictionary<string, string> content)
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
                    JObject jobject = JObject.Parse(results[Constants.SUCCESS]);
                    //Get the FileNotice details
                    TNotice fileNotice = new TNotice()
                    {
                         boardId = toPost.boardId,
                         title = toPost.title,
                         content = toPost.content,
                         noticeId  = (int)jobject["id"],
                         hasAttachment = (int)jobject["hasAttachment"],
                         dateCreated = DateTimeFormatter.format((long)jobject["dateCreated"]),
                       };
                    if (jobject["dateUpdated"]!=null) fileNotice.dateUpdated = DateTimeFormatter.format((long)jobject["dateUpdated"]);                    //We have results , get attachment details  

                    //Get the attachment 
                    TAttachment attachment = new TAttachment()
                    {
                        id = (int)jobject["attachment"]["id"],
                        type = (string)jobject["attachment"]["type"],
                        noticeId = (int)jobject["id"],
                        dateCreated = DateTimeFormatter.format((long)jobject["attachment"]["dateCreated"]),
                    };

                    if (jobject["attachment"]["dateUpdated"] != null) attachment.dateUpdated = DateTimeFormatter.format((long)jobject["attachment"]["dateUpdated"]);



                    //Getting the file Item 

                    TFile tfile = new TFile()
                    {
                        id = (int)jobject["file"]["id"],
                        attachmentId = (int)jobject["file"]["attachmentId"],
                        size = (long)jobject["file"]["fileSize"],
                        fileName = (string)jobject["file"]["fileName"],
                        link = (string)jobject["file"]["link"],
                        mimeType = (string)jobject["file"]["mime_type"],
                        dateCreated = DateTimeFormatter.format((long)jobject["file"]["dateCreated"]),
                    };
                    if (jobject["file"]["dateUpdated"] != null)
                    {
                        tfile.dateUpdated = DateTimeFormatter.format((long)jobject["file"]["dateUpdated"]);
                    }
                    await DBHelperBase.add(tfile);
                    await DBHelperBase.add(attachment);
                    await DBHelperBase.add(fileNotice);
                }
                else
                {
                    showErrorToast(results[Constants.ERROR]);
                }
            }
            catch
            {
                if(results != null)
                {
                    showErrorToast(results[Constants.ERROR]);
                }
               
            }
            StackPanelLoading.Visibility = Visibility.Collapsed;
            this.Frame.GoBack();
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
