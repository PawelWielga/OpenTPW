namespace OpenTPW;

/// <summary>
/// The weather thing as a park file's writer takes it (<c>docs/exe/saves.md</c>, "OpenTPW's writer, the weather
/// thing"; FileFormats <c>saves.md</c>, "The weather").
/// </summary>
public sealed partial class ParkWorld
{
	/// <summary>Where the weather thing's record lies in the body, or -1 with none.</summary>
	public int WeatherAt { get; private set; } = -1;

	/// <summary>The weather thing's fields as they were saved; null where the file holds no such thing.</summary>
	public WeatherState? WeatherRecord { get; private set; }

	/// <summary>
	/// The weather thing's fields that a running park changes, in the order <c>FUN_00512010</c> reads and writes them.
	/// <c>mRainSoundHandle</c>, <c>mPType</c> and <c>mOverridden</c> are not here: see <see cref="PutWeather"/>.
	/// </summary>
	/// <param name="Quality"><c>mQuality</c>, 1 to 100, low being bad.</param>
	/// <param name="LastChanged"><c>mLastChanged</c>: the park calendar, a <c>FILETIME</c>, as the quality was last applied.</param>
	/// <param name="LastForecasted"><c>mLastForecasted</c>: the park calendar as the last forecast was made.</param>
	/// <param name="ForecastTime">The forecast's <c>mTime</c>: when that forecast falls due, <c>DaysOfWarning</c> days after it was made.</param>
	/// <param name="ForecastQuality">The forecast's <c>mQuality</c>: the quality it brings.</param>
	/// <param name="ForecastValid">The forecast's <c>mValid</c>: it has not yet been delivered.</param>
	/// <param name="CurrentDrops"><c>mCurrentDrops</c>: where the ramp has got to.</param>
	/// <param name="TargetDrops"><c>mTargetDrops</c>: what the quality asks for.</param>
	/// <param name="Lightning"><c>mLightning</c>: the quality is in the lightning band.</param>
	/// <param name="LightningCountdown"><c>mLightningCountdown</c>: what is left before the next bolt.</param>
	/// <param name="LightningNumThisPeriod"><c>mLightningNumThisPeriod</c>: the bolts since the lightning armed.</param>
	/// <param name="ThunderCountdown"><c>mThunderCountdown</c>: one less every weather tick, the thunder sounding as it reaches nought.</param>
	/// <param name="LastLightningLocation">
	/// <c>mLastLightningLocation</c>: the top of the bolt that armed the thunder, in the original's axes (y up), or
	/// null where no bolt was ever put there (a file holds the debug heap's fill, or other bytes, then).
	/// </param>
	/// <param name="WindDir"><c>mWindDir</c>, degrees in [0, 360).</param>
	/// <param name="WindForce"><c>mWindForce</c>, 0 to 1.</param>
	public readonly record struct WeatherState(
		int Quality, long LastChanged, long LastForecasted, long ForecastTime, int ForecastQuality, bool ForecastValid,
		int CurrentDrops, int TargetDrops, bool Lightning, int LightningCountdown, int LightningNumThisPeriod,
		int ThunderCountdown, System.Numerics.Vector3? LastLightningLocation, float WindDir, float WindForce );

	// Each field's place in the 99-byte record: the sum of the sizes before it, in FUN_00512010's order.
	private const int WeatherQualityAt = 16;
	private const int WeatherLastChangedAt = 20;
	private const int WeatherLastForecastedAt = 28;
	private const int WeatherForecastTimeAt = 36;
	private const int WeatherForecastQualityAt = 44;
	private const int WeatherForecastValidAt = 48;
	private const int WeatherCurrentDropsAt = 49;
	private const int WeatherTargetDropsAt = 53;
	private const int WeatherLightningAt = 57;
	private const int WeatherLightningCountdownAt = 58;
	private const int WeatherLightningNumThisPeriodAt = 62;
	private const int WeatherThunderCountdownAt = 66;
	private const int WeatherLastLightningLocationAt = 70;
	private const int WeatherRainSoundHandleAt = 82;
	private const int WeatherWindForceAt = 94;

	/// <summary>The debug heap's fill, which a location no bolt was ever put at holds.</summary>
	private const uint NeverWritten = 0xcdcdcdcd;

