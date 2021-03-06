Define-Step -Name 'Update version info' -Target 'build' -Body {
	. (require 'psmake.mod.update-version-info')
	Update-VersionInAssemblyInfo $($Context.Version) 
}

Define-Step -Name 'Building' -Target 'build' -Body {
	call "$($Context.NuGetExe)" restore HealthMonitoring.sln -ConfigFile "$($Context.NuGetConfig)"
	call "msbuild.exe" HealthMonitoring.sln /t:"Clean,Build" /p:Configuration=Release /m /verbosity:m /nologo /p:TreatWarningsAsErrors=true /tv:14.0
}

Define-Step -Name 'Testing' -Target 'build' -Body {
	. (require 'psmake.mod.testing')
	
	$tests = @()
	$tests += Define-XUnitTests -GroupName 'Unit tests' -TestAssembly "*\bin\Release\*.UnitTests.dll"
	$tests += Define-XUnitTests -GroupName 'Acceptance tests' -TestAssembly "*\bin\Release\*.AcceptanceTests.dll"

	try {
		$tests | Run-Tests -EraseReportDirectory -Cover -CodeFilter '+[HealthMonitoring*]* -[*Tests*]*' -TestFilter '*Tests.dll' | Generate-CoverageSummary | Check-AcceptableCoverage -AcceptableCoverage 90
	}
	finally{
		if(Test-Path HealthMonitoring.AcceptanceTests\bin\Release\Reports)
		{
			cp HealthMonitoring.AcceptanceTests\bin\Release\Reports\FeaturesSummary.* reports 
		}
	}
}

Define-Step -Name 'Packaging' -Target 'build' -Body {
	. (require 'psmake.mod.packaging')

	Find-VSProjectsForPackaging | Package-VSProject
	
	Find-NuSpecFiles -filter "*-deploy.nuspec" | Package-DeployableNuSpec -Version $($Context.Version)
}
