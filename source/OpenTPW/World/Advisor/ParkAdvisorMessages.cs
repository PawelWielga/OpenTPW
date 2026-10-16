using System.Globalization;

namespace OpenTPW;

/// <summary>
/// The park's posted messages, selection mode 2: the door's two, a ride about to break down, a ride broken down
/// with no mechanic to come and a mechanic on his way to one, category 0, and three tutorial lines, category 1:
/// an opened tag message's, withdrawn as its bar leaves, and the buy screen's and the hire screen's own, each
/// withdrawn as its screen closes. The eight slots, strict tick threshold, helper recheck and per-message
/// histories follow FUN_0059a550, the gate FUN_0059abc0 and the withdrawal FUN_0059aa70;
/// docs/exe/advisor-park.md, Q90, "A ride about to break down", "A ride broken down and no mechanic to come",
/// "A mechanic on his way", "The line of an opened tag message", "The buy screen's own line" and "The hire
/// screen's own line".
/// <para>
/// Thirty messages are polled, not posted (FUN_0059c680, "The poll, and the control panel's line"): the control
/// panel's line and the tips', tutorial messages scored by the park's age; the line about a shut park people
/// wait outside, scored by how many wait ("A shut park people wait outside"); the lines about an open park with
/// nothing to ride and a park with no ride; five scored by the share of the park's rides, shops, sideshows,
/// toilets and staff rooms that are shut ("Shut shops, sideshows and toilets", "An open park's shut rides",
/// "A shut staff room"); the line about a queue not joined to a path ("A queue not joined to a path"); and the
/// lines about worn rides and about broken rides in a park with no mechanic ("Worn rides and no mechanic",
/// "Broken rides and no mechanic"); the line about more rides than the mechanics can keep ("More rides than the
/// mechanics can keep"); the lines about a poor selection of rides, of shops, of sideshows and
/// of features ("A poor selection of rides", "A poor selection of shops", "A poor selection of sideshows", "A
/// poor selection of features"); the lines about a park with rides and no shop, one with shops and no
/// sideshow and one with sideshows and no feature ("A park with rides and no shop", "A park with shops and no
/// sideshow", "A park with sideshows and no feature"); the line about a park with no toilet whose guests need
/// one ("A park with no toilet"); the line about a park with every kind of feature on offer built, told to
/// research more, and the one about every kind of sideshow, both Full Simulation's alone ("Every feature built,
/// and more to research", "Every sideshow built, and more to research"); and the lines
/// about too few
/// shops and too few sideshows for the park's size ("Too few shops for the park's size", "Too few sideshows for
/// the park's size"); and five about a kind of staff nobody has trained ("Staff nobody has trained"). The
/// other 124 polled rows are counted, and the other posted messages are unbuilt.
/// </para>
/// </summary>
internal sealed class ParkAdvisorMessages
{
	/// <summary>Message 1, polled: the line about a shut park people wait outside, scored by how many wait at the ticket booths (0x0059c8d0).</summary>
	internal const int OpenPark = 1;
	/// <summary>Message 2, polled: the line about an open park with nothing to ride, scored where neither a ride nor a sideshow stands (0x0059c960).</summary>
	internal const int ClosePark = 2;
	/// <summary>Message 5, polled: the line about worn rides in a park with no mechanic, scored by how many rides are worn or broken down (0x0059ca30).</summary>
	internal const int HireMechanics = 5;
	/// <summary>Message 6, polled: the line about rides breaking down in a park with no mechanic, scored by how many rides are broken down (0x0059ca80).</summary>
	internal const int HireMechanicsBroken = 6;
	/// <summary>Message 7, polled: the line about more rides than the mechanics can keep (0x0059cac0).</summary>
	internal const int HireMoreMechanics = 7;
	/// <summary>Message 0xa, polled: the line about a park too big for its handymen, scored by its developed cells over those they keep (0x0059cc20).</summary>
	internal const int HireMoreHandymen = 0xa;
	/// <summary>Message 0x1c, polled: the line about mechanics nobody has trained, scored where the park has one, their training budget is nought and their grades are low (0x0059d620).</summary>
	internal const int TrainMechanics = 0x1c;
	/// <summary>Message 0x1d, polled: the same of the handymen (0x0059d680).</summary>
	internal const int TrainHandymen = 0x1d;
	/// <summary>Message 0x1e, polled: the same of the guards (0x0059d6e0).</summary>
	internal const int TrainGuards = 0x1e;
	/// <summary>Message 0x1f, polled: the same of the entertainers (0x0059d740).</summary>
	internal const int TrainEntertainers = 0x1f;
	/// <summary>Message 0x20, polled: the same of the researchers (0x0059d7a0).</summary>
	internal const int TrainResearchers = 0x20;
	/// <summary>Message 0x3c, polled: the line about a poor selection of rides, scored where few of the kinds of ride on offer stand (0x0059d990).</summary>
	internal const int PoorSelection = 0x3c;
	internal const int PoorShopSelection = 0x47;
	/// <summary>Message 0x4b, polled: the line about a poor selection of sideshows, scored where few of the kinds of sideshow on offer stand (0x0059df30).</summary>
	internal const int PoorSideshowSelection = 0x4b;
	/// <summary>Message 0x4f, polled: the line about a poor selection of features, scored where few of the kinds of feature on offer stand (0x0059e1c0).</summary>
	internal const int PoorFeatureSelection = 0x4f;
	/// <summary>Message 0x49, polled: the line about a park with rides and no shop, scored by the park's age in days (0x0059dde0).</summary>
	internal const int BuildShops = 0x49;
	/// <summary>Message 0x4d, polled: the line about a park with shops and no sideshow, scored by the park's age in days (0x0059e070).</summary>
	internal const int BuildSideshows = 0x4d;
	/// <summary>Message 0x4c, polled: the line about a park with every kind of sideshow on offer built, scored where a researcher is on the staff, the lab holds a sideshow not yet researched and its sideshow research is getting no work (0x0059dfd0).</summary>
	internal const int ResearchMoreSideshows = 0x4c;
	/// <summary>Message 0x50, polled: the line about a park with every kind of feature on offer built, scored where a researcher is on the staff and the lab holds a feature not yet researched (0x0059e260).</summary>
	internal const int ResearchMoreFeatures = 0x50;
	/// <summary>Message 0x51, polled: the line about a park with sideshows and no feature, scored by the park's age in days (0x0059e2e0).</summary>
	internal const int BuildFeatures = 0x51;
	/// <summary>Message 0x52, polled: the line about a park with no toilet, scored by the share of the guests inside who need one (0x0059e380).</summary>
	internal const int NoToilets = 0x52;
	/// <summary>Message 0x4e, polled: the line about too few sideshows for the park's size, whose score sets the kinds of ride standing against the kinds of sideshow (0x0059e110).</summary>
	internal const int BuildMoreSideshows = 0x4e;
	/// <summary>Message 0x4a, polled: the line about too few shops for the park's size, whose score sets the kinds of shop standing against themselves (0x0059de80).</summary>
	internal const int BuildMoreShops = 0x4a;
	/// <summary>Row 62, polled: a park with no ride in it.</summary>
	internal const int BuildRides = 0x3e;
	/// <summary>Message 0x71, polled: the line about a queue not joined to a path, scored where a ride's queue is found unjoined (0x0059ef60).</summary>
	internal const int QueueUnconnected = 0x71;
	internal const int Opened = 128;
	internal const int Closed = 129;
	/// <summary>Message 0x42, posted by a ride's wear (FUN_004df670, 0x004df8be) with the ride as its subject.</summary>
	internal const int RideWornOut = 66;
	/// <summary>Message 0x44, posted by the mechanics' base as a ride breaks down in a park with no mechanic (FUN_004dadc0, 0x004dae62).</summary>
	internal const int NoMechanics = 68;
	/// <summary>Message 0x45, posted by the mechanics' base as a ride breaks down with every mechanic busy (FUN_004dadc0, 0x004daff0).</summary>
	internal const int MechanicsBusy = 69;
	/// <summary>Message 0x46, posted by the mechanic's decide (FUN_004da5b0, 0x004da6bf) with the ride he sets off for as its subject.</summary>
	internal const int MechanicOnHisWay = 70;
	/// <summary>Message 0x11c, posted as a tag's message is put on the bar (FUN_0048d9c0) and withdrawn as the bar leaves (FUN_0048e2f0, 0x1001d8).</summary>
	internal const int TagMessage = 284;
	/// <summary>Message 0xbc, posted as the buy screen is built (FUN_004acc70) and withdrawn as it is destroyed (FUN_004ac270, event 0x14).</summary>
	internal const int BuyScreen = 188;
	/// <summary>Message 0xbd, posted as the hire screen is built (FUN_0049bdd0) and withdrawn as it is destroyed (FUN_0049b650, event 0x14).</summary>
	internal const int HireScreen = 189;
	/// <summary>Message 0xb9, polled: the line about the control panel, scored by the park's age in days (0x005a06c0).</summary>
	internal const int ControlPanel = 185;
	/// <summary>Message 0x11a, polled: the line about the tips at the bottom of the screen, scored by the park's age in days (0x005a0720).</summary>
	internal const int ExplainTooltips = 282;
	/// <summary>Message 0x103, polled: the line about an open park's shut rides, scored by the share of at least a few rides that are shut (0x005a02f0).</summary>
	internal const int RidesClosed = 259;
	/// <summary>Message 0x104, polled: the line about shut shops, scored by the share of the shops that are shut (0x005a0370).</summary>
	internal const int ShopsClosed = 260;
	/// <summary>Message 0x105, polled: the line about shut sideshows, scored by the share of the sideshows that are shut (0x005a03d0).</summary>
	internal const int SideshowsClosed = 261;
	/// <summary>Message 0x106, polled: the line about shut toilets, scored by the share of the toilets that are shut (0x005a0430).</summary>
	internal const int ToiletsClosed = 262;
	/// <summary>Message 0x107, polled: the line about shut staff rooms, scored by the share of the staff rooms that are shut (0x005a0490).</summary>
	internal const int StaffroomClosed = 263;

