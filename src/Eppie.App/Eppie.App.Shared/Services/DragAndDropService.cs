using System.Collections.Generic;
using Eppie.App.ViewModels.Services;
using Tuvi.App.ViewModels;

namespace Eppie.App.Shared.Services
{
    public class DragAndDropService : IDragAndDropService
    {
        private static IReadOnlyList<MessageInfo> DraggedMessages { get; set; }

        public IReadOnlyList<MessageInfo> GetDraggedMessages()
        {
            return DraggedMessages;
        }

        public void SetDraggedMessages(IReadOnlyList<MessageInfo> messages)
        {
            DraggedMessages = messages;
        }
    }
}
