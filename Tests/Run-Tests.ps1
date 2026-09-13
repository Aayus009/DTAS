param(
    [string]$Case,
    [switch]$Unit,
    [switch]$Integration,
    [switch]$System,
    [switch]$Api,
    [switch]$Security,
    [switch]$List
)

$ErrorActionPreference = "Stop"
$testsRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo = Split-Path -Parent $testsRoot
$nunitProject = Join-Path $repo "DigitalTransparencySystem.Tests\DigitalTransparencySystem.Tests.csproj"
$sqlDir = Join-Path $testsRoot "Sql"
$collection = Join-Path $testsRoot "DTAS.postman_collection.json"
$server = "AAYUS-ARK009\SQLEXPRESS"
$database = "DigitalTransparencyDB"
$newman = Join-Path $testsRoot "node_modules\newman\bin\newman.js"

function Invoke-SqlCase([string]$id) {
    $file = Join-Path $sqlDir ($id + ".sql")
    if (-not (Test-Path $file)) { throw "SQL file not found: $file" }
    Write-Host "Tool: sqlcmd   File: $file"
    & sqlcmd -I -S $server -d $database -E -b -i $file
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed for $id" }
}

$newmanFolders = @{
    "API-01" = "API-01 ConnectApi.ashx refuses anonymous"
    "API-02" = "API-02 TaskProgressApi.ashx refuses anonymous"
    "API-03" = "API-03 GlobalSearch.ashx refuses anonymous"
    "API-04" = "API-04 EventsProgressApi.ashx refuses anonymous"
    "API-05" = "API-05 TransparencySync.ashx refuses anonymous"
    "SEC-01" = "SEC-01 EventWorkspace.aspx not open anonymously"
}

function Invoke-NewmanCase([string]$id) {
    if (-not (Test-Path $newman)) {
        Write-Host "Installing Newman (Postman CLI)..."
        Push-Location $testsRoot
        try { npm install --omit=optional } finally { Pop-Location }
    }
    $folder = $newmanFolders[$id]
    if (-not $folder) { $folder = $id }
    Write-Host "Tool: Postman / Newman   Request: $folder"
    node $newman run $collection --folder $folder -k --disable-unicode
    if ($LASTEXITCODE -ne 0) { throw "Newman failed for $id. Start the web project if the site was down." }
}

$nunitCases = @{
    "UT-01" = "FullyQualifiedName~UT01_"
    "UT-02" = "FullyQualifiedName~UT02_"
    "UT-03" = "FullyQualifiedName~UT03_"
    "UT-04" = "FullyQualifiedName~UT04_"
    "UT-05" = "FullyQualifiedName~UT05_"
}

$catalog = [ordered]@{
    "UT-01" = @{ Tool = "NUnit"; Method = "UT01_AdminRoleIsRecognised"; Component = "RoleAccess.IsAdmin"; Checks = "Admin is recognised as admin; Faculty and Student are not."; Expect = "True only for Admin." }
    "UT-02" = @{ Tool = "NUnit"; Method = "UT02_OnlyFacultyAndStaffMayBeEventLead"; Component = "RoleAccess.CanHoldEventAdmin"; Checks = "Only Faculty and Staff may be event lead."; Expect = "True for Faculty and Staff; false for Student and Admin." }
    "UT-03" = @{ Tool = "NUnit"; Method = "UT03_OnlyFacultyMayCreateAssignments"; Component = "RoleAccess.CanCreateAssignments"; Checks = "Only Faculty may create assignments."; Expect = "True for Faculty only." }
    "UT-04" = @{ Tool = "NUnit"; Method = "UT04_AdminIsNotACreateEventRole"; Component = "RoleAccess.CanCreateEvents"; Checks = "Admin cannot create events; Faculty and Staff can."; Expect = "False for Admin." }
    "UT-05" = @{ Tool = "NUnit"; Method = "UT05_PasswordHasherHashAndVerify"; Component = "PasswordHasher"; Checks = "Hash is PBKDF2, not the raw password; verify accepts the original and rejects a wrong password."; Expect = "pbkdf2 hash; verify true/false." }
    "IT-01" = @{ Tool = "sqlcmd"; Method = "Tests\Sql\IT-01.sql"; Component = "Users"; Checks = "A new user is gated until email and ID are verified."; Expect = "EmailVerified=0, IdentityVerified=0." }
    "IT-02" = @{ Tool = "sqlcmd"; Method = "Tests\Sql\IT-02.sql"; Component = "EventMembers"; Checks = "Public join is a request, not immediate membership."; Expect = "InviteStatus=Requested." }
    "IT-03" = @{ Tool = "sqlcmd"; Method = "Tests\Sql\IT-03.sql"; Component = "EventMembers / InviteCode"; Checks = "Invite-code join is accepted immediately."; Expect = "InviteStatus=Accepted." }
    "IT-04" = @{ Tool = "sqlcmd"; Method = "Tests\Sql\IT-04.sql"; Component = "Clubs / ClubMembers"; Checks = "Student can be club lead; Admin CreatedBy count stays 0."; Expect = "Student is Lead. Admin CreatedBy count=0." }
    "IT-05" = @{ Tool = "sqlcmd"; Method = "Tests\Sql\IT-05.sql"; Component = "Meetings"; Checks = "A meeting past start+duration is closed."; Expect = "Status=Completed; join URL null." }
    "ST-01" = @{ Tool = "sqlcmd"; Method = "Tests\Sql\ST-01.sql"; Component = "Users identity flags"; Checks = "Email is verified first, then identity; both true only at the end."; Expect = "Email then ID; both true only at the end." }
    "ST-02" = @{ Tool = "sqlcmd"; Method = "Tests\Sql\ST-02.sql"; Component = "Events + EventMembers"; Checks = "Propose to Planned, then request join, then accept."; Expect = "Proposed to Planned; Requested then Accepted." }
    "ST-03" = @{ Tool = "sqlcmd"; Method = "Tests\Sql\ST-03.sql"; Component = "Meetings"; Checks = "Meeting closes after duration."; Expect = "Meeting closes after duration." }
    "ST-04" = @{ Tool = "sqlcmd"; Method = "Tests\Sql\ST-04.sql"; Component = "Clubs + ClubMembers"; Checks = "Admin does not create the club; public join is Requested."; Expect = "Admin does not create. Public join Requested." }
    "ST-05" = @{ Tool = "sqlcmd"; Method = "Tests\Sql\ST-05.sql"; Component = "Users.AccountStatus"; Checks = "Admin accounts stay Active."; Expect = "Admin accounts stay Active." }
    "API-01" = @{ Tool = "Newman"; Method = "ConnectApi.ashx"; Component = "ConnectApi.ashx"; Checks = "Anonymous GET is refused."; Expect = "Response contains sign in." }
    "API-02" = @{ Tool = "Newman"; Method = "TaskProgressApi.ashx"; Component = "TaskProgressApi.ashx"; Checks = "Anonymous GET is refused."; Expect = "Response contains sign in." }
    "API-03" = @{ Tool = "Newman"; Method = "GlobalSearch.ashx"; Component = "GlobalSearch.ashx"; Checks = "Anonymous GET is refused."; Expect = "Response contains sign in." }
    "API-04" = @{ Tool = "Newman"; Method = "EventsProgressApi.ashx"; Component = "EventsProgressApi.ashx"; Checks = "Anonymous GET is refused."; Expect = "Response contains sign in." }
    "API-05" = @{ Tool = "Newman"; Method = "TransparencySync.ashx"; Component = "TransparencySync.ashx"; Checks = "Anonymous GET is refused."; Expect = "Response contains sign in." }
    "SEC-01" = @{ Tool = "Newman"; Method = "EventWorkspace.aspx"; Component = "EventWorkspace.aspx"; Checks = "Workspace is not open without a session."; Expect = "Redirect or login; workspace not open." }
}

