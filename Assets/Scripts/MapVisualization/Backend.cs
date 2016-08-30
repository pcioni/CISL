using System;
using System.Collections.Generic;

namespace Backend {

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
		public string entity_type;
	}

	[Serializable]
	class GraphLight {
		public List<GraphNodeLight> graph_nodes;
	}
}
