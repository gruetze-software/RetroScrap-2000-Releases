using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace RetroScrap2000
{
	public enum eMediaType
	{
		BoxImageFront = 0,
		BoxImageBack,
		BoxImageSide,
		BoxImageTexture,
		Video,
		Marquee,
		Fanart,
		ScreenshotGame,
		ScreenshotTitle,
		Wheel,
		Manual,
		Map,
		Unknown
	}

	public class GameMediaSettings
	{
		public eMediaType Type { get; set; }
		public string ApiKey { get; set; }
		public string XmlFolderAndKey { get; set; }
		public string Url { get; set; }
		public string? FilePath { get; set; }
		public byte[]? NewData { get; set; }
		public string? ContentType { get; set; }
		public bool IsUpToDate { get; set; } = false;
		public List<string> KnowFileExtensions
		{
			get
			{
				return new List<string>() {
					".jpg", ".jpeg", ".png", ".bmp", ".avi", ".mp4", ".mkv" };
			}
		}

		public GameMediaSettings(eMediaType typ)
		{
			Type = typ;
			ApiKey = string.Empty;
			Url = string.Empty;
			XmlFolderAndKey = string.Empty;
			NewData = null;
			FilePath = null;
		}

		public GameMediaSettings(eMediaType typ, string apikey, string xmlFolderAndKey) : this(typ)
		{
			ApiKey = apikey;
			XmlFolderAndKey = xmlFolderAndKey;
		}

		public GameMediaSettings(eMediaType typ, string apikey, string xmlFolderAndKey, string file) 
			: this(typ, apikey, xmlFolderAndKey)
		{
			FilePath = file;
		}

		public override string ToString()
		{
			switch (this.Type)
			{
				case eMediaType.BoxImageFront:
					if (((GameMediaFront)this).FrontType == eMediaBoxFrontType.TwoDim) return "Front-2D";
					else if (((GameMediaFront)this).FrontType == eMediaBoxFrontType.ThreeDim) return "Front-3D";
					else if (((GameMediaFront)this).FrontType == eMediaBoxFrontType.Mix1) return "Front-Mix1";
					else if (((GameMediaFront)this).FrontType == eMediaBoxFrontType.Mix2) return "Front-Mix2";
					else return "Front";

				case eMediaType.BoxImageBack: return "Box Back";
				case eMediaType.BoxImageSide:	return "Box Side";
				case eMediaType.BoxImageTexture: return "Box Texture";
				case eMediaType.Manual: return "Manual";
				case eMediaType.ScreenshotGame: return "Screenshot";
				case eMediaType.ScreenshotTitle: return "Screenshot Title";
				case eMediaType.Wheel: return "Wheel";
				default: return this.Type.ToString();
			}
		}
	}

	public enum eMediaWheelType
	{
		Normal = 0,
		Carbon,
		Steel
	};

	public enum eMediaBoxFrontType
	{
		TwoDim = 0,
		ThreeDim,
		Mix1,
		Mix2
	};

	public class GameMediaFront : GameMediaSettings
	{
		public eMediaBoxFrontType FrontType { get; private set; }

		public GameMediaFront(eMediaBoxFrontType typ) : base(eMediaType.BoxImageFront)
		{
			FrontType = typ;
			XmlFolderAndKey = "image";
			switch (FrontType)
			{
				case eMediaBoxFrontType.TwoDim: ApiKey = "box-2D"; break;
				case eMediaBoxFrontType.ThreeDim: ApiKey = "box-3D"; break;
				case eMediaBoxFrontType.Mix1: ApiKey = "mixrbv1"; break;
				case eMediaBoxFrontType.Mix2: ApiKey = "mixrbv2"; break;
				default:
					throw new ApplicationException("Unknown eMediaBoxFrontType!");
			}
		}
	}

	public class GameMediaWheel : GameMediaSettings
	{
		public eMediaWheelType WheelType { get; private set; }

		public GameMediaWheel(eMediaWheelType typ) : base(eMediaType.Wheel)
		{
			WheelType = typ;
			XmlFolderAndKey = "wheel";
			switch (WheelType)
			{
				case eMediaWheelType.Normal:
					ApiKey = "wheel";
					break;

				case eMediaWheelType.Steel:
					ApiKey = "wheel-steel";
					break;

				case eMediaWheelType.Carbon:
					ApiKey = "wheel-carbon";
					break;

				default:
					throw new ApplicationException("Unknown eMediaWheelType!");
			}
		}
	}
}
				