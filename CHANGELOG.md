# Version 2.0.0
Is there anybody who watches this?

### Additional (추가점)
- Added option that sets one-trigger event text's despawn time.
	> 한번만 작동되는 이벤트 텍스트가 사라지는 시간을 설정할 수 있는 설정을 추가했습니다.
- Added new logging system.
	> 새로운 로깅 시스템 추가.
- Added new random system to choose wider variety of events.
	> 이벤트를 더욱 다양하게 고르기 위해 새로운 랜덤 시스템을 도입했습니다.
- New mod icon! It's not that good, but better than before.
	> 새 모드 아이콘입니다! 완벽하진 않지만, 전보다는 낫죠.

### Changes (수정점)
- Changed option description of 'Family Friendly'.
	> "가족 친화적" 설정의 영어 설명을 수정했습니다.
- Changed the way how events are handled.
	> 이벤트를 관리하는 방식을 완전히 바꾸었습니다.
- Fixed a bug that caused config to not load all.
	> 설정이 전부 로드되지 않던 버그를 수정했습니다.
- Added some missing translations.
	> 놓쳤던 번역 추가.
- Removed camera bopping on `Club Mood` event.
	> `클럽 분위기` 이벤트에서 카메라가 비트 타던 기능을 삭제했습니다.
- Cleaned up code structure and folder organization.
	> 코드 구조와 폴더 정리를 하였습니다.
- (Source code) Changed some korean comments to english.
	> (소스 코드) 몇몇개의 한국어 주석을 영어로 바꿨습니다.
- Updated `README` in the source code.
	> 소스 코드 쪽의 `README`를 업데이트 하였습니다.
- Added `CHANGELOG` in the source code.
	> 소스 코드 쪽에 `CHANGELOG`를 추가했습니다.

# Version 1.5.0
### Additional (추가점)
- (Source code) Now you can easily add an callback for like player spawn/death, monster spawn/death on events.
	> (소스 코드) 이제 이벤트에 플레이어 스폰/사망, 몬스터 스폰/사망 등 이벤트에 콜백을 쉽게 추가할 수 있습니다.

### Changes (수정점)
- Fixed an issue where the event finishes twice.
	> 이벤트가 끝날 때 두 번 끝나는 문제를 수정했습니다.

# Version 1.4.0

I was very busy so heres new update before cosmetic update releases.
> 너무 바빠서 그냥 장식 업데이트 나오기 전에 업데이트를 합니다.

Fixing some bugs later because i actually don't know how to fix them...
> 몇가지 버그는 나중에 고칩니다. 솔직히 어떻게 고치는지 모르겠어요...

### Additional (추가점)
- Added event `Random Upgrade`.
	> `랜덤 업그레이드` 이벤트를 추가했습니다.
- Added event `Random Items`
	> `랜덤 아이템` 이벤트를 추가했습니다.
- Added event `Infinte Stamina`
	> `무한 스테미너` 이벤트를 추가했습니다.
- Added event `Invincible`
	> `무적` 이벤트를 추가했습니다.
- Added event `Infinite Jump`
	> `무한 점프` 이벤트를 추가했습니다.
- Events in settings now has description.
	> 설정에 있는 이벤트들은 이제 설명이 있습니다.
- Added `Timer Duration` option.
	> `타이머 길이` 설정 추가.
- Added options title.
	> 설정 제목 추가.

### Changes (수정점)
- Fixed event duration settings not working.
	> 이벤트 시간 길이 설정이 작동이 되지 않던 점을 수정했습니다.
- Fixed an issue where too many events were running and some were deleted before the end of time.
	> 실행되고 있는 이벤트가 너무 많으면 몇몇개가 시간이 끝나기 전에 삭제되는 문제를 수정했습니다.
- Fixed README.
	> README 수정.
- Now `Club Mood` event does not make carts and grabbed objects bounce.
	> 이제 `클럽 분위기` 이벤트는 잡고있는 물체랑 카트를 방방 뛰게 하지 않습니다.

# Version 1.3
I hope this works fine.

### Additional (추가점)
- I really couldn't put this off any longer, so I rebuilt the settings menu from scratch.
  Config files files from previous versions are **NO LONGER COMPATIBLE** starting from this version.
	> 더 이상 못참겠네요. 설정창 새로 만들었습니다. (예전 설정은 이 버전부터는 호환되지 않습니다.)
- You can now enable or disable each event, adjust its spawn chance, and set its duration from the new settings menu.
	> 새로운 설정창에서 각 이벤트의 활성 여부, 등장 확률, 시간 설정을 할 수 있습니다.
- The settings menu can be opened with F4 by default and is controlled with the mouse. This hotkey can be changed from the existing settings menu.
	> 설정창은 기본값인 F4키로 띄울 수 있고 마우스를 이용해 설정합니다. 이 키는 기존 설정창에 들어가시면 바꿀 수 있습니다.

