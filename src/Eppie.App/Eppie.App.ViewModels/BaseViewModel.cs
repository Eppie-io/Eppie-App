// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Eppie.App.ViewModels.Services;
using Tuvi.App.ViewModels.Services;
using Tuvi.OAuth2;
using Tuvi.Proton;

namespace Tuvi.App.ViewModels
{
    public class BaseViewModel : ObservableValidator
    {
        protected IErrorHandler ErrorHandler { get; private set; }
        public void SetErrorHandler(IErrorHandler errorHandler)
        {
            ErrorHandler = errorHandler;
            ErrorHandler?.SetMessageService(MessageService);
        }

        protected Tuvi.Core.ITuviMail Core { get { return CoreProvider(); } }
        protected Func<Tuvi.Core.ITuviMail> CoreProvider { get; private set; }
        public void SetCoreProvider(Func<Tuvi.Core.ITuviMail> coreProvider)
        {
            CoreProvider = coreProvider;
        }

        protected IAIService AIService { get { return AIServiceProvider(); } }
        private Func<IAIService> AIServiceProvider { get; set; }
        public void SetAIServiceProvider(Func<IAIService> provider)
        {
            AIServiceProvider = provider;
        }

        protected AuthorizationProvider AuthProvider { get; private set; }
        public void SetAuthProvider(AuthorizationProvider authProvider)
        {
            AuthProvider = authProvider;
        }

        protected IProtonLoginHelper ProtonLoginHelper { get; private set; }
        public void SetProtonLoginHelper(IProtonLoginHelper protonLoginHelper)
        {
            ProtonLoginHelper = protonLoginHelper;
        }

        protected INavigationService NavigationService { get; private set; }
        public void SetNavigationService(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        protected IPendingMailtoService PendingMailtoService { get; private set; }
        public void SetPendingMailtoService(IPendingMailtoService pendingMailtoService)
        {
            PendingMailtoService = pendingMailtoService;
        }

        protected ILocalSettingsService LocalSettingsService { get; private set; }
        public void SetLocalSettingsService(ILocalSettingsService localSettingsService)
        {
            LocalSettingsService = localSettingsService;
        }

        protected ILocalizationService LocalizationService { get; set; }
        public void SetLocalizationService(ILocalizationService localizationService)
        {
            LocalizationService = localizationService;
        }

        protected ITuviMailMessageService MessageService { get; private set; }
        public void SetMessageService(ITuviMailMessageService messageService)
        {
            MessageService = messageService;
            ErrorHandler?.SetMessageService(MessageService);
        }

        protected ILauncherService LauncherService { get; private set; }
        public void SetLauncherService(ILauncherService launcherService)
        {
            LauncherService = launcherService;
        }

        protected IDispatcherService DispatcherService { get; private set; }
        public void SetDispatcherService(IDispatcherService dispatcherService)
        {
            DispatcherService = dispatcherService;
        }

        protected IBrandService BrandService { get; private set; }
        public void SetBrandService(IBrandService brandService)
        {
            BrandService = brandService;
        }

        protected IAppStoreService AppStoreService { get; private set; }
        public void SetAppStoreService(IAppStoreService appStoreService)
        {
            AppStoreService = appStoreService;
        }

        protected IDragAndDropService DragAndDropService { get; private set; }
        public void SetDragAndDropService(IDragAndDropService dragAndDropService)
        {
            DragAndDropService = dragAndDropService;
        }

        public bool IsLocalAIAvailable => AIService.IsAvailable();

        public virtual void OnError(Exception e)
        {
            ErrorHandler?.OnError(e, false);
        }

        protected string GetLocalizedString(string resource)
        {
            return LocalizationService?.GetString(resource) ?? string.Empty;
        }
    }
}
