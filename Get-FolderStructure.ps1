# Requires: git, PowerShell 5+
# Description: Lit l'arborescence du dossier courant en respectant le .gitignore, et copie dans le presse-papier

$root = Get-Location
$tempFile = [System.IO.Path]::GetTempFileName()

# Utilise git pour lister les fichiers non ignorés
git ls-files --cached --others --exclude-standard | Out-File -Encoding utf8 $tempFile

# Lit tous les fichiers valides
$files = Get-Content $tempFile
$tree = @{}

foreach ($file in $files) {
    $parts = $file -split "[/\\]"
    $current = $tree

    foreach ($part in $parts) {
        if (-not $current.ContainsKey($part)) {
            $current[$part] = @{}
        }
        $current = $current[$part]
    }
}

function Format-Tree($node, $indent = "") {
    $output = ""
    foreach ($key in $node.Keys | Sort-Object) {
        $output += "$indent- $key`n"
        $output += Format-Tree $node[$key] "$indent  "
    }
    return $output
}

$treeStr = Format-Tree $tree
Set-Clipboard -Value $treeStr

Write-Host "✅ Structure copiée dans le presse-papier !"
