/*#define sneighbour 1
#define stag 2
*/
using UnityEngine; 
using System.Collections; 
using System.IO; 
using System.Collections.Generic; 
using System; 
//using UnityEditor;





using System.Text;
using System.Runtime.InteropServices;




class Node{
	private string name;
	private Dictionary<int, string> parents;
	private Dictionary<int, string> neighbours;
	private Dictionary<string, string> tags;
	private string description;

	public Node(string str){
		this.name = str;	
		this.parents = new Dictionary<int, string>();
		this.neighbours = new Dictionary<int, string>();
		this.tags = new Dictionary<string,string> ();
		this.description = "";
	}
	public void update_name(string tmp){
		this.name = tmp;
	}
	public string get_name(){
		return this.name;
	}
	public void add_neighbour(int num, string str){
		this.neighbours.Add (num, str);
	}
	public void add_description(string str){
		this.description = str;
	}
	public void add_parent(int num, string str){
		this.parents.Add (num, str);
	}
	public void add_tag(string key, string value){
		this.tags.Add (key, value);
	}
	public Dictionary<int, string> get_neighbours(){
		return this.neighbours;
	}
	public Dictionary<int, string> get_parents(){
		return this.parents;
	}
	public Dictionary<string,string> get_tags(){
		return this.tags;
	}
	public void clear_tag(){
		this.tags.Clear ();
	}
	public void clear_parent(){
		this.parents.Clear ();
	}
	public void clear_neighbour(){
		this.neighbours.Clear ();
	}
	public string get_description(){
		return this.description;
	}
	public void update_index(int removed_index){
		Dictionary<int, string> tmp_dic=new Dictionary<int,string>();
		foreach (KeyValuePair<int,string> pair in this.neighbours) {
			if (pair.Key==removed_index) continue;
			if (pair.Key>removed_index){
				tmp_dic.Add (pair.Key-1,pair.Value);
			}
			else{
				tmp_dic.Add (pair.Key,pair.Value);
			}
		}
		this.neighbours = tmp_dic;
		
	}
}

public class Text : MonoBehaviour {
	bool warning=false;
	string default_name="";
	string search_value="";
	string search_key="";
	string last_search_value="";
	string last_search_key="";
	string newnode="";
	string new_name="";
	string filename="";
	string filename_="";
	string newnodename="";
	int root_id=-1;
	int status=-1;
	float max_height=Screen.height;
	public Vector2 scrollPosition=Vector2.zero;	
	public Vector2 scrollPosition2=Vector2.zero;	
	public Vector2 scrollPosition3=Vector2.zero;
	ArrayList nodes_name = new ArrayList();
	ArrayList all_lines;
	int index=-1;
	string tmp_description="";
	ArrayList en=new ArrayList();
	ArrayList re=new ArrayList();
	ArrayList tag_key=new ArrayList();
	ArrayList tag_value=new ArrayList();
	ArrayList tag_en=new ArrayList();
	ArrayList remove=new ArrayList();

	//public GUISkin skin; 


	string convert(string tmp){
		string tmp2 = "";
		for (int i=0; i<tmp.Length; i++) {
			if (tmp[i]=='\''){
				tmp2+="&quot;";
			}
			else{
				tmp2+=tmp[i];
			}
		}
		return tmp2;
	}

	string rconvert(string tmp){
		string tmp2 = "";
		int i = 0;
		while (i<tmp.Length) {
			if (i+5<tmp.Length && tmp[i]=='&' && tmp[i+1]=='q' && tmp[i+2]=='u' && tmp[i+3]=='o' && tmp[i+4]=='t' && tmp[i+5]==';'){
				tmp2+='\'';
				i+=6;
			}
			else{
				tmp2+=tmp[i];
				i+=1;
			}
		}
		return tmp2;
	}



	void Start () 
		
	{ 	
	} 

	
	void CreateFile(string path,string name,string info) 
		
	{ 
		
		StreamWriter sw; 
		FileInfo t = new FileInfo(path+ name); 
		
		if(!t.Exists) 
			
		{ 
			
			sw = t.CreateText(); 
			
		} 
		
		else 
			
		{ 
			sw = t.AppendText(); 
			
		} 
		
		sw.WriteLine(info); 
		sw.Close(); 
		sw.Dispose(); 
		
	}  
	
	
	void LoadFile(string path,string name, ref ArrayList nodes_name) 
		
