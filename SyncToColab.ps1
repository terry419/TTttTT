# --- 설정: 이 부분은 사용자의 환경에 맞게 수정할 수 있습니다. ---

# 1. 원본 프로젝트 폴더 경로
$sourceDirectory = "D:\Unity\8th"

# 2. 복사할 대상 폴더 경로
$destinationDirectory = "D:\Unity\colab"

# 3. 복사할 폴더 목록
$foldersToCopy = @("Assets", "Packages", "ProjectSettings")

# --- 설정 끝 ---


# --- 스크립트 실행 본문 (이 아래는 수정할 필요 없습니다) ---

Write-Host "Colab 동기화 스크립트를 시작합니다..." -ForegroundColor Green
Write-Host "원본: $sourceDirectory"
Write-Host "대상: $destinationDirectory"
Write-Host "------------------------------------"

# 대상 폴더가 없으면 새로 생성합니다.
if (-not (Test-Path $destinationDirectory)) {
    Write-Host "'$destinationDirectory' 폴더가 없어 새로 생성합니다."
    New-Item -ItemType Directory -Force -Path $destinationDirectory
}

# 목록에 있는 각 폴더에 대해 복사 작업을 반복합니다.
foreach ($folder in $foldersToCopy) {
    $sourcePath = Join-Path $sourceDirectory $folder
    $destinationPath = Join-Path $destinationDirectory $folder
    
    # 원본 폴더가 실제로 있는지 확인합니다.
    if (Test-Path $sourcePath) {
        Write-Host "'$folder' 폴더를 복사하는 중..."
        # Copy-Item: 폴더를 복사하는 명령어
        # -Recurse: 폴더 안의 모든 하위 폴더와 파일까지 전부 복사합니다.
        # -Force: 대상 위치에 같은 이름의 파일이 있으면 덮어씁니다.
        Copy-Item -Path $sourcePath -Destination $destinationPath -Recurse -Force
        Write-Host "'$folder' 폴더 복사 완료." -ForegroundColor Cyan
    }
    else {
        Write-Host "'$sourcePath'를 찾을 수 없어 건너뜁니다." -ForegroundColor Yellow
    }
} # <<--- ★★★ 이 괄호가 이전 스크립트에 빠져있었습니다! foreach 반복문을 닫아줍니다. ★★★

Write-Host "------------------------------------"
Write-Host "모든 복사 작업이 완료되었습니다!" -ForegroundColor Green