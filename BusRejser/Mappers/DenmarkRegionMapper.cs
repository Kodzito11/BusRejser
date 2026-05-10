namespace BusRejser.Mappers
{
	public static class DenmarkRegionMapper
	{
		private static readonly Dictionary<string, string> MunicipalityToRegion =
			new(StringComparer.OrdinalIgnoreCase)
			{
				// Region Hovedstaden
				["København"] = "Region Hovedstaden",
				["Frederiksberg"] = "Region Hovedstaden",
				["Ballerup"] = "Region Hovedstaden",
				["Brøndby"] = "Region Hovedstaden",
				["Dragør"] = "Region Hovedstaden",
				["Gentofte"] = "Region Hovedstaden",
				["Gladsaxe"] = "Region Hovedstaden",
				["Glostrup"] = "Region Hovedstaden",
				["Herlev"] = "Region Hovedstaden",
				["Albertslund"] = "Region Hovedstaden",
				["Hvidovre"] = "Region Hovedstaden",
				["Høje-Taastrup"] = "Region Hovedstaden",
				["Lyngby-Taarbæk"] = "Region Hovedstaden",
				["Rødovre"] = "Region Hovedstaden",
				["Ishøj"] = "Region Hovedstaden",
				["Tårnby"] = "Region Hovedstaden",
				["Vallensbæk"] = "Region Hovedstaden",
				["Furesø"] = "Region Hovedstaden",
				["Allerød"] = "Region Hovedstaden",
				["Fredensborg"] = "Region Hovedstaden",
				["Helsingør"] = "Region Hovedstaden",
				["Hillerød"] = "Region Hovedstaden",
				["Hørsholm"] = "Region Hovedstaden",
				["Rudersdal"] = "Region Hovedstaden",
				["Egedal"] = "Region Hovedstaden",
				["Frederikssund"] = "Region Hovedstaden",
				["Gribskov"] = "Region Hovedstaden",
				["Halsnæs"] = "Region Hovedstaden",
				["Bornholm"] = "Region Hovedstaden",

				// Region Sjælland
				["Greve"] = "Region Sjælland",
				["Køge"] = "Region Sjælland",
				["Roskilde"] = "Region Sjælland",
				["Solrød"] = "Region Sjælland",
				["Faxe"] = "Region Sjælland",
				["Guldborgsund"] = "Region Sjælland",
				["Holbæk"] = "Region Sjælland",
				["Kalundborg"] = "Region Sjælland",
				["Lolland"] = "Region Sjælland",
				["Næstved"] = "Region Sjælland",
				["Odsherred"] = "Region Sjælland",
				["Ringsted"] = "Region Sjælland",
				["Slagelse"] = "Region Sjælland",
				["Sorø"] = "Region Sjælland",
				["Stevns"] = "Region Sjælland",
				["Vordingborg"] = "Region Sjælland",
				["Lejre"] = "Region Sjælland",

				// Region Syddanmark
				["Assens"] = "Region Syddanmark",
				["Billund"] = "Region Syddanmark",
				["Esbjerg"] = "Region Syddanmark",
				["Fanø"] = "Region Syddanmark",
				["Fredericia"] = "Region Syddanmark",
				["Faaborg-Midtfyn"] = "Region Syddanmark",
				["Haderslev"] = "Region Syddanmark",
				["Kerteminde"] = "Region Syddanmark",
				["Kolding"] = "Region Syddanmark",
				["Langeland"] = "Region Syddanmark",
				["Middelfart"] = "Region Syddanmark",
				["Nordfyns"] = "Region Syddanmark",
				["Nyborg"] = "Region Syddanmark",
				["Odense"] = "Region Syddanmark",
				["Svendborg"] = "Region Syddanmark",
				["Sønderborg"] = "Region Syddanmark",
				["Tønder"] = "Region Syddanmark",
				["Varde"] = "Region Syddanmark",
				["Vejen"] = "Region Syddanmark",
				["Vejle"] = "Region Syddanmark",
				["Ærø"] = "Region Syddanmark",
				["Aabenraa"] = "Region Syddanmark",

				// Region Midtjylland
				["Aarhus"] = "Region Midtjylland",
				["Favrskov"] = "Region Midtjylland",
				["Hedensted"] = "Region Midtjylland",
				["Herning"] = "Region Midtjylland",
				["Holstebro"] = "Region Midtjylland",
				["Horsens"] = "Region Midtjylland",
				["Ikast-Brande"] = "Region Midtjylland",
				["Lemvig"] = "Region Midtjylland",
				["Norddjurs"] = "Region Midtjylland",
				["Odder"] = "Region Midtjylland",
				["Randers"] = "Region Midtjylland",
				["Ringkøbing-Skjern"] = "Region Midtjylland",
				["Samsø"] = "Region Midtjylland",
				["Silkeborg"] = "Region Midtjylland",
				["Skanderborg"] = "Region Midtjylland",
				["Skive"] = "Region Midtjylland",
				["Struer"] = "Region Midtjylland",
				["Syddjurs"] = "Region Midtjylland",
				["Viborg"] = "Region Midtjylland",

				// Region Nordjylland
				["Aalborg"] = "Region Nordjylland",
				["Brønderslev"] = "Region Nordjylland",
				["Frederikshavn"] = "Region Nordjylland",
				["Hjørring"] = "Region Nordjylland",
				["Jammerbugt"] = "Region Nordjylland",
				["Læsø"] = "Region Nordjylland",
				["Mariagerfjord"] = "Region Nordjylland",
				["Morsø"] = "Region Nordjylland",
				["Rebild"] = "Region Nordjylland",
				["Thisted"] = "Region Nordjylland",
				["Vesthimmerlands"] = "Region Nordjylland",
			};

		public static string? GetRegionForMunicipality(string? municipality)
		{
			if (string.IsNullOrWhiteSpace(municipality))
				return null;

			var key = municipality.Trim();

			return MunicipalityToRegion.TryGetValue(key, out var region)
				? region
				: null;
		}
	}
}