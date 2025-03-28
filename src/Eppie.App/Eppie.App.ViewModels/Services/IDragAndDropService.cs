using System.Collections.Generic;
using Tuvi.App.ViewModels;

namespace Eppie.App.ViewModels.Services
{
    public interface IDragAndDropService
    {
        IReadOnlyList<MessageInfo> GetDraggedMessages();
        void SetDraggedMessages(IReadOnlyList<MessageInfo> messages);
    }
}
