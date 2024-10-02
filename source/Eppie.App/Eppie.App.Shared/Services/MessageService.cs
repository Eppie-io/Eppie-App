using System;
using System.Linq;
using System.Threading.Tasks;
using Tuvi.Core.Backup;
using Tuvi.Core.Entities;
using Tuvi.Core.Entities.Exceptions;
// ToDo: move file 'Exceptions.cs' from Tuvi.Core.Mail.Impl to Tuvi.Core.Mail
using Tuvi.Core.Mail.Impl;
using Tuvi.App.ViewModels;
using Tuvi.App.ViewModels.Common;
using Tuvi.App.ViewModels.Services;
using TuviPgpLib.Entities;
using Windows.ApplicationModel.Resources;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else 
using Microsoft.UI.Xaml;
#endif

namespace Tuvi.App.Shared.Services
{
    public partial class MessageService : ITuviMailMessageService
    {
        private ResourceLoader Loader { get; } = ResourceLoader.GetForCurrentView();

        private static Task ShowInfoMessageAsync(string title, string message, string closeButtonText)
        {
            return Common.UITools.ShowInfoMessageAsync(title, message, closeButtonText);
        }

        private static Task<bool> ShowErrorMessageAsync(string title, string message, string acceptButtonText, string rejectButtonText)
        {
            return Common.UITools.ShowErrorMessageAsync(title, message, acceptButtonText, rejectButtonText);
        }

        private static Task<bool> ShowDialogAsync(string title, string message, string acceptButtonText, string rejectButtonText)
        {
            return Common.UITools.ShowDialogAsync(title, message, acceptButtonText, rejectButtonText);
        }

        public Task ShowErrorMessageAsync(Exception exception)
        {
            if (exception is CryptoContextException cryptoContextException)
            {
                return ShowCryptoContextErrorMessageAsync(cryptoContextException);
            }
            else if (exception is CoreException coreException)
            {
                return ShowCoreErrorMessageAsync(coreException);
            }
            else if (exception is BackupException backupException)
            {
                return ShowBackupErrorMessageAsync(backupException);
            }
            else
            {
                return ShowDefaultErrorMessageAsync(exception);
            }
        }

        private Task ShowCryptoContextErrorMessageAsync(CryptoContextException exception)
        {
            if (exception is ImportPublicKeyException importPublicKeyException)
            {
                return ShowImportPublicKeyErrorMessageAsync(importPublicKeyException);
            }
            else if (exception is ExportPublicKeyException exportPublicKeyException)
            {
                return ShowPgpExportPublicKeyErrorMessageAsync(exportPublicKeyException.Message);
            }
            else if (exception is NoSecretKeyException noSecretKeyException)
            {
                return ShowNoSecretKeyErrorMessageAsync(noSecretKeyException.KeyId);
            }
            else if (exception is NoPublicKeyException noPublicKeyException)
            {
                return ShowNoPublicKeyErrorMessageAsync(noPublicKeyException.EmailAddress.DisplayName);
            }
            else if (exception is MessageDecryptionException decryptionException)
            {
                return ShowDecryptionErrorMessageAsync(decryptionException.Message);
            }
            else if (exception is MessageSignatureVerificationException signatureVerificationException)
            {
                return ShowSignatureVerificationErrorMessageAsync(signatureVerificationException.Message);
            }
            else if (exception is MessageSigningException signingException)
            {
                return ShowSigningErrorMessageAsync(signingException.Message);
            }
            else if (exception is MessageEncryptionException encryptionException)
            {
                return ShowEncryptionErrorMessageAsync(encryptionException.Message);
            }
            else
            {
                return ShowDefaultErrorMessageAsync(exception);
            }
        }

        private Task ShowImportPublicKeyErrorMessageAsync(ImportPublicKeyException exception)
        {
            if (exception is PublicKeyAlreadyExistException)
            {
                return ShowPgpPublicKeyAlreadyExistMessageAsync();
            }
            else if (exception is UnknownPublicKeyAlgorithmException)
            {
                return ShowPgpUnknownPublicKeyAlgorithmMessageAsync();
            }
            else
            {
                return ShowPgpPublicKeyImportErrorMessageAsync(exception.Message);
            }
        }

