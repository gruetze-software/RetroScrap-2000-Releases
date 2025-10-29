using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroScrap2000
{
	public sealed class GameScrap
	{
		public string? Name { get; set; }
		public string? Id { get; set; }
		public string? Source { get; set; }
		public string? Description { get; set; }
		public string? Genre { get; set; }
		public string? Players { get; set; }
		public string? Developer { get; set; }
		public string? Publisher { get; set; }
		public double? RatingNormalized { get; set; }  // 0..1
		public string? ReleaseDateRaw { get; set; }
		[JsonIgnore]
		public DateTime? ReleaseDate
		{
			get
			{
				if (string.IsNullOrEmpty(ReleaseDateRaw))
					return null;
				if (DateTime.TryParseExact(
								ReleaseDateRaw,
								"yyyy-MM-dd",
								null,
								System.Globalization.DateTimeStyles.None,
								out var dt))
					return dt;
				if (int.TryParse(ReleaseDateRaw, out int year) && year > 1900 && year < 3000)
					return new DateTime(year, 1, 1);
				return null;
			}
		}

		[JsonIgnore]
		public List<GameMediaSettings> PossibleMedien { get; private set; }
			

		public GameScrap()
		{
			PossibleMedien = [.. RetroScrapOptions.GetMediaSettingsList()];
		}

		public GameScrap CopyFrom(GameDataBase game, RetroScrapOptions opt)
		{
			GameScrap g = new GameScrap()
			{
				Description = game.GetDesc(opt),
				Developer = game.developpeur?.text,
				Genre = game.GetGenre(opt),
				Id = game.id,
				Name = game.GetName(opt),
				Players = game.joueurs?.text,
				Publisher = game.editeur?.text,
				ReleaseDateRaw = game.GetReleaseDate(opt),
				RatingNormalized = this.RatingNormalized,
				Source = this.Source,
			};

			g.PossibleMedien = [.. this.PossibleMedien];
			return g;
		}

	}

}