	private WeatherState ReadWeather( int start )
	{
		var location = start + WeatherLastLightningLocationAt;
		var neverStruck = (uint)ReadInt32At( location ) == NeverWritten && (uint)ReadInt32At( location + 4 ) == NeverWritten
			&& (uint)ReadInt32At( location + 8 ) == NeverWritten;

		return new WeatherState(
			Quality: ReadInt32At( start + WeatherQualityAt ),
			LastChanged: ReadInt64At( start + WeatherLastChangedAt ),
			LastForecasted: ReadInt64At( start + WeatherLastForecastedAt ),
			ForecastTime: ReadInt64At( start + WeatherForecastTimeAt ),
			ForecastQuality: ReadInt32At( start + WeatherForecastQualityAt ),
			ForecastValid: ReadByteAt( start + WeatherForecastValidAt ) != 0,
			CurrentDrops: ReadInt32At( start + WeatherCurrentDropsAt ),
			TargetDrops: ReadInt32At( start + WeatherTargetDropsAt ),
			Lightning: ReadByteAt( start + WeatherLightningAt ) != 0,
			LightningCountdown: ReadInt32At( start + WeatherLightningCountdownAt ),
			LightningNumThisPeriod: ReadInt32At( start + WeatherLightningNumThisPeriodAt ),
			ThunderCountdown: ReadInt32At( start + WeatherThunderCountdownAt ),
			LastLightningLocation: neverStruck ? null
				: new System.Numerics.Vector3( ReadSingleAt( location ), ReadSingleAt( location + 4 ), ReadSingleAt( location + 8 ) ),
			WindDir: ReadSingleAt( start + WeatherWindDirectionAt ),
			WindForce: ReadSingleAt( start + WeatherWindForceAt ) );
	}

	private long ReadInt64At( int offset )
	{
		if ( offset + 8 > _data.Length )
			throw new InvalidDataException( $"a qword at 0x{offset:x} runs past the end of the payload" );

		return BitConverter.ToInt64( _data, offset );
	}

	/// <summary>
	/// Writes the running park's weather over the file's in <paramref name="body"/>, a copy of this park's own, in the
	/// order <c>FUN_00512010</c> writes it. Where <paramref name="weather"/>'s location is null the file's twelve bytes
	/// are left. <c>mPType</c> (read by nothing) and <c>mOverridden</c> (written by the constructor alone, nought) are
	/// left the file's. <c>mRainSoundHandle</c> is written nought: the original writes its own voice's handle, which
	/// names nothing in the process that loads the file and keeps that one's rain from starting (<c>FUN_005131f0</c>
	/// starts a voice only over a nought handle).
	/// </summary>
	/// <exception cref="InvalidOperationException">The park file it was loaded from holds no weather thing.</exception>
	public void PutWeather( byte[] body, WeatherState weather )
	{
		ArgumentNullException.ThrowIfNull( body );

		if ( WeatherAt < 0 )
			throw new InvalidOperationException( "the park file it was loaded from holds no weather thing" );

		var at = body.AsSpan( WeatherAt );

		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( at[WeatherQualityAt..], weather.Quality );
		System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian( at[WeatherLastChangedAt..], weather.LastChanged );
		System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian( at[WeatherLastForecastedAt..], weather.LastForecasted );
		System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian( at[WeatherForecastTimeAt..], weather.ForecastTime );
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( at[WeatherForecastQualityAt..], weather.ForecastQuality );
		at[WeatherForecastValidAt] = (byte)(weather.ForecastValid ? 1 : 0);
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( at[WeatherCurrentDropsAt..], weather.CurrentDrops );
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( at[WeatherTargetDropsAt..], weather.TargetDrops );
		at[WeatherLightningAt] = (byte)(weather.Lightning ? 1 : 0);
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( at[WeatherLightningCountdownAt..], weather.LightningCountdown );
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( at[WeatherLightningNumThisPeriodAt..], weather.LightningNumThisPeriod );
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( at[WeatherThunderCountdownAt..], weather.ThunderCountdown );

		if ( weather.LastLightningLocation is { } location )
		{
			System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian( at[WeatherLastLightningLocationAt..], location.X );
			System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian( at[(WeatherLastLightningLocationAt + 4)..], location.Y );
			System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian( at[(WeatherLastLightningLocationAt + 8)..], location.Z );
		}

		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( at[WeatherRainSoundHandleAt..], 0 );
		System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian( at[WeatherWindDirectionAt..], weather.WindDir );
		System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian( at[WeatherWindForceAt..], weather.WindForce );
	}
}
