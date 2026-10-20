namespace OpenTPW;

/// <summary>
/// The sounds a ride script starts with <c>ADDOBJ</c> and <c>EVENT</c>: <c>FUN_005573d0</c>'s types 3 to 10, each a
/// <c>Sound_PlayEffect( 0, category, effect, x, y, z )</c> with no level set after it (<c>docs/exe/park.md</c>, "The
/// effect subsystem"; <c>docs/exe/audio.md</c>, "A script's sounds").
///
/// <para>
/// The type picks the category: 3 the level's <c>cat_rides</c> (<c>[0x00803a3c]</c>), 4 the level's <c>cat_ambient</c>
/// (<c>[0x00803a38]</c>), 5 the global <c>cat_rides</c> (<c>[0x00803a28]</c>), 6 <c>cat_kids</c>, 7 <c>cat_staff</c>,
/// 8 the global <c>cat_ambient</c> and 9 <c>cat_ui</c>; 10 is the thing's own bank. An effect its category does not
/// hold plays nothing and answers nought, which frees an <c>ADDOBJ</c>'s record.
/// </para>
///
/// <para>
/// The effect's flags word picks the voice's class (<c>0x006b6774</c>..<c>0x006b67f2</c>). Five are built here: the
/// one-shot (no bit <c>0x4</c>), looped under <c>0x8</c>; the plain class (<c>0x4</c> and <c>0x2</c>, none of
/// <c>0x10</c>, <c>0x100</c> and <c>0x400</c>), one sample after another, served as the land's own are
/// (<see cref="ServeFollowing"/>); the music's class (<c>0x4</c>, <c>0x2</c> and <c>0x400</c>), the same with each
/// later sample drawn among its variation's zones by the parameter <c>SETOBJPARAM</c> sets; the chain of <c>0x4</c>
/// alone, a one-shot child after another, each of any variation; and the held chain of <c>0x404</c>, the same chain
/// with each later child drawn among its variation's zones. Each is flat under <c>0x200</c>, else placed and heard over
/// its variation's range. The other classes and type 10 are counted, and start nothing.
/// </para>
/// </summary>
public sealed partial class ParkAudio
{
	/// <summary>The level's own <c>cat_ambient</c>, a script's type 4 (<c>[0x00803a38]</c>, registered by <c>FUN_0051ec50</c>).</summary>
	private SoundCategory? _levelAmbient;

	/// <summary>The global <c>cat_rides</c>, a script's type 5 (<c>[0x00803a28]</c>).</summary>
	private SoundCategory? _globalRides;

	/// <summary>The global <c>cat_ui</c>, a script's type 9 (<c>[0x00803a2c]</c>).</summary>
	private SoundCategory? _ui;

	private readonly List<ScriptVoice> _scriptVoices = [];
	private readonly Random _scriptRandom = new();

	/// <summary>The fade <c>FADEOBJ</c> stops a sound with: this project's 0.06 s, where the original steps x0.6 (Q509).</summary>
	private const float ScriptFadeSeconds = 0.06f;

	/// <summary>The three categories a script plays in that nothing else in a park loads.</summary>
	private void LoadScriptCategories( string root )
	{
		_levelAmbient = new SoundCategory( root, $"{root}/Sound", "ambient" );
		_globalRides = new SoundCategory( "global", "global/sound", "rides" );
		_ui = new SoundCategory( "global", "global/sound", "ui" );

		if ( !_levelAmbient.IsValid || !_globalRides.IsValid || !_ui.IsValid )
			Log.Warning( $"Park audio: a script's category would not load (level ambient {_levelAmbient.IsValid}, "
				+ $"global rides {_globalRides.IsValid}, ui {_ui.IsValid}), so its sounds are silent" );
	}

	/// <summary>The category a script's sound of <paramref name="type"/> plays in (<c>FUN_005573d0</c>), or null.</summary>
	private SoundCategory? ScriptCategory( int type ) => type switch
	{
		3 => _rides,
		4 => _levelAmbient,
		5 => _globalRides,
		6 => _kids,
		7 => _staff,
		8 => _ambient,
		9 => _ui,
		_ => null
	};

	/// <summary>The kind of voice an effect's flags word makes.</summary>
	internal enum ScriptVoiceClass
	{
		/// <summary>One sample, looped under <c>0x8</c> (<c>0x006bd7b0</c>).</summary>
		OneShot,

		/// <summary>One sample after another from any variation, a silence after each (<c>0x006be090</c>).</summary>
		Plain,

		/// <summary>
		/// The music's class (<c>0x006be610</c>): the plain class with each sample after the first drawn among the zones of
		/// the variation before that hold the voice's parameter (<c>0x006be5e0</c>), the first the effect's first variation.
		/// </summary>
		Music,

