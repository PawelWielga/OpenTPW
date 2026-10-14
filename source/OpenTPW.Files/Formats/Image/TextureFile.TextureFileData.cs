namespace OpenTPW;

public partial class TextureFile
{
	internal struct TextureFileData
	{
		public TextureFlags Flags { get; set; }
		public bool HasAlphaChannel { get; set; }
		public byte BitsPerPixel { get; set; }
		public byte Version { get; set; }

		public short Width { get; set; }
		public short Height { get; set; }

		/// <summary>
		/// What each channel's values are multiplied by, as the codec is handed them: a .wct's made from its header's
		/// four scales as the loader makes them, a sign's artwork its own four floats as they are.
		/// </summary>
		public float YMultiplier { get; set; }
		public float CbMultiplier { get; set; }
		public float CrMultiplier { get; set; }
		public float AMultiplier { get; set; }

		public int ColorChunkSize { get; set; }
		public int AlphaChunkSize { get; set; }

		public int Checksum { get; set; }

		public byte[] ColorChunk { get; set; }
		public byte[] AlphaChunk { get; set; }
	}
}
