param(
	[Parameter(Mandatory=$true)][string]$GamePath,
	[switch]$SmokeOnly
)
$ErrorActionPreference='Stop'
Push-Location -LiteralPath $PSScriptRoot
try {
	$tpwData=Join-Path $GamePath 'data'
	if(-not (Test-Path -LiteralPath $tpwData -PathType Container)) {
		throw 'Wskaz katalog gry zawierajacy podkatalog data.'
	}
	if($SmokeOnly) {
		& dotnet run --project tools/LocalizationSmoke -- $tpwData
		if($LASTEXITCODE -ne 0) { throw 'Test polskich zasobow nie powiodl sie.' }
		return
	}
	& dotnet build source/OpenTPW/OpenTPW.csproj
	if($LASTEXITCODE -ne 0) { throw 'Kompilacja nie powiodla sie. Sprawdz dostep do NuGet i .NET 10.' }
	& dotnet source/OpenTPW/bin/Debug/net10.0/OpenTPW.dll --game $GamePath --language pl
	if($LASTEXITCODE -ne 0) { throw 'Gra zakonczyla sie z bledem. Sprawdz console.log.' }
} finally {
	Pop-Location
}
