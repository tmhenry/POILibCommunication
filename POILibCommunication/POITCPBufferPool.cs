using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace POILibCommunication
{
    //A singleton for TCP buffer space allocate and free
    public class POITCPBufferPool
    {
        //non-static data members
        byte[] bufferSpace = null;
        int singleBufferSize;
        int maxNumberOfBuffer;

        List<SocketAsyncEventArgs> eventArgsQueue;
        Semaphore evArgPool;
        Mutex queueLock;

        //Static variables
        private static POITCPBufferPool instance = null;

        public static POITCPBufferPool Instance
        {
            get 
            { 
                if(instance == null)
                {
                    instance = new POITCPBufferPool();
                }

                return instance;
            }
        }

        private POITCPBufferPool()
        {
            //Allocate the whole buffer space
            maxNumberOfBuffer = 500;
            singleBufferSize = 2000;

            long bufferSpaceSize = maxNumberOfBuffer * (long) singleBufferSize;
            bufferSpace = new byte[bufferSpaceSize];

            //Pre-allocate a list of event args 
            eventArgsQueue = new List<SocketAsyncEventArgs>(maxNumberOfBuffer);
            for (int i = 0; i < maxNumberOfBuffer; i++)
            {
                SocketAsyncEventArgs evArg = new SocketAsyncEventArgs();
                evArg.SetBuffer(bufferSpace, i * singleBufferSize, singleBufferSize);
                eventArgsQueue.Add(evArg);

                //Set the user token
                POISocketAsyncUserToken token = new POISocketAsyncUserToken();
                token.BufferSize = singleBufferSize;
                evArg.UserToken = token;
                
            }

            evArgPool = new Semaphore(maxNumberOfBuffer, maxNumberOfBuffer);
            queueLock = new Mutex();
        }

        public static SocketAsyncEventArgs AllocEventArg()
        {
            //Wait for available event arg
            Instance.evArgPool.WaitOne();

            //Pop the first available event arg for use
            Instance.queueLock.WaitOne();
            SocketAsyncEventArgs arg = Instance.eventArgsQueue.ElementAt(0);
            Instance.eventArgsQueue.RemoveAt(0);
            Instance.queueLock.ReleaseMutex();

            return arg;
        }

        public static void FreeEventArg(SocketAsyncEventArgs arg)
        {
            //Return the event arg to the queue
            Instance.queueLock.WaitOne();
            Instance.eventArgsQueue.Add(arg);
            Instance.queueLock.ReleaseMutex();

            Instance.evArgPool.Release();
        }

        public static void InitPool()
        {
            instance = new POITCPBufferPool();
        }
    }

    public class POISocketAsyncUserToken
    {
        public int BufferSize { get; set; }
        public int PayloadReceived { get; set; }
        public int HeaderReceived { get; set; }
        public int HeaderSize { get; set; }
        public int PayloadSize { get; set; }
    }
}