	/// <summary>The tutorial messages' category (a row's +0x08): MessageGroups[1], refused while the Tutorial option is off (FUN_0059c610).</summary>
	internal const int Tutorial = 1;

	/// <summary>The category of the lines that tell the player to research more: MessageGroups[4], fifteen minutes between two sayings of one.</summary>
	internal const int ResearchMore = 4;

	/// <summary>
	/// A row of the metadata table 0x0076e300: the message, its score's key, its first response and that
	/// response's sample, how many lines it rotates (+0x28) and how many copies the slots may hold (+0x20).
	/// <paramref name="GestureRowLine"/> is the one line whose response names a row of the gesture table
	/// (the response table's +0x0c), -1 where every line's is the random chain. <paramref name="Prop"/> is what
	/// he wears for every line of it (the response table's +0x14; each built row's +0x18 is nought).
	/// <paramref name="Group"/> is the group of every line of it (the response table's +0x1c), the id of the tag
	/// a said line leaves (<see cref="ParkTags"/>). <paramref name="Category"/> is the row's +0x08, the
	/// MessageGroups entry of Advisor.sam the gate reads for it. <paramref name="Polled"/> is whether the row's
	/// +0x00 is nought, so that the poll scores it; a row's number is its message's id.
	/// <paramref name="AgeSection"/> names the Advisor.sam section whose AfterTime and CutoffTime bound the park's
	/// age in days for the score to be anything but nought, null where the score is the key's alone.
	/// <paramref name="PerWaitingGuest"/> is the score of row 1 (0x0059c8d0): the key's value for each guest
	/// waiting for a shut park to open, and nought for an open park or one with no ride and no sideshow.
	/// <paramref name="NothingToRide"/> is the score of row 2 (0x0059c960): the key's value in an open park with
	/// no ride and no sideshow, and nought otherwise.
	/// <paramref name="PerDayWithNoRide"/> makes the score row 62's (0x0059dad0): in a park where no ride stands
	/// the key's value and this key's for each whole day of the park's age, and nought where one stands.
	/// <paramref name="ShutShare"/> names the things whose shut share the score of rows 260 to 263 asks, and
	/// <paramref name="ShutSection"/> the Advisor.sam section whose MoreThanThisPercentClosed that share must be over.
	/// <paramref name="OpenWithAtLeast"/> is row 259's (0x005a02f0): the key of the least number of its things that
	/// must stand, in a park whose door is open, for the share to be asked at all; null where neither is asked.
	/// <paramref name="QueueNotJoined"/> is the score of row 113 (0x0059ef60): the key's value where the park
	/// holds a ride whose queue is not joined to a path, and nought otherwise.
	/// <paramref name="PerWornRideWithNoMechanic"/> is the score of row 5 (0x0059ca30): the key's value for each
	/// ride worn or broken down in a park with no mechanic, and nought in a park with one.
	/// <paramref name="PerBrokenRideWithNoMechanic"/> is the score of row 6 (0x0059ca80): the key's value for each
	/// ride broken down in a park with no mechanic, and nought in a park with one.
	/// <paramref name="RidesAMechanic"/> makes the score row 7's (0x0059cac0): it names the key of how many rides
	/// a mechanic keeps, and the score is the key's value for each ride over what the park's mechanics keep, and
	/// nought until <see cref="DaysForStaff"/> days after the newest member of staff was made.
	/// <paramref name="PoorSelection"/> makes the score row 60's (0x0059d990): it names the Advisor.sam section
	/// whose MoreRidesThan is the least number of rides that must stand and whose PoorerThan the share, in whole
	/// per cent, of the kinds of ride on offer that the kinds standing must be under for the key's value, and the
	/// score is nought until <see cref="DaysForObjects"/> days after the newest object was made.
	/// <paramref name="PoorOf"/> is whose kinds that score asks: the rides', or the shops' for row 71's
	/// (0x0059dca0), whose section's least number is MoreShopsThan, or the sideshows' for row 75's (0x0059df30),
	/// whose is MoreSideshowsThan, or the features' for row 79's (0x0059e1c0), whose is MoreFeaturesThan.
	/// <paramref name="PerDayWithNoShop"/> makes the score row 73's (0x0059dde0): in a park where a kind of ride
	/// stands and no kind of shop the key's value and this key's for each whole day of the park's age, and nought
	/// until <see cref="DaysForObjects"/> days after the newest object was made.
	/// <paramref name="PerDayWithNoSideshow"/> makes the score row 77's (0x0059e070): row 73's a kind on, in a
	/// park where a kind of shop stands and no kind of sideshow.
	/// <paramref name="PerDayWithNoFeature"/> makes the score row 81's (0x0059e2e0): row 77's a kind on, in a
	/// park where a kind of sideshow stands and no kind of feature.
	/// <paramref name="ShopsPerRide"/> makes the score row 74's (0x0059de80), and names its section: the key's
	/// value where a kind of shop stands, <see cref="DaysForObjects"/> days after the newest object was made, the
	/// kinds of shop standing times the section's ShopsPerRideRatio are more than the kinds of shop standing, and
	/// the bank holds more than the section's WealthMoreThan.
	/// <paramref name="SideshowsPerRide"/> makes the score row 78's (0x0059e110), and names its section: the key's
	/// value where a kind of sideshow stands, <see cref="DaysForObjects"/> days after the newest object was made,
	/// the kinds of ride standing times the section's SideshowsPerRideRatio are more than the kinds of sideshow
	/// standing, and the bank holds more than the section's WealthMoreThan.
	/// <paramref name="NeedingToiletWithNone"/> makes the score row 82's (0x0059e380), and names its section: the
	/// key's value in an open park where no toilet stands and more than the section's MorePeopleThanThisNeeding
	/// per cent, in whole numbers, of the guests inside have a toilet need over its NeedingMoreThan.
	/// <paramref name="EveryFeatureBuilt"/> is the score of row 80 (0x0059e260): the key's value in a park with a
	/// researcher where a kind of feature stands, the lab holds a project of a feature's kind that is not done, and
	/// the kinds of feature standing are as many as the kinds on offer; nought otherwise.
	/// <paramref name="EverySideshowBuilt"/> is the score of row 76 (0x0059dfd0): row 80's of the sideshows, and
	/// nought besides where the lab's sideshow research is getting any work at all.
	/// <paramref name="Untrained"/> makes the score rows 28 to 32's (0x0059d620 to 0x0059d7a0), and names its
	/// section: the key's value where a member of <paramref name="StaffKind"/>, a kind in
	/// <see cref="ParkStaffPool"/>'s order, is on the staff, the kind's monthly training budget is nought or less,
	/// and the kind's grades added up are under the section's WorseThan per cent, in whole numbers, of the top
	/// grade for every member; nought otherwise.
	/// <paramref name="SecondGestureRowLine"/> is a second line whose response names a row of the gesture table,
	/// -1 for none.
	/// <paramref name="CellsAHandyman"/> makes the score row 10's (0x0059cc20), and names its section: in an open park
	/// with a handyman and at least the section's MinCellsForConsideration developed cells, the key's value for each
	/// developed cell over the section's IdealDevelopedCellsPerHandyman for every handyman; nought otherwise.
	/// <paramref name="FullSimulationOnly"/> is whether the row's +0x0c is nought, so that the poll scores it in
	/// game type nought alone and never in Instant Action; every other built row's is 2, either game.
	/// </summary>
	internal sealed record Topic( int Id, string ScoreKey, int FirstResponse, int FirstSample, int Lines = 2, int MaxInstances = 1,
		int GestureRowLine = -1, int Prop = 0, int Group = ParkTags.NoTag, int Category = 0, bool Polled = false,
		string? AgeSection = null, bool PerWaitingGuest = false, bool NothingToRide = false, Shut ShutShare = Shut.None, string? ShutSection = null,
		string? PerDayWithNoRide = null, string? OpenWithAtLeast = null, bool QueueNotJoined = false,
		bool PerWornRideWithNoMechanic = false, bool PerBrokenRideWithNoMechanic = false, string? RidesAMechanic = null,
		string? PoorSelection = null, Shut PoorOf = Shut.Rides, string? PerDayWithNoShop = null, string? ShopsPerRide = null,
		string? PerDayWithNoSideshow = null, string? SideshowsPerRide = null, string? PerDayWithNoFeature = null,
		string? NeedingToiletWithNone = null, bool EveryFeatureBuilt = false, bool FullSimulationOnly = false,
		bool EverySideshowBuilt = false, string? Untrained = null, int StaffKind = 0, int SecondGestureRowLine = -1,
		string? CellsAHandyman = null );

	/// <summary>The key of the whole days since the newest hire that row 7's score waits for (the balance object's +0x630).</summary>
	internal const string DaysForStaff = "WaitingTimes.DaysForStaff";

	/// <summary>The key of the whole days since the newest object was made that rows 60, 71, 73 to 75, 77 to 79 and 81's scores wait for (the balance object's +0x634).</summary>
	internal const string DaysForObjects = "WaitingTimes.DaysForObjects";

