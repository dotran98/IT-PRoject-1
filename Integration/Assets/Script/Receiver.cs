using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using OpenDis.Core;
using UnityEngine;

public class Receiver : MonoBehaviour
{
    private static EndPoint remoteEP;

    private static Socket mcastSocket;

    private static int broadcastPort;

    private Manager manager;

    public GameObject Bird;

    private static void StartBroadcast()
    {
        try
        {
            remoteEP = (EndPoint) new IPEndPoint(IPAddress.Any, broadcastPort);

            mcastSocket =
                new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram,
                    ProtocolType.Udp);

            mcastSocket.ExclusiveAddressUse = false;

            mcastSocket
                .SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress,
                true);

            mcastSocket.Bind (remoteEP);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private void ReceiveBroadcastMessages()
    {
        bool done = false;
        byte[] bytes = new Byte[10000];
        int length = 0;

        PduProcessor pduProcessor = new PduProcessor(); //PduProcessor could use some work and move towards all static methods (PduXmlDecode is instance method)
        pduProcessor.Endian = Endian.Big; //DIS use Big Endian format
        List<object> pduList;

        try
        {
            while (!done)
            {
                length = mcastSocket.ReceiveFrom(bytes, ref remoteEP);
                pduList = pduProcessor.ProcessPdu(bytes, Endian.Big);
                foreach (object pduObj in pduList)
                {
                    //Check data
                    StringBuilder dataTrunk = pduProcessor.XmlDecodePdu(pduObj);

                    //Process data
                    XDocument dataBuffer =
                        XDocument.Parse(dataTrunk.ToString());
                    XElement root = dataBuffer.Element("EntityStatePdu");
                    int entityID =
                        Int32
                            .Parse(root
                                .Element("entityID")
                                .Element("entity")
                                .Value);
                    Vector2 velocity =
                        new Vector2(float
                                .Parse(root
                                    .Element("entityLinearVelocity")
                                    .Element("x")
                                    .Value),
                            float
                                .Parse(root
                                    .Element("entityLinearVelocity")
                                    .Element("y")
                                    .Value));

                    double[] xyz =
                    {
                        Double
                            .Parse(root
                                .Element("entityLocation")
                                .Element("x")
                                .Value),
                        Double
                            .Parse(root
                                .Element("entityLocation")
                                .Element("y")
                                .Value),
                        Double
                            .Parse(root
                                .Element("entityLocation")
                                .Element("z")
                                .Value)
                    };

                    double[] latlongal =
                        CoordinateConversions.xyzToLatLonRadians(xyz);

                    Vector2 location =
                        GPSEncoder
                            .GPSToUCS((float) latlongal[0],
                            (float) latlongal[1]);
                    Debug.Log("Done there");
                    if (manager.birdFlock.ContainsKey(entityID))
                    {
                        GameObject bird = manager.birdFlock[entityID];
                        bird.GetComponent<Rigidbody2D>().velocity = velocity;
                        bird.GetComponent<Rigidbody2D>().position = location;
                    }
                    else
                    {
                        GameObject bird =
                            Instantiate(Bird, location, Quaternion.identity) as
                            GameObject;
                        bird.GetComponent<Rigidbody2D>().velocity = velocity;
                        manager.birdFlock.Add (entityID, bird);
                    }
                }
            }

            mcastSocket.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        manager = new Manager();
        broadcastPort = 62040;
        StartBroadcast();
    }

    void Update()
    {
        ReceiveBroadcastMessages();
    }
}
