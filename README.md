# EasyRetro

Windows 11을 Windows 98 / Windows XP 분위기로 바꾸는 작은 테마 도구입니다. 배경, 시작메뉴와 시작버튼, 작업표시줄, 시스템 색상, 글꼴, 커서, 효과음을 한 번에 설정하고 매니저에서 전환하거나 원래대로 복구합니다.

## 시작하기

1. 이 저장소의 **Releases**에서 **`EasyRetro.exe` 하나를 다운로드**합니다.
2. 실행하고 **Windows 98 적용** 또는 **Windows XP 적용**을 클릭합니다.
3. 관리자 권한 요청(UAC)을 승인하고 완료 안내를 기다립니다. 처음에는 인터넷 연결이 필요합니다.

별도의 .NET 설치는 필요하지 않습니다. Windows 11에 포함된 .NET Framework를 사용합니다. **Windows 11 x64 전용**이며 ARM64는 지원하지 않습니다. 일반 관리자 계정의 UAC 승격만 지원합니다. 표준 사용자 계정에서 다른 관리자 계정의 암호를 입력하는 방식은 사용자 설정을 잘못 바꾸지 않도록 거부합니다.

적용 전 작업을 저장하세요. 설정을 반영하려고 탐색기를 재시작하므로 열려 있던 탐색기 창이 닫힐 수 있습니다. 회사·업무용 PC보다 복구 가능한 개인 테스트 환경에서 먼저 사용하기를 권장합니다.
**v0.2.0:** 복구는 등록 제거 프로그램 → 검증된 캐시 → 오프라인 폴더 순서로 구성요소를 제거합니다. 제거가 막혀도 사용자 설정 복구를 끝내고 남은 프로그램과 정확한 제거 경로를 경고합니다. 일부 백업이 없어도 나머지를 계속 복구하며 누락 경로와 이유를 보고합니다. 실행 파일 없이도 사용할 수 있는 PowerShell 텍스트 복구 도구와 소스 ZIP/SHA-256 목록을 제공합니다. 작업표시줄은 가로 타일 방식으로 바꾸고 98은 평면 회색, XP는 원본 픽셀 + 하단 행 연장으로 보간 번짐을 피합니다.


**v0.1.2:** 커서 다시 읽기는 `SPI_SETCURSORS`의 `fWinIni=0`으로 호출하며 재시도 후에도 실패하면 경고만 남깁니다(다음 로그인에 반영). 레지스트리 복구는 원래 있던 키를 지우지 않고 깊이 8까지 캡처한 값/하위 키 이미지를 합산 복원한 다음 추가된 항목만 제거합니다. 같은 값은 쓰지 않으며 항목별 권한 오류는 경로·코드와 함께 기록하고 나머지 복구를 계속합니다. `.reg`는 진단/수동 복구용으로 보관하며 구버전 백업은 삭제 없이 병합합니다. 적용 실패 시 실패 단계와 롤백 결과를 안내하며, 새 `--verify`로 원본 대비 남은 변경을 읽기 전용으로 확인할 수 있습니다.

**v0.1.1:** 콘솔 없이 실행한 매니저와 UAC 승격 작업에서 콘솔 인코딩/스트림에 접근하지 않도록 수정했습니다. 상태·계획·자체 검사·도움말만 부모 콘솔 연결에 성공한 경우 UTF-8로 출력하며, 콘솔 연결/출력 실패는 실행을 중단하지 않습니다.

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

오류가 나면 실패 도중 일부만 변경된 모듈까지 포함해 역순으로 되돌립니다. 첫 적용 실패는 최초 상태로, 테마 전환 실패는 전환 직전 테마로 되돌리고 최초 Windows 백업은 보존합니다. 구성요소 설치 직전의 부분 설치 저널(MSI ProductCode 포함), 자기설치 변경도 복구 대상입니다. 전원 차단·프로세스 종료로 중단되면 매니저에서 **원래대로 복구**를 실행하세요. 복구 자체가 실패하거나 원본 백업 파일이 없으면 나머지 복구를 계속하고 백업과 상태를 삭제하지 않습니다. 복구 완료 후 파일 정리 도중 중단된 경우에는 다음 복구에서 정리만 다시 수행합니다. `%LOCALAPPDATA%\EasyRetro\logs`의 로그를 확인하세요. **적용 중에는 `state.json`이나 `backup` 폴더를 지우지 마세요.**

Windows 업데이트나 다른 테마 도구가 설정을 다시 바꾸면 결과가 달라질 수 있습니다. 복구는 EasyRetro가 보관한 최초 값 기준이며, 적용 후 다른 도구로 변경한 같은 설정도 원본으로 돌아갑니다.

## 자동 설치되는 구성요소 · 크레딧

공식 GitHub에서 아래 고정 버전을 다운로드하고 **SHA-256이 정확히 일치할 때만** 실행합니다. 설치 파일을 EasyRetro 실행 파일 안에 재배포하지 않습니다. 기존 설치가 있으면 재설치·업데이트·제거하지 않습니다.

