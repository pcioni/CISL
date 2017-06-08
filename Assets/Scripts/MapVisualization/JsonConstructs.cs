using System;
using System.Collections.Generic;

namespace JsonConstructs {

	//namespace to contain backend data structures

	[Serializable]
	public class ChronologyRequest {
		public ChronologyRequest(int id, int turns) {
			this.id = id;
			this.turns = turns;
		}
		public int id;
		public int turns;
	}

	[Serializable]
	public class AddToChronologyRequest {
		public AddToChronologyRequest(int id, int previous_id) {
			this.id = id;
			this.turns = previous_id;
		}
		public int id;
		public int turns;
	}

    [Serializable]
    public class LoadXMLRequest
    {
        public LoadXMLRequest(string url)
        {
            this.url = url;
        }
        public string url;
    }
    [Serializable]
	public class StoryNode {
		public int graph_node_id;
		public List<StoryAct> story_acts;
		public string text;
	}
	[Serializable]
	public class StoryAct {
		public string Item1;
		public int Item2;
	}
	[Serializable]
	public class ChronologyResponse {
		public int anchor_node_id;
		public List<StoryNode> Sequence;
		public int length;
		public int starting_turn;
	}

	[Serializable]
	class GraphNodeLight {
		public int id;
		public List<string> entity_type;
	}

	[Serializable]
	class GraphLight {
		public List<GraphNodeLight> graph_nodes;
	}

	[Serializable]
	public class NodeData {
		public int id;
		public string text;
		public List<string> imgLabel;
		public List<string> imgUrl;
		public NodeData(int i, string t, List<string> url, List<string> label) {
			id = i;
			text = t;
			imgUrl = url;
			imgLabel = label;
		}
	}

	[Serializable]
	public class TestSequence {

		public List<ChronologyResponse> StorySequence;
		public int last_anchor_id;
		public int last_segment_turn;
		public int current_turn;

	}

}
