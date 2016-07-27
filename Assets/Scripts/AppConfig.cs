using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;

public static class AppConfig {
	private static string path = Path.Combine(Application.dataPath, "config.ini");
	public static class Settings {
		public static class Backend {
			public static string ip_address = "localhost";
			public static string port = "8084";

			//need this to force load
			static Backend() {
				var foo = path;
			}
		}
		public static class Frontend {
			public static string xml_location = "Assets/xml/roman_empire_1000.xml";

			//need this to force load
			static Frontend() {
				var foo = path;
			}
		}
	}

	static AppConfig() {
		//load INI configuration
		if (File.Exists(path)) {
			using (StreamReader sr = new StreamReader(path)) {
				string line;
				string theSection = "";
				string theKey = "";
				string theValue = "";
				while ((line = sr.ReadLine()) != null) {
					if (string.IsNullOrEmpty(line)) {
						continue;
					}
					line.Trim();
					if (line.StartsWith("[") && line.EndsWith("]")) {
						theSection = line.Substring(1, line.Length - 2);
						continue;
					}
					else {
						string[] ln = line.Split(new char[] { '=' });
						theKey = ln[0].Trim();
						theValue = ln[1].Trim();
					}
					if (theSection == "" || theKey == "" || theValue == "")
						continue;

					Type thisType = Type.GetType("AppConfig+Settings+"+theSection);
					thisType.GetField(theKey).SetValue(thisType, theValue);
				}
				Debug.Log("Configuration file loaded.");
			}
		}
		else {
			Debug.LogError("ERROR: unable to read config file: " + path);
		}
	}

	
}