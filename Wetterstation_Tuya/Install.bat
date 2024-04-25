powershell.exe -c "Get-Service -Name TuyaAPI | Stop-Service"
sc.exe delete "TuyaAPI"
powershell.exe -c "New-Service -Name TuyaAPI -BinaryPathName %~dp0Wetterstation_Tuya.exe -Description 'Get Weather Data from Tuya API Service. Stores data in InfluxDB and report to APRS.'"
powershell.exe -c "Get-Service -Name TuyaAPI | Start-Service"
sc.exe failure "TuyaAPI" reset=0 actions=restart/30000
