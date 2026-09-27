function GetNuGetPackageMetadata {
    param($Id, $Version)

    # 包信息查询 api
    # https://nuget.azure.cn/v3/index.json
    # RegistrationsBaseUrl
    $url = "https://api.nuget.org/v3/registration5-semver1/$($Id.ToLower())/index.json"

    try {
        # 查找匹配版本的条目
        do {
            $response = Invoke-RestMethod -Uri $url -Method Get -ErrorAction Stop
            if (!$flag) {
                $items = $response.items | ForEach-Object { $_.items } | Select-Object -ExpandProperty catalogEntry
            }
            else {
                $items = $response.items | Select-Object -ExpandProperty catalogEntry
                $flag = $false
            }
            if (!$items) {
                $a = [version]$Version.Split('-')[0]
                $items = $response.items | Where-Object {
                    $b = [version]$_.lower.Split('-')[0]
                    $c = [version]$_.upper.Split('-')[0]
                    $a -ge $b -and $a -le $c
                }
                $url = $items.'@id'
                if (!$items) {
                    throw '没找到，需要调试'
                }
                $items = $null
                $flag = $true
            }
        }until($items)
        $packageEntry = $items | Where-Object { $_.version -eq $Version } | Select-Object -First 1
        
        # 相较于原来缺失了一些数据，但是尽力了
        if ($packageEntry) {
            return [PSCustomObject]@{
                Package           = $Id
                Version           = $Version
                # License Information Origin 缺失
                LicenseExpression = $packageEntry.licenseExpression
                LicenseUrl        = $packageEntry.licenseUrl
                # Copyright 缺失
                Authors           = $packageEntry.authors
                PackageProjectUrl = $packageEntry.projectUrl
            }
        }
    }
    catch {
        Write-Warning "无法获取包 $Id ($Version) 的信息: $_"
        return [PSCustomObject]@{
            Package           = $Id
            Version           = $Version
            # License Information Origin 缺失
            LicenseExpression = "Unknown"
            LicenseUrl        = ""
            # Copyright 缺失
            Authors           = "Unknown"
            PackageProjectUrl = ""
        }
    }
}

function GetThirdPartyNotices {
    # 用于陈列项目使用的第三方依赖库
    # 但项目使用了中央包管理，原有工具无法正常工作
    # dotnet tool install --global dotnet-project-licenses --version 2.7.1
    $packages = dotnet package list --include-transitive --format json |
    ConvertFrom-Json |
    Select-Object -ExpandProperty projects |
    ForEach-Object { $_.frameworks } |
    ForEach-Object { $_.topLevelPackages + $_.transitivePackages } |
    Select-Object id, resolvedVersion -Unique |
    Sort-Object id

    "| Package | Version | License Expression | License Url | Authors | Package Project Url |" | Out-File .\THIRD_PARTY_NOTICES.md
    "| -------- | ------- | ------------------ | ----------- | ------- | -------------------- |" | Out-File .\THIRD_PARTY_NOTICES.md -Append
    foreach ($package in $packages) {
        $info = GetNuGetPackageMetadata $package.id $package.resolvedVersion
        "| $($info.Package) | $($info.Version) | $($info.LicenseExpression) | $($info.LicenseUrl) | $($info.Authors) | $($info.PackageProjectUrl) |" | Out-File .\THIRD_PARTY_NOTICES.md -Append
    }
}


Get-ChildItem obj,bin -Directory -Recurse | Remove-Item -Recurse
dotnet restore .\PopupBlocker.sln
dotnet build -c Release &
GetThirdPartyNotices
Remove-Item .\publish -Recurse
New-Item .\publish\PopupBlocker\document -Type Directory
Copy-Item .\README.md .\publish\PopupBlocker\document
Copy-Item .\VersionLog.md .\publish\PopupBlocker\document
Copy-Item .\ContributorChanges .\publish\PopupBlocker\document
New-Item .\publish\PopupBlocker\license -Type Directory
Copy-Item .\LICENSE.txt .\publish\PopupBlocker\license
Copy-Item .\THIRD_PARTY_NOTICES.md .\publish\PopupBlocker\license
Get-Job | Wait-Job
Copy-Item .\PopupBlocker\bin\Release\net8.0-windows\* .\publish\PopupBlocker -Recurse
tar -czvf .\publish\PopupBlocker.tar.gz -C .\publish PopupBlocker