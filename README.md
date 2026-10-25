# EasyRetro

Windows 11을 Windows 98 / Windows XP 분위기로 바꾸는 작은 테마 도구입니다. 배경, 시작메뉴와 시작버튼, 작업표시줄, 시스템 색상, 글꼴, 커서, 효과음을 한 번에 설정하고 매니저에서 전환하거나 원래대로 복구합니다.

## 시작하기

1. 이 저장소의 **Releases**에서 **`EasyRetro.exe` 하나를 다운로드**합니다.
2. 실행하고 **Windows 98 적용** 또는 **Windows XP 적용**을 클릭합니다.
3. 관리자 권한 요청(UAC)을 승인하고 완료 안내를 기다립니다. 처음에는 인터넷 연결이 필요합니다.

별도의 .NET 설치는 필요하지 않습니다. Windows 11에 포함된 .NET Framework를 사용합니다. **Windows 11 x64 전용**이며 ARM64는 지원하지 않습니다. 일반 관리자 계정의 UAC 승격만 지원합니다. 표준 사용자 계정에서 다른 관리자 계정의 암호를 입력하는 방식은 사용자 설정을 잘못 바꾸지 않도록 거부합니다.

적용 전 작업을 저장하세요. 설정을 반영하려고 탐색기를 재시작하므로 열려 있던 탐색기 창이 닫힐 수 있습니다. 회사·업무용 PC보다 복구 가능한 개인 테스트 환경에서 먼저 사용하기를 권장합니다.

### 유지하는 Windows 11 기능

- 기본 파일 탐색기를 계속 사용하므로 **탭 기능 유지**. 레거시 리본/명령바 설정은 변경하지 않습니다.
- 작업표시줄은 **항상 결합(묶기)**, 왼쪽 정렬입니다.
- Open-Shell 시작메뉴의 **프로그램 검색창 사용 가능**.
- **Win**은 레트로 시작메뉴, **Shift+Win**은 원래 Windows 11 시작메뉴를 엽니다.

## 매니저 · 테마 전환 · 복구

첫 적용에 성공하면 시작메뉴에 **EasyRetro 매니저**가 생깁니다. 다운로드한 파일을 옮겨도 이 바로가기로 관리할 수 있습니다.

- 다른 분위기로 바꾸려면 매니저에서 98 / XP를 클릭합니다.
- **원래대로 복구**를 누르면 **최초 적용 직전** 설정을 되돌립니다. 전환할 때 원본 백업을 덮어쓰지 않습니다.
- 기존에 직접 설치한 ExplorerPatcher / Open-Shell은 제거하지 않고 사용자 설정을 복원합니다. EasyRetro가 새로 설치한 구성요소만 제거합니다.
- 복구 후 매니저는 남아 다시 테마를 선택할 수 있습니다.
- EasyRetro까지 지우려면 **설정 → 앱 → 설치된 앱 → EasyRetro → 제거**를 사용합니다. 먼저 테마를 복구한 뒤 바로가기와 앱 등록을 제거하고, 열려 있는 매니저가 모두 닫힌 후 설치 파일을 삭제합니다.

오류가 나면 적용 중이던 모듈을 역순으로 되돌립니다. 복구 자체가 실패하면 백업을 삭제하지 않습니다. 매니저에서 복구를 다시 시도하고 `%LOCALAPPDATA%\EasyRetro\logs`의 로그를 확인하세요. **적용 중에는 `state.json`이나 `backup` 폴더를 지우지 마세요.**

Windows 업데이트나 다른 테마 도구가 설정을 다시 바꾸면 결과가 달라질 수 있습니다. 복구는 EasyRetro가 보관한 최초 값 기준이며, 적용 후 다른 도구로 변경한 같은 설정도 원본으로 돌아갑니다.

## 자동 설치되는 구성요소 · 크레딧

공식 GitHub에서 아래 고정 버전을 다운로드하고 **SHA-256이 정확히 일치할 때만** 실행합니다. 설치 파일을 EasyRetro 실행 파일 안에 재배포하지 않습니다. 기존 설치가 있으면 재설치·업데이트·제거하지 않습니다.

