$raw = Get-Content -Path .\summary-export.json -Raw
$jsonObject = ConvertFrom-Json $raw
$date = Get-Date -Format "yyyy-MM-ddThh:mm:ss"
$jsonObject | Add-Member -MemberType NoteProperty -Name 'timestamp' -Value $date
$jsonString = ConvertTo-Json $jsonObject -Compress -Depth 2
Write-Host $jsonString

$user = "elastic"
$password = "artdataroot"
$credential = "${user}:${password}"
$credentialBytes = [System.Text.Encoding]::ASCII.GetBytes($credential)
$base64Credential = [System.Convert]::ToBase64String($credentialBytes)
$basicAuthHeader = "Basic $base64Credential"
$headers = @{ Authorization = $basicAuthHeader }

add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
$AllProtocols = [System.Net.SecurityProtocolType]'Ssl3,Tls,Tls11,Tls12'
[System.Net.ServicePointManager]::SecurityProtocol = $AllProtocols

Invoke-WebRequest -UseBasicParsing -Uri 'https://artsearch2-1-test.artdata.slu.se:9200/sos-st-loadtests-summaries/_doc?pretty&refresh' -Headers $headers -ContentType "application/json" -Method Post -Body $jsonString