| 구성요소 | 용도 · 설치 범위 | 버전 · 라이선스 |
| --- | --- | --- |
| [ExplorerPatcher](https://github.com/valinet/ExplorerPatcher) | Windows 10식 작업표시줄. Windows 11 24H2 이상은 재구현 작업표시줄 사용 | [26100.8457.70.3](https://github.com/valinet/ExplorerPatcher/releases/tag/26100.8457.70.3) · [GPL-2.0](https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/LICENSE) 및 배포물에 포함된 구성요소별 조건 |
| [Open-Shell](https://github.com/Open-Shell/Open-Shell-Menu) | 시작메뉴 기능만 (`ADDLOCAL=StartMenu`). Classic Explorer / Classic IE 미설치 | [4.4.198](https://github.com/Open-Shell/Open-Shell-Menu/releases/tag/v4.4.198) · [MIT](https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/LICENSE), Ivo Beltchev 및 Open-Shell 팀 크레딧은 공식 배포물 참조 |

ExplorerPatcher의 공식 `/uninstall_silent`는 초기 확인을 생략하지만 **완료·재시작·오류 창이 추가로 뜰 수 있습니다**. 이 창은 upstream 동작이며 EasyRetro가 강제로 숨기거나 자동 클릭하지 않습니다.

Open-Shell은 `NOSTART=1`로 설치한 뒤 기존 시작메뉴 프로세스를 종료하고 설정을 기록합니다. 첫 실행 스타일 선택창을 표시하지 않으며 마지막 쉘 재시작 이후 일반 권한으로 시작합니다. EP 설치 프로그램이 먼저 탐색기를 재시작하더라도, EasyRetro가 **설치 → 설정 기록 → 최종 쉘 재시작** 순서로 다시 반영합니다.

ExplorerPatcher의 공식 릴리스는 **Smart App Control이 켜진 환경과 SentinelOne 설치 환경에서 쉘 시작/제거 문제가 생길 수 있음**을 경고합니다. 해당 환경에서는 사용하지 마세요. EasyRetro는 백신 예외를 추가하거나 보안 기능을 끄지 않습니다. [공식 릴리스 주의사항](https://github.com/valinet/ExplorerPatcher/releases/tag/26100.8457.70.3)을 먼저 확인하세요.

**파일 해시**

```text
ExplorerPatcher ep_setup.exe (26100.8457.70.3)
8146db4d3a87201fb80ad1d3712ba8f56883e9a0811758bd39f62d73a9f2c586
OpenShellSetup_4_4_198.exe
a4d2d4459de55b5e962ba2a14f7bb794170511649138173dfa72949837b48c3f
```

EasyRetro 자체 소스, 98 그림, 미리보기와 대체용 생성 에셋은 [MIT](LICENSE)입니다. **XP의 Bliss 사진·Luna 그림·Windows XP 효과음은 Microsoft 저작권 자산이며 MIT 대상이 아닙니다.** 공개 저장소와 배포 실행 파일에는 Microsoft 에셋을 넣지 않습니다. XP 적용 시 제3자 공개 보관소에서 원본을 내려받아 아래 고정 SHA-256·크기를 검사합니다. 공개 다운로드 가능 여부가 재배포 허가나 별도 사용권을 뜻하지는 않습니다. 해당 자산의 권리는 원저작권자에게 있으며 사용자는 적용되는 Microsoft 사용 조건을 확인해야 합니다.

### XP 원본 에셋 · 출처

| 자산 | 출처 페이지 / 직접 다운로드 | 두 번째 URL | SHA-256 | 크기 |
| --- | --- | --- | --- | --- |
| Bliss (XP 출시본 800×600, 업스케일 아님) | [보관 항목](https://archive.org/details/windows-xp-wallpapers_202310) / [bliss.jpg](https://archive.org/download/windows-xp-wallpapers_202310/bliss.jpg) | [IA 저장 노드](https://ia801609.us.archive.org/22/items/windows-xp-wallpapers_202310/bliss.jpg) | `eee00f376a6ff19c32e8670bf630bf8336f11247dd5ce74fb8cd91e5ca33327a` | 54,607 bytes |
| Windows XP Media 효과음 ZIP | [보관 항목](https://archive.org/details/windows-xp-sounds) / [Windows XP Sounds.zip](https://archive.org/download/windows-xp-sounds/Windows%20XP%20Sounds.zip) | [IA 저장 노드](https://ia800601.us.archive.org/22/items/windows-xp-sounds/Windows%20XP%20Sounds.zip) | `3259f9b70db0e1bf5b9b72ddf3736397adabf45569e02377e1882772ed3d8afd` | 8,118,271 bytes |
| 실제 XP 시작버튼의 3상태 캡처 | [Classic Shell 원 게시물](https://www.classicshell.net/forum/viewtopic.php?t=1152) / [첨부 550](https://www.classicshell.net/forum/download/file.php?id=550) | [동일 바이트 첨부 549](https://www.classicshell.net/forum/download/file.php?id=549) | `246c153ea4489a9dda5a28e2941ffbcd5fec80395963ea07ceaac567170b5dea` | 19,855 bytes |
| 원본 luna.msstyles의 파란 작업표시줄 비트맵 | [보관 항목](https://archive.org/details/luna_20240122) / [luna.msstyles](https://archive.org/download/luna_20240122/luna.msstyles) | [IA 저장 노드](https://ia800807.us.archive.org/12/items/luna_20240122/luna.msstyles) | `0dbeb9efb7165dc6d812b373b895b7cc84f86a674d0b13ee5e4e7d2ec4a6a852` | 4,190,352 bytes |

시작버튼 출처는 troy2000me의 실제 XP normal/hot/pressed 화면 캡처이며, Jcee/Gaurav의 투명 배경·ClearType 정리본입니다(새로 그린 모방 그림 아님). 원본 100×162 스트립의 각 100×54 프레임에서 위아래 12px 투명 여백만 잘라 **100×30 세 장을 세로로 쌓은 100×90 PNG**를 만듭니다. 보이는 `start` 글자는 원본 비트맵 안에 있고 추가 여백이나 리샘플링은 없습니다. 98 시작버튼도 글자가 포함된 **108×32 세 상태 = 108×96**입니다. `StartButtonSize=0`으로 원본 크기를 사용합니다([고정 Open-Shell 상태 분할 코드](https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/Src/StartMenu/StartMenuDLL/StartButton.cpp#L587-L589)). XP 작업표시줄의 실제 원본 `BLUE_TASKBARBACKGROUND_BMP`는 **50×28**이며, 28행을 그대로 복사하고 마지막 행만 반복해 **50×48**로 연장합니다. 그라데이션을 세로 확대하지 않습니다. `TaskbarTileH=Tile`, `TaskbarTileV=Stretch`는 [4.4.198에서 지원하는 값](https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/Src/StartMenu/StartMenuDLL/SettingsUI.cpp#L4475-L4481)입니다. 98은 50×32 평면 회색에 위 1px 밝은 선/아래 1px 그림자를 사용하며 [EP의 `TaskbarSmallIcons=1`](https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/ep_gui/resources/settings.reg#L97-L102)로 작은 아이콘 모드를 유지합니다. `luna.msstyles`는 `LoadLibraryEx(LOAD_LIBRARY_AS_DATAFILE)`로 리소스만 읽고 실행·설치·시스템 테마 패치하지 않습니다. ZIP은 Windows 내장 .NET 압축 API로 필요한 WAV만 추출합니다. IA 미러는 같은 보관 항목의 별도 저장 노드이며, 시작버튼 미러는 같은 사이트의 별도 첨부이므로 독립 서비스 장애 대비는 아닙니다.

모든 원본은 **스냅샷/시스템 설정 변경 전에** 캐시 다운로드·해시 검증·변환을 마칩니다. 원본과 두 번째 URL이 모두 실패하면 **실패한 항목만** 기존 자체 생성 그림/효과음으로 대체하고 로그 및 완료창 한 줄에 대체 목록을 표시합니다. 다른 원본은 그대로 사용합니다. 다음 XP 적용 때 원본을 다시 시도하며, `--selftest`에서는 대체를 성공으로 인정하지 않습니다. 전환 실패 시 기존 에셋 파일도 되돌려 원래 스냅샷 경로가 다른 그림/소리를 참조하지 않게 합니다.

**XP AppEvents 매핑** (`Windows XP ` 접두사와 `.wav` 확장자는 아래 파일명에서 생략):

| 애플리케이션 / 이벤트 | 원본 파일 |
| --- | --- |
| `.Default / SystemStart` | Startup |
| `.Default / WindowsLogon`, `WindowsLogoff`, `SystemExit` | Logon Sound, Logoff Sound, Shutdown (각각) |
| `.Default / .Default` | Default |
| `.Default / SystemAsterisk`, `SystemQuestion` | Ding |
| `.Default / AppGPFault`, `SystemHand`, `SystemExclamation` | Error, Critical Stop, Exclamation (각각) |
| `.Default / MailBeep`, `Notification.Default` | Notify |
| `.Default / SystemNotification` | Balloon |
| `Explorer / EmptyRecycleBin` | Recycle |
| `.Default / Minimize` | Minimize |
| `.Default / RestoreUp`, `RestoreDown`, `Maximize` | Restore |
| `.Default / MenuCommand` | Menu Command |
| `.Default / DeviceConnect`, `DeviceDisconnect`, `DeviceFail` | Hardware Insert, Hardware Remove, Hardware Fail (각각) |
| `.Default / LowBatteryAlarm`, `CriticalBatteryAlarm`, `PrintComplete` | Battery Low, Battery Critical, Print complete (각각) |
| `Explorer / Navigating` | Start |

`.Current`와 `EasyRetroXP` 스킴을 모두 기록하고 최초 값을 백업합니다. 98로 전환하면 XP 전용 이벤트는 무음으로, 기존 여섯 98 이벤트는 Win11 내장 레거시 소리로 설정합니다. 98 효과음은 이미 설치된 `%WINDIR%\Media`의 `chimes`, `chord`, `ding`, `notify`, `recycle`, `tada.wav`를 참조합니다. 커서는 해당 PC의 HKLM 내장 스킴에서 실제 존재하는 비-aero Windows Standard 파일을 선택합니다.

## 알려진 한계

- **v0.2.0에서도 창 모서리는 둥글게 남을 수 있습니다.** 앱별 `DwmSetWindowAttribute(DWMWCP_DONOTROUND)` 적용은 구현하지 않았습니다. `HKCU\Software\Microsoft\Windows\DWM\DwmWindowCornerPreference`는 검증된 전역 레지스트리 계약이 아니므로 쓰지 않습니다. EP의 `DisableRoundedCorners`는 단순 DWORD가 아니라 [별도 DWM 서비스 설치 호출](https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/ep_gui/GUI.c#L419-L421)이며, [고정 서비스 소스](https://github.com/valinet/ep_dwm/tree/256ebe77ef14db7aed2300f317e127b8c80f1e58)는 uDWM 메모리 패치를 사용하고 24H2 이상 지원을 보장하지 않아 사용하지 않습니다. 다음 버전의 검토 대상은 앱별 모서리 API이며, 이번 버전은 전역 패치를 추가하지 않습니다.
- Mica와 일부 UWP / WinUI 앱(설정 앱 등)은 클래식 색상·글꼴을 따르지 않습니다. 창 테두리 비트맵 스킨, 비서명 msstyles, 시스템 파일 테마 패치는 범위 밖입니다. **진짜 XP 창 테두리를 완전히 복제하지 않습니다.** 카드 이미지는 분위기를 설명하는 미리보기입니다.
- ExplorerPatcher는 일부 백신이 오탐할 수 있고, Windows 대형 업데이트 직후 새 호환 버전이 필요할 수 있습니다. 고정 버전이 현재 Windows 업데이트와 맞는지 테스트 VM에서 먼저 확인하세요.
- 보안 제품이 **EasyRetro 자체의 UAC 재실행**을 `ERROR_VIRUS_INFECTED (225)`로 차단할 수도 있습니다. 이때 한국어 안내를 표시하고 매니저 상태를 새로 고칩니다. 신뢰할 수 있는 공식 릴리스인지 확인한 다음 `EasyRetro.exe`와 `%LOCALAPPDATA%\EasyRetro` 폴더의 예외 처리를 보안 제품 관리자에게 문의하세요. EasyRetro는 예외를 자동 등록하거나 보안 기능을 끄지 않습니다. 관리자 권한으로도 쓰기 불가능한 HKLM 키는 경로/코드를 남기며, 실제로 변경된 항목을 복구하지 못하면 백업을 유지하고 복구 오류를 안내합니다.
- 고정 EP 설치 프로그램 자체가 탐색기를 종료하고 다시 띄우며, 같은 사용자로 상승 실행할 때 **잠시 상승된 탐색기가 존재할 가능성**이 있습니다. EasyRetro의 마지막 단계는 EP가 띄운 것을 포함해 **현재 세션의 모든 탐색기 프로세스**를 종료하고, 자동 재시작 또는 `/RL LIMITED /IT` 예약 작업으로 일반 권한 쉘을 시작한 뒤 토큰을 확인합니다. 다른 세션의 프로세스는 종료하지 않습니다. EP 자체의 재시작 동작을 없애는 옵션은 upstream에 없습니다. [고정 설치 소스](https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/ep_setup/ep_setup.c) · [upstream 쉘 시작 코드](https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/ExplorerPatcher/utility.h)
- 모니터별 원본 배경, 슬라이드쇼 목록·간격·셔플·사용 상태와 배경 캐시를 별도로 보관합니다. 복구 시 원본 모니터/슬라이드쇼 폴더에 접근할 수 없으면 백업을 보존하고 오류를 안내합니다. XP 배경은 출시본 Bliss 800×600이며 화면 채우기 확대는 Windows가 처리합니다. 현대 Windows가 비활성화한 시작/종료/로그인/로그오프 이벤트는 올바른 원본 WAV를 설정해도 재생되지 않을 수 있습니다. 이 프로그램은 재생을 강제하는 예약 작업이나 로그인/시스템 정책을 추가하지 않습니다.
- 이번 XP 원본 변경은 경고를 오류로 취급하는 C# 5 빌드, 모든 원본/미러 URL 8개·의존성 해시, WAV 22개, 무변경 계획·상태 출력, 이미지 실물 확인, 별도 임시 하네스의 픽셀 일치/캐시 손상/미러 재시도/항목별·전체 대체/에셋 롤백까지 검증했습니다. **실제 적용/제거 및 탐색기 재시작은 사용자의 PC 보호를 위해 실행하지 않았습니다.** Open-Shell에서의 최종 DPI·작업표시줄 표시와 테마 전환/원상복구 통합 동작은 VM에서 검증 후 사용하세요.

## 고급 사용 · 데이터 위치

```text
EasyRetro.exe --status
EasyRetro.exe --verify
EasyRetro.exe --dry-run --apply 98
EasyRetro.exe --dry-run --apply xp
EasyRetro.exe --dry-run --restore
EasyRetro.exe --selftest
EasyRetro.exe --help
```

위 명령은 테마/레지스트리 설정을 바꾸거나 구성요소를 설치하지 않습니다. `--selftest`는 임시 폴더에 미리보기/98 에셋을 생성하고 **모든 XP 원본 URL과 미러를 각각 실제 다운로드**해 URL별 PASS/FAIL·SHA-256·크기를 출력합니다. 원본 이미지 변환, 모든 WAV의 RIFF/fmt/data 헤더(Windows 호환 PCM/ADPCM), 기존 두 의존성 해시와 OS 빌드도 검사합니다. 어느 URL이든 실패하거나 대체 에셋이 필요하면 실패 종료합니다. 검사 결과의 임시 폴더는 이미지 확인 후 직접 지워도 됩니다. `--dry-run --apply xp`는 각 원본/미러 URL·해시·대상 파일과 이벤트 매핑을 나열하고 다운로드하지 않습니다. CLI는 부모 콘솔에 연결한 뒤 출력 코드 페이지를 UTF-8(65001)로 바꾸므로 한국어 CP949 콘솔에서도 한글을 표시합니다. PowerShell 5에서 출력을 파이프로 읽는 경우 먼저 `[Console]::OutputEncoding = [Text.UTF8Encoding]::new()`를 설정하세요. 부모 콘솔이 없으면 콘솔 출력 없이 실행하며, GUI와 실제 적용/복구/제거 작업은 콘솔을 사용하지 않습니다.

`--verify`는 관리자 권한 없이 현재 사용자의 적용 테마와 `backup\values.json` 대비 **남아 있는 변경** 표를 출력합니다. Colors 변경 수, 배경 경로, 소리/커서 구성표, DWM 강조색, 바탕화면 아이콘, EP/Open-Shell 설치·설정 및 구성요소/매니저 파일 유무를 확인합니다. 원본이 없거나 읽기 권한이 없는 항목은 비교 불가로 표시하며 **항상 종료 코드 0**입니다. 로그·상태·레지스트리·테마를 쓰지 않으므로 실패한 복구 뒤에도 안전하게 실행할 수 있습니다.

실제 작업용 CLI는 `--apply 98|xp`, `--restore`, `--uninstall`이며 필요한 경우 자동으로 UAC 승격합니다. 중복 작업은 차단합니다.

- 매니저: `%LOCALAPPDATA%\Programs\EasyRetro\EasyRetro.exe`
- 상태·원본 백업: `%LOCALAPPDATA%\EasyRetro\state.json`, `backup\`
- 런타임 에셋: `assets\`, SHA-256 검증 다운로드 캐시/변환 준비 폴더: `cache\`, 로그: `logs\` (저장소 제외)
- 복구 성공 시 상태·원본/전환 백업·생성 에셋과 새로 설치한 구성요소의 사용자 캐시를 정리하고 로그와 다운로드 캐시는 남깁니다. 기존 구성요소는 제거하지 않으며, 새 설치 전부터 존재한 캐시는 원본대로 복원합니다.

레지스트리 값/설치 명령의 고정 버전 출처는 [`src/Core/Dependencies.cs`](src/Core/Dependencies.cs), XP 자산 URL·해시·추출/이벤트 매핑은 [`src/Core/XpAssets.cs`](src/Core/XpAssets.cs)에 있습니다. Open-Shell 스킨 활성화 값은 `CustomTaskbar`이며, Classic 스타일의 실제 스킨 이름은 `Classic Skin`입니다.

원본 스냅샷에는 두 테마가 사용하는 값뿐 아니라 SPI가 간접 변경하는 Desktop/WindowMetrics, 배경·테마 캐시, EP/Open-Shell 설정 트리와 설치 프로그램이 변경하는 정책/RunOnce, HKLM 자동실행·ShellServiceObject·COM 등록 값도 포함합니다. 최초 원본 스냅샷은 전환 중 덮어쓰지 않습니다. 백업 JSON은 임시 파일을 디스크에 플러시한 뒤 원자적으로 교체합니다. EP 제거 캐시 파일을 지웠다면 검증된 설치 파일을 다시 내려받기 위해 인터넷이 필요합니다.

## 소스에서 빌드

Windows에서 저장소 루트의 `build.cmd`를 실행하면 `dist\EasyRetro.exe`가 만들어집니다. 별도 SDK 없이 Windows 내장 .NET Framework 64비트 `csc.exe`를 사용합니다(C# 5, `/warnaserror+`). `v*` 태그를 푸시하면 GitHub Actions가 같은 스크립트로 빌드해 GitHub Release에 `EasyRetro.exe`, `EasyRetro-<tag>-src.zip`, `SHA256SUMS.txt`를 첨부합니다. ZIP에는 소스, README, `build.cmd`, 복구/검증 도구와 같은 빌드의 실행 파일도 포함됩니다. `SHA256SUMS.txt`는 실행 파일과 ZIP의 해시를 담습니다(목록 자체의 자기 해시는 제외).

## 백신이 차단할 때

EasyRetro는 **코드 서명되지 않은 개인 프로젝트**입니다. 설치 파일 다운로드, HKCU 테마 키 변경, Explorer 재시작, 자동 쉘 시작이 실패한 경우의 일회성 예약 작업은 휴리스틱 백신이 의심할 수 있는 동작입니다. AhnLab V3 Internet Security 9.0에서 GitHub 실행 파일 다운로드와 앱 안의 설치 파일 다운로드가 차단된 사례가 확인되었습니다. 신뢰할 수 있는 공식 저장소와 해시를 확인하세요. 보안 기능을 끄는 방법은 권장하지 않으며 예외를 자동으로 추가하지 않습니다.

1. V3 → **설정 → 예외/검사 제외**(버전에 따라 이름이 다름)에서 다운로드할 **EasyRetro.exe**와 **%LOCALAPPDATA%\EasyRetro** 폴더를 검사 제외 대상으로 추가합니다. 회사 PC는 보안 담당자에게 요청하세요.
2. 적용 중 설치 파일도 차단되면 같은 예외 화면에서 ExplorerPatcher의 **ep_setup.exe** 및 Open-Shell의 **OpenShellSetup_4_4_198.exe**를 추가합니다. 실제 캐시 이름은 **explorerPatcher-26100.8457.70.3.exe**, **openShell-4.4.198.exe**이며 위치는 **%LOCALAPPDATA%\EasyRetro\cache**입니다. 설치 파일은 TEMP에 저장하지 않습니다.
3. 실행 파일 자체를 받을 수 없으면 Release의 **EasyRetro-v0.2.0-src.zip**을 받고 압축을 풉니다. 포함된 실행 파일을 사용하거나, 소스를 검토한 뒤 Windows 기본 **build.cmd**로 로컬 빌드합니다. ZIP도 차단되면 GitHub의 소스 파일과 아래 텍스트 복구 경로를 사용할 수 있습니다. **SHA256SUMS.txt**의 해시와 PowerShell **Get-FileHash .\EasyRetro.exe -Algorithm SHA256** 결과를 비교하세요.
4. 설치 파일을 다른 경로로 받았으면 **EasyRetro.exe --offline C:\Installers --restore** 또는 환경 변수 **EASYRETRO_OFFLINE=C:\Installers**를 사용합니다. 파일명은 원본 이름 또는 위 캐시 이름이며 SHA-256 일치가 필수입니다. 오프라인 폴더 지정 시 네트워크를 사용하지 않습니다. 등록 제거 프로그램이 있으면 폴더/캐시 없이도 먼저 사용합니다. 프로그램 제거 실패는 설정 복구를 막지 않으며, 남은 프로그램과 수동 제거 경로를 안내합니다.

### 실행 파일 없이 텍스트만으로 복구

[tools/easyretro-restore.ps1](tools/easyretro-restore.ps1)의 **Raw/원시 파일**을 다른 이름으로 저장하거나, 아래 전체 내용을 복사해 메모장에 붙여 넣고 **UTF-8(BOM 포함)**으로 easyretro-restore.ps1을 저장합니다. 인터넷이나 외부 모듈은 필요하지 않습니다. 최초 백업이 없으면 원래 설정을 알아낼 수 없으므로 새 값을 추측하지 않습니다. 도구는 백업을 삭제하지 않습니다.

Windows PowerShell 5.1에서 먼저 계획을 확인합니다:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\easyretro-restore.ps1
# 확인 후 실제 복구 (필요한 시스템 작업에만 같은 사용자 UAC)
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\easyretro-restore.ps1 -Apply
# 명시적으로 구성요소 제거/Explorer 재시작까지 요청할 때만 추가
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\easyretro-restore.ps1 -Apply -RemoveComponents -RestartShell
# 백업 폴더를 옮겨 두었다면 -BackupDir C:\SavedBackup 지정
```

기본 실행은 변경하지 않고 계획만 보여 줍니다. **-RemoveComponents**는 등록된 EP/Open-Shell 제거 프로그램만 실행하고 다운로드하지 않습니다. **-RestartShell**은 현재 세션 Explorer를 종료한 뒤 /RL LIMITED 작업으로 일반 권한 쉘을 실행하므로 열린 탐색기 창이 닫힙니다. 원본 레지스트리, 소리/커서 스킴의 기본값, 색상, 글꼴, 배경, DWM/Personalize/DesktopIcons를 복구하며, 일부 파일이 없으면 가능한 나머지 항목을 계속 처리하고 경로와 이유를 보고합니다. 일반 사용자 HKCU 백업만 복구할 때는 관리자 권한이 필요하지 않습니다.

<details>
<summary>복사 가능한 전체 복구 스크립트 (v0.2.0)</summary>

```powershell
# EasyRetro 0.2.0 - Windows PowerShell 5.1, offline, no external modules.
[CmdletBinding()]
param(
    [string]$BackupDir = (Join-Path $env:LOCALAPPDATA 'EasyRetro\backup'),
    [switch]$Apply,
    [switch]$RemoveComponents,
    [switch]$RestartShell,
    [string]$OriginalUserSid = ([Security.Principal.WindowsIdentity]::GetCurrent().User.Value)
)
$ErrorActionPreference = 'Stop'
$failures = New-Object 'System.Collections.Generic.List[string]'
$leftovers = New-Object 'System.Collections.Generic.List[string]'
if ($Apply -and $OriginalUserSid -ne [Security.Principal.WindowsIdentity]::GetCurrent().User.Value) {
    throw '다른 관리자 계정으로 복구할 수 없습니다. 원래 사용자 계정에서 실행하세요.'
}
function Restore-Item([string]$Label, [scriptblock]$Action) {
    if (!$Apply) { Write-Host "[계획/변경 없음] $Label"; return }
    try { & $Action; Write-Host "[복구] $Label" }
    catch { $message = "$Label : $($_.Exception.Message)"; $failures.Add($message); Write-Warning $message }
}
function Invoke-Native([string]$File, [string]$Arguments) {
    # reg.exe writes successful completion to stderr; PS 5.1 must not turn that into
    # a terminating NativeCommandError. Judge native programs by their exit codes.
    $info = New-Object Diagnostics.ProcessStartInfo
    $info.FileName = $File; $info.Arguments = $Arguments
    $info.UseShellExecute = $false; $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true; $info.RedirectStandardError = $true
    $process = [Diagnostics.Process]::Start($info)
    try {
        $stdout = $process.StandardOutput.ReadToEndAsync(); $stderr = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $text = $stdout.Result + $stderr.Result
        if ($process.ExitCode -ne 0) { Write-Warning "$File : $text" }
        return $process.ExitCode
    } finally { $process.Dispose() }
}
function Read-Backup([string]$Name) {
    $file = Join-Path $BackupDir $Name
    if (!(Test-Path -LiteralPath $file)) { throw "원본 백업 파일 없음: $file" }
    Get-Content -LiteralPath $file -Raw -Encoding UTF8 | ConvertFrom-Json
}
function Registry-Root([string]$Hive) {
    if (!$Hive -or $Hive -eq 'HKCU') { return [Microsoft.Win32.Registry]::CurrentUser }
    if ($Hive -eq 'HKLM') { return [Microsoft.Win32.Registry]::LocalMachine }
    throw "지원하지 않는 하이브: $Hive"
}
function Value-Data($Value) {
    switch ($Value.kind) {
        'Binary' { return ,([Convert]::FromBase64String($Value.data)) }
        'None' { return ,([Convert]::FromBase64String($Value.data)) }
        'MultiString' { return ,([string[]]@($Value.strings)) }
        'DWord' { return [int]::Parse($Value.data, [Globalization.CultureInfo]::InvariantCulture) }
        'QWord' { return [long]::Parse($Value.data, [Globalization.CultureInfo]::InvariantCulture) }
        'String' { return [string]$Value.data }
        'ExpandString' { return [string]$Value.data }
        default { throw "지원하지 않는 값 종류: $($Value.kind)" }
    }
}
function Value-Matches($Value) {
    $key = (Registry-Root $Value.hive).OpenSubKey($Value.key)
    try {
        $exists = $null -ne $key -and @($key.GetValueNames()) -contains [string]$Value.name
        if ($exists -ne [bool]$Value.existed) { return $false }
        if (!$exists) { return $true }
        if ($key.GetValueKind($Value.name).ToString() -ne $Value.kind) { return $false }
        $data = $key.GetValue($Value.name, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
        switch ($Value.kind) {
            { $_ -in 'Binary', 'None' } { return [Convert]::ToBase64String($data) -ceq $Value.data }
            'MultiString' {
                if (@($data).Count -ne @($Value.strings).Count) { return $false }
                for ($i = 0; $i -lt @($data).Count; $i++) { if ($data[$i] -cne $Value.strings[$i]) { return $false } }
                return $true
            }
            default { return [Convert]::ToString($data, [Globalization.CultureInfo]::InvariantCulture) -ceq $Value.data }
        }
    } finally { if ($null -ne $key) { $key.Dispose() } }
}
function Restore-Value($Value) {
    if (Value-Matches $Value) { return }
    $root = Registry-Root $Value.hive
    if ($Value.existed) {
        $key = $root.CreateSubKey($Value.key)
        try { $key.SetValue([string]$Value.name, (Value-Data $Value), [Microsoft.Win32.RegistryValueKind][Enum]::Parse([Microsoft.Win32.RegistryValueKind], $Value.kind)) }
        finally { $key.Dispose() }
    } else {
        $key = $root.OpenSubKey($Value.key, $true)
        try { if ($null -ne $key) { $key.DeleteValue([string]$Value.name, $false) } }
        finally { if ($null -ne $key) { $key.Dispose() } }
    }
}
function Key-Matches($Record) {
    $root = Registry-Root $Record.hive
    $key = $root.OpenSubKey($Record.key)
    try {
        if (!$Record.existed) { return $null -eq $key }
        if ($null -eq $key) { return $false }
    } finally { if ($null -ne $key) { $key.Dispose() } }
    if ($null -eq $Record.image) {
        $export = Join-Path $BackupDir $Record.file
        if (!(Test-Path -LiteralPath $export)) { throw "레지스트리 내보내기 백업 없음: $export" }
        $temporary = Join-Path ([IO.Path]::GetTempPath()) ('EasyRetro-reg-' + [guid]::NewGuid().ToString('N') + '.reg')
        try {
            $code = Invoke-Native 'reg.exe' ('export "' + $Record.hive + '\' + $Record.key + '" "' + $temporary + '" /y')
            if ($code -ne 0) { return $false }
            return [IO.File]::ReadAllText($temporary) -ceq [IO.File]::ReadAllText($export)
        } finally { if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary -Force } }
    }
    foreach ($image in $Record.image) {
        $key = $root.OpenSubKey($image.path)
        try {
            if ($null -eq $key -or $key.ValueCount -ne @($image.values).Count) { return $false }
            foreach ($value in $image.values) { if (!(Value-Matches $value)) { return $false } }
            if ($image.childrenCaptured) {
                $actual = @($key.GetSubKeyNames() | Sort-Object)
                $expected = @($image.subkeys | Sort-Object)
                if ($actual.Count -ne $expected.Count -or ($actual -join "`0") -ine ($expected -join "`0")) { return $false }
            }
        } finally { if ($null -ne $key) { $key.Dispose() } }
    }
    return $true
}
function Restore-Key($Record) {
    $root = Registry-Root $Record.hive
    if (!$Record.existed) { $root.DeleteSubKeyTree($Record.key, $false); return }
    $export = Join-Path $BackupDir $Record.file
    if (Test-Path -LiteralPath $export) {
        $code = Invoke-Native 'reg.exe' ('import "' + $export + '"')
        if ($code -ne 0) { throw "레지스트리 가져오기 실패: $export ($code)" }
    } elseif ($null -eq $Record.image) { throw "레지스트리 내보내기 백업 없음: $export" }
    if ($null -eq $Record.image) { return } # Legacy exports merge; never delete the live tree first.
    foreach ($image in @($Record.image)) {
        $key = $root.CreateSubKey($image.path); $key.Dispose()
        foreach ($value in @($image.values)) { Restore-Value $value }
    }
    $images = @($Record.image)
    for ($i = $images.Count - 1; $i -ge 0; $i--) {
        $image = $images[$i]; $key = $root.OpenSubKey($image.path, $true)
        try {
            foreach ($name in $key.GetValueNames()) { if (@($image.values | ForEach-Object { $_.name }) -notcontains $name) { $key.DeleteValue($name, $false) } }
            if ($image.childrenCaptured) { foreach ($name in $key.GetSubKeyNames()) { if (@($image.subkeys) -notcontains $name) { $key.DeleteSubKeyTree($name, $false) } } }
        } finally { $key.Dispose() }
    }
}
function Delete-Empty([string]$Hive, [string]$Path) {
    $root = Registry-Root $Hive; $key = $root.OpenSubKey($Path)
    if ($null -eq $key) { return }
    try { $empty = $key.ValueCount -eq 0 -and $key.SubKeyCount -eq 0 } finally { $key.Dispose() }
    if ($empty) { $root.DeleteSubKey($Path, $false) }
}
function Run-Registered([string]$Command, [string]$Defaults) {
    $command = [Environment]::ExpandEnvironmentVariables($Command).Trim()
    if ($command -match '^"([^"]+)"\s*(.*)$') { $file = $matches[1]; $arguments = $matches[2] }
    elseif ($command -match '^(.+?\.exe)\s*(.*)$') { $file = $matches[1]; $arguments = $matches[2] }
    else { throw "등록된 제거 명령을 해석할 수 없음: $command" }
    if (![IO.Path]::IsPathRooted($file)) { $file = Join-Path $env:SystemRoot "System32\$file" }
    if (!(Test-Path -LiteralPath $file -PathType Leaf)) { throw "등록된 제거 실행 파일 없음: $file" }
    if (!$arguments) { $arguments = $Defaults }
    $process = Start-Process -FilePath $file -ArgumentList $arguments -PassThru -Wait
    if ($process.ExitCode -notin 0, 3010, 1605, 1614) { throw "제거 종료 코드 $($process.ExitCode): $file" }
}
Write-Host "EasyRetro 0.2.0 텍스트 복구 / 백업: $BackupDir / 적용: $([bool]$Apply) (네트워크 사용 안 함)"
$snapshot = $null
try { $snapshot = Read-Backup 'values.json' } catch { $failures.Add($_.Exception.Message); Write-Warning $_.Exception.Message }
if ($null -eq $snapshot) {
    # Salvage exports even when the JSON index was lost; absent values/keys cannot be inferred.
    $snapshot = [pscustomobject]@{ values = @(); keys = @(); missingKeys = @() }
    if (Test-Path -LiteralPath $BackupDir) {
        foreach ($export in Get-ChildItem -LiteralPath $BackupDir -Filter 'key-*.reg' -File) {
            try {
                $header = [regex]::Match([IO.File]::ReadAllText($export.FullName), '(?m)^\[(HKEY_CURRENT_USER|HKEY_LOCAL_MACHINE)\\([^\r\n]+)\]\r?$')
                if (!$header.Success) { throw "백업 키 경로를 읽을 수 없음: $($export.FullName)" }
                $snapshot.keys += [pscustomobject]@{ module = 'RecoveredExport'; hive = $(if ($header.Groups[1].Value -eq 'HKEY_CURRENT_USER') { 'HKCU' } else { 'HKLM' }); key = $header.Groups[2].Value; existed = $true; image = $null; file = $export.Name }
            } catch { $failures.Add($_.Exception.Message); Write-Warning $_.Exception.Message }
        }
    }
}
# Elevate only a requested mutation requiring machine-wide access; HKCU-only backups need no admin.
$machine = @($snapshot.values) + @($snapshot.keys) | Where-Object { $_.hive -eq 'HKLM' }
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if ($Apply -and !$admin -and (@($machine).Count -gt 0 -or $RemoveComponents -or $RestartShell)) {
    Write-Host '요청한 시스템 복구에 관리자 권한이 필요합니다. 같은 사용자 계정의 UAC를 승인하세요.'
    if (!$PSCommandPath) { throw '붙여넣은 내용을 UTF-8 BOM .ps1 파일로 저장한 뒤 다시 실행하세요.' }
    $arguments = '-NoProfile -ExecutionPolicy Bypass -File "' + $PSCommandPath + '" -BackupDir "' + $BackupDir + '" -Apply -OriginalUserSid ' + $OriginalUserSid
    if ($RemoveComponents) { $arguments += ' -RemoveComponents' }; if ($RestartShell) { $arguments += ' -RestartShell' }
    $process = Start-Process powershell.exe -Verb RunAs -ArgumentList $arguments -Wait -PassThru
    exit $process.ExitCode
}
if ($Apply -and $null -eq ('EasyRetroRestoreNative' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class EasyRetroRestoreNative {
 [DllImport("user32.dll", CharSet=CharSet.Unicode, SetLastError=true)] public static extern bool SystemParametersInfo(uint action,uint param,IntPtr data,uint flags);
 [DllImport("user32.dll", EntryPoint="SystemParametersInfoW", CharSet=CharSet.Unicode, SetLastError=true)] public static extern bool Wallpaper(uint action,uint param,string data,uint flags);
 [DllImport("user32.dll")] public static extern bool SetSysColors(int count,int[] indexes,int[] colors);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern IntPtr SendMessageTimeout(IntPtr window,uint message,IntPtr w,string text,uint flags,uint timeout,out IntPtr result);
 public static void Spi(uint action,byte[] bytes) { IntPtr p=Marshal.AllocHGlobal(bytes.Length);try {Marshal.Copy(bytes,0,p,bytes.Length);if(!SystemParametersInfo(action,(uint)bytes.Length,p,3))throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());}finally{Marshal.FreeHGlobal(p);} }
}
'@
}
$modules = @(@($snapshot.values) + @($snapshot.keys) | ForEach-Object { $_.module } | Sort-Object -Unique)
if ($modules -contains 'SysColors' -or (Test-Path -LiteralPath (Join-Path $BackupDir 'syscolors.json'))) { Restore-Item '시스템 색상 (syscolors.json)' {
    [int[]]$colors = Read-Backup 'syscolors.json'
    [int[]]$indexes = @(0..24) + @(26..30)
    if ($colors.Length -ne $indexes.Length) { throw 'syscolors.json 색상 개수 불일치' }
    if (![EasyRetroRestoreNative]::SetSysColors($colors.Length, $indexes, $colors)) { throw 'SetSysColors 실패' }
} }
if ($modules -contains 'Fonts' -or (Test-Path -LiteralPath (Join-Path $BackupDir 'nonclientmetrics.bin')) -or (Test-Path -LiteralPath (Join-Path $BackupDir 'iconfont.bin'))) {
    foreach ($pair in @(@('nonclientmetrics.bin', 0x2a), @('iconfont.bin', 0x22))) {
        $name = $pair[0]; $action = $pair[1]
        Restore-Item "글꼴 $name" { [EasyRetroRestoreNative]::Spi($action, [IO.File]::ReadAllBytes((Join-Path $BackupDir $name))) }
    }
}
if ($modules -contains 'Wallpaper' -or (Test-Path -LiteralPath (Join-Path $BackupDir 'wallpaper-state.json'))) { Restore-Item '배경 SPI_SETDESKWALLPAPER (레지스트리 원본 재적용 전)' {
    $wallpaper = @($snapshot.values | Where-Object { $_.key -eq 'Control Panel\Desktop' -and $_.name -eq 'Wallpaper' })
    if ($wallpaper.Count -eq 0) { $wallpaper = @($snapshot.keys | ForEach-Object { $_.image } | ForEach-Object { $_.values } | Where-Object { $_.key -eq 'Control Panel\Desktop' -and $_.name -eq 'Wallpaper' }) }
    if ($wallpaper.Count -eq 0 -and (Test-Path -LiteralPath (Join-Path $BackupDir 'wallpaper-state.json'))) {
        $saved = Read-Backup 'wallpaper-state.json'
        if (@($saved.images).Count -gt 0) { $wallpaper = @([pscustomobject]@{ data = [string]$saved.images[0] }) }
    }
    if ($wallpaper.Count -eq 0) { throw '원본 Wallpaper 값 없음' }
    $desktopValues = @($snapshot.values) + @($snapshot.keys | ForEach-Object { $_.image } | ForEach-Object { $_.values })
    foreach ($value in $desktopValues) { if ($value.key -eq 'Control Panel\Desktop' -and $value.name -in 'WallpaperStyle', 'TileWallpaper') { Restore-Value $value } }
    if (![EasyRetroRestoreNative]::Wallpaper(0x14, 0, [string]$wallpaper[0].data, 3)) { throw 'SPI_SETDESKWALLPAPER 실패' }
} }
# Uninstallers can rewrite settings, so remove them before restoring the captured registry.
if ($RemoveComponents) { Restore-Item '등록된 ExplorerPatcher/Open-Shell 제거 프로그램만 실행 (다운로드 안 함)' {
    $seen = @{}
    foreach ($hive in @([Microsoft.Win32.RegistryHive]::CurrentUser, [Microsoft.Win32.RegistryHive]::LocalMachine)) {
        foreach ($view in @([Microsoft.Win32.RegistryView]::Registry64, [Microsoft.Win32.RegistryView]::Registry32)) {
            $root = [Microsoft.Win32.RegistryKey]::OpenBaseKey($hive, $view)
            try {
                $uninstall = $root.OpenSubKey('SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall')
                if ($null -eq $uninstall) { continue }
                try { foreach ($name in $uninstall.GetSubKeyNames()) {
                    $entry = $uninstall.OpenSubKey($name)
                    try {
                        if ($null -eq $entry) { continue }
                        $display = [string]$entry.GetValue('DisplayName')
                        if ($display -notmatch 'ExplorerPatcher|Open-Shell') { continue }
                        $registered = [string]$entry.GetValue('QuietUninstallString')
                        if (!$registered) { $registered = [string]$entry.GetValue('UninstallString') }
                        $identity = "$hive\$name\$registered"
                        if ($seen.ContainsKey($identity)) { continue }; $seen[$identity] = $true
                        $command = $registered; $code = [guid]::Empty
                        if ($display -match 'Open-Shell' -and [guid]::TryParse($name, [ref]$code)) { $command = 'msiexec.exe /x ' + $code.ToString('B') + ' /qn /norestart' }
                        try {
                            try { Run-Registered $command $(if ($display -match 'ExplorerPatcher') { '/uninstall_silent' } else { '/qn /norestart' }) }
                            catch { if ($command -ne $registered -and $registered) { Run-Registered $registered '/qn /norestart' } else { throw } }
                            $remaining = $uninstall.OpenSubKey($name)
                            if ($null -ne $remaining) { $remaining.Dispose(); throw "제거 후에도 앱 등록이 남음: $registered" }
                        } catch { $leftovers.Add("$display : 설정 → 앱에서 제거하거나 $registered 를 실행하세요. $($_.Exception.Message)"); Write-Warning $_.Exception.Message }
                    } finally { if ($null -ne $entry) { $entry.Dispose() } }
                } } finally { $uninstall.Dispose() }
            } finally { $root.Dispose() }
        }
    }
    $menu = Join-Path $env:ProgramFiles 'Open-Shell\StartMenu.exe'
    $location = [Microsoft.Win32.Registry]::LocalMachine.OpenSubKey('SOFTWARE\OpenShell\OpenShell')
    try { if ($null -ne $location -and $location.GetValue('Path')) { $menu = Join-Path ([string]$location.GetValue('Path')) 'StartMenu.exe' } }
    finally { if ($null -ne $location) { $location.Dispose() } }
    if (Test-Path -LiteralPath $menu) {
        try { Run-Registered ('"' + $menu + '" -exit') '' } catch { Write-Warning $_.Exception.Message }
        $leftovers.Add("Open-Shell : 설정 → 앱에서 제거하거나 $menu 를 확인하세요.")
    }
    $ep = Join-Path $env:WINDIR 'dxgi.dll'
    if (Test-Path -LiteralPath $ep) { $leftovers.Add("ExplorerPatcher : 설정 → 앱에서 제거하거나 $(Join-Path $env:ProgramFiles 'ExplorerPatcher\ep_setup.exe') 를 실행하세요.") }
} }
$keys = @($snapshot.keys)
for ($i = $keys.Count - 1; $i -ge 0; $i--) {
    $record = $keys[$i]; if ($null -eq $record) { continue }
    try { if (Key-Matches $record) { Write-Host "[일치/건너뜀] $($record.hive)\$($record.key)"; continue } }
    catch { $failures.Add($_.Exception.Message); Write-Warning $_.Exception.Message; continue }
    Restore-Item "키 $($record.hive)\$($record.key) / $($record.file)" { Restore-Key $record }
}
$values = @($snapshot.values)
for ($i = $values.Count - 1; $i -ge 0; $i--) {
    $value = $values[$i]; if ($null -eq $value) { continue }
    Restore-Item "값 $($value.hive)\$($value.key) [$($value.name)] 종류=$($value.kind) 존재=$($value.existed)" {
        Restore-Value $value
        if (!$value.keyExisted) { Delete-Empty $value.hive $value.key }
    }
}
foreach ($missing in @($snapshot.missingKeys | Sort-Object { $_.Length } -Descending)) {
    if (!$missing) { continue }; $hive = 'HKCU'; $path = $missing
    if ($missing -match '^(HKCU|HKLM)\\(.+)$') { $hive = $matches[1]; $path = $matches[2] }
    Restore-Item "원래 없던 빈 키 제거: $missing" { Delete-Empty $hive $path }
}
if ($modules -contains 'Cursors') { Restore-Item '원본 커서 구성표 다시 읽기 (SPI_SETCURSORS, fWinIni=0)' {
    if (![EasyRetroRestoreNative]::SystemParametersInfo(0x57, 0, [IntPtr]::Zero, 0)) { Write-Warning '커서 레지스트리는 복구되었습니다. 다시 로그인하면 반영됩니다.' }
} }
if ($modules | Where-Object { $_ -in 'Wallpaper', 'SysColors', 'Fonts', 'Sounds', 'Cursors', 'DwmAccent', 'DesktopIcons', 'Shell' }) {
    Restore-Item 'WM_SETTINGCHANGE 브로드캐스트 (소리/커서 구성표 기본값 포함)' {
        $result = [IntPtr]::Zero
        [void][EasyRetroRestoreNative]::SendMessageTimeout([IntPtr]0xffff, 0x1a, [IntPtr]::Zero, 'ImmersiveColorSet', 2, 2000, [ref]$result)
        [void][EasyRetroRestoreNative]::SendMessageTimeout([IntPtr]0xffff, 0x1a, [IntPtr]::Zero, 'Control Panel', 2, 2000, [ref]$result)
    }
}
if ($RestartShell) { Restore-Item 'Explorer 재시작: 현재 세션 종료 후 /RL LIMITED 작업으로 일반 권한 실행' {
    $task = 'EasyRetro-Restore-' + [guid]::NewGuid().ToString('N'); $user = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    $code = Invoke-Native 'schtasks.exe' ('/Create /TN "' + $task + '" /SC ONCE /ST 00:00 /TR "' + $env:WINDIR + '\explorer.exe" /RU "' + $user + '" /RL LIMITED /IT /F')
    if ($code -ne 0) { throw '일반 권한 Explorer 예약 작업 생성 실패' }
    try {
        Get-Process explorer -ErrorAction SilentlyContinue | Where-Object { $_.SessionId -eq (Get-Process -Id $PID).SessionId } | Stop-Process -Force
        Start-Sleep -Seconds 3
        $code = Invoke-Native 'schtasks.exe' ('/Run /TN "' + $task + '"')
        if ($code -ne 0) { throw '일반 권한 Explorer 예약 작업 실행 실패' }
        Start-Sleep -Seconds 3
    } finally { [void](Invoke-Native 'schtasks.exe' ('/Delete /TN "' + $task + '" /F')) }
} }
if ($failures.Count -gt 0) {
    Write-Warning ("나머지 복구를 계속했습니다. 복구하지 못한 항목 (백업 보존):`n" + ($failures -join "`n"))
    exit 1
}
if ($leftovers.Count -gt 0) { Write-Warning ("EasyRetro 설정은 모두 복구했습니다. ExplorerPatcher/Open-Shell 프로그램만 남아 있습니다:`n" + ($leftovers -join "`n")) }
Write-Host $(if ($Apply) { '완료: 원본 설정 복구. 백업 파일은 보존했습니다.' } else { '계획 완료: 실제 시스템 변경 없음. 적용하려면 -Apply를 추가하세요.' })
```

</details>

### 안전한 검증 도구

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\test-recovery.ps1
```

이 검증은 **HKCU\Software\EasyRetroTest*** 아래의 합성 백업에 대해서만 dry-run/적용을 실행합니다. 모든 값 종류, 하위 키, 원래 없던 키, .reg 내보내기, 등록 제거 프로그램 우선 사용과 네트워크 호출 0회, 읽기 전용 CLI, 콘솔 없는 GUI를 검사하고 합성 키/생성 하네스 바이너리를 삭제합니다. 실제 테마 적용/복구, EP/Open-Shell 설치, Explorer 종료는 실행하지 않습니다.