	/// <summary>The things a row's score counts shut: none, the rides (0x005a02f0), the shops (0x005a0370), the sideshows (0x005a03d0), the toilets (0x005a0430) or the staff rooms (0x005a0490); the features are counted shut by none, and are a poor selection's alone (0x0059e1c0).</summary>
	internal enum Shut
	{
		None,
		Rides,
		Shops,
		Sideshows,
		Toilets,
		StaffRooms,
		Features
	}

	/// <summary>
	/// What row 1's score function reads of the running park: whether its door is shut (FUN_004c9050), how many
	/// things of a ride's item and of a sideshow's stand in it (FUN_004ca8e0 asked for kind 0 and kind 2), and how
	/// many guests wait for the opening on the two ticket booths' cells (FUN_004c91c0); and what rows 260 to 262's
	/// read: the things of a shop's item (FUN_004ca8e0 asked for kind 1) and the toilets (FUN_004c9ee0, an object's
	/// +0x32 &amp; 1), and how many of the shops, the sideshows and the toilets are shut (FUN_004ca9b0 and
	/// FUN_004c9f90: mCanLoad nought); and what row 259's reads: how many of the rides are shut (FUN_004ca9b0
	/// asked for kind 0); and what row 263's reads: the staff rooms (FUN_004ca030, an object's +0x32 &amp; 2) and how
	/// many of them are shut (FUN_004ca0c0); and what row 113's reads: the thing FUN_004ccb00 answers, a ride whose queue is not
	/// joined to a path, nought for none; and what row 5's reads: the mechanics (FUN_004c9960 asked for type 4: a
	/// thing of that type whose +3 is nought) and the rides worn (FUN_004ca3a0: a thing of a ride's item whose
	/// State of repair's whole number is under <see cref="WornBelow"/>, or which is broken down); and what row
	/// 6's reads: the rides broken down (FUN_004ca310: a thing of a ride's item whose mState is 1); and what row
	/// 7's reads: the whole days since the newest member of staff was made (FUN_004c90d0, from the analyser's
	/// mLastStaffHired); and what row 60's reads: the kinds of ride on offer (0x004ca450 asked with nought: a
	/// catalogue item of a ride's kind that is researched and costs no golden ticket), those of them of which a
	/// thing stands (0x004ca7b0 asked with nought and nought) and the whole days since the newest object was made
	/// (0x004c9070, from the analyser's mLastObjectBuilt); and what row 71's reads: the kinds of shop on offer
	/// (0x004ca510: a catalogue item of a shop's kind that is researched, a golden ticket's or not) and those of
	/// which a thing stands (0x004ca7b0 asked with one and nought, which passes over a golden ticket's); and what
	/// row 73's reads: the kinds of ride and of shop of which a thing stands and the days since the newest object;
	/// and what row 74's reads besides: the bank's balance (FUN_005195d0's thing's +0xc); and what row 75's
	/// reads: the kinds of sideshow on offer (0x004ca570: a catalogue item of a sideshow's kind that is researched,
	/// a golden ticket's or not) and those of which a thing stands (0x004ca7b0 asked with two and nought); and
	/// what row 79's reads: the things of a feature's item (FUN_004ca8e0 asked for kind 3), the kinds of feature
	/// on offer (0x004ca5d0: a catalogue item of a feature's kind that is researched, a golden ticket's or not)
	/// and those of which a thing stands (0x004ca7b0 asked with three and nought); and what row 82's reads: the
	/// guests inside the park (FUN_004c9130: a guest on a cell FUN_004fa990 passes) and those of them whose toilet
	/// need's whole number is over <see cref="ToiletNeedOver"/> (0x004c9290); and what row 80's reads: the
	/// researchers (FUN_004c9960 asked for type 8) and the lab's projects of a feature's kind that are not done
	/// (0x004ca690: FUN_00503b30 asked for kind 3 on the lab, a project whose mResearchDone is under its mCost);
	/// and what row 76's reads besides: the lab's projects of a sideshow's kind that are not done (0x004ca670,
	/// the same asked for kind 2) and how much of the lab's work that kind is getting (FUN_00503980 asked for
	/// kind 2: <see cref="ParkResearch.Rate"/>), with the two numbers the rate sets against each other, the
	/// staff's researchers' ability added up (FUN_004c9e60) and the most a park's could be (FUN_004c9ed0); and
	/// what rows 28 to 32's read: each kind of staff's <see cref="KindTraining"/>; and what row 10's reads besides: the
	/// developed cells (FUN_004c81b0, <see cref="ParkState.DevelopedCells"/>).
	/// </summary>
	internal readonly record struct ParkFacts( bool Closed, int Rides, int Sideshows, int WaitingToOpen,
		int Shops = 0, int Toilets = 0, int ShopsShut = 0, int SideshowsShut = 0, int ToiletsShut = 0, int RidesShut = 0,
		int UnjoinedQueue = 0, int StaffRooms = 0, int StaffRoomsShut = 0, int Mechanics = 0, int RidesWorn = 0, int RidesBroken = 0,
		int DaysSinceStaffHired = 0, int RideKindsOnOffer = 0, int RideKindsStanding = 0, int DaysSinceObjectBuilt = 0,
		int ShopKindsOnOffer = 0, int ShopKindsStanding = 0, int Balance = 0,
		int SideshowKindsOnOffer = 0, int SideshowKindsStanding = 0,
		int Features = 0, int FeatureKindsOnOffer = 0, int FeatureKindsStanding = 0,
		int GuestsInside = 0, int GuestsNeedingToilet = 0, int Researchers = 0, int FeatureProjectsUnfinished = 0,
		int SideshowProjectsUnfinished = 0, int SideshowResearchRate = 0, int ResearchAbility = 0, int MostResearchAbility = 0,
		StaffTraining Staff = default, int DevelopedCells = 0 )
	{
		/// <summary>How many of <paramref name="share"/>'s things stand, and how many of them are shut.</summary>
		internal (int Standing, int Shut) Of( Shut share ) => share switch
		{
			Shut.Rides => (Rides, RidesShut),
			Shut.Shops => (Shops, ShopsShut),
			Shut.Sideshows => (Sideshows, SideshowsShut),
			Shut.Toilets => (Toilets, ToiletsShut),
			Shut.StaffRooms => (StaffRooms, StaffRoomsShut),
			_ => (0, 0)
		};

		/// <summary>
		/// What a poor selection's score reads of <paramref name="of"/>: its kinds on offer and standing, how many
		/// of its things stand, the key of the least number of them in its section, and its word in the poll line.
		/// </summary>
		internal (int OnOffer, int Standing, int Things, string Least, string Word) Kinds( Shut of ) => of switch
		{
			Shut.Shops => (ShopKindsOnOffer, ShopKindsStanding, Shops, "MoreShopsThan", "shops"),
			Shut.Sideshows => (SideshowKindsOnOffer, SideshowKindsStanding, Sideshows, "MoreSideshowsThan", "sideshows"),
			Shut.Features => (FeatureKindsOnOffer, FeatureKindsStanding, Features, "MoreFeaturesThan", "features"),
			_ => (RideKindsOnOffer, RideKindsStanding, Rides, "MoreRidesThan", "rides")
		};
	}

	/// <summary>
	/// What rows 28 to 32's scores read of one kind of staff: its members (FUN_004c9960: a thing of the kind's type
	/// whose +3 is nought), its monthly training budget (the staff HQ's +0xc and on, a kind each) and its members'
	/// grades added up (FUN_004c9c00 adds each thing of the type's +0x1e4).
	/// </summary>
	internal readonly record struct KindTraining( int Members, int Budget, int Grades )
	{
		/// <summary>The grades as a whole per cent of the top grade for every member, nought with no member (FUN_004c9c00: the sum times 100 over five times the members).</summary>
		internal int GradeShare => Members == 0 ? 0 : Grades * 100 / (Members * ParkWorld.StaffState.PayGrades);
	}

	/// <summary>The five kinds of staff's <see cref="KindTraining"/>, in <see cref="ParkStaffPool"/>'s order.</summary>
	internal readonly record struct StaffTraining( KindTraining Handymen, KindTraining Mechanics, KindTraining Entertainers, KindTraining Guards, KindTraining Researchers )
	{
		internal KindTraining Of( int kind ) => kind switch
		{
			0 => Handymen,
			1 => Mechanics,
			2 => Entertainers,
			3 => Guards,
			4 => Researchers,
			_ => default
		};
	}

	/// <summary>The metadata table's rows, 0 to 0x15e.</summary>
	internal const int Rows = 351;

	/// <summary>
	/// The rows the poll scores, those whose +0x00 is nought in the running original's table (0x0076e300, filled
	/// before WinMain): 155 of the 351, so a round of the poll is 155 ticks.
	/// </summary>
	private static readonly (int First, int Last)[] PolledRanges =
	[
		(1, 32), (58, 65), (71, 82), (85, 92), (98, 98), (100, 105), (107, 124), (140, 141), (144, 160), (169, 173),
		(185, 185), (238, 257), (259, 265), (267, 269), (274, 276), (278, 282), (314, 314), (316, 317), (347, 350)
	];

	internal static bool IsPolled( int row )
	{
		foreach ( var (first, last) in PolledRanges )
			if ( row >= first && row <= last ) return true;
		return false;
	}

	/// <summary>How many rows are polled, the ticks of one round.</summary>
	internal static readonly int PolledRows = PolledRanges.Sum( range => range.Last - range.First + 1 );

	/// <summary>
	/// The row the next tick's poll starts from: the dword at 0x00fb3540, nought as the program starts
	/// (FUN_0059bea0), written to a park file as its advisor-scoring module (FUN_0059c890) and read back from it
	/// (FUN_0059c860), so a loaded park goes on from its file's row and a park made with no file from wherever the
	/// last park left it.
	/// </summary>
	internal sealed class PollCursor
	{
		public int Row;
	}