		/// <summary>
		/// A chain of one-shot children, each of any variation at its own volume and pitch, the next a wait after the
		/// last one's start (<c>0x006bdc60</c>): the held scream's chain with the plain class's draw.
		/// </summary>
		Chain,

		/// <summary>
		/// The held chain (<c>0x006bdd30</c>): the same chain, its first child the effect's first variation and each
		/// later one drawn by weight among the zones of the variation before that hold the voice's parameter
		/// (<c>0x006c3e00</c>), as a held scream's are.
		/// </summary>
		HeldChain,

		/// <summary>A class not built here; <see cref="ClassOf"/> names its constructor.</summary>
		NotBuilt
	}

	/// <summary>
	/// The class of voice an effect of <paramref name="flags"/> makes (<c>0x006b6774</c>..<c>0x006b67f2</c>), and for one
	/// not built the name its count goes under, by its constructor: <c>0x10</c> <c>0x006be680</c>; with <c>0x2</c>,
	/// <c>0x400</c> the music's <c>0x006be610</c> and <c>0x100</c> <c>0x006be330</c>; without it, <c>0x400</c> the held
	/// chain <c>0x006bdd30</c> and else the chain of any variation, <c>0x006bdc60</c>.
	/// </summary>
	internal static (ScriptVoiceClass Class, string? Gap) ClassOf( int flags )
	{
		if ( (flags & 0x4) == 0 )
			return (ScriptVoiceClass.OneShot, null);

		if ( (flags & 0x10) != 0 )
			return (ScriptVoiceClass.NotBuilt, "RIDE_EFFECT_SOUND_CLASS_6BE680");

		if ( (flags & 0x2) != 0 )
		{
			if ( (flags & 0x400) != 0 )
				return (ScriptVoiceClass.Music, null);

			return (flags & 0x100) != 0
				? (ScriptVoiceClass.NotBuilt, "RIDE_EFFECT_SOUND_CLASS_6BE330")
				: (ScriptVoiceClass.Plain, null);
		}

		return (flags & 0x400) != 0
			? (ScriptVoiceClass.HeldChain, null)
			: (ScriptVoiceClass.Chain, null);
	}

	/// <summary>How a script's sound fared at its start.</summary>
	internal enum ScriptSoundStart
	{
		/// <summary>A voice was made; it is in the <see cref="ScriptVoice"/> answered.</summary>
		Started,

		/// <summary>Its category holds no such effect: the engine answers nought, and an <c>ADDOBJ</c>'s record is freed.</summary>
		Absent,

		/// <summary>Of a class, or a type, not built here: counted, and the record kept.</summary>
		NotBuilt,

		/// <summary>No sound device: nothing loaded and nothing played.</summary>
		NoDevice
	}

	/// <summary>One sound a script started, for as long as it can sound.</summary>
	internal sealed class ScriptVoice
	{
		/// <summary>The script's id, its type and its effect.</summary>
		public required int Script { get; init; }

		public required int Type { get; init; }

		public required int Effect { get; init; }

		public required SoundCategory Category { get; init; }

		/// <summary>Whether it is of the plain class, served a sample at a time.</summary>
		public bool Plain { get; init; }

		/// <summary>Whether it is of the music's class, served a sample at a time as the plain class is, by its zones.</summary>
		public bool Music { get; init; }

		/// <summary>Whether it plays one sample after another itself: the plain class and the music's.</summary>
		public bool Follows => Plain || Music;

		/// <summary>The key its parameter is set by, the effect's own (its record's <c>+0x12</c>, controller slot 0's key).</summary>
		public int Key { get; init; }

		/// <summary>
		/// A music voice whose last draw found no zone holding its parameter: the original makes the parameter <c>0x7f</c>
		/// and flags the voice <c>0x40</c> (<c>0x006be55e</c>), and draws again only at a set to anything else (<c>0x006be580</c>).
		/// </summary>
		public bool Parked { get; set; }

		/// <summary>Whether it is a chain of children (<see cref="ScriptVoiceClass.Chain"/>); <see cref="Voice"/> is its newest.</summary>
		public bool Chain { get; init; }

		/// <summary>Whether it is a held chain (<see cref="ScriptVoiceClass.HeldChain"/>), whose children follow their zones.</summary>
		public bool Held { get; init; }

		/// <summary>
		/// The parameter, controller slot 0, which picks among the zones: nought at the start, as the controllers are
		/// made zeroed (<c>FUN_006b7150</c>), and set by <c>SETOBJPARAM</c> (<see cref="SetScriptSoundParameter"/>), whose
		/// only shipped users set the bus's music voice.
		/// </summary>
		public int Parameter { get; set; }

