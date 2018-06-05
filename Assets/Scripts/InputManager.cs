using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
			RaycastHit raycastHit;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit)) {
				var hitObj = raycastHit.collider.gameObject;
				if (hitObj.CompareTag("Block") && !Wolf.Player.Auto) {
                    Wolf.Player.TargetBlock = hitObj.GetComponent<Block>();
                    PathManager.Instance.pathfinderQueue.Enqueue(Wolf.Player);
                }
			}
		}
        if (Input.GetMouseButtonDown(1)) {
            RaycastHit raycastHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit)) {
                var hitObj = raycastHit.collider.gameObject;
                if (hitObj.CompareTag("Block")) {
                    var block = hitObj.GetComponent<Block>();
                    //Debug.Log(Block.ManhattanDistance(player.Block, block));
                    var prey = Instantiate(ChunkLoader.Instance.prefabPrey).GetComponent<Prey>();
                    prey.AttachedBlock = block;
                }
            }
            /*RaycastHit raycastHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit)) {
                var hitObj = raycastHit.collider.gameObject;
                if (hitObj.CompareTag("Block")) {
                    var block = hitObj.GetComponent<Block>();
                    block.Occupied = true;

                    var tempObj = block.Chunk.CreateTempObj(Chunk.ETempObjType.tempItem);
                    if (tempObj != null) {
                        ((MonoBehaviour)tempObj).transform.position = block.transform.position
                            + Vector3.up * (block.transform.localScale[1] / 2 + transform.localScale[1] / 2);
                    }
                    tempObj.AttachedBlock = block;
                }
            }*/
        }
        if (Input.GetMouseButtonDown(2)) {
            RaycastHit raycastHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit)) {
                var hitObj = raycastHit.collider.gameObject;
                if (hitObj.CompareTag("Block")) {
                    var block = hitObj.GetComponent<Block>();
                    var wanderer = Instantiate(ChunkLoader.Instance.prefabWanderer).GetComponent<Wanderer>();
                    wanderer.AttachedBlock = block;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
            foreach (var adjacentBlock in Wolf.Player.Block.AdjacentBlocks())
                Debug.Log(string.Format("Chunk[{0}, {1}], Block[{2}, {3}]"
                , adjacentBlock.Chunk.Row, adjacentBlock.Chunk.Col
                , adjacentBlock.Row, adjacentBlock.Col));
    }
}