	/// <summary>The running program's own cursor.</summary>
	internal static readonly PollCursor ProgramCursor = new();

	/// <summary>The ticket hat, the door's lines' prop.</summary>
	internal const int TicketHat = 9;
	/// <summary>The hard hat, the prop of every line about a ride breaking down.</summary>
	internal const int HardHat = 5;
	/// <summary>The research hat, costume id 10.</summary>
	internal const int ResearchHat = 10;
	/// <summary>The handymen's cap, costume id 7: the prop of every line about their training.</summary>
	internal const int HandymanCap = 7;
	/// <summary>The guards' cap, costume id 6: the prop of every line about their training.</summary>
	internal const int GuardCap = 6;

	internal static readonly Topic[] DoorTopics =
	[
		new( Opened, "ParkNowOpen.Score", 308, 342, Prop: TicketHat, Group: 150 ),
		new( Closed, "ParkNowClosed.Score", 310, 344, Prop: TicketHat, Group: 151 ),
		new( OpenPark, "OpenPark.ScorePerWaitingPerson", 2, 1, Lines: 3, Prop: TicketHat, Group: 3, Polled: true, PerWaitingGuest: true ),
		new( ClosePark, "ClosePark.Score", 5, 4, Lines: 3, Prop: TicketHat, Group: 4, Polled: true, NothingToRide: true )
	];
	internal static readonly Topic[] ShutTopics =
	[
		new( RidesClosed, "RidesClosed.Score", 501, 484, Lines: 3, Prop: TicketHat, Polled: true, ShutShare: Shut.Rides, ShutSection: "RidesClosed", OpenWithAtLeast: "RidesClosed.AtLeastThisManyRides" ),
		new( ShopsClosed, "ShopsClosed.Score", 504, 487, Lines: 3, Prop: TicketHat, Polled: true, ShutShare: Shut.Shops, ShutSection: "ShopsClosed" ),
		new( SideshowsClosed, "SideshowsClosed.Score", 507, 490, Lines: 3, Prop: TicketHat, Polled: true, ShutShare: Shut.Sideshows, ShutSection: "SideshowsClosed" ),
		new( ToiletsClosed, "ToiletsClosed.Score", 510, 493, Lines: 3, Prop: TicketHat, Polled: true, ShutShare: Shut.Toilets, ShutSection: "ToiletsClosed" ),

		// Row 263's one response names sample 638, the bank's z_error.mp2, as thirteen other responses do: the line was
		// never recorded. No screen of the original shuts a staff room, so only a park file can make the row score
		// (docs/exe/advisor-park.md, "A shut staff room").
		new( StaffroomClosed, "StaffroomClosed.Score", 513, 638, Lines: 1, Polled: true, ShutShare: Shut.StaffRooms, ShutSection: "StaffroomClosed" )
	];
	internal static readonly Topic[] ParkTopics =
	[
		.. DoorTopics,
		.. ShutTopics,
		new( HireMechanics, "StaffHireMechanics1.ScorePerWornRide", 14, 13, Lines: 3, Prop: HardHat, Group: 7, Polled: true, PerWornRideWithNoMechanic: true ),
		new( HireMechanicsBroken, "StaffHireMechanics2.ScorePerBrokenRide", 17, 16, Lines: 3, GestureRowLine: 1, Prop: HardHat, Group: 8, Polled: true, PerBrokenRideWithNoMechanic: true ),
		new( HireMoreMechanics, "StaffHireMechanics3.ScorePerExtraRide", 20, 20, Lines: 3, Prop: HardHat, Group: 9, Polled: true, RidesAMechanic: "StaffHireMechanics3.IdealNumberOfRidesPerMechanic" ),
		new( HireMoreHandymen, "StaffHireHandymen3.ScorePerExtra10Cells", 29, 29, Lines: 3, Prop: HandymanCap, Group: 12, Polled: true, CellsAHandyman: "StaffHireHandymen3" ),

		// Rows 28 to 32, one a kind of staff in the table's own order. The entertainers' three responses name the
		// theme's own advisor model and its costume (docs/exe/advisor-park.md, "Staff nobody has trained").
		new( TrainMechanics, "StaffTrainMechanics.Score", 82, 80, Lines: 3, GestureRowLine: 0, SecondGestureRowLine: 2, Prop: HardHat, Group: 30, Polled: true, Untrained: "StaffTrainMechanics", StaffKind: 1 ),
		new( TrainHandymen, "StaffTrainHandymen.Score", 85, 83, Lines: 3, Prop: HandymanCap, Group: 31, Polled: true, Untrained: "StaffTrainHandymen", StaffKind: 0 ),
		new( TrainGuards, "StaffTrainGuards.Score", 88, 86, Lines: 3, GestureRowLine: 2, Prop: GuardCap, Group: 32, Polled: true, Untrained: "StaffTrainGuards", StaffKind: 3 ),
		new( TrainEntertainers, "StaffTrainEntertainers.Score", 91, 89, Lines: 3, Prop: AdvisorProps.ThemeCostume, Group: 33, Polled: true, Untrained: "StaffTrainEntertainers", StaffKind: 2 ),
		new( TrainResearchers, "StaffTrainResearchers.Score", 94, 92, Lines: 3, Prop: ResearchHat, Group: 34, Polled: true, Untrained: "StaffTrainResearchers", StaffKind: 4 ),
		new( PoorSelection, "RidesPoorSelection.Score", 176, 173, Lines: 3, Group: 62, Polled: true, PoorSelection: "RidesPoorSelection" ),
		new( PoorShopSelection, "ShopsPoorSelection.Score", 207, 205, Group: 73, Polled: true, PoorSelection: "ShopsPoorSelection", PoorOf: Shut.Shops ),
		new( PoorSideshowSelection, "SideshowsPoorSelection.Score", 215, 213, Group: 77, Polled: true, PoorSelection: "SideshowsPoorSelection", PoorOf: Shut.Sideshows ),

		// Row 79's two responses name the theme's own advisor model and its costume (docs/exe/advisor-park.md,
		// "A poor selection of features").
		new( PoorFeatureSelection, "FeaturesPoorSelection.Score", 223, 221, Prop: AdvisorProps.ThemeCostume, Group: 81, Polled: true, PoorSelection: "FeaturesPoorSelection", PoorOf: Shut.Features ),
		new( BuildShops, "ShopsBuildShops.Score", 211, 209, Prop: HardHat, Group: 75, Polled: true, PerDayWithNoShop: "ShopsBuildShops.AdditionalPerDay" ),
		new( BuildMoreShops, "ShopsBuildMoreShops.Score", 213, 211, Prop: HardHat, Group: 76, Polled: true, ShopsPerRide: "ShopsBuildMoreShops" ),
		new( BuildSideshows, "SideshowsBuildSideshows.Score", 219, 217, Prop: HardHat, Group: 79, Polled: true, PerDayWithNoSideshow: "SideshowsBuildSideshows.AdditionalPerDay" ),

		// Row 81's one response names the theme's own advisor model and its costume, as row 79's two do
		// (docs/exe/advisor-park.md, "A park with sideshows and no feature").
		new( BuildFeatures, "FeaturesBuildFeatures.Score", 226, 224, Lines: 1, Prop: AdvisorProps.ThemeCostume, Group: 83, Polled: true, PerDayWithNoFeature: "FeaturesBuildFeatures.AdditionalPerDay" ),
		new( NoToilets, "FeaturesNoToilets.Score", 227, 225, Lines: 3, Group: 84, Polled: true, NeedingToiletWithNone: "FeaturesNoToilets" ),
		new( ResearchMoreFeatures, "FeaturesResearchMore.Score", 225, 223, Lines: 1, Prop: ResearchHat, Group: 82, Category: ResearchMore, Polled: true, EveryFeatureBuilt: true, FullSimulationOnly: true ),
		new( ResearchMoreSideshows, "SideshowsResearchMore.Score", 217, 215, Prop: ResearchHat, Group: 78, Category: ResearchMore, Polled: true, EverySideshowBuilt: true, FullSimulationOnly: true ),
		new( BuildMoreSideshows, "SideshowsBuildMoreSideshows.Score", 221, 219, Prop: HardHat, Group: 80, Polled: true, SideshowsPerRide: "SideshowsBuildMoreSideshows" ),
		new( BuildRides, "RidesBuildRides.Score", 182, 179, Lines: 3, Prop: HardHat, Group: 64, Polled: true, PerDayWithNoRide: "RidesBuildRides.AdditionalPerDay" ),
		new( QueueUnconnected, "QueueUnconnected.Score", 288, 312, MaxInstances: 255, Group: 135, Polled: true, QueueNotJoined: true ),
		new( RideWornOut, "RidesBreakdownImminent.Score", 194, 191, Lines: 3, MaxInstances: 255, Prop: HardHat, Group: 68 ),
		new( NoMechanics, "RidesBreakdownNoMechanics.Score", 198, 196, Lines: 3, MaxInstances: 255, Prop: HardHat, Group: 70 ),
		new( MechanicsBusy, "RidesBreakdownBusyMechanics.Score", 201, 199, Lines: 3, MaxInstances: 255, Prop: HardHat, Group: 71 ),
		new( MechanicOnHisWay, "RidesBreakdownMechanicOnIt.Score", 204, 202, Lines: 3, MaxInstances: 255, GestureRowLine: 2, Prop: HardHat, Group: 72 ),
		new( TagMessage, "HelpMessage.Score", 544, 561, Lines: 1, Category: Tutorial ),
		new( BuyScreen, "HelpMessage.Score", 407, 380, Lines: 1, Category: Tutorial ),
		new( HireScreen, "HelpMessage.Score", 408, 381, Lines: 1, Category: Tutorial ),
		new( ControlPanel, "TutorialUIPanel.Score", 404, 377, Lines: 1, Category: Tutorial, Polled: true, AgeSection: "TutorialUIPanel" ),
		new( ExplainTooltips, "TutorialExplainTooltips.Score", 542, 559, Lines: 1, Category: Tutorial, Polled: true, AgeSection: "TutorialExplainTooltips" )
	];

