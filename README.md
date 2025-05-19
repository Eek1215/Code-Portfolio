<div align= "center">
    <img src="https://capsule-render.vercel.app/api?type=rounded&color=bbf1c9&height=120&text=Style%20참조용%20Sample%20Code%20입니다&animation=fadeIn&fontColor=2a2727&fontSize=40" />
    </div>
    <div style="text-align: left;"> 
    <h2 style="border-bottom: 1px solid #d8dee4; color: #282d33;"> 이재교 [ Unity 클라이언트 프로그래머 ] </h2>  
    <div style="font-weight: 700; font-size: 15px; text-align: left; color: #282d33;">코딩을 하면서 세운 규칙과 스타일의 예시입니다</li> </div> 
    </div>

<br><br>
### [ 코드를 작성 시 중요하게 생각하는 부분들 ]
-----------------------
* 명명규칙을 세우고 준수하도록 합니다 - 카멜, 파스칼 혼용 사용
* 남들이 쉽게 아는 키워드가 아니라면 축약하지 않습니다
* 함수나 변수명을 누구나 쉽게 알만한 것이 아니라면 주석으로 간단한 설명 남겨둡니다
* 가독성을 최우선으로 합니다 - if문이나 for문을 짧게 쓴다고 무조건 같은 줄에 다닥다닥 붙이는 행위 X
* 자료형을 사용할 때 최대값을 생각하여 용량이 작은 자료형을 택하려고 노력합니다
* 코드는 항상 상 -> 하의 방향을 가진다는 생각으로 코드간의 상관구조 역시 하 -> 상으로 가지 않도록 주의합니다
* 많은 경우에서 코드는 확장성의 가능성을 가지도록 합니다
* 코드는 비슷한 분류가 많을 경우 region으로 묶고, 아니라면 public -> protected -> private -> IEnuemrator 순으로 정리합니다 - 단, private 함수 중 public 함수에서 파생 되어 다른 곳에 쓰이지 않는다면 바로 아래에 두기도 합니다. 순서보다 찾는 가독성을 위함입니다
* 하나의 스크립트에 모든 기능을 넣지 않고 분리가 가능하다면 분리를 하도록 합니다
---------------------
<br><br>
< 업로드 된 파일에 대한 간략한 설명 >
* GameMgr <br>
  - MVC 구조의 가장 주된 설계로 여러 싱글턴 클래스들이 올바른 실행 순서를 가질 수 있도록 가장 최우선적으로 정의되고 초기화 순서를 잡는 역할을 합니다.
<br><br>
* PostBoxData <br>
  - 데이터를 저장하는 방식중의 하나로 우편함과 관련 된 데이터를 관리합니다. Firebase의 RealTime Database를 사용하여 공용우편함의 경우
  유저의 데이터가 아닌 Official Root를 통해 게임을 시작 시 or 게임 플레이 중 Trigger로 새로운 우편함이 생기거나 하였을 때 추가가되며,
  받은 우편은 상태를 'Received'로 바꾸어, 받은 우편목록으로 저장합니다. 우편물의 키는 'O' = 공용(Official), 'P' = 개인(Personal)과
  종료시간을 같이 나열한 'O_19001231_235959' 와 같이 정합니다. 초단위로 키값이 다르기 때문에 중첩될 일은 없으나 동시에 여러개의 우편을 보내는
  경우가 있다면 뒤에 보상의 타입을 더 추가하는 방안을 생각할 것 같습니다. 우편은 받을 수 있는 종료시간이 지났다면 'TimeOver' 처리되어 보관됩니다.
<br><br>
* SoundMgr <br>
  - 사운드를 관리하는 싱글턴 클래스입니다. BGM, UI, SFX로 관리하며, 각각의 AudioSource를 달고 있습니다. 2D이기 때문에 한개씩으로 관리가 됩니다.
  SFX를 여러 번 내야 하는 경우 PlayOneShot을 사용하여 delay를 주어 사용합니다. BGM의 경우 미리듣기 기능을 위하여 미리듣기 정보와 현재 정보를
  나누어 담고 있어, 미리듣기가 가능합니다.
  Android 기기와 IOS 기기는 각각 고유의 문제가 있는데, Android 기기 중 일부는 이어폰과 같이 이어폰 기기를 장착하거나 장착해제 했을 경우
  사운드가 들리지 않게 되거나, IOS의 경우 광고 재생 후에 사운드가 들리지 않는 문제가 있습니다.
  Android의 경우 AudioSettings.OnAudioConfigurationChanged에 이벤트를 추가 해 기기의 변화가 있을 때 사운드 볼륨을 다시 재정의해주며,
  IOS의 경우 광고 재생 전 Configuration에 사운드 정보를 담고, 그 값을 AudioSettings.Reset으로 초기화 해줍니다.
<br><br>
* FacilityInfo <br>
  - 굉장히 간단한 코드로 ScriptableObject를 사용하는 예시입니다. 해당 정보를 InfotableMgr라는 코드에서 관리를 하며, 필요한 경우
  원하는 정보만 불러다가 사용합니다. 일종의 데이터 테이블을 간략화 한 것입니다.
