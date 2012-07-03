using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

namespace POILibCommunication
{
    public class POIUserEventArgs : EventArgs
    {
        private POIUser myUser;
        private double myX;
        private double myY;

        public POIUser User { get { return myUser; } }
        public double OffsetX { get { return myX; } }
        public double OffsetY { get { return myY; } }

        public POIUserEventArgs(POIUser user, Point myOffset)
        {
            myUser = user;
            myX = myOffset.X;
            myY = myOffset.Y;
        }
    }

    public class POIRotateEventArgs : EventArgs
    {
        private POIUser myUser;
        public POIUser User { get { return myUser; } }

        private double myDegree;
        private double myVelocity;
        public double Degree { get { return myDegree; } }
        public double Velocity { get { return myVelocity; } }

        public POIRotateEventArgs(double degree, double velocity, POIUser curUser)
        {
            myDegree = degree;
            myVelocity = velocity;
            myUser = curUser;
        }
    }

    public class POIZoomEventArgs : EventArgs
    {
        private double scaleFactor;
        public double ScaleFactor { get { return scaleFactor; } }

        private POIUser myUser;
        public POIUser User { get { return myUser; } }

        private double myVelocity;
        public double Velocity { get { return myVelocity; } }

        public POIZoomEventArgs(double scale, double velocity, POIUser curUser)
        {
            scaleFactor = scale;
            myVelocity = velocity;
            myUser = curUser;
        }

    }

    public class POIScrollEventArgs : EventArgs
    {
        private POIUser myUser;
        public POIUser User { get { return myUser; } }

        public POIScrollEventArgs(POIUser curUser)
        {
            myUser = curUser;
        }
    }

    public class POISwipeEventArgs : EventArgs
    {
        public enum SwipeDirection { UP, DOWN, LEFT, RIGHT };
        private SwipeDirection myDirection;

        public SwipeDirection Direction { get { return myDirection; } set { myDirection = value; } }

        private POIUser myUser;
        public POIUser User { get { return myUser; } }

        public POISwipeEventArgs(int curDirection, POIUser curUser)
        {
            myDirection = (SwipeDirection)curDirection;
            myUser = curUser;
        }
    }

    public class POIInputEventArgs : EventArgs
    {
        private char myChar;
        public char InputKey { get { return myChar; } }

        private POIUser myUser;
        public POIUser User { get { return myUser; } }

        public POIInputEventArgs(char curKey, POIUser curUser)
        {
            myChar = curKey;
            myUser = curUser;
        }
    }

    public class POIDoubleTapEventArgs: EventArgs
    {
        private POIUser myUser;
        public POIUser User { get { return myUser; } }

        public POIDoubleTapEventArgs(POIUser curUser)
        {
            myUser = curUser;
        }
    }

    public class POITapEventArgs : EventArgs
    {
        private POIUser myUser;
        public POIUser User { get { return myUser; } }

        public POITapEventArgs(POIUser curUser)
        {
            myUser = curUser;
        }
    }

    public class POITouchMoveEventArgs: EventArgs
    {
        private POIUser myUser;
        public POIUser User { get { return myUser; } }

        public POITouchMoveEventArgs(POIUser curUser)
        {
            myUser = curUser;
        }
    }


}
