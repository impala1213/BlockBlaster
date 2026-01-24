

=================================================
==============     업적 (AchievementManager)
=================================================

LSM.Linker_Achievement
 - 업적에 대한 값들을 받아오기 편하게 만든 링커 클래스..

--------------- 옵저버 패턴과 Delegate ---------------
----- 값 변경 구독
Subscribe(E_Achievements_Code _mod, I_Observer _ob)
  - 업적의 값이 변경될 때마다 Notify를 호출합니다.

----- 값 변경 구독 해제
UnSubscribe(E_Achievements_Code _mod, I_Observer _ob)
  - 구독 해제합니다. 

----- 레벨 변경 함수 호출
Add_LevelChange(E_Achievements_Code _code, Action<int> _action) 
  - 원하는 업적의 레벨이 변경될 때 마다 호출됩니다.
  - 함수의 int 매개변수로는 '변경된 레벨'이 들어갑니다.

----- 레벨 변경 함수 호출 제거
Remove_LevelChange(E_Achievements_Code _code, Action<int> _action)
  - 레벨이 변경될 때마다 호출하는 함수를 제거합니다.


--------------- 값의 변경 ---------------

----- 원하는 업적의 값을 추가하는 함수
Add_AchievementValue(E_Achievements_Code _code, int _value)
  - 원하는 업적의 값을 +- 합니다.

----- 원하는 업적의 값을 변경하는 함수
Set_AchievementValue(E_Achievements_Code _code, int _value)
  - 원하는 업적의 값을 변경합니다.

--------------- 받아오기(Getter) ---------------

----- 업적의 총 개수를 받아오기
Get_AchievementLength()
  - 업적의 개수를 반환합니다.

----- 원하는 업적의 값을 받아오기
Get_AchievementValue(E_Achievements_Code _code)
  - 업적의 값을 받아옵니다.

----- 원하는 업적의 레벨을 받아오기
Get_AchievementLevel(E_Achievements_Code _code)
  - 업적의 레벨을 받아옵니다.

----- 원하는 업적의 이름을 받아오기
Get_AchievementName(E_Achievements_Code _code)
  - 업적의 이름(설정한 이름. 코드가 아님.)을 받아옵니다.




=================================================
==============     사운드 (SoundManager)
=================================================

LSM.Linker_Sound
 - 사운드에 대하여 쉽게 조작하기 위한 링커 클래스..

--------------- 조작 ---------------
----- 노래 변경
Audio_Set(E_SoundType _type, string _name)
  - BGM 혹은 SFX의 코드를 매개변수로 줄 경우 BGM은 변경, SFX는 추가 실행합니다.

----- 오디오 뮤트
Audio_MuteSet(E_SoundType _type, bool _mute) 
  - Master, BGM, SFX 를 뮤트. True 혹은 False로 변경 가능합니다.

----- 오디오 뮤트 (토글)
Audio_MuteToggle(E_SoundType _type)
  - Master, BGM, SFX 뮤트 토글.

----- 볼륨을 더하기
Audio_VolumeAlpha(E_SoundType _type, float _volume) 
  - Master, BGM, SFX 의 볼륨을 +-합니다.

----- 볼륨을 세팅.
Audio_VolumeSet(E_SoundType _type, float _volume)
  - 볼륨 세팅. 0~1의 값으로 조정이 가능합니다.

--------------- 받아오기(Getter) ---------------
----- 현재 볼륨의 값 받아오기
Audio_GetVolume(E_SoundType _type) 
  - Master, BGM, SFX 의 현재 볼륨 값 받아오기. 0~1의 값으로 반환합니다.

----- 현재 뮤트상태 받아오기
Audio_GetMute(E_SoundType _type)
  - Master, BGM, SFX의 현재 뮤트 상태를 받아옵니다.

----- 현재 혹은 가장 마지막의 노래의 코드를 반환
Audio_GetCode(E_SoundType _type)
  - BGM, SFX의 현재 노래 '코드'를 반환합니다.







