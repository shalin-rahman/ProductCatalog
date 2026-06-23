$cs = 'Data Source=.;Persist Security Info=True;User ID=api;Password=api;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=5'
$conn = New-Object System.Data.SqlClient.SqlConnection($cs)
try {
    $conn.Open()
    Write-Output 'CONNECTED'
    $conn.Close()
} catch {
    Write-Output ("ERROR: $($_.Exception.Message)")
    exit 1
}
