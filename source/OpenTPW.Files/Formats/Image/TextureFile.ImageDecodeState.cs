namespace OpenTPW;

public partial class TextureFile
{
	struct ImageDecodeState
	{
		public List<float> DequantizationBuffer;
		public List<float> RowDecodeBuffer;

		/// <summary>
		/// Sized from the indices <c>DecodeChannel</c> reaches, not from the cube of the side: step 1 writes
		/// the dequantization buffer over [0, size*size), step 2 the row buffer over the same, and step 3
		/// reads no further. A 256-pixel texture needs about 520 KB here.
		/// </summary>
		public ImageDecodeState( int size )
		{
			DequantizationBuffer = Enumerable.Repeat( 0f, size * size ).ToList();
			RowDecodeBuffer = Enumerable.Repeat( 0f, size * size ).ToList();
		}
	}
}