	{ 
		StreamReader sr =null; 
		try{ 
			sr = File.OpenText(path+ name); 
			
		}
		catch(Exception e) 	
		{			print (e);
		} 
		
		string line; 
		

		ArrayList all_lines = new ArrayList(); 

		while ((line = sr.ReadLine()) != null) 
			
		{ 	
			all_lines.Add (line);
				
		} 
		for (int i=0; i<all_lines.Count; i++) {
			string tmp=(string)all_lines[i];
			if (tmp.IndexOf("<Root")!=-1){
				int pos1=0;
				int pos2=0;
				pos1=tmp.IndexOf("\"");
				pos2=tmp.IndexOf("\"",pos1+1);
				root_id=int.Parse(tmp.Substring(pos1+1,pos2-pos1-1));
			}
			if (tmp.IndexOf("<Feature")!=-1){
				int pos1=0;
				int pos2=0;
				pos2=tmp.LastIndexOf("\"");
				pos1=tmp.LastIndexOf("\"",pos2-1);
				Node tmp2=new Node(rconvert( tmp.Substring(pos1+1,pos2-pos1-1)));
				nodes_name.Add(tmp2); 
			}
		
		}

		int index = 0;
		for (int i=0; i<all_lines.Count; i++) {
			string tmp=(string)all_lines[i];
			if (tmp.IndexOf("<Feature")!=-1){ 
				int k=i+1;
				while (k<all_lines.Count && ((string)all_lines[k]).IndexOf("</Feature")==-1 ){
					string tmp3=(string)all_lines[k];
					if (tmp3.IndexOf("<neighbor")!=-1){
						int pos1=0;
						int pos2=0;
						pos1=tmp3.IndexOf("\"");
						pos2=tmp3.IndexOf("\"",pos1+1);
						int num=int.Parse(tmp3.Substring(pos1+1,pos2-pos1-1));
						pos2=tmp3.LastIndexOf("\"");
						pos1=tmp3.LastIndexOf("\"",pos2-1);
						string relation=tmp3.Substring(pos1+1,pos2-pos1-1);
						((Node)nodes_name[index]).add_neighbour(num,relation);
					}
					
					k+=1;
				}
				index+=1;
			}
			
		}


		index = 0;
		for (int i=0; i<all_lines.Count; i++) {
			string tmp=(string)all_lines[i];
			if (tmp.IndexOf("<Feature")!=-1){
				int k=i+1;
				while (k<all_lines.Count && ((string)all_lines[k]).IndexOf("</Feature")==-1 ){
					string tmp3=(string)all_lines[k];
					if (tmp3.IndexOf("<parent")!=-1){
						int pos1=0;
						int pos2=0;
						pos1=tmp3.IndexOf("\"");
						pos2=tmp3.IndexOf("\"",pos1+1);
						int num=int.Parse(tmp3.Substring(pos1+1,pos2-pos1-1));
						pos2=tmp3.LastIndexOf("\"");
						pos1=tmp3.LastIndexOf("\"",pos2-1);
						string relation=tmp3.Substring(pos1+1,pos2-pos1-1);
						((Node)nodes_name[index]).add_parent(num,relation);
					}
					
					k+=1;
				}
				index+=1;
			}
			
		}


		index = 0;
		for (int i=0; i<all_lines.Count; i++) {
			string tmp=(string)all_lines[i];
			if (tmp.IndexOf("<Feature")!=-1){

				int k=i+1;
				while (k<all_lines.Count && ((string)all_lines[k]).IndexOf("</Feature")==-1 ){
					string tmp3=(string)all_lines[k];
					if (tmp3.IndexOf("<speak")!=-1){
						int pos1=0;
						int pos2=0;
						pos1=tmp3.IndexOf("\"");
						pos2=tmp3.LastIndexOf("\"");
						string description=tmp3.Substring(pos1+1,pos2-pos1-1);
						((Node)nodes_name[index]).add_description(rconvert(description));
					}
					
					k+=1;
				}
				index+=1;
			}
			
		}

		index = 0;
		for (int i=0; i<all_lines.Count; i++) {
			string tmp=(string)all_lines[i];
			if (tmp.IndexOf("<Feature")!=-1){

				int k=i+1;
				while (k<all_lines.Count && ((string)all_lines[k]).IndexOf("</Feature")==-1 ){
					string tmp3=(string)all_lines[k];
					if (tmp3.IndexOf("<tag")!=-1){
						int pos1=0;
						int pos2=0;
						int pos3=0;
						int pos4=0;
						pos1=tmp3.IndexOf ("\"");
						pos2=tmp3.IndexOf ("\"",pos1+1);
						pos3=tmp3.IndexOf ("\"",pos2+1);
						pos4=tmp3.IndexOf ("\"",pos3+1);
						string key=tmp3.Substring(pos1+1,pos2-pos1-1);
						string value=tmp3.Substring(pos3+1,pos4-pos3-1);
						((Node)nodes_name[index]).add_tag(rconvert(key),rconvert(value));
					}
					
					k+=1;
				}
				index+=1;
			}
			
		}
		remove.Clear();

		for (int i=0; i<nodes_name.Count; i++) {

			remove.Add(false);
		}

		sr.Close(); 

		sr.Dispose(); 
		

		
	}   
	
	

	
	
