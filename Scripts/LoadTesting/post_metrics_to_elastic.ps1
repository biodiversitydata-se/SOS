
$user = "elastic"
$password = "***REMOVED***"
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
$lines = Get-Content -Path .\test.json
$lineCount = $lines.Length
Write-Host "Total number of metrics ${lineCount}"
$index = 0;
foreach($line in $lines){    
    Write-Host "Posting metric: ${index}/${lineCount}"    
    Invoke-WebRequest -Uri 'https://artsearch2-1-test.artdata.slu.se:9200/sos-st-loadtests-metrics/_doc?pretty&refresh' -Headers $headers -ContentType "application/json" -Method Post -Body $line
    $index++;
}