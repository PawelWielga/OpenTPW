using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenTPW.Tests;

[TestClass]
public class BfmuEncodingTests
{
	[TestMethod]
	public void VersionZeroKeepsSingleByteCharacters()
	{
		var table = new BFMUReader( new MemoryStream( Alphabet( 0, 3, 0, "AąZ" ) ) );
		Assert.AreEqual( "AąZ", table.Decode( new byte[] { 1, 2, 3 } ) );
	}

	[TestMethod]
	public void VersionOneDecodesEscapePairsAsOneCharacter()
	{
		var table = new BFMUReader( new MemoryStream( Alphabet( 1, 3, 2, "ABąŁ" ) ) );
		Assert.AreEqual( 4, table.Count );
		Assert.AreEqual( "AąBŁ", table.Decode( new byte[] { 1, 3, 1, 2, 3, 2 } ) );
	}

	[TestMethod]
	public void AnIncompleteOrInvalidEscapeDoesNotBecomeText()
	{
		var table = new BFMUReader( new MemoryStream( Alphabet( 1, 3, 2, "ABąŁ" ) ) );
		Assert.ThrowsException<InvalidDataException>( () => table.Decode( new byte[] { 3 } ) );
		Assert.ThrowsException<InvalidDataException>( () => table.Decode( new byte[] { 3, 0 } ) );
		Assert.ThrowsException<InvalidDataException>( () => table.Decode( new byte[] { 3, 3 } ) );
		Assert.ThrowsException<InvalidDataException>( () => table.Decode( new byte[] { 0 } ) );
	}

	[TestMethod]
	public void ATruncatedAlphabetIsRejected()
	{
		Assert.ThrowsException<InvalidDataException>( () =>
			new BFMUReader( new MemoryStream( Alphabet( 1, 3, 2, "AB" ) ) ).CharacterArray() );
	}

	private static byte[] Alphabet( ushort version, byte single, byte extended, string characters )
	{
		using var bytes = new MemoryStream();
		using var writer = new BinaryWriter( bytes );
		writer.Write( Encoding.ASCII.GetBytes( "BFMU" ) );
		writer.Write( version );
		writer.Write( single );
		writer.Write( extended );
		writer.Write( Encoding.Unicode.GetBytes( characters ) );
		return bytes.ToArray();
	}
}
