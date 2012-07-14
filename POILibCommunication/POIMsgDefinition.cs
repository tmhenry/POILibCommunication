using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    #region struct and enum definitions for Realtime messages
    public struct TouchOffset
    {
        public int xOffset;
        public int yOffset;
    };
    public enum TouchPhase
    {
        TOUCH_BEGIN,
        TOUCH_MOVE,
        TOUCH_END
    };
    public struct CGPoint
    {
        public float x;
        public float y;
    };
    public enum GestureType
    {
        PINCH,
        PAN,
        SWIPE,
        ROTATION,
        SCROLLING
    };
    public enum UISwipeGestureDirection
    {
        RIGHT = 1 << 0,
        LEFT = 1 << 1,
        UP = 1 << 2,
        DOWN = 1 << 3
    };
    public enum ScrollingGestureDirection
    {
        ScrollingRight = 1 << 0,
        ScrollingLeft = 1 << 1,
        ScrollingUp = 1 << 2,
        ScrollingDown = 1 << 3
    };
    public struct PinchGestureData
    {
        public float scale;
        public float velocity;
    };

    public struct RotationGestureData
    {
        public float rotation;
        public float velocity;
    };
    public struct ScrollingGestureData
    {
        public ScrollingGestureDirection direction;
        public float distance;
        public float velocity;

    };
    public struct SwipeGestureData
    {
        public UISwipeGestureDirection direction;
    };


    #endregion

    #region struct for broadcast

    public struct BroadcastBeginPar
    {
        public int frameNum;
        public int numPackets;
        public int packetPayload;
        public int lastPacketPayload;
    }

    public struct BroadcastEndPar
    {
        public int frameNum;
    }

    public struct BroadcastContentPar
    {
        public int frameNum;
        public int seqNum;
    }

    public struct BroadcastRequestMissingPacketPar
    {
        public int frameNum;
        public int seqNumStart;
        public int seqNumEnd;
    }

    public struct BroadcastBeginAckPar
    {
        public int frameNum;
    }

    public struct BroadcastEndAckPar
    {
        public int frameNum;
    }

    #endregion

    #region Struct for Initialization

    public struct HelloPar
    {
        public byte userType;
        public byte connectionType;
    }

    public struct WelcomePar
    {
        public int status;
    }

    #endregion

    #region Struct for push/pull

    public struct PushPar
    {
        public int type; //Image, website or PPT
        public int dataSize;
    }

    public struct PullPar
    {

    }

    #endregion

    #region Struct for Presentation Control

    public enum PresCtrlType
    {
        Next = 0,
        Prev
    }

    public struct PresentationControlPar
    {
        public int ctrlType;
        public int slideIndex;
    }

    #endregion


    static class POIMsgDefinition
    {
        //Global constants
        public const int POI_MAXPARAMETERSSIZE = 100;

        //Constants for initial handshaking
        public const int POI_HELLO                            = 0;
        public const int POI_WELCOME                          = 1;

        public const int POI_HELLOTYPE_VIEWER                 = 0;
        public const int POI_HELLOTYPE_COMMANDER              = 1;

        public const int POI_CONTROL_CHANNEL                  = 0;
        public const int POI_DATA_CHANNEL                     = 1;

        //Constants for push and pull
        public const int POI_PULL                             = 10;
        public const int POI_PUSH                             = 11;

        //Constants for broadcast
        public const int POI_BROADCASTBEGIN                   = 17;
        public const int POI_BROADCASTCONTENT                 = 18;
        public const int POI_BROADCASTEND                     = 19;
        public const int POI_BROADCASTREQUESTMISSINGPACKET    = 20;
        public const int POI_BROADCASTBEGINACK                = 21;
        public const int POI_BROADCASTENDACK                  = 22;

        //Constants for Presentation control
        public const int POI_PRESENTATION_CONTROL             = 30;

        //Constants for user comments
        public const int POI_USER_COMMENTS                    = 50;

        //Constants for realtime messages
        public const int POI_TOUCHBEGIN                       = 70;
        public const int POI_TOUCHMOVE                        = 71;
        public const int POI_TOUCHEND                         = 72;
        public const int POI_SCALE                            = 73;
        public const int POI_ROTATE                           = 74;

        //Constants for whiteboard control msg
        public const int POI_WHITEBOARD_SHOW                  = 90;
        public const int POI_WHITEBOARD_HIDE                  = 91;
    }
}
