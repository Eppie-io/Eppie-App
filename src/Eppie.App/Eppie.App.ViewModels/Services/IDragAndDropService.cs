using System;
using System.Collections.Generic;
using System.Text;
using Tuvi.App.ViewModels;

namespace Eppie.App.ViewModels.Services
{
    public interface IDragAndDropService
    {
        IReadOnlyList<MessageInfo> GetDraggedMessages();
        void SetDraggedMessages(IReadOnlyList<MessageInfo> messages);
    }
}
