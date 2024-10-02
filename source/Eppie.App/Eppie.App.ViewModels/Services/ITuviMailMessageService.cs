using System.Threading.Tasks;

namespace Tuvi.App.ViewModels.Services
{
    public interface ITuviMailMessageService : IMessageService
    {
        Task ShowEnableImapMessageAsync(string forEmail);
        Task ShowAddAccountMessageAsync();

        Task ShowSeedPhraseNotValidMessageAsync();

        Task ShowPgpPublicKeyAlreadyExistMessageAsync(string fileName);
        Task ShowPgpUnknownPublicKeyAlgorithmMessageAsync(string fileName);
        Task ShowPgpPublicKeyImportErrorMessageAsync(string detailedReason, string fileName);

        Task<bool> ShowWipeAllDataDialogAsync();
        Task<bool> ShowRemoveAccountDialogAsync();

        Task ShowNeedToCreateSeedPhraseMessageAsync();
    }
}
