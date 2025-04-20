using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaylistDisplayUIController : MonoBehaviour
{
    public GameObject playlistScrollView;
    public Transform playlistDisplayPanel;
    public GameObject movieInfoContainerPrefab;

    private void Start()
    {
        PlexDataFetcher.Instance.OnPlaylistItemsFetched += DisplayLoadedPlaylist;
    }

    public void DisplayLoadedPlaylist(List<MovieInfo> movies)
    {
        playlistScrollView.gameObject.SetActive(true);

        //1. Clear any previous items
        foreach (Transform child in playlistDisplayPanel)
            Destroy(child.gameObject);

        //Spawn new items
        foreach(MovieInfo movie in movies)
        {
            GameObject container = Instantiate(movieInfoContainerPrefab, playlistDisplayPanel);

            //Set up poster
            RawImage posterImage = container.transform.Find("MoviePosterImage")?.GetComponent<RawImage>();
            if(posterImage != null && movie.posterTexture != null)
            {
                posterImage.texture = movie.posterTexture;
            }

            //Set up title
            TMP_Text titleText = container.transform.Find("MovieTitleText")?.GetComponent<TMP_Text>();
            if (titleText != null)
                titleText.text = movie.title;

            //Set up year
            TMP_Text yearText = container.transform.Find("MovieYearText")?.GetComponent<TMP_Text>();
            if (yearText != null)
                yearText.text = movie.year.ToString();
        }

        Debug.Log($"[PlaylistDisplayUIController][DisplayLoadedPlaylist] Displaying {movies.Count} movies");
    }
}
