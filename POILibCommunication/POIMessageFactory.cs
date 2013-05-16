using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public class POIMessageFactory
    {
        Dictionary<int, Type> msgTypeMap = new Dictionary<int, Type>();

        private POIMessageFactory()
        {
            RegisterMsgTypes();
        }

        private void RegisterMsgTypes()
        {
            msgTypeMap[POIMsgDefinition.POI_PRESENTATION_CONTENT] = typeof(POIPresentation);
            msgTypeMap[POIMsgDefinition.POI_PRESENTATION_CONTROL] = typeof(POIPresCtrlMsg);
            msgTypeMap[POIMsgDefinition.POI_WHITEBOARD_CONTROL] = typeof(POIWhiteboardMsg);
            msgTypeMap[POIMsgDefinition.POI_USER_COMMENTS] = typeof(POIComment);
            msgTypeMap[POIMsgDefinition.POI_SESSION_CONTROL] = typeof(POISessionMsg);
            msgTypeMap[POIMsgDefinition.POI_POINTER_CONTROL] = typeof(POIPointerMsg);
            msgTypeMap[POIMsgDefinition.POI_HELLO] = typeof(POIHelloMsg);
            msgTypeMap[POIMsgDefinition.POI_WELCOME] = typeof(POIWelcomeMsg);
        }

        private static POIMessageFactory sharedInstance;

        //Accessing method to get the singleton instance
        public static POIMessageFactory Instance 
        {
            get 
            {
                if (sharedInstance == null)
                {
                    sharedInstance = new POIMessageFactory();
                }
                
                return sharedInstance;
            }
        }

        //For creating message using the factory method
        public POIMessage CreateMessage(int msgTypeByte)
        {
            Type msgType = msgTypeMap[msgTypeByte];
            POIMessage message = null;
            if (msgType != null)
            {
                message = Activator.CreateInstance(msgType) as POIMessage;
            }

            return message;
        }

    }
}
