using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    //Handle broadcast type of messages sent from client
    public interface POIBroadcastClientMsgCB
    {
        Tuple<bool, bool> getStateVariables();
        void resetStateVariables();
        void broadcastBeginAckMsgReceived(ref BroadcastBeginAckPar par);
        void broadcastEndAckMsgReceived(ref BroadcastEndAckPar par);
        void broadcastRequestMissingPacketMsgReceived(ref BroadcastRequestMissingPacketPar par);
    }

    public interface POIInitializeClientMsgCB
    {
        void helloMsgReceived(ref HelloPar par);
    }

    //Handle broadcast type of message sent from server
    public interface POIBroadcastServerMsgCB
    {
        void broadcastBeginMsgReceived(ref BroadcastBeginPar par);
        void broadcastEndMsgReceived(ref BroadcastEndPar par);
        void broadcastContentMsgReceived(ref BroadcastContentPar par);
    }

    //Handle push and pull message sent from client
    public interface POIPushPullClientMsgCB
    {
        void pullMsgReceived(ref PullPar par);
        void pushMsgReceived(ref PushPar par, byte[] data);
    }

    //Handle Presentation control messages
    public interface POIPresentationControlMsgCB
    {
        void presCtrlMsgReceived(ref PresentationControlPar par);
    }

    public interface POIRealtimeMsgCB
    {
        void Handle_Scale(ref PinchGestureData pinchData);
        void Handle_Swipe(ref SwipeGestureData data);
        void Handle_Rotate(ref RotationGestureData rotationData);

        unsafe void Handle_Keyboard(byte[] keyboardData);

        unsafe void Handle_TouchBegin(byte* touchData);
        unsafe void Handle_TouchEnd(byte* touchData);
        unsafe void Handle_TouchMove(byte* touchData);
    }
}
