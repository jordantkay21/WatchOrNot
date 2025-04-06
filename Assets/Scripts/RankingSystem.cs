using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankingSystem
{
    private List<MovieInfo> allMovies;
    public Queue<MovieInfo> remainingQueue = new();
    public Dictionary<int, MovieInfo> rankings = new();
    public HashSet<int> usedRanks = new();

    public RankingSystem(List<MovieInfo> movies)
    {
        allMovies = movies.OrderBy(movies => UnityEngine.Random.value).Take(12).ToList();

        foreach (var movie in allMovies)
            remainingQueue.Enqueue(movie);
    }

    public MovieInfo GetNextMovie()
    {
        return remainingQueue.Count > 0 ? remainingQueue.Dequeue() : null;
    }

    public bool AssignRank(MovieInfo movie, int rank)
    {
        if (usedRanks.Contains(rank) || rankings.ContainsValue(movie)) return false;

        rankings[rank] = movie;
        usedRanks.Add(rank);
        return true;
    }

    public bool IsFinished => remainingQueue.Count == 0;

    public List<(int rank, MovieInfo movie)> GetRankedResults()
    {
        return rankings.OrderBy(kv => kv.Key).Select(kv => (kv.Key, kv.Value)).ToList();
    }
}
