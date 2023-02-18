# TravelShortPathFinder

Please, refer to the WIKI documentation: 
https://github.com/Stridemann/TravelShortPathFinder/wiki/TravelShortPathFinder-algorithm-documentation 



# Short technical intro.
This algorithm has been optimized for use in game development or bot creation, where each iteration of the algorithm must be as fast as possible. It can support nav grids of any size, as long as they are sensibly large, and provides a very fast response for the next move point. However, there is a cost to this speed in the form of an initial segmentation, which takes some time to complete, but only needs to be done once (please refer to the Benchmarks section).

The algorithm does not have a strict, sequential path output. It is adaptive and responds to the player's current position by providing the next point. The player can be teleported to any position on the nav map at any time, and the algorithm will still function properly.

The core algorithm does not provide the shortest path directly, but it does provide the point to which the player should move. As the player moves, the algorithm will mark the graph nodes as "Passed" within the player's view radius, which will trigger the algorithm to provide a new point to move towards.

If you need the shortest sequential path as a result, then a simulation is required. You can use the AlgorithmUtils.GetShortestPath() function to obtain the full path.

![IExe096wrd](https://user-images.githubusercontent.com/7633163/219903741-3ef56bf5-c547-4c0c-af90-a3e8dd03b501.gif)