	/// <summary>The two props <paramref name="response"/>'s row names, nought and nought for one of no built topic.</summary>
	internal static (int First, int Second) PropsOf( int response )
	{
		foreach ( var topic in ParkTopics )
		{
			if ( response >= topic.FirstResponse && response < topic.FirstResponse + topic.Lines )
				return (topic.Prop, 0);
		}

		return (0, 0);
	}

	/// <summary>
	/// The model slot <paramref name="response"/>'s row names (the response table's <c>+0x10</c>, low half): 1, the
	/// park's own advisor, for a built topic that wears the theme's costume, as every row with that prop is on
	/// that slot; nought for any other.
	/// </summary>
	internal static int SlotOf( int response )
		=> PropsOf( response ).First == AdvisorProps.ThemeCostume ? Advisor.ThemeSlot : 0;

	private sealed class History
	{
		public int LastTick;
		public int Line = -1;
		public bool Said;
		public int Slaps;
	}

	/// <summary>A taken slot. <paramref name="Secondary"/> is the record's second word where a park file gave it, null for a post made here.</summary>
	private sealed record Pending( Topic Topic, int Score, int Subject, int? Secondary = null );
	private readonly SettingsFile _settings;
	private readonly Dictionary<int, Topic> _topics;
	private readonly Dictionary<int, History> _histories;
	private readonly Pending?[] _slots = new Pending?[ParkWorld.AdvisorSlots];
	// The slots a park file's message this game cannot say stands in: taken, and never said or written over.
	private readonly bool[] _carried = new bool[ParkWorld.AdvisorSlots];
	private readonly Func<bool> _tutorial;
	private readonly Func<bool> _instantAction;
	private readonly PollCursor _cursor;
	// What the poll last answered for each polled message, so the log holds a line a change.
	private readonly Dictionary<int, string> _polled = [];
	// The tick of the last sweep or post, which the census scores each message at.
	private int _lastTick;
	// The last action (the advisor's +0xd8, +0xdc, +0x16ec): the moment the last line said began, how long he
	// waits from it, and the message it was. The moment is null until a line is said or a file's is taken.
	private long? _started;
	private int _duration;
	private int _playing;
	// The running park as the last sweep handed it, none before the first.
	private Func<ParkFacts>? _park;
	private long WaitUntil => (_started ?? 0) + _duration;
	internal int Attempts { get; private set; }
	internal int PendingCount => _slots.Count( slot => slot != null ) + _carried.Count( carried => carried );
	internal int Threshold => Value( "GeneralAdvisor.MinScoreForConsideration" );
	/// <summary>The State of repair a ride is worn under for row 5's score, the limit its function hands FUN_004ca3a0 ([ECX + 0xc0]).</summary>
	internal int WornBelow => Value( "StaffHireMechanics1.PoorerStateThan" );
	/// <summary>The toilet need a guest is over for row 82's score, the byte its function hands 0x004c9290 ([ECX + 0x2b8]'s low byte).</summary>
	internal int ToiletNeedOver => Value( "FeaturesNoToilets.NeedingMoreThan" ) & 0xff;

	/// <summary>
	/// The game type the debug console holds the poll to, whatever the player's is: true Instant Action's, false
	/// Full Simulation's, null the player's own. An instrument: nothing in the game sets it.
	/// </summary>
	internal bool? InstantActionHeld { get; set; }

	/// <summary>Whether the poll takes the game for Instant Action's.</summary>
	internal bool InstantAction => InstantActionHeld ?? _instantAction();

	/// <param name="tutorial">Whether the Tutorial option is on (0x0078d90d), the player's own where none is given.</param>
	/// <param name="cursor">The poll's cursor; the game hands every park <see cref="ProgramCursor"/>, and with none given the poll starts at row nought.</param>
	/// <param name="instantAction">Whether the game type is Instant Action's (0x00fb3b7c reads 2), the player's own where none is given.</param>
	internal ParkAdvisorMessages( SettingsFile settings, IEnumerable<Topic>? topics = null, Func<bool>? tutorial = null,
		PollCursor? cursor = null, Func<bool>? instantAction = null )
	{
		_instantAction = instantAction ?? (() => Level.InstantAction);
		_settings = settings;
		_cursor = cursor ?? new PollCursor();
		_tutorial = tutorial ?? (() => GameOptions.Current.Tutorial);
		_topics = (topics ?? ParkTopics).ToDictionary( topic => topic.Id );
		_histories = _topics.Keys.ToDictionary( id => id, _ => new History() );
	}

	/// <summary>
	/// Takes each built message's record from a park file's advisor (<paramref name="saved"/> answers null for
	/// one the file does not hold, which stays as it is), as the original's advisor is read back whole with its
	/// thing. Says how many records were taken that the constructor would not have left.
	/// </summary>
	internal int Restore( Func<int, ParkWorld.AdvisorHistory?> saved )
	{
		var taken = 0;
		foreach ( var (id, history) in _histories )
		{
			if ( saved( id ) is not { } record ) continue;
			history.LastTick = record.LastTick;
			history.Line = record.Line;
			history.Said = record.Said;
			history.Slaps = record.Slaps;
			if ( record == ParkWorld.AdvisorHistory.Fresh ) continue;
			++taken;
			Log.Info( $"Park advisor: restored message={id} line={record.Line} lastTick={record.LastTick} said={(record.Said ? 1 : 0)} slaps={record.Slaps}" );
		}

		return taken;
	}

	/// <summary>Each built message's record as a park file's advisor holds it (<see cref="ParkWorld.PutAdvisorHistories"/>).</summary>
	internal IReadOnlyDictionary<int, ParkWorld.AdvisorHistory> Written()
		=> _histories.ToDictionary( pair => pair.Key,
			pair => new ParkWorld.AdvisorHistory( pair.Value.LastTick, pair.Value.Line, pair.Value.Said, pair.Value.Slaps ) );

	/// <summary>
	/// Takes a park file's waiting messages into the slots they stand in (<paramref name="saved"/> answers null
	/// for a file with no advisor), as the original's advisor is read back whole with its thing: a built message
	/// with its file's score, subject and second word. A taken slot this game cannot say - a message of no built
	/// topic, a line asked for by number, a forced post or a score the poster gave - is counted and stays taken,
	/// said by nothing and written back as the file's. Says how many were taken to be said.
	/// </summary>
	internal int RestoreSlots( Func<int, ParkWorld.AdvisorSlot?> saved )
	{
		var taken = 0;
		for ( var index = 0; index < _slots.Length; ++index )
		{
			if ( saved( index ) is not { Valid: true } slot ) continue;
			if ( !_topics.TryGetValue( slot.Message, out var topic ) || slot.Line >= 0 || slot.OverrideOnlyOnce || slot.ScoreOverridden )
			{
				Unimplemented.Report( "PARK_ADVISOR_SAVED_SLOT_NOT_BUILT" );
				_carried[index] = true;
				Log.Info( $"Park advisor: carried slot={index} message={slot.Message} subject={slot.Thing} score={slot.Score}" );
				continue;
			}

			_slots[index] = new Pending( topic, slot.Score, slot.Thing, slot.Secondary );
			++taken;
			Log.Info( $"Park advisor: restored slot={index} message={slot.Message} subject={slot.Thing} secondary={slot.Secondary} score={slot.Score}" );
		}

		return taken;
	}

	/// <summary>
	/// The eight slots as a park file's advisor holds them (<see cref="ParkWorld.PutAdvisorSlots"/>): a waiting
	/// message as FUN_0059b590 builds an unforced post with no score given, its second word the file's or, for
	/// one posted here about a thing, that thing's item by <paramref name="itemOf"/>; an empty slot not taken;
	/// null for a slot a file's unbuilt message stands in.
	/// </summary>
	internal IReadOnlyList<ParkWorld.AdvisorSlot?> WrittenSlots( Func<int, int> itemOf )
	{
		var written = new ParkWorld.AdvisorSlot?[_slots.Length];
		for ( var index = 0; index < _slots.Length; ++index )
		{
			if ( _carried[index] ) continue;
			written[index] = _slots[index] is { } slot
				? new ParkWorld.AdvisorSlot( slot.Subject, slot.Secondary ?? (slot.Subject != 0 ? itemOf( slot.Subject ) : 0),
					slot.Topic.Id, slot.Score, true, -1, false, false )
				: default( ParkWorld.AdvisorSlot );
		}

		return written;
	}

	/// <summary>
	/// Takes a park file's last action, as the original's advisor is read back whole and its second stopwatch
	/// made to read the file's again (FUN_00402e80): the wait a file was saved inside goes on for what was left
	/// of it, and a withdrawal inside it still finds the message he was saying. <paramref name="stopwatch"/> is
	/// the file's second stopwatch and <paramref name="milliseconds"/> the load's moment here. Says how many
	/// milliseconds of the wait are left, nought or less for one that is over.
	/// </summary>
	internal long RestoreAction( ParkWorld.AdvisorAction saved, uint stopwatch, long milliseconds )
	{
		_started = milliseconds + unchecked((int)(saved.Started - stopwatch));
		_duration = unchecked((int)saved.Duration);
		_playing = saved.Playing;
		var left = WaitUntil - milliseconds;
		Log.Info( $"Park advisor: restored action started={saved.Started} duration={saved.Duration} playing={saved.Playing} stopwatch={stopwatch} at={milliseconds} left={left}" );
		return left;
	}

