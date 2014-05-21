properties {
    $BaseDir = Resolve-Path "."
    $OutputPath = "$BaseDir\output"
    $SolutionPath = "$BaseDir\src\Nerve.EventStore.sln"
    $NugetPath = "$BaseDir\src\.nuget\nuget.exe"
    $BuiltPath = "$OutputPath\built"
    $OpenCoverVersion = "4.5.2506"
    $MSpecVersion = "0.8.2"
    $Configuration = "Release"
}

task default -depends Build

task Clean {
	if (Test-Path -Path $OutputPath)
	{
		Remove-Item -Recurse -Force $OutputPath
	}
	New-Item -ItemType Directory -Force $OutputPath
	msbuild "$SolutionPath" /t:Clean /p:Configuration=$Configuration
}

task Prerequisites {
	Exec { & "$NugetPath" install OpenCover -OutputDirectory ".\src\packages" -version $OpenCoverVersion }
}

task Test -depends Build, Prerequisites {
	$TestDlls = ls "$BaseDir\src\specs\*\bin\$Configuration" -rec `
	    | where { $_.Name.EndsWith(".Specs.dll") } `
	    | foreach { $_.FullName }

	Exec { & ".\src\packages\OpenCover.$OpenCoverVersion\OpenCover.Console.exe" -register:user `
		"-target:.\src\packages\Machine.Specifications-Signed.$MSpecVersion\tools\mspec-clr4.exe" `
		"-targetargs:$TestDlls -x Unstable" "-output:$OutputPath\coverage.xml" "-filter:+[*]* -[*-Specs]*" "-returntargetcode" }
}

task Pack -depends Test {
        $AssemblyVersionPattern = 'AssemblyVersion\((\"\d+\.\d+\.\d+\.\d+\")\)'
        $Version = Get-Content "$BaseDir\src\GlobalAssemblyInfo.cs" |
		Select-String -pattern $AssemblyVersionPattern |
		Select -first 1 |
		% { $_.Matches[0].Groups[1] }

	Exec { & ".\src\.nuget\nuget.exe" pack nuget\Nerve.EventStore.nuspec -version $Version -OutputDirectory $OutputPath }
}

task Build -depends Clean {
	msbuild "$SolutionPath" /t:Build /p:Configuration=$Configuration
	
	New-Item $BuiltPath -Type Directory
	Copy-Item "$BaseDir\src\main\Nerve.EventStore\bin\$Configuration\*.*" $BuiltPath -Recurse
}
