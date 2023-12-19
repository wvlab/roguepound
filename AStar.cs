using FunctionalRoguePound;
using FunctionalRoguePound.FUtility;

namespace RoguePound;


public static class AStar
{
    static int[] dx = [0, 0, -1, 1];
    static int[] dy = [-1, 1, 0, 0];

    static public void FindPath((int x, int y) start, (int x, int y) goal, List<(int, int)> saveTo)
    {
        var queue = new PriorityQueue<(int, int), int>();
        var cameFrom = new Dictionary<(int, int), (int, int)>();
        var gScore = new Dictionary<(int, int), int>();
        var fScore = new Dictionary<(int, int), int>();

        queue.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = (int)FMath.EuclideanDistance(start.x, start.y, goal.x, goal.y);

        while (queue.Count > 0)
        {
            (int currentX, int currentY) = queue.Dequeue();

            if (currentX == goal.x && currentY == goal.y)
            {
                ReconstructPath(cameFrom, (currentX, currentY), saveTo);
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                int nx = currentX + dx[i];
                int ny = currentY + dy[i];

                if (nx >= 0 && nx < Settings.TileWidth && ny >= 0 && ny < Settings.TileHeight && Tile.isTraversable(GameStorage.Tiles[nx, ny]))
                {
                    var neighbor = (nx, ny);
                    int tentativeGScore = gScore[(currentX, currentY)] + 1;

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = (currentX, currentY);
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + (int)FMath.EuclideanDistance(nx, ny, goal.x, goal.y);

                        if (!queue.UnorderedItems.Any(a => a.Item1.Equals(neighbor)))
                        {
                            queue.Enqueue(neighbor, fScore[neighbor]);
                        }
                    }
                }
            }
        }
    }

    static private void ReconstructPath(Dictionary<(int, int), (int, int)> cameFrom, (int, int) current, List<(int, int)> saveTo)
    {
        saveTo.Clear();
        List<(int, int)> path = new();

        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }

        saveTo.AddRange(path.AsEnumerable().Reverse());
    }
}
