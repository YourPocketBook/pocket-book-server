$sourceuri = "ftp://script@cyclistmanager.me.uk/pocketbook/app_offline.htm"
$ftprequest = [System.Net.FtpWebRequest]::create($sourceuri)
$ftprequest.Credentials =  New-Object System.Net.NetworkCredential("script","8P~8ue9n")
$ftprequest.Method = [System.Net.WebRequestMethods+Ftp]::DeleteFile
$ftprequest.GetResponse()