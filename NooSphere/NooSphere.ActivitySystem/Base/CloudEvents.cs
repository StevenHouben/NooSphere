using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.ActivitySystem
{
    public enum CloudEvents
    {
        ActivityAdded,
        ActivityUpdated,
        ActivityDeleted,
        FileDownload,
        FileUpload,
        FileDeleted,
        FriendRequest,
        FriendAdded,
        FriendDeleted,
        Message,
        ParticipantAdded,
        ParticipantRemoved
    }
}