        private Task ShowBackupErrorMessageAsync(BackupException backupException)
        {
            if (backupException is BackupBuildingException buildingException)
            {
                return ShowBackupBuildErrorMessageAsync(buildingException);
            }
            else if (backupException is BackupParsingException parsingException)
            {
                return ShowBackupParsingErrorMessageAsync(parsingException);
            }
            else
            {
                return ShowBackupDefaultErrorMessageAsync(backupException.Message);
            }
        }

        private Task ShowBackupBuildErrorMessageAsync(BackupBuildingException buildingException)
        {
            Exception inner = buildingException.InnerException;

            if (inner is BackupSerializationException serializationException)
            {
                return ShowBackupSerializationErrorMessageAsync();
            }
            else if (inner is BackupDataProtectionException protectionException)
            {
                return ShowBackupProtectionErrorMessageAsync(protectionException);
            }
            else
            {
                return ShowBackupBuildDefaultErrorMessageAsync(buildingException.Message);
            }
        }

        private Task ShowBackupParsingErrorMessageAsync(BackupParsingException parsingException)
        {
            Exception inner = parsingException.InnerException;

            if (inner is NotBackupPackageException)
            {
                return ShowNotBackupPackageErrorMessageAsync();
            }
            else if (inner is UnknownBackupProtectionException)
            {
                return ShowUnknownBackupProtectionErrorMessageAsync();
            }
            else if (inner is BackupDataProtectionException protectionException)
            {
                return ShowBackupProtectionErrorMessageAsync(protectionException);
            }
            else if (inner is BackupParsingException innerParsingException)
            {
                return ShowBackupParsingErrorMessageAsync(innerParsingException);
            }
            else
            {
                return ShowBackupParsingDefaultErrorMessageAsync(parsingException.Message);
            }
        }

        private Task ShowBackupProtectionErrorMessageAsync(BackupDataProtectionException protectionException)
        {
            if (protectionException.InnerException is BackupVerificationException corruptedBackup)
            {
                return ShowBackupVerificationErrorMessageAsync();
            }
            else
            {
                return ShowBackupProtectionErrorMessageAsync();
            }
        }

        private async Task ShowDefaultErrorMessageAsync(Exception exception)
        {
            var title = Loader.GetString("SendErrorReportTitle");
            var message = $"{exception.Message} \n {exception.StackTrace}";
            if (exception.InnerException != null)
            {
                message += $"\n {exception.InnerException.Message} \n {exception.InnerException.StackTrace}";
            }

            if (await ShowErrorMessageAsync(title, message, Loader.GetString("MsgBtnOk"), Loader.GetString("MessageButtonCancel")).ConfigureAwait(true))
            {
                SendErrorReport(message);
            }
        }

        private void SendErrorReport(string message)
        {
            var brand = new Models.BrandLoader();
            var navigationService = GetNavigationService();
            var messageData = new ErrorReportNewMessageData(brand.GetSupport(), Loader.GetString("ErrorReportEmailTitle"), message);
            navigationService?.Navigate(nameof(NewMessagePageViewModel), messageData);
        }

