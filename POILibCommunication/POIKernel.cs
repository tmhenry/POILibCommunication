using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public class POIKernel
    {
        //Define all the events that need to be handled
        virtual public void Handle_UserJoin(POIUserEventArgs userEA) { }
        virtual public void Handle_UserLeave(POIUserEventArgs userEA) { }

        //virtual public void Handle_UserMove(POIUserEventArgs userEA, DateTime time) { }
        //virtual public void Handle_UserTap(POIUIUser user) { }
        //virtual public void Handle_UserSwipe(POISwipeEventArgs arg) { }

        virtual public void Handle_Push(ref PushPar par, byte [] data) { }
        virtual public void Handle_Pull(ref PullPar par) { }
    }
}
