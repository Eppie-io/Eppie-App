using Tuvi.App.ViewModels.Services;
using Tuvi.Core.Entities;

// ToDo: Change namespace
namespace Tuvi.App.Converters
{
    public class AttachmentConverter : JoinConverter<Attachment, IFileOperationProvider>
    { }
}
