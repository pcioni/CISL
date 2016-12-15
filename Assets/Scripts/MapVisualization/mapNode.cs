using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class mapNode : MonoBehaviour {

	public timelineNode master; //mimic the state of the master, but be on map

	public Vector2 mapPosition;
	private LineRenderer lr;
	private Image img;

	public List<mapNode> neighbors = new List<mapNode>();

	// Use this for initialization
	void Start () {
		lr = GetComponent<LineRenderer>();
		img = GetComponent<Image>();
		master.callback.Add(OnStateChange);
		name = "mapNode(" + master.name + ")";
        OnStateChange(master.state.ToString());
	}
	
	public void OnStateChange(string state) {
		//print("state change: " + state);
		Color tmp = master.sr.color;
		switch (state) {
			case "IN":
				drawLines(true);
				transform.SetAsLastSibling();
				tmp.a = 1;
				img.color = tmp;
				break;
			case "OUT":
                lr.enabled = false;
                transform.SetAsFirstSibling();
                tmp.a = 0.75f;
                img.color = tmp;
				break;
			case "HALF":
				lr.enabled = false;
				transform.SetAsFirstSibling();
				img.color = tmp;
				break;
			case "PAST":
				lr.enabled = false;
				img.color = tmp;
				break;
			case "RE":
				drawLines(false);
				transform.SetAsLastSibling();
				img.color = tmp;
				break;
			case "BACK":
                img.color = tmp;
                if (master.state == timelineNode.focusState.IN)
                {
                    drawLines(true);
                }
                else
                {
                    lr.enabled = false;
                }
                break;
		}
	}

	private IEnumerator tmplcr = null;
	public void drawLines(bool _in) {
		lr.enabled = true;
        if (_in)
        {
            lr.SetColors(Color.red, Color.red);
        }
        else
        {
            lr.SetColors(Color.cyan, Color.cyan);
        }
        
		if (tmplcr != null) {
			StopCoroutine(tmplcr);
		}
		tmplcr = _drawLines();
		StartCoroutine(tmplcr);
	}

	private IEnumerator _drawLines() {
		//yield return new WaitForSeconds(1);
		Vector3 centralNodePos = transform.position;
		Vector3[] points = new Vector3[Mathf.Max(neighbors.Count * 2, 1)]; //TODO: why is this empty, this is a discrepency between the two versions
		points[0] = centralNodePos;
		int nCount = 0;
		for (int i = 1; i < points.Length; i += 2) {
			points[i - 1] = centralNodePos;
			points[i] = neighbors[nCount].transform.position;
			//points[i].z = 1;
			nCount++;
			yield return null;
		}
		lr.SetVertexCount(points.Length);
		lr.SetPositions(points);

		tmplcr = null;

	}
}