function Write-CaseBanner([string]$id) {
    $info = $catalog[$id]
    if (-not $info) { return }
    Write-Host ""
    Write-Host "========================================"
    Write-Host "Case:      $id"
    Write-Host "Tool:      $($info.Tool)"
    Write-Host "Method:    $($info.Method)"
    Write-Host "Component: $($info.Component)"
    Write-Host "Checks:    $($info.Checks)"
    Write-Host "Expect:    $($info.Expect)"
    Write-Host "========================================"
}

function Invoke-NUnitCase([string]$id) {
    Write-CaseBanner $id
    $filter = $nunitCases[$id]
    dotnet test $nunitProject --nologo --filter $filter --logger "console;verbosity=detailed"
    if ($LASTEXITCODE -ne 0) { throw "NUnit failed for $id" }
}

if ($List) {
    Write-Host ""
    Write-Host "Unit (NUnit)"
    foreach ($id in @("UT-01","UT-02","UT-03","UT-04","UT-05")) {
        $info = $catalog[$id]
        Write-Host ("  {0}  {1}" -f $id, $info.Method)
        Write-Host ("           {0}" -f $info.Checks)
    }
    Write-Host ""
    Write-Host "Integration (sqlcmd): IT-01 .. IT-05"
    Write-Host "System (sqlcmd):      ST-01 .. ST-05"
    Write-Host "Interface (Newman):   API-01 .. API-05  (start the site)"
    Write-Host "Security (Newman):    SEC-01  (start the site)"
    return
}

if ($Case) {
    $key = $Case.ToUpperInvariant()
    if ($nunitCases.ContainsKey($key)) {
        Invoke-NUnitCase $key
    }
    elseif ($key -like "IT-*" -or $key -like "ST-*") {
        Write-CaseBanner $key
        Invoke-SqlCase $key
    }
    elseif ($key -like "API-*" -or $key -eq "SEC-01") {
        Write-CaseBanner $key
        Invoke-NewmanCase $key
    }
    else {
        throw "Unknown case '$Case'. Use -List."
    }
    return
}

if ($Unit) {
    foreach ($id in @("UT-01","UT-02","UT-03","UT-04","UT-05")) { Invoke-NUnitCase $id }
}
elseif ($Integration) {
    foreach ($id in @("IT-01","IT-02","IT-03","IT-04","IT-05")) {
        Write-CaseBanner $id
        Invoke-SqlCase $id
    }
}
elseif ($System) {
    foreach ($id in @("ST-01","ST-02","ST-03","ST-04","ST-05")) {
        Write-CaseBanner $id
        Invoke-SqlCase $id
    }
}
elseif ($Api) {
    foreach ($id in @("API-01","API-02","API-03","API-04","API-05")) {
        Write-CaseBanner $id
        Invoke-NewmanCase $id
    }
}
elseif ($Security) {
    Write-CaseBanner "SEC-01"
    Invoke-NewmanCase "SEC-01"
}
else {
    throw "Choose -Unit, -Integration, -System, -Api, -Security, or -Case UT-01 (use -List)."
}
