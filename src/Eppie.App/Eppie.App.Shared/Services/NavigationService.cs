using Tuvi.App.ViewModels.Services;
using System;
using System.Linq;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else 
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Tuvi.App.Shared.Services
{
    public class NavigationService : INavigationService
    {
        protected Frame MainFrame { get; }
        protected string PageTypePrefix { get; }

        public NavigationService(Frame mainFrame, string pageTypePrefix)
        {
            MainFrame = mainFrame;
            PageTypePrefix = pageTypePrefix;
        }

        public bool CanGoBack()
        {
            return MainFrame != null && MainFrame.CanGoBack;
        }

        public void GoBack()
        {
            MainFrame?.GoBack();
        }

        public void Navigate(string pageKey, object parameter = null)
        {
            string pageTypeName = GetPageTypeNameFromKey(pageKey);
            Type pageType = Type.GetType(pageTypeName);
            if (pageType != null)
            {
                MainFrame?.Navigate(pageType, parameter);
            }
        }

        public bool CanGoBackTo(string pageKey)
        {
            string pageTypeName = GetPageTypeNameFromKey(pageKey);
            return MainFrame?.BackStack.LastOrDefault(item => item.SourcePageType.ToString() == pageTypeName) != null;
        }

        public void GoBackTo(string pageKey)
        {
            string pageTypeName = GetPageTypeNameFromKey(pageKey);
            int backStepsCount = GetBackStepsCountTo(pageTypeName);
            for (int i = 0; i < backStepsCount; i++)
            {
                MainFrame?.GoBack();
            }
        }

        private int GetBackStepsCountTo(string pageTypeName)
        {
            if (MainFrame != null)
            {
                for (int i = MainFrame.BackStack.Count - 1; i >= 0; i--)
                {
                    if (MainFrame.BackStack[i].SourcePageType?.ToString() == pageTypeName)
                    {
                        return MainFrame.BackStack.Count - i;
                    }
                }
            }

            return 0;
        }

        public void GoBackOrNavigate(string pageKey, object parameter = null)
        {
            if (CanGoBack())
            {
                GoBack();
            }
            else
            {
                Navigate(pageKey, parameter);
            }
        }

        public void GoBackToOrNavigate(string pageKey, object parameter = null)
        {
            if (CanGoBackTo(pageKey))
            {
                GoBackTo(pageKey);
            }
            else
            {
                Navigate(pageKey, parameter);
            }
        }

        public void ExitApplication()
        {
            Application.Current.Exit(); // ToDo: Uno0001
        }

        public void ClearHistory()
        {
            MainFrame?.BackStack.Clear();
        }

        protected string GetPageTypeNameFromKey(string pageKey)
        {
            if (pageKey == null)
            {
                throw new ArgumentNullException(nameof(pageKey));
            }

            const string ending = "viewmodel";

            if (pageKey.ToLower().EndsWith(ending, StringComparison.Ordinal))
            {
                int indexToRemoveFrom = pageKey.Length - ending.Length;
                return PageTypePrefix + pageKey.Remove(indexToRemoveFrom);
            }
            else
            {
                return PageTypePrefix + pageKey;
            }
        }
    }
}