		/// <summary>When a chain's next child is made: its newest child's start and that child's wait (<c>0x006bdb88</c>).</summary>
		public float NextAt { get; set; }

		/// <summary>What it is, for the log and the census.</summary>
		public string Kind => Plain ? "plain" : Music ? "music" : Held ? "held chain" : Chain ? "chain" : Looped ? "looped" : "one-shot";

		/// <summary>Whether its one sample loops (flag <c>0x8</c>).</summary>
		public bool Looped { get; init; }

		/// <summary>Where it sounds, in whole units of the world, or null for one flagged <c>0x200</c>, which is heard flat.</summary>
		public Vector3? Place { get; set; }

		/// <summary>The sample playing.</summary>
		public Voice? Voice { get; set; }

		/// <summary>A plain or music voice's variation, or a chain's newest child's, zero-based, or -1 before its first.</summary>
		public int Variation { get; set; } = -1;

		/// <summary>The volume, of a hundred, and the pitch its first sample drew.</summary>
		public (int Volume, int Pitch) Kept { get; set; }

		/// <summary>A plain voice's silence after the sample playing, in milliseconds.</summary>
		public int Wait { get; set; }

		/// <summary>When a plain voice's last sample was found ended, or null while one plays.</summary>
		public float? EndedAt { get; set; }

		/// <summary>
		/// A plain or music voice's sample of its variation, drawn with it, which a start out of range holds; a one-shot's
		/// one sample, drawn at its start and held for its life (<c>0x006bbf60</c>, the voice's <c>+0xc</c>).
		/// </summary>
		public AudioClip? Clip { get; set; }

		/// <summary>
		/// When a one-shot not looped is dropped wherever it is: its start, 250 ms and its sample record's length
		/// (<c>0x006bbf60</c>'s <c>+0x1c</c>, read by <c>0x006bcb60</c> on each pass).
		/// </summary>
		public float DropAt { get; set; }

		/// <summary>
		/// Whether the listener was last found past its range (<see cref="InRange"/>): the original's voice flag
		/// <c>0x800</c>, its channel given up and nothing next made until it is in range again.
		/// </summary>
		public bool Away { get; set; }

		/// <summary>How many samples it has started.</summary>
		public int Samples { get; set; }

		/// <summary>Whether a <c>KILLOBJ</c>, a <c>FADEOBJ</c> or its script's end has stopped it.</summary>
		public bool Stopped { get; set; }
	}

	/// <summary>The script sounds still sounding, or still to serve a sample.</summary>
	internal IReadOnlyList<ScriptVoice> ScriptVoices => _scriptVoices;

	/// <summary>How many script sounds started, found absent, were stopped, were faded and were moved.</summary>
	internal (int Started, int Absent, int Stopped, int Faded, int Moved) ScriptSounds { get; private set; }

