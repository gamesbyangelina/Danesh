using UnityEngine;
using System.Collections;

public class Tile {

	//True if the tile is solid. In the case of generators currently implemented, this means it's a wall.
	public bool BLOCKS_MOVEMENT = false;

	public int x;
	public int y;

	public Tile(int x, int y, bool blocks = false){
		this.x = x;
		this.y = y;
		this.BLOCKS_MOVEMENT = blocks;
	}

}
