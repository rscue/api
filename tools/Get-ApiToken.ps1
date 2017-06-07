param(
    [string] $ClientId = "$env:RSCUE_AUTOMATION_CLIENT_CLIENT_ID",
    [string] $ClientSecret = "$env:RSCUE_AUTOMATION_CLIENT_CLIENT_SECRET",
    [string] $Audience = "$env:RSCUE_AUTOMATION_CLIENT_AUDIENCE",
    [string] $GrantType = "$env:RSCUE_AUTOMATION_CLIENT_GRANT_TYPE"
)

if ([string]::IsNullOrEmpty($ClientId)) {
    Write-Host "ClientId cannot be blank. Pass a parameter or set RSCUE_AUTOMATION_CLIENT_CLIENT_ID environment variable."
    return 1
}

if ([string]::IsNullOrEmpty($ClientSecret)) {
    Write-Host "ClientSecret cannot be blank. Pass a parameter or set RSCUE_AUTOMATION_CLIENT_CLIENT_SECRET environment variable."
    return 1
}

if ([string]::IsNullOrEmpty($Audience)) {
    Write-Host "Audience cannot be blank. Pass a parameter or set RSCUE_AUTOMATION_CLIENT_AUDIENCE environment variable."
    return 1
}

if ([string]::IsNullOrEmpty($GrantType)) {
    Write-Host "GrantType cannot be blank. Pass a parameter or set RSCUE_AUTOMATION_CLIENT_GRANT_TYPE environment variable."
    return 1
}

[string] $body = ConvertTo-Json (
    @{
        'client_id' = $ClientId
        'client_secret' = $ClientSecret
        'audience' = $Audience
        'grant_type' = $GrantType
    })


[Microsoft.PowerShell.Commands.BasicHtmlWebResponseObject] $response = `
    Invoke-WebRequest `
        -Method Post `
        -Uri 'https://rscue.auth0.com/oauth/token' `
        -Headers (@{ 'content-type' = 'application/json' }) `
        -Body $body `
        -UseBasicParsing

$result = `
    (ConvertFrom-Json $response.Content)

Write-Host "Authorization: $($result.token_type) $($result.access_token)"
