﻿using Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoService.Collections;
using MongoService.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoService.Models
{
    public class Anime : IModel
    {
        private AnimeCollection _animeCollection = new AnimeCollection();

        public Anime() { }

        public Anime(AnilistResponse.ResponseMedia media)
        {
            this.Titles = new Dictionary<string, string>();
            this.Titles[LocalizationEnum.English] = media.Title.Romaji;
            this.Titles[LocalizationEnum.Japanese] = media.Title.Native;

            this.Descriptions = new Dictionary<string, string>();
            this.Descriptions[LocalizationEnum.English] = media.Description;

            this.AnilistId = media.Id;
            this.MyAnimeListId = media.IdMal;
            this.Format = media.Format;
            this.Status = media.Status.HasValue ? media.Status.Value : AnimeStatusEnum.FINISHED;

            if (media.StartDate.Year.HasValue)
            {
                int? month = media.StartDate.Month;
                int? day = media.StartDate.Day;

                this.StartDate = new DateTime(media.StartDate.Year.Value, month.HasValue ? month.Value : 1, day.HasValue ? day.Value : 1);
            }

            if (media.EndDate.Year.HasValue)
            {
                int? month = media.EndDate.Month;
                int? day = media.EndDate.Day;

                this.EndDate = new DateTime(media.EndDate.Year.Value, month.HasValue ? month.Value : 1, day.HasValue ? day.Value : 1);
            }

            this.SeasonPeriod = media.Season.HasValue ? media.Season.Value : AnimeSeasonEnum.UNKNOWN;
            this.SeasonYear = media.SeasonYear;

            if (media.Episodes.HasValue)
            {
                this.EpisodesCount = media.Episodes.Value;
            }
            else if(media.Status == AnimeStatusEnum.RELEASING && media.AiringEpisode != null)
            {
                this.EpisodesCount = media.AiringEpisode.Episode - 1;
            }
            else
            {
                this.EpisodesCount = 0;
            }

            this.EpisodeDuration = media.Duration;

            if(media.Trailer != null)
            {
                switch (media.Trailer.Site)
                {
                    case "youtube":
                        this.TrailerUrl = $"https://www.youtube.com/watch?v={media.Trailer.Id}";
                        break;
                    case "dailymotion":
                        this.TrailerUrl = $"https://www.dailymotion.com/video/{media.Trailer.Id}";
                        break;
                    default:
                        throw new Exception($"Trailer site {media.Trailer.Site} not handled!");
                }
            }

            this.CoverImage = media.CoverImage.Large;
            this.CoverColor = media.CoverImage.Color;
            this.BannerImage = media.BannerImage;
            this.Genres = media.Genres;
            this.Score = media.AverageScore.HasValue ? media.AverageScore.Value : 0;

            foreach(var edge in media.Relations.Edges)
            {
                if(edge.Node.Format != "TV")
                {
                    continue;
                }

                if(edge.Type == "SEQUEL" || edge.Type == "PREQUEL")
                {
                    var query = this._animeCollection.GetList(new AnimeFilter()
                    {
                        AnilistId = edge.Node.Id,
                        Page = 1
                    });

                    if(query.Count > 0)
                    {
                        int id = (int)query.Documents[0].Id;
                        if (edge.Type == "SEQUEL")
                        {
                            this.Sequel = id;
                        }
                        else
                        {
                            this.Prequel = id;
                        }
                    }
                }
            }
        }

        [BsonElement("anilist_id")]
        public int AnilistId { get; set; }

        [BsonElement("mal_id")]
        public int? MyAnimeListId { get; set; }

        [BsonElement("format")]
        public AnimeFormatEnum Format { get; set; }

        [BsonElement("status")]
        public AnimeStatusEnum Status { get; set; }

        [BsonElement("titles")]
        public Dictionary<string, string> Titles { get; set; }

        [BsonElement("descriptions")]
        public Dictionary<string, string> Descriptions { get; set; }

        [BsonElement("start_date")]
        public DateTime? StartDate { get; set; }

        [BsonElement("end_date")]
        public DateTime? EndDate { get; set; }

        [BsonElement("season_period")]
        public AnimeSeasonEnum SeasonPeriod { get; set; }

        [BsonElement("season_year")]
        public int? SeasonYear { get; set; }

        [BsonElement("episodes_count")]
        public int EpisodesCount { get; set; }

        [BsonElement("episode_duration")]
        public int? EpisodeDuration { get; set; }

        [BsonElement("trailer_url")]
        public string TrailerUrl { get; set; }

        [BsonElement("cover_image")]
        public string CoverImage { get; set; }

        [BsonElement("cover_color")]
        public string CoverColor { get; set; }

        [BsonElement("banner_image")]
        public string BannerImage { get; set; }

        [BsonElement("genres")]
        public List<string> Genres { get; set; }

        [BsonElement("sequel")]
        public int? Sequel { get; set; }

        [BsonElement("prequel")]
        public int? Prequel { get; set; }

        [BsonElement("score")]
        public int Score { get; set; }

    }
}