	/// <summary>
	/// The last action as a park file's advisor holds it (<see cref="ParkWorld.PutAdvisorAction"/>), its start a
	/// reading of the second stopwatch, which reads <paramref name="stopwatch"/> at <paramref name="milliseconds"/>
	/// here. Before any line is said and with no file's taken it is the constructor's, noughts (FUN_00599e70).
	/// </summary>
	internal ParkWorld.AdvisorAction WrittenAction( uint stopwatch, long milliseconds )
		=> _started is { } started
			? new ParkWorld.AdvisorAction( unchecked(stopwatch - (uint)(int)(milliseconds - started)), unchecked((uint)_duration), _playing )
			: default;

	// Missing or malformed settings are installation errors, not permission to invent scores.
	private int Value( string key ) => int.Parse( _settings[key], CultureInfo.InvariantCulture );

	/// <summary>A key the balance object holds as a float.</summary>
	private float Fraction( string key ) => float.Parse( _settings[key], CultureInfo.InvariantCulture );

	/// <summary>
	/// Whole days since mGameTick nought: FUN_004f8800 over 0xC92A69C000, the tick times
	/// <see cref="GameCalendar.Rate"/> over four, in whole seconds, whatever date the calendar began on.
	/// </summary>
	internal static int DaysSinceTheFirstSweep( int tick )
		=> (int)((long)(uint)tick * GameCalendar.Rate / GameCalendar.AdvancesPerSecond / 86400);

	/// <summary>
	/// A row's score function (+0x10). The control panel's (0x005a06c0) and the tips' (0x005a0720) are their
	/// section's Score while the park is more than AfterTime and less than CutoffTime days old, and nought
	/// otherwise; row 1's (0x0059c8d0) is its key's value for each guest waiting at the booths of a shut park
	/// that has a ride or a sideshow, and nought otherwise; row 2's (0x0059c960) is its key's value in an open
	/// park with neither, and nought otherwise; row 62's (0x0059dad0) is its key's value and its per-day key's
	/// for each whole day of the park's age where no ride stands, whatever the door and the sideshows, and nought
	/// where one does; rows 260 to 263's (0x005a0370, 0x005a03d0, 0x005a0430, 0x005a0490)
	/// are their key's value where any of their things stand and more than the section's
	/// MoreThanThisPercentClosed per cent of them, in whole numbers, are shut, whatever the park's door, and nought
	/// otherwise; row 259's (0x005a02f0) is the same of the rides, and nought besides in a shut park or one with
	/// fewer rides than its section's AtLeastThisManyRides; row 113's (0x0059ef60) is its key's value where a
	/// ride's queue is not joined to a path, and nought otherwise; row 5's (0x0059ca30) is its key's value for
	/// each ride worn or broken down in a park with no mechanic, and nought in a park with one; row 6's
	/// (0x0059ca80) is its key's value for each ride broken down in a park with no mechanic, and nought in a
	/// park with one; row 7's (0x0059cac0) is nought until DaysForStaff days after the newest member of staff
	/// was made, and then its key's value for each ride over those its mechanics keep, where there is one;
	/// row 10's (0x0059cc20) is nought in a shut park, with no handyman and with fewer developed cells than its
	/// section's MinCellsForConsideration, and then its key's value for each developed cell over those its handymen keep;
	/// row 60's (0x0059d990) is nought where no kind of ride is on offer, until DaysForObjects days after the
	/// newest object was made, and with fewer rides than its section's MoreRidesThan, and then its key's value
	/// where the kinds standing are under its section's PoorerThan per cent, in whole numbers, of those on offer;
	/// row 71's (0x0059dca0) is the same of the shops, its least number its section's MoreShopsThan, and row
	/// 75's (0x0059df30) the same of the sideshows, its least number its section's MoreSideshowsThan, and row
	/// 79's (0x0059e1c0) the same of the features, its least number its section's MoreFeaturesThan;
	/// row 73's (0x0059dde0) is nought where no kind of ride stands, until DaysForObjects days after the newest
	/// object was made, and where a kind of shop stands, and then its key's value and its per-day key's for each
	/// whole day of the park's age; row 77's (0x0059e070) is the same a kind on, nought where no kind of shop
	/// stands and where a kind of sideshow does, and row 81's (0x0059e2e0) the same a kind on again, nought where
	/// no kind of sideshow stands and where a kind of feature does;
	/// row 74's (0x0059de80) is nought where no kind of shop stands and until DaysForObjects days after the newest
	/// object was made, nought unless the kinds of shop standing times its section's ShopsPerRideRatio are more
	/// than the kinds of shop standing, which the shipped ratio, 0.15, never is, and nought unless the bank holds
	/// more than its section's WealthMoreThan, and then its key's value;
	/// row 78's (0x0059e110) is nought where no kind of sideshow stands and until DaysForObjects days after the
	/// newest object was made, nought unless the kinds of ride standing times its section's SideshowsPerRideRatio
	/// are more than the kinds of sideshow standing, and nought unless the bank holds more than its section's
	/// WealthMoreThan, and then its key's value;
	/// row 82's (0x0059e380) is nought in a shut park, where a toilet stands and with no guest inside, and then its
	/// key's value where the guests inside whose toilet need is over its section's NeedingMoreThan are more than its
	/// section's MorePeopleThanThisNeeding per cent of them, in whole numbers;
	/// row 80's (0x0059e260) is nought with no researcher, with no kind of feature standing, with no project of a
	/// feature's kind left undone in the lab, and unless the kinds of feature standing are as many as the kinds
	/// on offer, and then its key's value;
	/// rows 28 to 32's (0x0059d620 to 0x0059d7a0) are nought with no member of their kind of staff, with a training
	/// budget for the kind above nought, and unless the kind's grades are under its section's WorseThan per cent
	/// of the top grade's, in whole numbers, and then its key's value;
	/// every other built row's is its key's value.
	/// </summary>
	internal int ScoreOf( Topic topic, int gameTick )
	{
		if ( topic.PerWaitingGuest )
		{
			if ( _park?.Invoke() is not { Closed: true } park || (park.Rides < 1 && park.Sideshows < 1) ) return 0;
			return park.WaitingToOpen * Value( topic.ScoreKey );
		}

		if ( topic.NothingToRide )
		{
			if ( _park?.Invoke() is not { Closed: false } park || park.Rides >= 1 || park.Sideshows >= 1 ) return 0;
			return Value( topic.ScoreKey );
		}

		if ( topic.PerDayWithNoRide is { } perDay )
		{
			if ( _park?.Invoke() is not { Rides: < 1 } ) return 0;
			return Value( topic.ScoreKey ) + Value( perDay ) * DaysSinceTheFirstSweep( gameTick );
		}

		if ( topic.PerDayWithNoShop is { } shopless )
		{
			if ( _park?.Invoke() is not { RideKindsStanding: > 0, ShopKindsStanding: < 1 } bare ) return 0;
			if ( bare.DaysSinceObjectBuilt < Value( DaysForObjects ) ) return 0;
			return Value( topic.ScoreKey ) + Value( shopless ) * DaysSinceTheFirstSweep( gameTick );
		}

		if ( topic.PerDayWithNoSideshow is { } showless )
		{
			if ( _park?.Invoke() is not { ShopKindsStanding: > 0, SideshowKindsStanding: < 1 } plain ) return 0;
			if ( plain.DaysSinceObjectBuilt < Value( DaysForObjects ) ) return 0;
			return Value( topic.ScoreKey ) + Value( showless ) * DaysSinceTheFirstSweep( gameTick );
		}

		if ( topic.PerDayWithNoFeature is { } featureless )
		{
			if ( _park?.Invoke() is not { SideshowKindsStanding: > 0, FeatureKindsStanding: < 1 } drab ) return 0;
			if ( drab.DaysSinceObjectBuilt < Value( DaysForObjects ) ) return 0;
			return Value( topic.ScoreKey ) + Value( featureless ) * DaysSinceTheFirstSweep( gameTick );
		}

		if ( topic.NeedingToiletWithNone is { } unrelieved )
		{
			if ( _park?.Invoke() is not { Closed: false, Toilets: 0, GuestsInside: not 0 } crossed ) return 0;
			return crossed.GuestsNeedingToilet * 100 / crossed.GuestsInside > Value( $"{unrelieved}.MorePeopleThanThisNeeding" ) ? Value( topic.ScoreKey ) : 0;
		}

		if ( topic.EveryFeatureBuilt )
		{
			if ( _park?.Invoke() is not { Researchers: > 0, FeatureKindsStanding: > 0, FeatureProjectsUnfinished: > 0 } built ) return 0;
			return built.FeatureKindsStanding == built.FeatureKindsOnOffer ? Value( topic.ScoreKey ) : 0;
		}

		if ( topic.EverySideshowBuilt )
		{
			if ( _park?.Invoke() is not { Researchers: > 0, SideshowKindsStanding: > 0, SideshowProjectsUnfinished: > 0, SideshowResearchRate: 0 } shown ) return 0;
			return shown.SideshowKindsStanding == shown.SideshowKindsOnOffer ? Value( topic.ScoreKey ) : 0;
		}

		if ( topic.Untrained is { } untrained )
		{
			if ( _park?.Invoke() is not { } employing || employing.Staff.Of( topic.StaffKind ) is not { Members: > 0, Budget: <= 0 } kind ) return 0;
			return kind.GradeShare < Value( $"{untrained}.WorseThan" ) ? Value( topic.ScoreKey ) : 0;
		}

		if ( topic.ShopsPerRide is { } crowded )
		{
			if ( _park?.Invoke() is not { } sized || sized.DaysSinceObjectBuilt < Value( DaysForObjects ) ) return 0;
			// Both sides are the kinds of shop standing: the original asks 0x004ca7b0 for them twice, and no ride is
			// counted. Its first test, nought with no kind standing, is this one's answer too: nought is not more than nought.
			if ( !(sized.ShopKindsStanding * (double)Fraction( $"{crowded}.ShopsPerRideRatio" ) > sized.ShopKindsStanding) ) return 0;
			return sized.Balance > Value( $"{crowded}.WealthMoreThan" ) ? Value( topic.ScoreKey ) : 0;
		}

		if ( topic.SideshowsPerRide is { } showy )
		{
			if ( _park?.Invoke() is not { SideshowKindsStanding: > 0 } fair || fair.DaysSinceObjectBuilt < Value( DaysForObjects ) ) return 0;
			// The ratio is a float and the product a double, as the x87 takes it: five kinds times the shipped 0.2 are over one.
			if ( !(fair.RideKindsStanding * (double)Fraction( $"{showy}.SideshowsPerRideRatio" ) > fair.SideshowKindsStanding) ) return 0;
			return fair.Balance > Value( $"{showy}.WealthMoreThan" ) ? Value( topic.ScoreKey ) : 0;
		}

		if ( topic.PerWornRideWithNoMechanic )
			return _park?.Invoke() is { Mechanics: < 1 } worn ? worn.RidesWorn * Value( topic.ScoreKey ) : 0;

		if ( topic.PerBrokenRideWithNoMechanic )
			return _park?.Invoke() is { Mechanics: < 1 } broken ? broken.RidesBroken * Value( topic.ScoreKey ) : 0;

		if ( topic.RidesAMechanic is { } kept )
		{
			if ( _park?.Invoke() is not { } staffed || staffed.DaysSinceStaffHired < Value( DaysForStaff ) ) return 0;
			var over = staffed.Rides - staffed.Mechanics * Value( kept );
			return over > 0 ? over * Value( topic.ScoreKey ) : 0;
		}

		if ( topic.CellsAHandyman is { } swept )
		{
			// The key names extra ten cells; the original's score multiplies it by every cell over, with no division (0x0059ccab).
			if ( _park?.Invoke() is not { Closed: false } big || big.Staff.Handymen.Members < 1 ) return 0;
			if ( big.DevelopedCells < Value( $"{swept}.MinCellsForConsideration" ) ) return 0;
			var over = big.DevelopedCells - big.Staff.Handymen.Members * Value( $"{swept}.IdealDevelopedCellsPerHandyman" );
			return over > 0 ? over * Value( topic.ScoreKey ) : 0;
		}

		if ( topic.PoorSelection is { } selection )
		{
			if ( _park?.Invoke() is not { } offered ) return 0;
			var kinds = offered.Kinds( topic.PoorOf );
			if ( kinds.OnOffer <= 0 || offered.DaysSinceObjectBuilt < Value( DaysForObjects ) ) return 0;
			if ( kinds.Things < Value( $"{selection}.{kinds.Least}" ) ) return 0;
			return kinds.Standing * 100 / kinds.OnOffer < Value( $"{selection}.PoorerThan" ) ? Value( topic.ScoreKey ) : 0;
		}

		if ( topic.QueueNotJoined )
			return _park?.Invoke() is { UnjoinedQueue: not 0 } ? Value( topic.ScoreKey ) : 0;

		if ( topic.ShutShare != Shut.None )
		{
			var (standing, shut) = _park?.Invoke().Of( topic.ShutShare ) ?? (0, 0);
			if ( topic.OpenWithAtLeast is { } least && (_park?.Invoke() is not { Closed: false } || standing < Value( least )) ) return 0;
			if ( standing == 0 || shut * 100 / standing <= Value( $"{topic.ShutSection}.MoreThanThisPercentClosed" ) ) return 0;
			return Value( topic.ScoreKey );
		}

		if ( topic.AgeSection is { } section )
		{
			var days = DaysSinceTheFirstSweep( gameTick );
			if ( days <= Value( $"{section}.AfterTime" ) || days >= Value( $"{section}.CutoffTime" ) ) return 0;
		}

		return Value( topic.ScoreKey );
	}

