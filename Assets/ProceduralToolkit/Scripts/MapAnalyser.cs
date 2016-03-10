//author: mike cook

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapAnalyser : MonoBehaviour {

    [Metric("Water %")]
    public static float WaterCoverage(object _map){

        if(_map == null || !(_map is DSWorld)){
            Debug.Log("Incorrect input");
            return 0f;
        }

        DSWorld world = (DSWorld) _map;

        float count = 0f;
        float total = world.elevation.GetLength(0) * world.elevation.GetLength(1);
        for(int i=0; i<world.elevation.GetLength(0); i++){
            for(int j=0; j<world.elevation.GetLength(1); j++){
                if(!GTLim(i, j, world.elevation, world.waterLimit)){
                    count++;
                }
            }
        }

        return count/total;
    }

    [Metric("Hill %")]
    public static float HillCoverage(object _map){

        if(_map == null || !(_map is DSWorld)){
            Debug.Log("Incorrect input");
            return 0f;
        }

        DSWorld world = (DSWorld) _map;

        float count = 0f;
        float total = world.elevation.GetLength(0) * world.elevation.GetLength(1);
        for(int i=0; i<world.elevation.GetLength(0); i++){
            for(int j=0; j<world.elevation.GetLength(1); j++){
                if(GTLim(i, j, world.elevation, world.hillsLimit)){
                    count++;
                }
            }
        }

        return count/total;
    }

    [Metric("Avg. Continent %")]
    public static float Fragmentation(object _map){

        if(_map == null || !(_map is DSWorld)){
            Debug.Log("Incorrect input");
            return 0f;
        }

        DSWorld world = (DSWorld) _map;

        int numContinents = 0;
        List<int> sizes = new List<int>();

        //Flood fill to discover continents
        bool[,] marked = new bool[world.elevation.GetLength(0), world.elevation.GetLength(1)];
        for(int i=0; i<marked.GetLength(0); i++){
            for(int j=0; j<marked.GetLength(1); j++){
                if(!GTLim(i, j, world.elevation, world.waterLimit) || marked[i,j])
                    continue;

                //Start a flood fill from here.
                List<int[]> openList = new List<int[]>();
                List<int[]> closedList = new List<int[]>();
                openList.Add(new int[]{i, j});

                int size = 0;
                numContinents++;

                while(openList.Count > 0){
                    int[] c = openList[0];
                    openList.RemoveAt(0);
                    closedList.Add(c);
                    marked[c[0],c[1]] = true;

                    size++;

                    AddNeighbours(world.elevation, world.waterLimit, c[0], c[1], openList, closedList);
                    // Debug.Log("Openlist: "+openList.Count);
                }
                sizes.Add(size);
                Debug.Log("Found a continent of size "+size);
            }
        }

        float total = 0;
        foreach(float t in sizes){
            total += t;
        }

        return (total/(float)sizes.Count)/(world.elevation.GetLength(0)*world.elevation.GetLength(1));
    }

    public static bool GTLim(int x, int y, float[,] map, int lim){
        return map[x,y] > lim;
    }

    public static void AddNeighbours(float[,] map, int lim, int x, int y, List<int[]> openList, List<int[]> closedList){
        for(int i=-1; i<2; i++){
            for(int j=-1; j<2; j++){
                if(i == 0 && j == 0)
                    continue;

                int dx = i + x;
                int dy = j + y;
                if(dx < 0 || dy < 0 || dx >= map.GetLength(0) || dy >= map.GetLength(1)){
                    continue;
                }
                if(!GTLim(dx, dy, map, lim)){
                    continue;
                }
                bool seen = false;
                foreach(int[] p in openList){
                    if(p[0] == dx && p[1] == dy){
                        seen = true;
                        break;
                    }

                }
                foreach(int[] p in closedList){
                    if(p[0] == dx && p[1] == dy){
                        seen = true;
                        break;
                    }
                }
                if(seen)
                    continue;
                openList.Add(new int[]{dx, dy});
            }
        }
    }

}
