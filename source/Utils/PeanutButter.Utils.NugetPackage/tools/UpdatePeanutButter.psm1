function Update-PeanutButter() {
	param(
		[parameter(mandatory=$false)]
		$version
	)
	if (!$version) {
    write-host "Searching for latest version..."
		$version = (find-package -exactmatch peanutbutter.utils).Version
		write-host " -> $version"
	}
	$projects = get-project -all
	foreach ($project in $projects) {
        $projectName = $project.ProjectName
        write-host "Examining $projectName"
		$packages = get-package -project $projectName | Where-Object {$_.Id.StartsWith("PeanutButter.")}
        if ($packages.Length -eq 0) {
            continue
        }
		foreach ($package in $packages) {
            $packageId = $package.Id
            if ($package.Version -eq $version) {
                write-host "$packageId already at version $version"
                continue
            }
            write-host "   ... $packageId"
			install-package -projectname $projectName -id $packageId -version $version
		}
	}
}