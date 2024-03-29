﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public class POIMsgDelegateContainer
    {
        public POIBroadcastServerMsgCB BroadcastServerCtrlDelegate { get; set; }
        public POIBroadcastClientMsgCB BroadcastCtrlHandler { get; set; }

        public POIPushPullClientMsgCB pushPullClientMsgDelegate { get; set; }
        public POIPresentationControlMsgCB PresCtrlHandler { get; set; }
        public POIRealtimeMsgCB RealtimeCtrlHandler { get; set; }
        public POIWhiteBoardMsgCB WhiteboardCtrlHandler { get; set; }
        public POICommentCB CommentHandler { get; set; }
        public POISessionCtrlMsgCB SessionHandler { get; set; }
        public POIPointerCtrlMsgCB PointerHandler { get; set; }
        public POIAudioContentMsgCB AudioContentHandler { get; set; }
    }
}
