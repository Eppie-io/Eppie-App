using System;
using System.Linq;
using System.Threading.Tasks;
using Eppie.App.Resources;
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
        private StringProvider StringProvider { get; } = StringProvider.GetInstance();

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
            var title = StringProvider.GetString("SendErrorReportTitle");

            var message = $"\n________________________________________________________________________________" +
                          $"\n {exception.TargetSite.Name} \n {exception}";

            if (exception.InnerException != null)
            {
                message += $"\n {exception.InnerException.TargetSite.Name} \n {exception.InnerException}";
            }

            if (await ShowErrorMessageAsync(title, message, StringProvider.GetString("MsgBtnOk"), StringProvider.GetString("MessageButtonCancel")).ConfigureAwait(true))
            {
                SendErrorReport(message);
            }
        }

        private void SendErrorReport(string message)
        {
            var brand = new Models.BrandLoader();
            var navigationService = (Application.Current as Eppie.App.Shared.App).NavigationService;
            var messageData = new ErrorReportNewMessageData(brand.GetSupport(), StringProvider.GetString("ErrorReportEmailTitle"), message);
            navigationService?.Navigate(nameof(NewMessagePageViewModel), messageData);
        }

        public Task ShowEnableImapMessageAsync(string forEmail)
        {
            return ShowInfoMessageAsync(
                StringProvider.GetString("FailedToAddAccountTitle"),
                string.Format(StringProvider.GetString("EnableImapMessage"), forEmail),
                StringProvider.GetString("MsgBtnOk"));
        }
        public Task ShowAddAccountMessageAsync()
        {
            return ShowInfoMessageAsync(
                StringProvider.GetString("CantSendMessages"),
                StringProvider.GetString("AddEmailAccountFirst"),
                StringProvider.GetString("MsgBtnOk"));
        }
        public Task ShowSeedPhraseNotValidMessageAsync()
        {
            return ShowInfoMessageAsync(
                StringProvider.GetString("SeedPhraseNotValidTitle"),
                StringProvider.GetString("SeedPhraseNotValidMessage"),
                StringProvider.GetString("SeedPhraseNotValidAction"));
        }
        public Task ShowPgpPublicKeyAlreadyExistMessageAsync(string fileName = "")
        {
            string message = StringProvider.GetString("PgpKeyAlreadyExistMessage");

            if (!string.IsNullOrEmpty(fileName))
            {
                string fromFile = StringProvider.GetString("PgpKeyImportFromFileMessage");
                message = $"{message}\n{fromFile}: '{fileName}'";
            }

            return ShowInfoMessageAsync(
                StringProvider.GetString("PgpKeyImportErrorMessageTitle"),
                message,
                StringProvider.GetString("MsgBtnOk"));
        }
        public Task ShowPgpUnknownPublicKeyAlgorithmMessageAsync(string fileName = "")
        {
            string message = StringProvider.GetString("PgpKeyAlgorithmUnknownMessage");

            if (!string.IsNullOrEmpty(fileName))
            {
                string fromFile = StringProvider.GetString("PgpKeyImportFromFileMessage");
                message = $"{message}\n{fromFile}: '{fileName}'";
            }

            return ShowInfoMessageAsync(
                StringProvider.GetString("PgpKeyImportErrorMessageTitle"),
                message,
                StringProvider.GetString("MsgBtnOk"));
        }
        public Task ShowPgpPublicKeyImportErrorMessageAsync(string detailedReason, string fileName = "")
        {
            string message = StringProvider.GetString("PgpKeyImportErrorMessage");

            if (string.IsNullOrEmpty(fileName))
            {
                message = $"{message}\n{detailedReason}";
            }
            else
            {
                string fromFile = StringProvider.GetString("PgpKeyImportFromFileMessage");
                message = $"{message}\n{fromFile}: '{fileName}'\n{detailedReason}";
            }

            return ShowInfoMessageAsync(
                StringProvider.GetString("PgpKeyImportErrorMessageTitle"),
                message,
                StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowPgpExportPublicKeyErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                StringProvider.GetString("PgpKeyExportErrorMessageTitle"),
                $"{StringProvider.GetString("PgpKeyExportErrorMessage")}\n{detailedReason}",
                StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowNoSecretKeyErrorMessageAsync(string keyId)
        {
            return ShowInfoMessageAsync(
                StringProvider.GetString("CryptoContextErrorMessageTitle"),
                $"{StringProvider.GetString("NoSecretKeyMessage")}\nID: {keyId}",
                StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowNoPublicKeyErrorMessageAsync(string keyId)
        {
            return ShowInfoMessageAsync(
                StringProvider.GetString("CryptoContextErrorMessageTitle"),
                $"{StringProvider.GetString("NoPublicKeyMessage")}\nEmail: {keyId}",
                StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowEncryptionErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                StringProvider.GetString("CryptoContextErrorMessageTitle"),
                $"{StringProvider.GetString("EncryptionErrorMessage")}\n{detailedReason}",
                StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowSigningErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                StringProvider.GetString("CryptoContextErrorMessageTitle"),
                $"{StringProvider.GetString("SigningErrorMessage")}\n{detailedReason}",
                StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowDecryptionErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                StringProvider.GetString("CryptoContextErrorMessageTitle"),
                $"{StringProvider.GetString("DecryptionErrorMessage")}\n{detailedReason}",
                StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowSignatureVerificationErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                StringProvider.GetString("CryptoContextErrorMessageTitle"),
                $"{StringProvider.GetString("SignatureVerificationErrorMessage")}\n{detailedReason}",
                StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowBackupDefaultErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                    StringProvider.GetString("BackupErrorMessageTitle"),
                    detailedReason,
                    StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowBackupBuildDefaultErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                    StringProvider.GetString("BackupBuildErrorMessageTitle"),
                    detailedReason,
                    StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowBackupParsingDefaultErrorMessageAsync(string detailedReason)
        {
            return ShowInfoMessageAsync(
                    StringProvider.GetString("BackupParsingErrorMessageTitle"),
                    detailedReason,
                    StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowBackupSerializationErrorMessageAsync()
        {
            return ShowInfoMessageAsync(
                    StringProvider.GetString("BackupBuildErrorMessageTitle"),
                    StringProvider.GetString("BackupSerializationErrorMessage"),
                    StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowNotBackupPackageErrorMessageAsync()
        {
            return ShowInfoMessageAsync(
                    StringProvider.GetString("BackupParsingErrorMessageTitle"),
                    StringProvider.GetString("NotBackupPackageErrorMessage"),
                    StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowUnknownBackupProtectionErrorMessageAsync()
        {
            return ShowInfoMessageAsync(
                    StringProvider.GetString("BackupParsingErrorMessageTitle"),
                    StringProvider.GetString("UnknownBackupProtectionErrorMessage"),
                    StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowBackupProtectionErrorMessageAsync()
        {
            return ShowInfoMessageAsync(
                    StringProvider.GetString("BackupProtectionErrorMessageTitle"),
                    StringProvider.GetString("BackupProtectionErrorMessage"),
                    StringProvider.GetString("MsgBtnOk"));
        }
        private Task ShowBackupVerificationErrorMessageAsync()
        {
            return ShowInfoMessageAsync(
                    StringProvider.GetString("BackupParsingErrorMessageTitle"),
                    StringProvider.GetString("BackupVerificationErrorMessage"),
                    StringProvider.GetString("MsgBtnOk"));
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
                message = StringProvider.GetString("FailedToReceiveNewMessagesFromErrorMessage");
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
                message = StringProvider.GetString("FailedToReceiveNewMessagesErrorMessage");
            }

            if (newMessagesCheckFailedException.InnerException != null)
            {
                message += $"\n{newMessagesCheckFailedException.InnerException.Message} \n{newMessagesCheckFailedException.InnerException.StackTrace}";
            }

            return ShowInfoMessageAsync(
                StringProvider.GetString("NewMessagesCheckFailedErrorTitle"),
                message,
                StringProvider.GetString("MsgBtnOk"));
        }

        public Task<bool> ShowWipeAllDataDialogAsync()
        {
            return ShowDialogAsync(
                StringProvider.GetString("WipeAllDataDialogTitle"),
                StringProvider.GetString("WipeAllDataDialogMessage"),
                StringProvider.GetString("WipeAllDataDialogAcceptText"),
                StringProvider.GetString("WipeAllDataDialogRejectText"));
        }
        public Task<bool> ShowRemoveAccountDialogAsync()
        {
            return ShowDialogAsync(
                StringProvider.GetString("RemoveAccountDialogTitle"),
                StringProvider.GetString("RemoveAccountDialogMessage"),
                StringProvider.GetString("RemoveAccountDialogAcceptText"),
                StringProvider.GetString("RemoveAccountDialogRejectText"));
        }

        public Task ShowNeedToCreateSeedPhraseMessageAsync()
        {
            return ShowInfoMessageAsync(
                StringProvider.GetString("SeedPhraseNotInitializedTitle"),
                StringProvider.GetString("SeedPhraseNotInitializedMessage"),
                StringProvider.GetString("MsgBtnOk"));
        }
    }
}