	/// <summary>
	/// A script's sound of <paramref name="type"/> and <paramref name="effect"/> at <paramref name="at"/>, in the world's
	/// axes: each coordinate cut to a whole number (<c>__ftol</c>, <c>FUN_005573d0</c>), played at its variation's own
	/// volume and a pitch drawn from it, as a <c>Sound_PlayEffect</c> with no level set after it is.
	/// </summary>
	/// <param name="at">Where it sounds, or null where the caller cannot place it: a sound flagged <c>0x200</c> needs no place,
	/// and any other is counted and not started.</param>
	internal ScriptSoundStart StartScriptSound( int scriptId, int type, int effect, Vector3? at, out ScriptVoice? sound )
	{
		sound = null;

		if ( !Audio.Ready )
			return ScriptSoundStart.NoDevice;

		// The thing's own bank (FUN_004145e0), which no shipped script asks for.
		if ( type == RideEffects.LastType )
		{
			Unimplemented.Report( "RIDE_EFFECT_SOUND_ITEM_BANK" );
			return ScriptSoundStart.NotBuilt;
		}

		if ( ScriptCategory( type ) is not { IsValid: true } category || category.VariationsOf( effect ).Count == 0 )
		{
			ScriptSounds = ScriptSounds with { Absent = ScriptSounds.Absent + 1 };
			Log.Info( $"Park audio: script {scriptId} asked for effect {effect} of type {type}, which its category does not hold" );

			return ScriptSoundStart.Absent;
		}

		var flags = category.FlagsOf( effect );
		var (kind, gap) = ClassOf( flags );

		if ( gap != null )
		{
			Unimplemented.Report( gap );
			return ScriptSoundStart.NotBuilt;
		}

		// The first child takes the effect's first variation only for an effect with a key (FUN_006bdb10); one with
		// none would draw its first from the zones of the parent's own draw (0x006bbf60). All 18 shipped have a key.
		if ( kind == ScriptVoiceClass.HeldChain && category.ParameterOf( effect ) == 0 )
		{
			Unimplemented.Report( "RIDE_EFFECT_SOUND_HELD_CHAIN_NO_KEY" );
			return ScriptSoundStart.NotBuilt;
		}

		var flat = (flags & SoundCategory.NoPlaceFlag) != 0;

		if ( !flat && at is null )
		{
			Unimplemented.Report( "RIDE_EFFECT_SOUND_NO_PLACE" );
			return ScriptSoundStart.NotBuilt;
		}

		sound = new ScriptVoice
		{
			Script = scriptId,
			Type = type,
			Effect = effect,
			Category = category,
			Plain = kind == ScriptVoiceClass.Plain,
			Music = kind == ScriptVoiceClass.Music,
			Key = category.ParameterOf( effect ),
			Chain = kind is ScriptVoiceClass.Chain or ScriptVoiceClass.HeldChain,
			Held = kind == ScriptVoiceClass.HeldChain,
			Looped = kind == ScriptVoiceClass.OneShot && (flags & SoundCategory.LoopedFlag) != 0,
			Place = flat ? null : Whole( at!.Value )
		};

		if ( sound.Follows )
		{
			NextScriptSample( sound );
		}
		else if ( sound.Chain )
		{
			// Out of range at the start, its first child waits for the listener (FUN_006b62b0 ticks it no sooner).
			if ( Reached( sound ) )
				NextChainChild( sound );
		}
		else
		{
			sound.Clip = category.PickSample( effect, out var variation, out var length );
			sound.Variation = variation;
			sound.DropAt = Time.Now + OneShotGraceSeconds + (float)length.TotalSeconds;

			// Out of range at the start it plays nothing until it is in range (FUN_006b62b0 ticks it no sooner).
			if ( Reached( sound ) )
				StartOneShot( sound );
		}

		_scriptVoices.Add( sound );
		ScriptSounds = ScriptSounds with { Started = ScriptSounds.Started + 1 };

		Log.Info( $"Park audio: script {scriptId} started effect {effect} of type {type} (flags 0x{flags:x}, {sound.Kind}) "
			+ $"sample '{sound.Voice?.Name ?? sound.Clip?.Name}'{(sound.Away ? " held" : "")} volume {sound.Voice?.Volume ?? 0f:0.0000} "
			+ (sound.Place is { } place ? $"at ({place.X:0},{place.Y:0},{place.Z:0}) range {sound.Voice?.Range ?? 0f:0} gain {sound.Voice?.DistanceGain ?? 0f:0.0000}" : "flat") );

		return ScriptSoundStart.Started;
	}

	/// <summary>
	/// A placed script sound moved to <paramref name="at"/>, each coordinate cut to a whole number, as the sweep moves one
	/// on its script's every eighth tick (<c>FUN_005516b0</c>, <c>0x00551a1c</c>; <c>FUN_0051c270</c>). A flat one is not.
	/// </summary>
	internal void MoveScriptSound( ScriptVoice sound, Vector3 at )
	{
		if ( sound.Stopped || sound.Place is null )
			return;

		var whole = Whole( at );

		if ( whole == sound.Place )
			return;

		sound.Place = whole;
		sound.Voice?.MoveTo( whole );
		ScriptSounds = ScriptSounds with { Moved = ScriptSounds.Moved + 1 };
	}

	/// <summary>
	/// A script sound stopped: <c>Sound_Stop</c> for a <c>KILLOBJ</c> and a script's end, <c>Sound_StopFading</c> for a
	/// <c>FADEOBJ</c> (<paramref name="fade"/>), which fades here over <see cref="ScriptFadeSeconds"/>. A chain's newest
	/// child is cut either way: the chain has no channel to fade, and its stop cuts the children (<c>0x006bcc71</c>,
	/// <c>0x006bd9b0</c>); an older child plays out.
	/// </summary>
	internal void StopScriptSound( ScriptVoice sound, bool fade )
	{
		if ( sound.Stopped )
			return;

		sound.Stopped = true;

		if ( fade && !sound.Chain )
			sound.Voice?.FadeOut( ScriptFadeSeconds );
		else
			sound.Voice?.Stop();

		// A looped voice holds its effect in the category's own throttle (SoundCategory.Play).
		if ( sound.Looped )
			sound.Category.Release( sound.Effect );

		_scriptVoices.Remove( sound );
		ScriptSounds = fade
			? ScriptSounds with { Faded = ScriptSounds.Faded + 1 }
			: ScriptSounds with { Stopped = ScriptSounds.Stopped + 1 };

		Log.Info( $"Park audio: script {sound.Script}'s effect {sound.Effect} of type {sound.Type} {(fade ? sound.Chain ? "faded (cut, a chain)" : "faded" : "stopped")} after {sound.Samples} sample(s)" );
	}

