using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net; 
using System.Net.Sockets;
using System.Threading;

public class SocketListener : MonoBehaviour {
	//SocketPermission permission;
	Socket senderSock;
	IPEndPoint ipEndPoint;

	//this will be checked if Single Node is active
	public bool running_single=false;
	public GameObject nodeprefab;
	
	byte[] bytes = new byte[1024]; 
	
	//public GameObject loader_object;
	// Use this for initialization
	void Awake () 
	{
		//Find the loader game object.
		//Find the game object with name "loader"
		//loader_object = GameObject.Find ("loader");
		initializeConnection ();	

	}

	public void initializeConnection()
	{
		print ("test");
		
		// Gets first IP address associated with a localhost 
		IPAddress ipAddr = IPAddress.Parse("127.0.0.1"); 
		//IPAddress ipAddr = IPAddress.Parse ("129.161.139.41");
		//IPAddress ipAddr = IPAddress.Parse ("129.161.70.248");
		//IPAddress ipAddr = IPAddress.Parse ("129.161.59.160");
		//IPAddress ipAddr = IPAddress.Parse ("129.161.58.145");

		// Creates a network endpoint 
		ipEndPoint = new IPEndPoint(ipAddr, 4510);
		
		// Create one Socket object to listen the incoming connection 
		senderSock = new Socket(
			ipAddr.AddressFamily,// Specifies the addressing scheme 
			SocketType.Stream,   // The type of socket  
			ProtocolType.Tcp     // Specifies the protocols  
			);
		
		senderSock.NoDelay = false;   // Using the Nagle algorithm 
		
		// Establishes a connection to a remote host 
		senderSock.Connect(ipEndPoint);
		
		print ("SSocket connected to " + ipEndPoint.Address + " port: " + ipEndPoint.Port);
		
		
		string theMessageToSend = "";
		byte[] msg = Encoding.Unicode.GetBytes(theMessageToSend + "<EOF>");
		//byte[] msg = Encoding.Unicode.GetBytes (theMessageToSend);
		
		// Sends data to a connected Socket. 
		//int bytesSend = senderSock.Send(msg);
		
		//ReceiveDataFromServer();

		//Start loader now that connection is made.

		//loader_object.GetComponent<Loader> ().Initialize ();
		gameObject.GetComponent<LoadXML>().Initialize();
	}//end method initializeConnection

	public void sendMessageToServer(String messageToSend)
	{
		print ("Sending message " + messageToSend + " to server");
		byte[] message = Encoding.Unicode.GetBytes (messageToSend + "<EOF>");
		int bytes_sent = 0;
		if (senderSock != null)
			try
			{	
				bytes_sent = senderSock.Send (message);
			}//end try
			catch (SocketException e)
			{
				print ("SocketException: " + e);
			}//end catch
	}//end method sendMessageToServer

	public String ReceiveDataFromServer()
	{
		if (senderSock == null)
			return "";
		// Receives data from a bound Socket. 
		int bytesRec = 0;
		try
		{
			bytesRec = senderSock.Receive(bytes);
		}//end try
		catch (SocketException e)
		{
			print ("SocketException: " + e);
			return "";
		}//end catch
		// Converts byte array to string 
		String theMessageToReceive = Encoding.Unicode.GetString(bytes, 0, bytesRec);


		// Continues to read the data till data isn't available 
		while (senderSock.Available > 0)
		{
			bytesRec = senderSock.Receive(bytes);
			theMessageToReceive += Encoding.Unicode.GetString(bytes, 0, bytesRec);
		}
		
		print ( "The server reply: " + theMessageToReceive);
		return theMessageToReceive;
	}
	
	// Update is called once per frame
	void Update () {
		//ReceiveDataFromServer ();
	}
}//end class SocketListener