	void DeleteFile(string path,string name) 
		
	{ 
		
		File.Delete(path+ name); 
		
		
	} 
	

	
	void OnGUI() 
		
	{ 	
		bool scrolled = false;
		GUI.Label(new Rect(1100, 280, 160, 20), "Save As Name:");
		new_name = GUI.TextField (new Rect (1100,300,160, 20), new_name);
		filename = filename_ + new_name + ".xml";

		if (new_name == "") {
			filename = default_name;
		}

		if (status == 0) {
			GUI.Label(new Rect(500, 280, 150, 20), "Search key words:");
			search_key = GUI.TextField (new Rect (500, 300, 300, 20), search_key);
			if (search_key != last_search_key) {
				scrollPosition.x = 0;
				scrollPosition.y = 0;
				last_search_key = search_key;
				max_height = Screen.height;
			}
		}

		if (status == 1) {
			//if (GUI.Button (new Rect (1100, 80, 150, 35), "Update Current Node")) {}
			if (GUI.Button (new Rect (25, 120, 150, 35), "Go Back")) {



				bool check = true;
				for (int kkkk=0; kkkk<nodes_name.Count; kkkk++) {
					if ((((bool)en [kkkk]) != true) && ((string)re [kkkk]) != "") {
						check = false;
					}
				}
				if (check == true) {
					((Node)nodes_name [index]).clear_neighbour ();
					((Node)nodes_name [index]).update_name (newnodename);
					for (int kkkk=0; kkkk<nodes_name.Count; kkkk++) {
						if (((bool)en [kkkk]) == true) {
							((Node)nodes_name [index]).add_neighbour (kkkk, ((string)re [kkkk]));
						} else {
							re [kkkk] = "";
						}
						
					}
					((Node)nodes_name [index]).add_description (tmp_description);
					warning = false;
				} else {
					warning = true;
				}

				if (warning) {
					GUI.Label (new Rect (800, 640, 300, 20), "Error in tags! Nodes not update");
				}
				else{
				status = 0;
					index = -1;}
			}
			if (GUI.Button (new Rect (1100, 160, 150, 35), "Make this node root")) {
				root_id = index;
			}
			if (GUI.Button (new Rect (1100, 200, 150, 35), "Remove this node")) {
				nodes_name.Remove (nodes_name [index]);
				for (int iii=0; iii<nodes_name.Count; iii++) {
					((Node)nodes_name [iii]).update_index (index);
				}

				status = 0;
				index = -1;
			}
			if (GUI.Button (new Rect (1100, 240, 150, 35), "Edit tags")) {
				status = 2;
				Node tmp_node = (Node)nodes_name [index];
				Dictionary<string,string> tmpdic=tmp_node.get_tags();
				tag_en.Clear();
				tag_key.Clear();
				tag_value.Clear();
				foreach (KeyValuePair<string,string> pair in tmpdic){
					tag_en.Add(true);
					tag_key.Add(pair.Key);
					tag_value.Add (pair.Value);
				}
			}


		}


		if (GUI.Button (new Rect (1100, 40, 100, 35), "Save XML")) {
			DeleteFile ("", filename); 
			CreateFile ("", filename, "<AIMind>");
			CreateFile ("", filename, "<Root id=\"" + root_id.ToString () + "\"/>");
			int indexx = 0;
			foreach (Node tmp in nodes_name) { 
				string line_tmp = "";
				line_tmp += "<Feature id=\"" + indexx.ToString () + "\" data=\"" + convert (tmp.get_name ()) + "\">";
				CreateFile ("", filename, line_tmp);

				Dictionary<int, string> neighbours_out = tmp.get_neighbours ();

				foreach (KeyValuePair<int,string> pair in neighbours_out) {
					string line_tmp2 = "";
					line_tmp2 += "<neighbor dest=\"" + pair.Key.ToString () + "\" weight=\"1\" relationship=\"" + pair.Value + "\"/>";
					CreateFile ("", filename, line_tmp2);
				
				}


				
				Dictionary<int, string> parents_out = tmp.get_parents ();
				
				foreach (KeyValuePair<int,string> pair in parents_out) {
					string line_tmp2 = "";
					line_tmp2 += "<parent dest=\"" + pair.Key.ToString () + "\" weight=\"1\" relationship=\"" + pair.Value + "\"/>";
					CreateFile ("", filename, line_tmp2);
					
				}
				CreateFile ("", filename, "<speak value=\"" + convert (tmp.get_description ()) + "\"/>");

				Dictionary<string, string> tags_out = tmp.get_tags ();
				foreach (KeyValuePair<string,string> pair in tags_out){
					string line_tmp3="";
					line_tmp3="<tag key=\""+ convert (pair.Key)+"\" value=\""+convert (pair.Value)+"\"/>";
					CreateFile("",filename,line_tmp3);
				}

				CreateFile ("", filename, "</Feature>");




				indexx += 1;


			} 



			CreateFile ("", filename, "</AIMind>");
					
		}
		if (GUI.Button (new Rect (1100, 0, 100, 35), "Open XML")) {	
			status = 0;
			index = -1;
			nodes_name.Clear ();
			max_height = Screen.height;
			OpenFileName ofn = new OpenFileName ();
			
			ofn.structSize = Marshal.SizeOf (ofn);
			
			ofn.filter = "XML File\0*.xml\0\0";
			
			ofn.file = new string (new char[256]);
			
			ofn.maxFile = ofn.file.Length;
			
			ofn.fileTitle = new string (new char[64]);
			
			ofn.maxFileTitle = ofn.fileTitle.Length;
			
			ofn.initialDir = UnityEngine.Application.dataPath;
			
			ofn.title = "Open XML File";

			ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
			
			if (DllTest.GetOpenFileName (ofn)) {
				string tmp = ofn.file;
				string tmp2 = "";
				for (int kkk=0; kkk<tmp.Length; kkk++) {
					if (tmp [kkk] == '\\') {
						tmp2 += "//";
					} else {
						tmp2 += tmp [kkk];
					}
				}
				LoadFile ("", tmp2, ref nodes_name);
				filename_ = tmp2.Substring (0, tmp2.LastIndexOf ("//")) + "//";
				default_name = tmp2;
			}
			
		}


		if (status == 1) {
			GUI.Label (new Rect (25,20, 200, 20),"Search key words:");
			search_value = GUI.TextField (new Rect (25, 40, 150, 20), search_value);
			if (search_value != last_search_value) {
				scrollPosition2.x = 0;
				scrollPosition2.y = 0;
				last_search_value = search_value;
				max_height = Screen.height;
			}
//			Node tmp_node = (Node)nodes_name [index];
			GUI.Label (new Rect (750, 40, 300, 20), "Description:");
			tmp_description = GUI.TextArea (new Rect (750, 60, 280, 400), tmp_description);
			GUI.Label (new Rect (750, 0, 100, 20),"Node Name:");
			newnodename = GUI.TextField (new Rect (750, 20, 280, 20), newnodename);
			if (warning) {
				GUI.Label (new Rect (800, 640, 300, 20), "Error in tags! Nodes not update");
			}
			scrollPosition2 = GUI.BeginScrollView (new Rect (0, 0, Screen.width, Screen.height), scrollPosition2, new Rect (0, 0, Screen.width, max_height));
			scrolled = true;
			int hh = 20;
			GUI.Label (new Rect (230, 0, 150, 20), "Tag:");
			GUI.Label (new Rect (400, 0, 150, 20), "Node Name:");

			for (int kk=0; kk<nodes_name.Count; kk++) {
				if (kk != index && (search_value == "" || ((Node)nodes_name [kk]).get_name ().ToLower ().IndexOf (search_value.ToLower ()) != -1)) {
					en [kk] = GUI.Toggle (new Rect (400, hh, 300, 20), ((bool)en [kk]), ((Node)nodes_name [kk]).get_name ());
					re [kk] = GUI.TextField (new Rect (230, hh, 150, 20), ((string)re [kk]));
					hh += 20;
					if ((hh + 20) > max_height) {
						max_height = hh + 20;
					}
				}
			}

		}



		int i = 0;
		int j = 0;
		if (status == 0) {
			GUI.Label (new Rect (500, 20, 100, 20), "New node name:");
			newnode = GUI.TextField (new Rect (500, 40, 300, 100), newnode);

			if (GUI.Button (new Rect (1100, 120, 150, 35), "Add new Node")) {
				Node new_n = new Node (newnode);
				nodes_name.Add (new_n);
				newnode = "";
				remove.Add(false);
			}
			if (GUI.Button (new Rect (1100, 160, 150, 35), "Remove selected Nodes")) {
				for (int ij=nodes_name.Count-1;ij>=0;ij--){
					if (((bool)remove[ij])==false) continue;
					nodes_name.Remove (nodes_name [ij]);
					for (int iii=0; iii<nodes_name.Count; iii++) {
						((Node)nodes_name [iii]).update_index (ij);
					}
				}
				remove.Clear();
				for (int ij=0;ij<nodes_name.Count;ij++){
					remove.Add (false);
				}
			}
			GUI.Label (new Rect (800, 550, 300, 20), "Total Nodes: " + nodes_name.Count.ToString ());
			scrollPosition = GUI.BeginScrollView (new Rect (0, 0, Screen.width, Screen.height), scrollPosition, new Rect (0, 0, Screen.width, max_height));
			scrolled = true;
			foreach (Node tmp in nodes_name) { 
				if (search_key == "" || tmp.get_name ().ToLower ().IndexOf (search_key.ToLower ()) != -1) {
					remove[j] = GUI.Toggle (new Rect (50, i, 10, 20), ((bool)remove [j]),"");
					if (GUI.Button (new Rect (100, i, 20 + (float)7.2 * tmp.get_name ().Length, 20), tmp.get_name ())) {	

						scrollPosition2.x = 0;
						scrollPosition2.y = 0;
						newnodename = tmp.get_name ();
						status = 1;
						search_value = "";
						en.Clear ();
						re.Clear ();
						index = j;
//						Node current_node = tmp;
						tmp_description = tmp.get_description ();

						for (int kk=0; kk<nodes_name.Count; kk++) {
							string re_tmp = "";
							bool flag = false;
							Node tmp_node = (Node)nodes_name [index];
							Dictionary<int, string> neighbours = tmp_node.get_neighbours (); 
							foreach (KeyValuePair<int,string> pair in neighbours) {
								if (pair.Key == kk) {
									flag = true;
									re_tmp = pair.Value;
									break;
								}
							}
							en.Add (flag);
							re.Add (re_tmp);
				
						}
					}
				
					i += 20;
					if ((i + 20) > max_height) {
						max_height = i + 20;
					}
					if (status == 1)
						max_height = Screen.height;
				}
				j += 1;

			} 

		} 
		if (status == 2) {
			GUI.Label (new Rect (650, 0, 500, 20), "Current Node Name: "+newnodename);
			int hh=20;
			//if (GUI.Button (new Rect (1100, 80, 150, 35), "Update Current Node")) {}

			if (GUI.Button (new Rect (1100, 120, 150, 35), "Add tag")) {
				tag_en.Add(false);
				tag_key.Add ("");
				tag_value.Add ("");

			}
			if (GUI.Button (new Rect (25, 120, 150, 35), "Go Back")) {
				((Node)nodes_name[index]).clear_tag();
				for (int kkk=0; kkk<tag_en.Count; kkk++) {
					if ((bool)tag_en[kkk]){
						((Node)nodes_name[index]).add_tag(((string)tag_key[kkk]),((string)tag_value[kkk]));						
					}
				}
				status=1;
			}
			GUI.Label (new Rect (230, 0, 100, 20),"Key:");
			GUI.Label (new Rect (400, 0, 100, 20), "Value:");
			for (int kkk=0; kkk<tag_en.Count; kkk++) {
				//if (kk != index && (search_value == "" || ((Node)nodes_name [kk]).get_name ().ToLower ().IndexOf (search_value.ToLower ()) != -1)) {
					tag_en[kkk] = GUI.Toggle (new Rect (210, hh, 10, 20), ((bool)tag_en [kkk]),"");
					tag_key[kkk] = GUI.TextField (new Rect (230, hh, 150, 20), ((string)tag_key [kkk]));
					tag_value[kkk] = GUI.TextField (new Rect (400, hh, 150, 20), ((string)tag_value [kkk]));
					hh += 20;
					if ((hh + 20) > max_height) {
						max_height = hh + 20;
					}
				//}
			}

		}
	
		if (scrolled) GUI.EndScrollView ();
	}
	
	
	
} 