	/// <summary>A place with each coordinate cut toward nought, as <c>__ftol</c> cuts it.</summary>
	private static Vector3 Whole( Vector3 at ) => new( MathF.Truncate( at.X ), MathF.Truncate( at.Y ), MathF.Truncate( at.Z ) );

	/// <summary>
	/// Each plain script voice's next sample once the last has ended and its silence is over, as a following voice's
	/// is (<see cref="ServeFollowing"/>), each chain's next child once the clock passes its newest child's time, and
	/// every one-shot served (<see cref="ServeOneShot"/>). A voice out of range makes nothing (<see cref="InRange"/>): a
	/// plain, music or one-shot voice's sample is cut, and in range again its sample starts afresh.
	/// </summary>
	internal void ServeScriptSounds()
	{
		for ( var index = _scriptVoices.Count - 1; index >= 0; --index )
		{
			var sound = _scriptVoices[index];

			if ( sound.Chain )
			{
				// The child before plays out: the chain has no channel to give up, and its children are not in the pass.
				if ( !Reached( sound ) )
					continue;

				if ( Time.Now > sound.NextAt )
					NextChainChild( sound );

				continue;
			}

			if ( !sound.Follows )
			{
				if ( ServeOneShot( sound ) )
					_scriptVoices.RemoveAt( index );

				continue;
			}

			if ( sound.Voice is { Playing: true } )
			{
				if ( !Reached( sound ) )
					sound.Voice.Stop();

				continue;
			}

			if ( sound.Parked )
				continue;

			// In range again, the tick starts a channel on the sample the voice holds, its volume and pitch drawn as a
			// first's are (0x006bdf50, its slot +0x44 FUN_006bc410), with no silence before it.
			if ( sound.Away )
			{
				if ( Reached( sound ) )
					SoundAgain( sound );

				continue;
			}

			if ( !Reached( sound ) )
				continue;

			sound.EndedAt ??= Time.Now;

			if ( Time.Now < sound.EndedAt + (sound.Wait / 1000f) )
				continue;

			NextScriptSample( sound );
		}
	}

	/// <summary>The time a one-shot is kept past its sample's length before it is dropped (<c>0x006bbf60</c>: 0xfa ms).</summary>
	internal const float OneShotGraceSeconds = 0.25f;

	/// <summary>How many one-shots were dropped by their time (<see cref="ScriptVoice.DropAt"/>).</summary>
	internal int ScriptOneShotsDropped { get; private set; }

	/// <summary>
	/// A one-shot's turn of the sound service (<c>FUN_006b62b0</c>), answering whether it is let go. Not looped, it is
	/// dropped once its time passes (<c>0x006bcb60</c>), in range or not, and a sample still playing plays out, as the
	/// drop moves the voice to the free list and stops no channel (<c>0x006b6440</c>, <c>0x006b6a50</c>). Ended by
	/// itself in range, it is let go (voice flag <c>0x10</c>, <c>0x006bbe40</c>). Out of range its sample is cut
	/// (<c>FUN_006bca70</c>); in range again it starts afresh from the top, a volume and a pitch drawn for it again
	/// (<c>0x006bd800</c>, which starts a channel whenever none plays). A looped one is never dropped by time.
	/// </summary>
	private bool ServeOneShot( ScriptVoice sound )
	{
		if ( !sound.Looped && Time.Now > sound.DropAt )
		{
			++ScriptOneShotsDropped;
			Log.Info( $"Park audio: script {sound.Script}'s {sound.Kind} {sound.Effect} dropped after {sound.Samples} sample(s), "
				+ $"its length and 250 ms past{(sound.Away ? ", out of range" : "")}" );

			return true;
		}

		if ( sound.Voice is { Playing: true } )
		{
			if ( !Reached( sound ) )
				sound.Voice.Stop();

			return false;
		}

		if ( !sound.Away )
			return true;

		if ( Reached( sound ) )
			StartOneShot( sound, again: true );

		return false;
	}

