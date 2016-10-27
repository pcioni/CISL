//
//	  UnityOSC - Example of usage for OSC receiver
//
//	  Copyright (c) 2012 Jorge Garcia Martin
//
// 	  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// 	  documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// 	  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// 	  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// 	  The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// 	  of the Software.
//
// 	  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// 	  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// 	  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// 	  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// 	  IN THE SOFTWARE.
//

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityOSC;

public class OSCController : MonoBehaviour {

	private Dictionary<string, ServerLog> servers;

	private bool audioFinalized = false;
	private bool audioInitialized = false;

	// Script initialization
	void Start() {	
		OSCHandler.Instance.Init(); //init OSC
		servers = new Dictionary<string, ServerLog>();

        OSCHandler.Instance.SendMessageToClient("SpeechToTextSend", "start", 1.0f);
        OSCHandler.Instance.SendMessageToClient("SpeechToTextSend", "next", 1.0f);
    }

    // NOTE: The received messages at each server are updated here
    // Hence, this update depends on your application architecture
    // How many frames per second or Update() calls per frame?

    long lastMessageTime = -1;
    string lastMessage = "";
    
    void Update() {

		OSCHandler.Instance.UpdateLogs();
		servers = OSCHandler.Instance.Servers;

		foreach( KeyValuePair<string, ServerLog> item in servers )
		{
            // If we have received at least one packet,
            // show the last received from the log in the Debug console
            if (item.Value.log.Count > 0) 
			{
				int lastPacketIndex = item.Value.packets.Count - 1;

                if (lastMessageTime == item.Value.packets[lastPacketIndex].TimeStamp) continue;

                long messagetime = item.Value.packets[lastPacketIndex].TimeStamp;

                long dt = (messagetime - lastMessageTime) / 6666; // TODO: figure out what this unit is

                lastMessageTime = messagetime;

                string message = item.Value.packets[lastPacketIndex].Address;

                if (dt < 3000 && lastMessage == message) {
                    continue;
                }

                lastMessage = message;

                print("OSCController.Update() :: new message = " + message);

                //Debug.Log(string.Format("server: {0} address: {1}",
                //    item.Key, // server name
                //    item.Value.packets[lastPacketIndex].Address)); // osc address
                //print(item.Value.packets[lastPacketIndex].TimeStamp);

                EventManager.TriggerEvent(EventManager.EventType.OSC_SPEECH_INPUT, item.Value.packets[lastPacketIndex].Address);
            }
        }

		// handle audio keypresses
		bool shiftDown = (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift));
		// ESCAPE -OR- SHIFT + A + F
		bool finalizeAudio = ( Input.GetKeyDown(KeyCode.Escape) || (shiftDown && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.F)) );
		// SHIFT + A + I
		bool initAudio = ( shiftDown && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.I) );

		if (finalizeAudio) {
			OSCHandler.Instance.SendMessageToClient ("MaxServer", "/finalize/", 1.0f);
			Debug.Log ("QuitOnEscape.Update() :: finalizing audio");
			audioFinalized = true;
		} else {
			audioFinalized = false;
		}

		if (initAudio) {
			OSCHandler.Instance.SendMessageToClient ("MaxServer", "/initialize/", 1.0f);
			Debug.Log ("QuitOnEscape.Update() :: initializing audio");	
			audioInitialized = true;
		} else {
			audioInitialized = false;
		}

	}
}