        public Task ShowEnableImapMessageAsync(string forEmail)
        {
            return ShowInfoMessageAsync(
                Loader.GetString("FailedToAddAccountTitle"),
                string.Format(Loader.GetString("EnableImapMessage"), forEmail),
                Loader.GetString("MsgBtnOk"));
        }
        public Task ShowAddAccountMessageAsync()
        {
            return ShowInfoMessageAsync(
                Loader.GetString("CantSendMessages"),
                Loader.GetString("AddEmailAccountFirst"),
                Loader.GetString("MsgBtnOk"));
        }
        public Task ShowSeedPhraseNotValidMessageAsync()
        {
            return ShowInfoMessageAsync(
                Loader.GetString("SeedPhraseNotValidTitle"),
                Loader.GetString("SeedPhraseNotValidMessage"),
                Loader.GetString("SeedPhraseNotValidAction"));
        }
        public Task ShowPgpPublicKeyAlreadyExistMessageAsync(string fileName = "")
        {
            string message = Loader.GetString("PgpKeyAlreadyExistMessage");

            if (!string.IsNullOrEmpty(fileName))
            {
                string fromFile = Loader.GetString("PgpKeyImportFromFileMessage");
                message = $"{message}\n{fromFile}: '{fileName}'";
            }

            return ShowInfoMessageAsync(
                Loader.GetString("PgpKeyImportErrorMessageTitle"),
                message,
                Loader.GetString("MsgBtnOk"));
        }
        public Task ShowPgpUnknownPublicKeyAlgorithmMessageAsync(string fileName = "")
        {
            string message = Loader.GetString("PgpKeyAlgorithmUnknownMessage");

            if (!string.IsNullOrEmpty(fileName))
            {
                string fromFile = Loader.GetString("PgpKeyImportFromFileMessage");
                message = $"{message}\n{fromFile}: '{fileName}'";
            }

            return ShowInfoMessageAsync(
                Loader.GetString("PgpKeyImportErrorMessageTitle"),
                message,
                Loader.GetString("MsgBtnOk"));
        }
        public Task ShowPgpPublicKeyImportErrorMessageAsync(string detailedReason, string fileName = "")
        {
            string message = Loader.GetString("PgpKeyImportErrorMessage");

            if (string.IsNullOrEmpty(fileName))
            {
                message = $"{message}\n{detailedReason}";
            }
            else
            {
                string fromFile = Loader.GetString("PgpKeyImportFromFileMessage");
                message = $"{message}\n{fromFile}: '{fileName}'\n{detailedReason}";
            }

            return ShowInfoMessageAsync(
                Loader.GetString("PgpKeyImportErrorMessageTitle"),
                message,
                Loader.GetString("MsgBtnOk"));
        }
        private Task ShowPgpExportPublicKeyErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                Loader.GetString("PgpKeyExportErrorMessageTitle"),
                $"{Loader.GetString("PgpKeyExportErrorMessage")}\n{detailedReason}",
                Loader.GetString("MsgBtnOk"));
        }
        private Task ShowNoSecretKeyErrorMessageAsync(string keyId)
        {
            return ShowInfoMessageAsync(
                Loader.GetString("CryptoContextErrorMessageTitle"),
                $"{Loader.GetString("NoSecretKeyMessage")}\nID: {keyId}",
                Loader.GetString("MsgBtnOk"));
        }
        private Task ShowNoPublicKeyErrorMessageAsync(string keyId)
        {
            return ShowInfoMessageAsync(
                Loader.GetString("CryptoContextErrorMessageTitle"),
                $"{Loader.GetString("NoPublicKeyMessage")}\nEmail: {keyId}",
                Loader.GetString("MsgBtnOk"));
        }
        private Task ShowEncryptionErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                Loader.GetString("CryptoContextErrorMessageTitle"),
                $"{Loader.GetString("EncryptionErrorMessage")}\n{detailedReason}",
                Loader.GetString("MsgBtnOk"));
        }
        private Task ShowSigningErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                Loader.GetString("CryptoContextErrorMessageTitle"),
                $"{Loader.GetString("SigningErrorMessage")}\n{detailedReason}",
                Loader.GetString("MsgBtnOk"));
        }
        private Task ShowDecryptionErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                Loader.GetString("CryptoContextErrorMessageTitle"),
                $"{Loader.GetString("DecryptionErrorMessage")}\n{detailedReason}",
                Loader.GetString("MsgBtnOk"));
        }
        private Task ShowSignatureVerificationErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                Loader.GetString("CryptoContextErrorMessageTitle"),
                $"{Loader.GetString("SignatureVerificationErrorMessage")}\n{detailedReason}",
                Loader.GetString("MsgBtnOk"));
        }
        private Task ShowBackupDefaultErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                    Loader.GetString("BackupErrorMessageTitle"),
                    detailedReason,
                    Loader.GetString("MsgBtnOk"));
        }
        private Task ShowBackupBuildDefaultErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                    Loader.GetString("BackupBuildErrorMessageTitle"),
                    detailedReason,
                    Loader.GetString("MsgBtnOk"));
        }
        private Task ShowBackupParsingDefaultErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                    Loader.GetString("BackupParsingErrorMessageTitle"),
                    detailedReason,
                    Loader.GetString("MsgBtnOk"));
        }
        private Task ShowBackupSerializationErrorMessageAsync()
        {
            return ShowInfoMessageAsync(
                    Loader.GetString("BackupBuildErrorMessageTitle"),
                    Loader.GetString("BackupSerializationErrorMessage"),
                    Loader.GetString("MsgBtnOk"));
        }
        private Task ShowNotBackupPackageErrorMessageAsync()
        {
            return ShowInfoMessageAsync(
                    Loader.GetString("BackupParsingErrorMessageTitle"),
                    Loader.GetString("NotBackupPackageErrorMessage"),
                    Loader.GetString("MsgBtnOk"));
        }
        private Task ShowUnknownBackupProtectionErrorMessageAsync()
        {
            return ShowInfoMessageAsync(
                    Loader.GetString("BackupParsingErrorMessageTitle"),
                    Loader.GetString("UnknownBackupProtectionErrorMessage"),
                    Loader.GetString("MsgBtnOk"));
        }
        private Task ShowBackupProtectionErrorMessageAsync()
        {
            return ShowInfoMessageAsync(
                    Loader.GetString("BackupProtectionErrorMessageTitle"),
                    Loader.GetString("BackupProtectionErrorMessage"),
                    Loader.GetString("MsgBtnOk"));
        }
        private Task ShowBackupVerificationErrorMessageAsync()
        {
            return ShowInfoMessageAsync(
                    Loader.GetString("BackupParsingErrorMessageTitle"),
                    Loader.GetString("BackupVerificationErrorMessage"),
                    Loader.GetString("MsgBtnOk"));
        }

        private Task ShowCoreErrorMessageAsync(CoreException coreException)
        {
            if (coreException is NewMessagesCheckFailedException newMessagesCheckFailedException)
            {
                return ShowNewMessagesCheckFailedErrorMessageAsync(newMessagesCheckFailedException);
            }
            else
            {
                return ShowDefaultErrorMessageAsync(coreException);
            }
        }

        private Task ShowNewMessagesCheckFailedErrorMessageAsync(NewMessagesCheckFailedException newMessagesCheckFailedException)
        {
            string message = string.Empty;
            if (newMessagesCheckFailedException.ErrorsCollection.Count > 0)
            {
                message = Loader.GetString("FailedToReceiveNewMessagesFromErrorMessage");
                foreach (var email in newMessagesCheckFailedException.ErrorsCollection.Select(e => e.Email.Address).Distinct())
                {
                    message += $"\n {email}";
                }

                foreach (var exception in newMessagesCheckFailedException.ErrorsCollection.Select(e => e.Exception).Distinct())
                {
                    message += $"\n\n {exception.Message} \n {exception.StackTrace}";
                }
            }
            else
            {
                message = Loader.GetString("FailedToReceiveNewMessagesErrorMessage");
            }

            if (newMessagesCheckFailedException.InnerException != null)
            {
                message += $"\n{newMessagesCheckFailedException.InnerException.Message} \n{newMessagesCheckFailedException.InnerException.StackTrace}";
            }

            return ShowInfoMessageAsync(
                Loader.GetString("NewMessagesCheckFailedErrorTitle"),
                message,
                Loader.GetString("MsgBtnOk"));
        }

        public Task<bool> ShowWipeAllDataDialogAsync()
        {
            return ShowDialogAsync(
                Loader.GetString("WipeAllDataDialogTitle"),
                Loader.GetString("WipeAllDataDialogMessage"),
                Loader.GetString("WipeAllDataDialogAcceptText"),
                Loader.GetString("WipeAllDataDialogRejectText"));
        }
        public Task<bool> ShowRemoveAccountDialogAsync()
        {
            return ShowDialogAsync(
                Loader.GetString("RemoveAccountDialogTitle"),
                Loader.GetString("RemoveAccountDialogMessage"),
                Loader.GetString("RemoveAccountDialogAcceptText"),
                Loader.GetString("RemoveAccountDialogRejectText"));
        }

        public Task ShowNeedToCreateSeedPhraseMessageAsync()
        {
            return ShowInfoMessageAsync(
                Loader.GetString("SeedPhraseNotInitializedTitle"),
                Loader.GetString("SeedPhraseNotInitializedMessage"),
                Loader.GetString("MsgBtnOk"));
        }
    }
}