	/// <summary>
	/// A one-shot's held sample started (<c>0x006bd800</c>): at its variation's own volume and a pitch drawn from it
	/// (<see cref="OneShotGain"/>, <see cref="OneShotRate"/>; <c>FUN_006bc410</c>), looped under <c>0x8</c>.
	/// </summary>
	/// <param name="again">Whether it is in range again after a start or a stretch out of range, which the log says.</param>
	private void StartOneShot( ScriptVoice sound, bool again = false )
	{
		sound.Away = false;

		if ( sound.Clip is null )
			return;

		sound.Voice = sound.Category.PlaySample( sound.Effect, sound.Clip, sound.Variation, loop: sound.Looped,
			bus: AudioBus.Effects, position: sound.Place, ownRange: true, gain: OneShotGain, rate: OneShotRate );

		if ( sound.Voice is null )
			return;

		++sound.Samples;

		if ( again )
			Log.Info( $"Park audio: script {sound.Script}'s {sound.Kind} {sound.Effect} in range again: sample '{sound.Voice.Name}' "
				+ $"from the top (start {sound.Samples}), volume {sound.Voice.Volume:F4}, {sound.DropAt - Time.Now:F3} s before its drop" );
	}

	/// <summary>
	/// A plain or music voice's next sample: a plain voice's variation drawn among all of the effect's
	/// (<see cref="AnyVariation"/>), a music voice's among the zones of the one before that hold its parameter
	/// (<see cref="ParkScreams.NextVariation"/>, <c>0x006be450</c>), the first the effect's first; the volume and pitch
	/// its first drew (<see cref="HeldSample"/>), and after any but the first a silence drawn from the variation's wait
	/// (<c>0x006bdf00</c>). A music voice with no zone for its parameter is parked (<c>0x006be55e</c>).
	/// </summary>
	private void NextScriptSample( ScriptVoice sound )
	{
		var headers = sound.Category.VariationsOf( sound.Effect );
		var first = sound.Variation < 0;
		var next = sound.Music
			? ParkScreams.NextVariation( headers, sound.Variation, sound.Parameter, _scriptRandom )
			: AnyVariation( headers, sound.Variation, _scriptRandom );

		if ( next < 0 )
		{
			if ( sound.Music && !sound.Parked )
			{
				sound.Parked = true;
				sound.Parameter = ParkedParameter;
				Log.Info( $"Park audio: script {sound.Script}'s music {sound.Effect} parked: no zone of variation {sound.Variation + 1} holds its parameter" );
			}

			return;
		}

		sound.Variation = next;
		sound.Kept = HeldSample( headers[next], first, sound.Kept, 0, 0, _scriptRandom );
		sound.Wait = first ? 0 : ParkScreams.GapMilliseconds( headers[next], _scriptRandom );
		sound.EndedAt = null;
		sound.Clip = sound.Category.PickFrom( sound.Effect, next );

		// Started out of range, it holds its first variation and plays nothing until it is in range (see SoundAgain).
		if ( first && !Reached( sound ) )
			return;

		if ( !PlayScriptSample( sound ) )
			return;

		if ( !first )
			Log.Info( $"Park audio: script {sound.Script}'s effect {sound.Effect}, sample '{sound.Voice!.Name}' variation {next + 1} "
				+ (sound.Music ? $"at parameter {sound.Parameter} " : "")
				+ $"volume {sound.Kept.Volume} plays {sound.Voice.Volume:F4} pitch {sound.Kept.Pitch}, {sound.Wait} ms of silence after it" );
	}

	/// <summary>A plain or music voice's sample (<see cref="ScriptVoice.Clip"/>) played at its kept volume and pitch.</summary>
	private bool PlayScriptSample( ScriptVoice sound )
	{
		var headers = sound.Category.VariationsOf( sound.Effect );

		sound.Voice = Audio.Play( sound.Clip, ChannelGain( sound.Kept.Volume ), bus: AudioBus.Effects, position: sound.Place,
			range: sound.Place is null ? null : headers[sound.Variation].Range );

		if ( sound.Voice is null )
			return false;

		sound.Voice.SetRate( ParkCarSounds.Rate( sound.Kept.Pitch ) );
		++sound.Samples;

		return true;
	}

	/// <summary>
	/// A plain or music voice in range again: the sample it holds started afresh at once, a volume and a pitch drawn for
	/// it as for a first (<c>0x006bdf50</c>, <c>FUN_006bc410</c>). <b>A deviation:</b> one that left range in a silence
	/// plays here the sample that last played, where the original's has already queued the next (slot 0,
	/// <c>0x006bde00</c>); when the original queues it is not read.
	/// </summary>
	private void SoundAgain( ScriptVoice sound )
	{
		var headers = sound.Category.VariationsOf( sound.Effect );

		sound.Away = false;
		sound.EndedAt = null;
		sound.Kept = HeldSample( headers[sound.Variation], first: true, sound.Kept, 0, 0, _scriptRandom );

		if ( !PlayScriptSample( sound ) )
			return;

		Log.Info( $"Park audio: script {sound.Script}'s {sound.Kind} {sound.Effect} in range again: sample '{sound.Voice!.Name}' variation {sound.Variation + 1} afresh, "
			+ $"volume {sound.Kept.Volume} plays {sound.Voice.Volume:F4} pitch {sound.Kept.Pitch}" );
	}

