using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.DbHelpers.Notice;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Validation;
using Shared.Models.NoticeModels;
using Shared.Server.ServerModels.Notices;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
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
        int noticeType, boardID;
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
            //fetch all the boards and feed them to the combobox
            List<TBoard> boards = await BoardHelper.fetchAllBoards();
            if (boards.Count <= 0)
            {
                StackPanelNoBoards.Visibility = Visibility.Visible;
            }
            else
            {
                comboBoards.ItemsSource = boards;
                comboBoards.SelectedIndex = 0;
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
                    boardID = (comboBoards.SelectedItem as TBoard).id;
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
                                    content = desc,
                                    dateCreated = DateTimeFormatter.format((long)obj["dateCreated"]["date"]),
                                    dateUpdated = DateTimeFormatter.format((long)obj["dateUpdated"]["date"]),
                                   
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
                 type = noticeType,
                 boardId = boardID
            };
            Dictionary<string, string> results = null;
            try
            {
                 results= await NoticeService.postItem(toPost, file);
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    JObject jobject = JObject.Parse(results[Constants.SUCCESS]);
                    //Get the FileNorice details
                    TNotice fileNotice = new TNotice()
                    {
                         boardId = toPost.boardId,
                         title = toPost.title,
                         content = toPost.content,
                         noticeId  = (int)jobject["id"],
                         attachmentId = (int)jobject["attachment"]["id"],
                         dateCreated = (DateTime)jobject["dateCreated"]["date"],
                         dateUpdated = (DateTime)jobject["dateUpdated"]["date"]
                    };
                    //We have results , get attachment details  
                    TAttachment attachment = new TAttachment()
                    {
                        id = (int)jobject["attachment"]["id"],
                        type = (int)jobject["attachment"]["type"],
                        filesize = (long)jobject["attachment"]["fileSize"],
                        link = (string)jobject["attachment"]["link"],
                        noticeId = (int)jobject["id"],
                        dateCreated = (DateTime)jobject["attachment"]["dateCreated"]["date"],
                        dateUpdated = (DateTime)jobject["attachment"]["dateUpdated"]["date"]
                    };
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
        private void Attachment_Clicked(object sender, RoutedEventArgs e)
        {
            AppBarButton btn = sender as AppBarButton;
            string label = btn.Label;

            view.Activated += View_Activated;
            switch (label)
            {
                case "photo":
                    FilePickerHelper.pickFile(FilePickerHelper.PHOTOS , Windows.Storage.Pickers.PickerLocationId.MusicLibrary);
                    
                    break;
                case "audio":
                    FilePickerHelper.pickFile(FilePickerHelper.AUDIO , Windows.Storage.Pickers.PickerLocationId.MusicLibrary);
                    break;
                case "video":
                    FilePickerHelper.pickFile(FilePickerHelper.VIDEO , Windows.Storage.Pickers.PickerLocationId.VideosLibrary);
                    break;
                case "document":
                    FilePickerHelper.pickFile(FilePickerHelper.DOCUMENTS , Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary);
                    break;
                default:
                    break;
            }

          
        }
        private void View_Activated(CoreApplicationView sender, Windows.ApplicationModel.Activation.IActivatedEventArgs args)
        {
            if (args != null)
            {
                FileOpenPickerContinuationEventArgs fileArgs = args as FileOpenPickerContinuationEventArgs;
                if (fileArgs.Files.Count == 0) return;
                file = fileArgs.Files[0];
                if (file != null)
                {
                    noticeType = getNoticeType(file);
                }
               
            }
        }
        private int getNoticeType(StorageFile file)
        {
            if (file != null)
            {
                string mime_type = file.ContentType;
                if(mime_type.Contains("image"))
                {
                    return 1;
                }else if (mime_type.Contains("video"))
                {
                    return 3;
                }else if (mime_type.Contains("audio"))
                {
                    return 2;
                }
                else if(mime_type.Contains("plain")|| mime_type.Contains("application"))
                {
                    return 4;
                }
            }
            return 0;
        }
    }
}