### Changes (수정점)
- The `Driver of the Year` event has been removed because it kept causing issues every single update.
	> `올해의 운전사` 이벤트가 맨날 업데이트 할때마다 버그만 내뿜는 이유로 그냥 지워버렸습니다.
- Fixed several translation issues.
	> 몇가지의 번역을 수정했습니다.
- Fixed an issue where the `Say random thing` event was displayed as `Multiplayer only.` (honestly this one was kinda funny)
	> `무작위 대사 말하기` 이벤트 이름이 `멀티플레이어 전용입니다.`로 보이는 점을 수정했습니다.
- The UI size has been adjusted to fit the screen properly.
	> UI 크기가 화면에 맞도록 수정되었습니다.
- UI objects were converted into prefab to reduce and simplify the code.
	> UI 오브젝트를 프리팹으로 만들어 코드를 단축했습니다.
- Fixed an issue where the `Club Mood` event was incorrectly affected by the sound effects volume.
	> `클럽 분위기` 이벤트가 사운드 효과 볼륨으로 포함되는 걸 수정했습니다.

# Version 1.2.2
- Fixed a bug that translation file does not exists.
	> 번역 파일이 존재하지 않는다는 버그를 수정했습니다.

# Version 1.2.1
### Additional (추가점)
- I have changed entire logic that how translation works. Now you don't have see a random korean letters in the game.
	> 번역 체계를 완전히 바꾸었습니다. (한국어랑 상관 X)
- You don't need to change the language option when you first launch this mod.
	> 이제 이 모드를 처음 실행할 때마다 설정을 들어가서 언어를 바꿀 필요가 없습니다.

### Changes (수정점)
- Removed the `Invert Gravity` event as it made the game unplayable.
	> 게임 플레이를 불가능하게 만드는 `중력 반대로` 이벤트를 삭제했습니다.
- I FORGOT TO TURN OFF DEBUG MODE RAAAAAAAAAAAAAAAA
	> 디버그 모드 끄는거 까먹음 ㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏ

# Version 1.2
### Additional (추가점)
- Added event `THINK FAST, CHUCKLENUTS!`
	> `빨리 좀 생각해봐, 돌대가리야!` 이벤트를 추가했습니다.
- Added event `Club Mood`
	> `클럽 분위기` 이벤트를 추가했습니다.
- Added event `Gravity Mult`
	> `중력 배수` 이벤트를 추가했습니다.
- Added event `Timer Speed`
	> `타이머 속도` 이벤트를 추가했습니다.
- Added option `Devloper Mode`
	> `개발자 모드` 설정을 추가했습니다.

### Changes (수정점)
- Fixed for REPOLib v3
	> REPOLib v3 버전에 맞게 수정

# Version 1.1
### Additional (추가점)
- Added event `Pupil Dilation`
	> `동공 확장` 이벤트를 추가했습니다.
- Added event `Driver of the year` (Singleplayer only yet)
	> `올해의 운전사` 이벤트를 추가했습니다. (아직은 싱글플레이어만 해당)
- Added event `Spawn Monster`
	> `몬스터 소환` 이벤트를 추가했습니다.
- Added event `No Gravity`
	> `무중력` 이벤트를 추가했습니다.
- Added event `Disable All Lighting`
	> `전체 조명 비활성화` 이벤트를 추가했습니다.
- Now using REPOLib.
	> REPOLib를 사용합니다.
- Added Source code link to the mod page.
	> 모드 페이지에 소스코드 링크를 추가했습니다.

### Changes (수정점)
* Fixed `Game Speed` event playing once per level.
	> 레벨마다 한 번만 실행되던 `게임 속도` 이벤트 버그를 수정했습니다.
* Fixed `Revive all players` event shows error on the console.
	> `죽은 플레이어 소생` 이벤트가 콘솔에 오류만 띄우던 점을 수정하였습니다.
* Changed an event's name `Disable Post Processing` to `Disable Screen Effects` to understand easier.
	> `포스트 프로세싱 비활성화` 이벤트 이름을 이해하기 쉽게 `화면 효과 비활성화`로 바꾸었습니다.
* Fixed an issue where level events would still appear even when disabled.
	> 레벨 이벤트를 비활성화해도 계속 뜨던 문제를 수정했습니다.
* Changed `Earthquake` event sound.
	> `지진` 이벤트의 소리를 변경했습니다.
* If a time-limited event occurs again, the remaining time of the existing event is extended.
	> 만약 시간 제한이 있는 이벤트가 다시 발생하면, 기존 이벤트에 시간을 추가합니다.
* Added Missing Translations
	> 번역 추가

### Version 1.0.2
* oops i forgot to add credit link
  > 앗 크레딧 링크 넣는거 깜빡함;;

# Version 1.0.1
Changes (수정점)
* Fixed an issue where the AssetBundle was not loading in-game.
  > 에셋번들이 불러와지지 않는 문제를 수정했습니다.
* Fixed README.
  > README를 수정했습니다.