	internal bool PostDoor( bool closed, int gameTick ) => Post( closed ? Closed : Opened, gameTick );

	/// <summary>
	/// The accept gate, FUN_0059abc0 with nothing forced: null where it lets <paramref name="topic"/> by, and
	/// otherwise why not, in its own order: a tutorial message with the Tutorial option off; one said before and
	/// inside its category's MinTimeSameMessage; one said, of a category said only once; one cut off as often as
	/// its category's DiscardAfterSlaps, where that is not nought; one with all its copies in the slots.
	/// </summary>
	private string? Refusal( Topic topic, int gameTick )
	{
		var history = _histories[topic.Id];
		var group = $"MessageGroups[{topic.Category}]";
		var slaps = Value( $"{group}.DiscardAfterSlaps" );
		if ( topic.Category == Tutorial && !_tutorial() )
			return "tutorial-off";
		if ( (uint)history.LastTick >> 2 != 0
			&& unchecked(((uint)gameTick >> 2) - ((uint)history.LastTick >> 2))
				< (uint)Value( $"{group}.MinTimeSameMessage" ) )
			return "cooldown";
		if ( history.Said && Value( $"{group}.SayOnlyOnce" ) != 0 )
			return "said-once";
		if ( slaps != 0 && slaps <= history.Slaps )
			return "slapped";
		if ( _slots.Count( slot => slot?.Topic.Id == topic.Id ) >= topic.MaxInstances )
			return "duplicate";
		return null;
	}

	/// <summary>What the poll last answered for <paramref name="id"/>, null before its row's first turn.</summary>
	internal string? PollAnswer( int id ) => _polled.GetValueOrDefault( id );

	/// <summary>The row the next poll starts from, as a park file's advisor-scoring module holds it.</summary>
	internal int PollRow => _cursor.Row;

	/// <summary>
	/// Takes a park file's poll cursor (<see cref="ParkWorld.AdvisorPollCursorOf"/>), as the load reads the dword
	/// back into the program's own (FUN_0059c860). A row below nought, which the original would read the table
	/// before its start by, is refused. Says whether it was taken.
	/// </summary>
	internal bool RestoreCursor( int row )
	{
		if ( row < 0 )
		{
			Log.Warning( $"Park advisor: the file's poll cursor {row} is below the table, so the cursor stays on row {_cursor.Row}" );
			return false;
		}

		Log.Info( $"Park advisor: restored poll cursor row={row} (it stood on {_cursor.Row})" );
		_cursor.Row = row;
		return true;
	}

	/// <summary>What <paramref name="topic"/>'s score read of the park, as its poll line ends: nothing for a row that reads none.</summary>
	internal string PollFacts( Topic topic )
	{
		var shut = topic.ShutShare != Shut.None && _park?.Invoke().Of( topic.ShutShare ) is var (standing, closed) ? $" shut={closed}/{standing}" : "";
		if ( topic.OpenWithAtLeast != null && _park?.Invoke() is { } door ) shut += $" closed={(door.Closed ? 1 : 0)}";
		if ( topic.NothingToRide && _park?.Invoke() is { } facts ) shut = $" closed={(facts.Closed ? 1 : 0)} rides={facts.Rides} sideshows={facts.Sideshows}";
		if ( topic.PerDayWithNoRide != null && _park?.Invoke() is { } park ) shut = $" rides={park.Rides}";
		if ( topic.QueueNotJoined && _park?.Invoke() is { } queues ) shut = $" unjoined={queues.UnjoinedQueue}";
		if ( topic.PerWornRideWithNoMechanic && _park?.Invoke() is { } wear ) shut = $" worn={wear.RidesWorn} mechanics={wear.Mechanics}";
		if ( topic.PerBrokenRideWithNoMechanic && _park?.Invoke() is { } breaks ) shut = $" broken={breaks.RidesBroken} mechanics={breaks.Mechanics}";
		if ( topic.RidesAMechanic != null && _park?.Invoke() is { } staffed ) shut = $" rides={staffed.Rides} mechanics={staffed.Mechanics} sincehire={staffed.DaysSinceStaffHired}";
		if ( topic.CellsAHandyman != null && _park?.Invoke() is { } big ) shut = $" closed={(big.Closed ? 1 : 0)} handymen={big.Staff.Handymen.Members} cells={big.DevelopedCells}";
		if ( topic.PoorSelection != null && _park?.Invoke() is { } offered )
		{
			var kinds = offered.Kinds( topic.PoorOf );
			shut = $" kinds={kinds.Standing}/{kinds.OnOffer} {kinds.Word}={kinds.Things} sincebuild={offered.DaysSinceObjectBuilt}";
		}
		if ( topic.PerDayWithNoShop != null && _park?.Invoke() is { } bare )
			shut = $" ridekinds={bare.RideKindsStanding} shopkinds={bare.ShopKindsStanding} sincebuild={bare.DaysSinceObjectBuilt}";
		if ( topic.PerDayWithNoSideshow != null && _park?.Invoke() is { } plain )
			shut = $" shopkinds={plain.ShopKindsStanding} sideshowkinds={plain.SideshowKindsStanding} sincebuild={plain.DaysSinceObjectBuilt}";
		if ( topic.PerDayWithNoFeature != null && _park?.Invoke() is { } drab )
			shut = $" sideshowkinds={drab.SideshowKindsStanding} featurekinds={drab.FeatureKindsStanding} sincebuild={drab.DaysSinceObjectBuilt}";
		if ( topic.NeedingToiletWithNone != null && _park?.Invoke() is { } crossed )
			shut = $" closed={(crossed.Closed ? 1 : 0)} toilets={crossed.Toilets} needing={crossed.GuestsNeedingToilet}/{crossed.GuestsInside}";
		if ( topic.EveryFeatureBuilt && _park?.Invoke() is { } built )
			shut = $" researchers={built.Researchers} kinds={built.FeatureKindsStanding}/{built.FeatureKindsOnOffer} unfinished={built.FeatureProjectsUnfinished}";
		if ( topic.EverySideshowBuilt && _park?.Invoke() is { } shown )
			shut = $" researchers={shown.Researchers} kinds={shown.SideshowKindsStanding}/{shown.SideshowKindsOnOffer} unfinished={shown.SideshowProjectsUnfinished} rate={shown.SideshowResearchRate} ability={shown.ResearchAbility}/{shown.MostResearchAbility}";
		if ( topic.Untrained != null && _park?.Invoke() is { } employing && employing.Staff.Of( topic.StaffKind ) is var kind )
			shut = $" members={kind.Members} budget={kind.Budget} grades={kind.Grades} share={kind.GradeShare}";
		if ( topic.ShopsPerRide is { } crowded && _park?.Invoke() is { } sized )
			shut = FormattableString.Invariant( $" shopkinds={sized.ShopKindsStanding} ratio={Fraction( $"{crowded}.ShopsPerRideRatio" )} balance={sized.Balance} sincebuild={sized.DaysSinceObjectBuilt}" );
		if ( topic.SideshowsPerRide is { } showy && _park?.Invoke() is { } fair )
			shut = FormattableString.Invariant( $" sideshowkinds={fair.SideshowKindsStanding} ridekinds={fair.RideKindsStanding} ratio={Fraction( $"{showy}.SideshowsPerRideRatio" )} balance={fair.Balance} sincebuild={fair.DaysSinceObjectBuilt}" );
		return shut;
	}

