# Polish edition support

Base: maexah/OpenTPW commit 44b6ea16fa88a961be54314622e4179e64125e00.
This fork adds language selection and fixes decoding of the Polish edition's two-byte BFMU characters.

## Start

```powershell
dotnet build source/OpenTPW.sln
dotnet source/OpenTPW/bin/Debug/net10.0/OpenTPW.dll --game "D:\Games\Theme Park World" --language pl
```

The game folder must contain data/levels and data/Language/Polish. An installation containing only Polish is supported. The original game files are supplied by the player and are not included in this repository.

Supported selection:
- --language pl or --language Polish
- --language en, English or American
- --language auto (the default): system UI language, then installed English/American, then the first complete installed language
- OPENTPW_LANGUAGE environment variable, overridden by --language
- A complete language has UITEXT.str, UIHELPTEXT.str and MBToUni.dat.
- An explicitly requested missing language stops startup with a diagnostic; it is not silently changed.

Selection happens before the renderer, fonts, UI tables and save systems are initialized. UI/help, bitmap fonts, advisor tag text, theme/object/item names, employee kinds/names, ingredients, staff/visitor states and visitor thoughts use the selected folder.

## BFMU version 1

Measured on the user's PC disc and installed Polish assets:
- English MBToUni.dat: 506 bytes, header 42 46 4D 55 00 00 F9 00.
- Dutch and Italian: 504 bytes, header 42 46 4D 55 00 00 F8 00.
- Polish: 666 bytes, header 42 46 4D 55 01 00 FF 4B.

Version 0 uses one-byte indexes into its alphabet.
In the measured version 1 file, byte 6 (FF) is the escape code, byte 7 (4B) is the extended alphabet count. There are FF-1+4B = 329 UTF-16 entries after the eight-byte header. An FF escape followed by N selects the one-based alphabet index FF-1+N. Thus FF 05 is Z and FF 14 is K. BFST text-part lengths and padding count encoded bytes, not decoded characters.

The reader decodes complete text-part byte spans, preserving parameter parts and byte-based padding. Unknown BFMU versions, invalid indexes, incomplete escape pairs and truncated alphabets are rejected. The existing version 0 path is retained.

Evidence was checked using all 21 Polish STR files (2365 rows). Exact assertions include Options -> Opcje and the first theme -> Zaginione Królestwo. Lowercase Polish diacritics were found in the decoded corpus and in six principal bitmap-font glyph directories. File-level glyph checks do not verify rendering.

## Tests

```powershell
dotnet test source/OpenTPW.sln --filter "FullyQualifiedName~BfmuEncodingTests|FullyQualifiedName~GameLanguageTests"
dotnet run --project tools/LocalizationSmoke -- "D:\Games\Theme Park World\data"
```

The smoke tool links the production selector, VFS, text readers and localization code directly; it needs no NuGet packages or graphics device. Without a data argument it runs language-selection checks only. With data it reads the player's files without writing to them.

Reintroducing single-byte-only decoding causes the exact Polish UI assertion to fail.

## Limits

This is a language-support change, not a completion of OpenTPW's simulation. In-game layout, full scene rendering and Polish speech playback must still be verified on the desktop. Speech/audio archive selection is not changed. The full upstream test suite has many tests tied specifically to the American data set; passing tests without assets does not establish Polish gameplay correctness.
