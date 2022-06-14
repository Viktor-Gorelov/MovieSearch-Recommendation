using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Диплом
{
    public class SearchResult
    {
        public int page { get; set; }
        public List<MovieResult> results { get; set; }
        public int total_results { get; set; }
        public int total_pages { get; set; }

    }

    public class MovieResult
    {
        public string title { get; set; }
        public string release_date { get; set; }
        public List<int> genre_ids { get; set; }
        public string overview { get; set; }
        public string poster_path { get; set; }
        public double vote_average { get; set; }

        override public string ToString()
        {
            return "title: " + title + "release_date: " + release_date + "genre_ids: " + genre_ids + 
                "overview: " + overview + "poster_path: " + poster_path + "vote_average: " + vote_average;
        }

    }
    public class Movie
    {
        public string title { get; set; }
        public string release_date { get; set; }
        public List<string> genres { get; set; }
        public string overview { get; set; }
        public string poster_path { get; set; }
        public double vote_average { get; set; }

        override public string ToString()
        {
            return "title: " + title + "release_date: " + release_date + "genres: " + GetGenreName() +
                "overview: " + overview + "poster_path: " + poster_path + "vote_average: " + vote_average;
        }
        public string GetGenreName()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if(genres.Count == 0)
            {
                return string.Empty;
            }
            foreach(string genre in genres)
            {
                if(stringBuilder.Length > 0)
                {
                    stringBuilder.Append(", ");
                }
                stringBuilder.Append(genre);
            }
            return stringBuilder.ToString();
            
        }
        public string GetShortOverview(int max_overwiev_length)
        {
            if(overview == null)
            {
                return string.Empty;
            }
            if(overview.Length < max_overwiev_length)
            {
                return overview;
            }
            string resultoverview = overview;
            while(resultoverview.Length > max_overwiev_length)
            {
                int pointindex = resultoverview.LastIndexOf(".");
                resultoverview = resultoverview.Substring(0, pointindex);
            }
            return resultoverview;
        }
    }

    public class GenresResult
    {
        public List<Genre> genres { get; set; }

        override public string ToString()
        {
            return "genres: " + genres;
        }
    }
    public class Genre
    {
        public int id { get; set; }
        public string name { get; set; }

        override public string ToString()
        {
            return "id: " + id + "name: " + name;
        }
    }
}