	/// <summary>
	/// Whether a script voice is in range of the listener (<see cref="InRange"/>), with <see cref="ScriptVoice.Away"/> set
	/// as the original's <c>0x800</c> is (<c>FUN_006bca70</c>) and each change logged.
	/// </summary>
	private bool Reached( ScriptVoice sound )
	{
		var near = InRange( sound, Audio.Listener.Position );

		if ( near != sound.Away )
			return near;

		if ( !near )
		{
			sound.Away = true;
			Log.Info( $"Park audio: script {sound.Script}'s {sound.Kind} {sound.Effect} out of range at ({sound.Place!.Value.X:0},{sound.Place.Value.Y:0}), "
				+ $"variation {sound.Variation + 1} held after {sound.Samples} sample(s)" );
		}
		else if ( sound.Chain )
		{
			sound.Away = false;
			Log.Info( $"Park audio: script {sound.Script}'s {sound.Kind} {sound.Effect} in range again after {sound.Samples} child(ren)" );
		}

		return near;
	}

	/// <summary>
	/// Whether a script voice at <see cref="ScriptVoice.Place"/> is in range of <paramref name="ears"/>, as the sound
	/// service's pass asks of each voice (<c>FUN_006bca10</c>, from <c>FUN_006b62b0</c>): the squared distance over the
	/// ground's two axes alone (<c>FUN_006bb9f0</c>) not past the range's square, the range its current variation's
	/// (<c>FUN_006bc650</c>; a chain's is its newest child's, which the child's draw writes into it, <c>0x006c3e00</c>,
	/// and before its first the effect's first variation's). A flat voice is never out of range. Past it the original
	/// flags the voice <c>0x800</c> and gives its channel up (<c>FUN_006bca70</c>), and only a voice in range is ticked
	/// (slot <c>+0x14</c>): so it makes no child and draws no next sample.
	/// </summary>
	internal static bool InRange( ScriptVoice sound, Vector3 ears )
	{
		if ( sound.Place is not { } place )
			return true;

		var range = sound.Category.VariationsOf( sound.Effect )[Math.Max( sound.Variation, 0 )].Range;
		var x = place.X - ears.X;
		var y = place.Y - ears.Y;

		return x * x + y * y <= range * range;
	}

	/// <summary>The parameter a parked music voice is left at (<c>0x006be55e</c>); a set to it does not wake the voice.</summary>
	internal const int ParkedParameter = 0x7f;

	/// <summary>How many parameter sets reached a script voice.</summary>
	internal int ScriptParameterSets { get; private set; }

	/// <summary>
	/// <c>FUN_0051bc40( handle, id, level )</c> on a script's voice, for <c>SETOBJPARAM</c>: answers whether the voice still
	/// lives, which is whether the original's lookup (<c>0x006b6620</c>) finds it and answers its handle rather than
	/// nought. Every controller slot whose key is <paramref name="id"/>'s low byte takes <paramref name="level"/>'s
	/// (<c>0x006bbb80</c>); slot 0's key is the effect's own (<see cref="ScriptVoice.Key"/>), and on a music voice it is the
	/// zone parameter, a parked voice drawing again at a set to anything but <see cref="ParkedParameter"/>
	/// (<c>0x006be580</c>). A match past slot 0 applies volume and pitch again (<c>0x006bbbe0</c>), and a set on a voice of
	/// another class: both counted, as no shipped script reaches either (the bus sets its effect's own key 20 alone).
	/// </summary>
	internal bool SetScriptSoundParameter( ScriptVoice? sound, int id, int level )
	{
		if ( sound is null || sound.Stopped || !_scriptVoices.Contains( sound ) )
			return false;

		++ScriptParameterSets;
		var key = id & 0xff;
		var value = level & 0xff;

		if ( !sound.Music )
		{
			Unimplemented.Report( "RIDE_EFFECT_SOUND_PARAMETER_OTHER_CLASS" );
			return true;
		}

		var headers = sound.Category.VariationsOf( sound.Effect );
		var current = sound.Variation >= 0 && sound.Variation < headers.Count ? headers[sound.Variation] : (SoundCategoryFile.Variation?)null;

		// Slots 1 and 2 carry the current variation's keys, slot 3 the key nought (FUN_006bbf00).
		if ( key == 0 || (current is { } header && (header.FirstKey == key || header.SecondKey == key)) )
			Unimplemented.Report( "RIDE_EFFECT_SOUND_PARAMETER_APPLIES" );

		if ( key != sound.Key )
			return true;

		var before = sound.Parameter;
		sound.Parameter = value;

		if ( sound.Parked && value != ParkedParameter )
			sound.Parked = false;

		if ( value != before )
			Log.Info( $"Park audio: script {sound.Script}'s music {sound.Effect} parameter {key} set to {value} (was {before}), on variation {sound.Variation + 1}"
				+ (sound.Place is { } place ? $" at ({place.X:0},{place.Y:0},{place.Z:0})" : "") );

		return true;
	}

