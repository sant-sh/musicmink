﻿using MusicMink.Common;
using MusicMink.MediaSources;
using MusicMink.Pages;
using MusicMinkAppLayer.Diagnostics;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;
using Windows.Storage;

namespace MusicMink
{
    enum NavigationLocation
    {
        Home,
        NewHome,

        Library,

        AlbumList,
        ArtistList,
        PlaylistList,
        SongList,
        MixList,

        AlbumPage,
        ArtistPage,
        PlaylistPage,
        MixPage,

        SearchPage,
        SettingsPage,
        ManageLibrary,

        Queue,
        NowPlaying
    }

    abstract class ContinuationInfo { }

    /// <summary>
    /// Used to help cleanly handle navigation across the root app frame.
    /// Also helps manage ContinuationInfo (information passed through app sessions
    /// like with the AlbumArt File Picker)
    /// </summary>
    class NavigationManager : NotifyPropertyChangedUI
    {
        private static NavigationManager _current;
        public static NavigationManager Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new NavigationManager();
                }

                return _current;
            }
        }

        public static class Properties
        {
            public const string IsHome = "IsHome";
        }

        private Frame mainNavigationFrame;

        public ContinuationInfo ContinuationInfo = null;

        public void GoHome()
        {
            while (mainNavigationFrame.CanGoBack)
            {
                mainNavigationFrame.GoBack();
            }
        }

        public bool IsHome
        {
            get
            {
                return mainNavigationFrame.CanGoBack;
            }
        }
        
        public void Navigate(NavigationLocation type, object parameter = null)
        {
            mainNavigationFrame.Navigate(NavigationLocationToPageType(type), parameter);
        }

        private Type NavigationLocationToPageType(NavigationLocation location)
        {
            switch (location)
            {
                case NavigationLocation.AlbumList:
                    return typeof(AlbumList);
                case NavigationLocation.AlbumPage:
                    return typeof(AlbumPage);
                case NavigationLocation.ArtistList:
                    return typeof(ArtistList);
                case NavigationLocation.ArtistPage:
                    return typeof(ArtistPage);
                case NavigationLocation.Home:
                    return typeof(HomePage);
                case NavigationLocation.ManageLibrary:
                    return typeof(ManageLibrary);
                case NavigationLocation.MixList:
                    return typeof(MixList);
                case NavigationLocation.MixPage:
                    return typeof(MixPage);
                case NavigationLocation.PlaylistList:
                    return typeof(PlaylistList);
                case NavigationLocation.PlaylistPage:
                    return typeof(PlaylistPage);
                case NavigationLocation.SearchPage:
                    return typeof(SearchPage);
                case NavigationLocation.SettingsPage:
                    return typeof(Settings);
                case NavigationLocation.SongList:
                    return typeof(SongList);
                case NavigationLocation.Queue:
                    return typeof(Queue);
                case NavigationLocation.NowPlaying:
                    return typeof(NowPlaying);
                case NavigationLocation.NewHome:
                    return typeof(NewHomePage);
                case NavigationLocation.Library:
                    return typeof(Library);
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected NavigationLocation {0}", location);
                    return typeof(HomePage);
            }
        }


        internal void SetRootFrame(Frame MainContentFrame)
        {
            if (mainNavigationFrame != null)
            {
                mainNavigationFrame.Navigated -= HandleMainNavigationFrameNavigated;
            }

            mainNavigationFrame = MainContentFrame;

            mainNavigationFrame.Navigated += HandleMainNavigationFrameNavigated;
        }

        void HandleMainNavigationFrameNavigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            NotifyPropertyChanged(Properties.IsHome);
        }

        internal void HandleContinuation(IContinuationActivatedEventArgs continuationEventArgs)
        {
            if (mainNavigationFrame != null && mainNavigationFrame.Content is AlbumPage)
            {
                if (continuationEventArgs is FileOpenPickerContinuationEventArgs)
                {
                    AlbumPage currentAlbumPage = DebugHelper.CastAndAssert<AlbumPage>(mainNavigationFrame.Content);
                    FileOpenPickerContinuationEventArgs filePickerOpenArgs = DebugHelper.CastAndAssert<FileOpenPickerContinuationEventArgs>(continuationEventArgs);

                    currentAlbumPage.HandleFilePickerLaunch(filePickerOpenArgs);
                }
            }
            else if (mainNavigationFrame != null && mainNavigationFrame.Content is ManageLibrary)
            {
                if (continuationEventArgs is FileOpenPickerContinuationEventArgs)
                {
                    FileOpenPickerContinuationEventArgs filePickerOpenArgs = DebugHelper.CastAndAssert<FileOpenPickerContinuationEventArgs>(continuationEventArgs);

                    if (filePickerOpenArgs.Files.Count > 0)
                    {
                        DebugHelper.Assert(new CallerInfo(), filePickerOpenArgs.Files.Count == 1);

                        IStorageFile pickedFile = filePickerOpenArgs.Files[0];

                        MediaImportManager.Current.HandleFilePickerLaunch(pickedFile);
                    }
                }
                else if (continuationEventArgs is FolderPickerContinuationEventArgs)
                {
                    FolderPickerContinuationEventArgs folderOpenArgs = DebugHelper.CastAndAssert<FolderPickerContinuationEventArgs>(continuationEventArgs);

                    if (folderOpenArgs.Folder != null)
                    {
                        MediaImportManager.Current.HandleSyncFolderLaunch(folderOpenArgs.Folder);
                    }
                }
            }
        }  
    }
}