| 구성요소 | 용도 · 설치 범위 | 버전 · 라이선스 |
| --- | --- | --- |
| [ExplorerPatcher](https://github.com/valinet/ExplorerPatcher) | Windows 10식 작업표시줄. Windows 11 24H2 이상은 재구현 작업표시줄 사용 | [26100.8457.70.3](https://github.com/valinet/ExplorerPatcher/releases/tag/26100.8457.70.3) · [GPL-2.0](https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/LICENSE) 및 배포물에 포함된 구성요소별 조건 |
| [Open-Shell](https://github.com/Open-Shell/Open-Shell-Menu) | 시작메뉴 기능만 (`ADDLOCAL=StartMenu`). Classic Explorer / Classic IE 미설치 | [4.4.198](https://github.com/Open-Shell/Open-Shell-Menu/releases/tag/v4.4.198) · [MIT](https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/LICENSE), Ivo Beltchev 및 Open-Shell 팀 크레딧은 공식 배포물 참조 |

ExplorerPatcher의 공식 `/uninstall_silent`는 초기 확인을 생략하지만 **완료·재시작·오류 창이 추가로 뜰 수 있습니다**. 이 창은 upstream 동작이며 EasyRetro가 강제로 숨기거나 자동 클릭하지 않습니다.

ExplorerPatcher의 공식 릴리스는 **Smart App Control이 켜진 환경과 SentinelOne 설치 환경에서 쉘 시작/제거 문제가 생길 수 있음**을 경고합니다. 해당 환경에서는 사용하지 마세요. EasyRetro는 백신 예외를 추가하거나 보안 기능을 끄지 않습니다. [공식 릴리스 주의사항](https://github.com/valinet/ExplorerPatcher/releases/tag/26100.8457.70.3)을 먼저 확인하세요.

**파일 해시**

```text
ExplorerPatcher ep_setup.exe (26100.8457.70.3)
8146db4d3a87201fb80ad1d3712ba8f56883e9a0811758bd39f62d73a9f2c586
OpenShellSetup_4_4_198.exe
a4d2d4459de55b5e962ba2a14f7bb794170511649138173dfa72949837b48c3f
```

EasyRetro 자체의 소스와 런타임 생성 에셋은 [MIT](LICENSE)입니다. Microsoft 로고·배경사진·원본 효과음을 포함하지 않습니다. XP풍 배경/버튼/텍스처/미리보기/효과음은 코드로 새로 생성합니다. 98 효과음은 이미 설치된 `%WINDIR%\Media`의 `chimes`, `chord`, `ding`, `notify`, `recycle`, `tada.wav`를 참조합니다. 커서는 해당 PC의 HKLM 내장 스킴에서 실제로 존재하는 비-aero Windows Standard 파일을 선택합니다. 이 PC에서 확인된 기본 선택은 `Windows Standard (large)`입니다.

## 알려진 한계

- Windows 11의 둥근 창 모서리, Mica, 일부 UWP / WinUI 앱(설정 앱 등)은 클래식 색상·글꼴을 따르지 않습니다.
- 창 테두리 비트맵 스킨, 비서명 msstyles, 시스템 파일 테마 패치는 v0.1 범위 밖입니다. **진짜 XP 창 테두리를 완전히 복제하지 않습니다.** 카드 이미지는 분위기를 설명하는 미리보기입니다.
- ExplorerPatcher는 일부 백신이 오탐할 수 있고, Windows 대형 업데이트 직후 새 호환 버전이 필요할 수 있습니다. 고정 버전이 현재 Windows 업데이트와 맞는지 테스트 VM에서 먼저 확인하세요.
- 다중 모니터별 배경/슬라이드쇼 구성의 완전한 재현은 보장하지 않습니다. XP 배경은 실행 시 주 모니터 해상도로 생성합니다. Windows가 비활성화한 로그인/로그오프 소리는 스킴을 설정해도 재생되지 않을 수 있습니다.
- 이 버전의 실기기 검증은 빌드, 무변경 계획 출력, 생성 이미지 확인, 격리 레지스트리 복구, 일반 권한 GUI까지입니다. 실제 적용/제거 및 탐색기 재시작은 사용자의 PC 보호를 위해 실행하지 않았습니다. VM에서 통합 검증 후 사용하세요.

## 고급 사용 · 데이터 위치

```text
EasyRetro.exe --status
EasyRetro.exe --dry-run --apply 98
EasyRetro.exe --dry-run --apply xp
EasyRetro.exe --dry-run --restore
EasyRetro.exe --selftest
```

위 명령은 테마/레지스트리 설정을 바꾸거나 구성요소를 설치하지 않습니다. `--selftest`는 임시 폴더에 에셋과 의존성 설치 파일을 생성·다운로드하고 해시를 검사합니다. 검사 결과의 임시 폴더는 이미지 확인 후 직접 지워도 됩니다. PowerShell 5에서 한글 계획 출력을 파이프로 읽는 경우 먼저 `[Console]::OutputEncoding = [Text.UTF8Encoding]::new()`를 설정하세요.

실제 작업용 CLI는 `--apply 98|xp`, `--restore`, `--uninstall`이며 필요한 경우 자동으로 UAC 승격합니다. 중복 작업은 차단합니다.

- 매니저: `%LOCALAPPDATA%\Programs\EasyRetro\EasyRetro.exe`
- 상태·원본 백업: `%LOCALAPPDATA%\EasyRetro\state.json`, `backup\`
- 생성 에셋: `assets\`, 검증된 다운로드 캐시: `cache\`, 로그: `logs\`
- 복구 성공 시 상태·백업·생성 에셋을 삭제하고 로그와 캐시는 남깁니다.

레지스트리 값/설치 명령의 고정 버전 출처는 [`src/Core/Dependencies.cs`](src/Core/Dependencies.cs)에 있습니다. Open-Shell 스킨 활성화 값은 `CustomTaskbar`이며, Classic 스타일의 실제 스킨 이름은 `Classic Skin`입니다.

## 소스에서 빌드

Windows에서 저장소 루트의 `build.cmd`를 실행하면 `dist\EasyRetro.exe`가 만들어집니다. 별도 SDK 없이 Windows 내장 .NET Framework 64비트 `csc.exe`를 사용합니다(C# 5). `v*` 태그를 푸시하면 GitHub Actions가 같은 스크립트로 빌드해 GitHub Release에 실행 파일을 첨부합니다.