	/// <summary>
	/// A chain's next child (<c>0x006c3a80</c>): a variation, a sample of it at a volume and a pitch of its own
	/// (<c>0x006c3ae0</c>), where the chain stands now, and the next child its variation's wait after this one's start
	/// (<c>0x006bc2d0</c>). The child before plays out, as the original prunes it without a stop. A chain of any variation's
	/// child (class <c>0x0070a970</c>) draws by weight among all of the effect's (<c>FUN_006bc810</c> asked with 0, so with
	/// no rule against the one before); a held chain's (<c>0x0070a9e0</c>) takes the zones' rule (<c>0x006c3e00</c>), and
	/// with no zone for its parameter makes nothing, as the original parks it.
	/// </summary>
	private void NextChainChild( ScriptVoice sound )
	{
		var headers = sound.Category.VariationsOf( sound.Effect );
		var next = sound.Held
			? ParkScreams.NextVariation( headers, sound.Variation, sound.Parameter, _scriptRandom )
			: AnyVariation( headers, -1, _scriptRandom );

		if ( next < 0 )
			return;

		sound.Variation = next;
		sound.Kept = HeldSample( headers[next], first: true, sound.Kept, 0, 0, _scriptRandom );
		sound.Wait = ParkScreams.GapMilliseconds( headers[next], _scriptRandom );
		sound.NextAt = Time.Now + (sound.Wait / 1000f);
		sound.Voice = Audio.Play( sound.Category.PickFrom( sound.Effect, next ), ChannelGain( sound.Kept.Volume ),
			bus: AudioBus.Effects, position: sound.Place, range: sound.Place is null ? null : headers[next].Range );

		if ( sound.Voice is null )
			return;

		sound.Voice.SetRate( ParkCarSounds.Rate( sound.Kept.Pitch ) );
		++sound.Samples;

		Log.Info( $"Park audio: script {sound.Script}'s {(sound.Held ? "held chain" : "chain")} {sound.Effect}, child {sound.Samples} at {Time.Now:F3} s, sample '{sound.Voice.Name}' variation {next + 1} "
			+ $"volume {sound.Kept.Volume} plays {sound.Voice.Volume:F4} pitch {sound.Kept.Pitch}, the next in {sound.Wait} ms" );
	}

	/// <summary>Every script sound let go as the park ends.</summary>
	private void StopScriptSounds()
	{
		foreach ( var sound in _scriptVoices )
		{
			sound.Stopped = true;
			sound.Voice?.FadeOut( StopSeconds );

			if ( sound.Looped )
				sound.Category.Release( sound.Effect );
		}

		_scriptVoices.Clear();
	}

	/// <summary>The script sounds as they stand, for the console's <c>scriptsounds</c>.</summary>
	internal string ScriptSoundCensus()
	{
		var ears = Camera.Ears;
		var (started, absent, stopped, faded, moved) = ScriptSounds;
		var lines = new List<string>
		{
			$"listener ({ears.X:F0},{ears.Y:F0},{ears.Z:F0}), started {started}, absent {absent}, stopped {stopped}, faded {faded}, moved {moved}, one-shots dropped {ScriptOneShotsDropped}, sounding {_scriptVoices.Count}"
		};

		foreach ( var sound in _scriptVoices )
		{
			var voice = sound.Voice;
			var where = sound.Place is { } place
				? $"at ({place.X:F0},{place.Y:F0},{place.Z:F0}) {(place - ears).Length:F1} off range {voice?.Range ?? 0f:F0} gain {voice?.DistanceGain ?? 0f:F4}"
				: "flat";

			var music = sound.Music ? $" parameter {sound.Parameter} variation {sound.Variation + 1}{(sound.Parked ? " parked" : "")} pitch {sound.Kept.Pitch}" : "";

			lines.Add( $"script {sound.Script} type {sound.Type} effect {sound.Effect} {sound.Kind} "
				+ $"'{voice?.Name}' volume {voice?.Volume ?? 0f:F4} {(voice is { Playing: true } ? "playing" : "silent")}{(sound.Away ? " away" : "")} samples {sound.Samples}{music} {where}" );
		}

		return string.Join( "; ", lines );
	}
}