	/// <summary>
	/// The tick's first act, FUN_0059c680 asked for one row: from the cursor's row, nought again past the last,
	/// the first polled row is scored and the cursor left on the row after it; a row scoring above nought that
	/// the gate lets by and above MinScoreForConsideration is posted about no thing, as any post is. A row for
	/// Full Simulation alone is not scored in Instant Action: its answer is -1 there, which is above nothing. A
	/// polled row of no built topic is counted.
	/// </summary>
	private void Poll( int gameTick )
	{
		var row = _cursor.Row;
		while ( !IsPolled( row ) )
			row = row + 1 >= Rows ? 0 : row + 1;
		_cursor.Row = row + 1;
		if ( !_topics.TryGetValue( row, out var topic ) || !topic.Polled )
		{
			Unimplemented.Report( "PARK_ADVISOR_POLLED_ROW_NOT_BUILT" );
			return;
		}

		var otherGame = topic.FullSimulationOnly && InstantAction;
		var score = otherGame ? -1 : ScoreOf( topic, gameTick );
		var result = otherGame ? "other-game" : score <= 0 ? "no-score" : Refusal( topic, gameTick ) ?? (score > Threshold ? "post" : "under-threshold");
		if ( !_polled.TryGetValue( topic.Id, out var last ) || last != result )
		{
			_polled[topic.Id] = result;
			Log.Info( $"Park advisor: poll message={topic.Id} row={row} days={DaysSinceTheFirstSweep( gameTick )} score={score} threshold={Threshold} tick={gameTick} result={result}{PollFacts( topic )}" );
		}

		if ( result == "post" ) Post( topic.Id, gameTick );
	}

	/// <summary>
	/// FUN_0059a940 after FUN_0059b590: a message through the accept gate (<see cref="Refusal"/>) into a slot,
	/// at its score function's answer. <paramref name="subject"/> is the thing the record's first word names,
	/// nought for none; nothing here reads it but the log.
	/// </summary>
	internal bool Post( int id, int gameTick, int subject = 0 )
	{
		_lastTick = gameTick;
		var topic = _topics[id];
		var score = ScoreOf( topic, gameTick );
		var reason = Refusal( topic, gameTick ) ?? "queued";
		if ( reason == "queued" )
		{
			var index = -1;
			for ( var i = 0; i < _slots.Length && index < 0; ++i )
				if ( _slots[i] == null && !_carried[i] ) index = i;
			if ( index < 0 )
			{
				for ( var i = 0; i < _slots.Length; ++i )
					if ( _slots[i] is { } held && (index < 0 || held.Score < _slots[index]!.Score) ) index = i;
				if ( index < 0 || score <= _slots[index]!.Score ) reason = "priority";
			}
			if ( reason == "queued" ) _slots[index] = new Pending( topic, score, subject );
		}
		Log.Info( $"Park advisor: post message={id} subject={subject} score={score} threshold={Threshold} tick={gameTick} result={reason} pending={PendingCount}" );
		return reason == "queued";
	}

	/// <summary>
	/// FUN_0059aa70: every slot holding <paramref name="id"/> is emptied, and where he is saying it (inside the
	/// last line's time and its second) <paramref name="stop"/> cuts him off (Advisor_StopSpeaking), the message
	/// is no longer said, it has been cut off once more and the wait is over. Says whether it did either.
	/// </summary>
	/// <remarks>
	/// The kept message the original also forgets here (the advisor's +0x16d0, a tutorial one of this id, raised
	/// again by FUN_0059ab50) is kept by nothing in this game.
	/// </remarks>
	internal bool Withdraw( int id, long milliseconds, Action stop )
	{
		var cleared = 0;
		for ( var i = 0; i < _slots.Length; ++i )
		{
			if ( _slots[i]?.Topic.Id != id ) continue;
			_slots[i] = null;
			++cleared;
		}

		var history = _histories[id];
		var cut = milliseconds < WaitUntil && _playing == id;
		if ( cut )
		{
			stop();
			history.Said = false;
			++history.Slaps;
			_duration = 0;
		}

		Log.Info( $"Park advisor: withdraw message={id} cleared={cleared} cut={(cut ? 1 : 0)} slaps={history.Slaps} pending={PendingCount}" );
		return cleared > 0 || cut;
	}

	/// <summary>
	/// One world sweep: the poll's one row, then, past the wait, the best slot. Playback returns the response duration (zero when the speaker cannot start).
	/// The helper still succeeds after a zero playback return; its score recheck alone can reject here.
	/// <paramref name="said"/> hears the event a said line sends, with the response's group and the subject.
	/// <paramref name="park"/> answers what a score function reads of the running park; with none, such a score is nought.
	/// </summary>
	internal void Tick( int gameTick, long milliseconds, Func<int, int, int> play, Action<int, int>? said = null,
		Func<ParkFacts>? park = null )
	{
		_lastTick = gameTick;
		_park = park;
		Poll( gameTick );
		if ( milliseconds < WaitUntil ) return;
		var index = -1;
		for ( var i = 0; i < _slots.Length; ++i )
			if ( _slots[i] is { } slot && (index < 0 || slot.Score > _slots[index]!.Score) ) index = i;
		if ( index < 0 || _slots[index]!.Score <= Threshold ) return;

		var pending = _slots[index]!;
		_slots[index] = null;
		var score = ScoreOf( pending.Topic, gameTick );
		if ( score < Threshold )
		{
			Log.Info( $"Park advisor: dropped message={pending.Topic.Id} subject={pending.Subject} queued={pending.Score} score={score} threshold={Threshold} tick={gameTick}" );
			return;
		}

		var history = _histories[pending.Topic.Id];
		var line = (history.Line + 1) % pending.Topic.Lines;
		var response = pending.Topic.FirstResponse + line;
		var sample = pending.Topic.FirstSample + line;
		// A response with a gesture of its own plays that row of the gesture table (FUN_00598bf0); the speaker
		// here builds the random chain for every line.
		if ( line == pending.Topic.GestureRowLine || line == pending.Topic.SecondGestureRowLine )
			Unimplemented.Report( "ADVISOR_RESPONSE_GESTURE_ROW" );
		var duration = play( response, sample );
		_started = milliseconds;
		_duration = duration + 1000;
		history.LastTick = gameTick;
		history.Line = line;
		history.Said = true;
		_playing = pending.Topic.Id;
		++Attempts;
		// The helper then sends an event of type 0x18 carrying the response's group and the record's subject
		// (FUN_0059b620, FUN_0040f740, FUN_0040fb10); its one hearer is the tag system (FUN_00509bf0).
		said?.Invoke( pending.Topic.Group, pending.Subject );
		Log.Info( $"Park advisor: attempt={Attempts} message={pending.Topic.Id} subject={pending.Subject} score={score} threshold={Threshold} response={response} sample={sample} line={line} tick={gameTick} duration={duration} waitUntil={WaitUntil}" );
	}

	internal string Census() => $"park advisor: pending={PendingCount} attempts={Attempts} threshold={Threshold} "
		+ string.Concat( _slots.Where( slot => slot != null ).Select( slot => $"slot={slot!.Topic.Id}/subject={slot.Subject} " ) )
		+ string.Join( " ", _topics.Values.Select( topic =>
			$"message={topic.Id}/score={ScoreOf( topic, _lastTick )}/line={_histories[topic.Id].Line}/lastTick={_histories[topic.Id].LastTick}/said={(_histories[topic.Id].Said ? 1 : 0)}/slaps={_histories[topic.Id].Slaps}" ) )
		+ $" action={(_started is { } started ? $"{started}+{_duration}" : "none")}/playing={_playing} poll={_cursor.Row}";
}
