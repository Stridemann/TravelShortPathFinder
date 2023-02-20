# TravelShortPathFinder

Attempt to solve Traveling Salesman problem in polilinear time + nav grid segmentation (convert to graph)

WIKI [full documentation](https://github.com/Stridemann/TravelShortPathFinder/wiki/TravelShortPathFinder-algorithm-documentation)

# Short technical intro.
This algorithm has been optimized for use in game development or bot creation, where each iteration of the algorithm must be as fast as possible. It can support nav grids of any size, as long as they are sensibly large, and provides a very fast response for the next move point. However, there is a cost to this speed in the form of an initial segmentation, which takes some time to complete, but only needs to be done once (please refer to the Benchmarks section).

The algorithm does not have a strict, sequential path output. It is adaptive and responds to the player's current position by providing the next point. The player can be teleported to any position on the nav map at any time, and the algorithm will still function properly.

The core algorithm does not provide the shortest path directly, but it does provide the point to which the player should move. As the player moves, the algorithm will mark the graph nodes as "Passed" within the player's view radius, which will trigger the algorithm to provide a new point to move towards.

If you need the shortest sequential path as a result, then a simulation is required. You can use the AlgorithmUtils.GetShortestPath() function to obtain the full path.


![FullAlgoDemo](https://user-images.githubusercontent.com/7633163/219960587-623a6fa2-785b-4e80-8dfc-acdc7daff222.gif)


# Fast start

```CS
using System.Drawing;
using TravelShortPathFinder.Algorithm.Data;
using TravelShortPathFinder.Algorithm.Logic;
using TravelShortPathFinder.Algorithm.Utils;

public class FastStart
{
    private readonly GraphMapExplorer _explorer;
    private Node _currentPlayerNode;

    public FastStart(Bitmap myBitmap)
    {
        //Convert black/white image to NavGrid
        NavGrid navGrid = NavGridProvider.FromBitmap(myBitmap);

        Settings settings = new Settings(
            segmentationSquareSize: 50, //The max size of segment square (in pixels) (check settings image visualization)
            segmentationMinSegmentSize: 100, //Remove segments that less than this square (in pixels^2)
            playerVisibilityRadius: 60); //The player visibility radius (in pixels)

        _explorer = new GraphMapExplorer(navGrid, settings);

        _explorer.ProcessSegmentation(new Point(123, 123)); //player location pixel

        if (_explorer.Graph.Nodes.Count == 0)
        {
            throw new InvalidOperationException("Segmentation failed. Probably because of wrong initial point.");
        }

        //The first node  is player location used in ProcessSegmentation method
        _currentPlayerNode = _explorer.Graph.Nodes.First();
        _explorer.Update(_currentPlayerNode.Pos);
    }

    public void Update(Point playerPosition)
    {
        //Optimized and can be called each frame, usually will update _explorer.NextRunNode
        _explorer.Update(playerPosition); 

        if (_explorer.NextRunNode != null) //or _explorer.HasLocation
        {
            //This will be the next node to run to.
            //it will not be the nearest node and can be in any position of map (check documentation)
            Node nextPoint = _explorer.NextRunNode;

            //Now use own pathfinder to run player to.
            //Or use graph path finder to get a path to next point by graph
            List<Node>? path = GraphPathFinder.FindPath(_currentPlayerNode, nextPoint);
            //You can use just the first point in path to run player to, after player arrived just call Update and FindPath again

            _currentPlayerNode = nextPoint;
        }
    }
}
```

Settings parameters visualized:

![Settings](https://user-images.githubusercontent.com/7633163/220201652-25fe89cb-62f9-4c77-a548-47c9316535fd.png)
