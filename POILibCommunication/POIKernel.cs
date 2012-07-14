using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public interface POIKernel
    {
        void HandleUserJoin(POIUser user);
        void SetHandlersForUser(POIUser user);
        void HandleUserLeave(POIUser user);

        //void Handle_Push(ref PushPar par, byte[] data);
        //void Handle_Pull(ref PullPar par);
    }
}
