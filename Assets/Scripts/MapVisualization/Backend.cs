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
		public List<StoryAct> StorySequence;
		public int turn;
	}
	[Serializable]
	public class StoryAct {
		public string Item1;
		public int Item2;
	}
	[Serializable]
	public class ChronologyResponse {
		public int AnchorNodeId;
		public List<StoryNode> StorySequence;
		public int current_turn;
	}
}
