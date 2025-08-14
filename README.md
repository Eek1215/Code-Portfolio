<div align= "center">
    <img src="https://capsule-render.vercel.app/api?type=rounded&color=bbf1c9&height=120&text=Style%20참조용%20Code%20입니다&animation=fadeIn&fontColor=2a2727&fontSize=40" />
    </div>
    <div style="text-align: left;"> 
    <h2 style="border-bottom: 1px solid #d8dee4; color: #282d33;"> 이재교 [ Unity 클라이언트 프로그래머 ] </h2>  
    <div style="font-weight: 700; font-size: 15px; text-align: left; color: #282d33;">코딩을 하면서 세운 규칙과 스타일의 예시입니다. 협업 시 남들이 제 코드를 보고 어려움을 겪지 않았으면 좋겠다는 생각을 늘 품고 있습니다.</li> </div> 
    </div>

<br><br>
### [ 코드를 작성 시 중요하게 생각하는 부분들 ]
-----------------------
* 명명규칙을 세우고 준수하도록 합니다 <br>
  - 카멜, 파스칼 혼용 사용
* 코드는 비슷한 분류가 많을 경우 region으로 묶고, 아니라면 public -> protected -> private -> IEnuemrator 순으로 정리합니다 <br>
  - 단, private 함수 중 public 함수에서 파생 되어 다른 곳에 쓰이지 않는다면 바로 아래에 두기도 합니다. 순서보다 찾는 가독성을 위함입니다
* if문이나 for문을 짧게 쓴다고 무조건 같은 줄에 다닥다닥 붙이는 행위를 하지 않습니다
* 남들이 쉽게 아는 키워드가 아니라면 축약하지 않습니다
* 함수나 변수명을 누구나 쉽게 알만한 것이 아니라면 주석으로 간단한 설명 남겨둡니다
* 자료형을 사용할 때 최대값을 생각하여 용량이 작은 자료형을 택하려고 노력합니다
* 코드는 항상 상 -> 하의 방향을 가진다는 생각으로 코드간의 상관구조 역시 하 -> 상으로 가지 않도록 주의합니다
* 많은 경우에서 코드는 확장성의 가능성을 가지도록 합니다
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
* CustomerController <br>
  - [어서와! 수달타운]은 손님이 타운을 이용하는 게임입니다. 이에 손님이 나오고 관리되는 프로세스입니다. 각 손님마다 특색이 있고 애니메이션이 존재하기에
  하나의 클론으로 생성 후 이미지를 위에다 붙이는 방식은 불가하여 Prefab화로 Dictionary에서 관리를 합니다. 중간중간 손님이 해금되며 종류가 추가 되기에
  다른 자료구조는 사용하면 안됩니다.
<br><br>
* QuestItemData
  - 퀘스트의 카운팅을 위한 이벤트 등록 코드입니다. 모든 퀘스트가 상속을 받으며 delegate에 옵저버 패턴을 혼용하여 만든 방법입니다. 퀘스트라는 명확한 목표의 경우
  람다식을 사용하지 않아 함수를 추가하고 제거하는 부분이 명확하기 때문에 사용하였으며, 람다식을 사용하거나 추가/제거 작업이 자주 일어난다면
  인터페이스를 활용한 옵저버 패턴을 사용할 것 같습니다
<br><br>
* FacilityInfo <br>
  - 굉장히 간단한 코드로 ScriptableObject를 사용하는 예시입니다. 해당 정보를 InfotableMgr라는 코드에서 관리를 하며, 필요한 경우
  원하는 정보만 불러다가 사용합니다. 일종의 데이터 테이블을 간략화 한 것입니다.
<br><br>
* AddressableMgr <br>
  - Addressables를 사용하게 되었을 때 Instance의 생성이 필요한지, 또는 단순한 Resource가 필요한지에 따라 실제로 Load하고 Release를
    할 수 있도록 관리하는 매니저 클래스입니다.
<br><br>
* BuildingObj - FieldObj <br>
  - DI를 프레임워크 없이 사용하기 위하여 최근에 작업한 Isometric에 포함 된 건물 오브젝트입니다. 상속을 기반으로 만든 부모 클래스이며,
    FieldObj의 경우 BuildingObj를 상속하여 사용하여 만든, 텃밭 오브젝트로 여러 작물을 심고 재배할 수 있습니다.
<br><br>
* BuildingBehaviorInjector <br>
  - BuildingObj에 DI를 주입시키기 위하여 사용하는 중간자의 역할로 인터페이스로 구현 된 클래스들을 갖고 있으며, 주입을 통하여 실행 함수를
    주입시키고 BuildingObj에서 실행함수를 호출하게 하여 연결 된 실행함수를 호출할 수 있도록 하는 역할을 합니다.
-----------------------
<br><br><br>
<h3 style="border-bottom: 1px solid #d8dee4; color: #282d33;"> < DI > </h3>
- 협업에 중요한 의존성 주입을 통하여 개발하는 방식을 새 프로젝트에 다루어보았습니다. Zenject, VContainer를 사용하지 않은 기본 BehaviorInjector라는 주입기를
직접 생성하여 BuildingObj에 주입하는 방식으로 하였습니다. 이는 프레임워크를 사용하기 이전 돌아가는 프로세스를 이해하기 위함이었으며, 차후에는 VContainer를 별도로
공부를 하였습니다.

* BuildingObj <br>
  - Isometric 타일에 건물을 지을 때 사용하는 가장 기본 클래스입니다. 모든 건물이 가지는 공통적인 성능을 갖고 있습니다. 실행 함수를 BuildingBehaviorInjector에 호출시켜,
    직접적인 실행 로직이 Injector에 주입되어 있는 인터페이스 구현체를 통해 실행할 수 있습니다.

* BuildingBehaviorInjector <br>
  - BuildingObj와 실행 인터페이스 구현체를 잇는 중재자로 외부에서 구현체를 주입받아 관리를 하고 실행 로직을 구현체에 전달합니다.
 
* FieldObj <br>
  - BuildingObj를 상속받은 밭 오브젝트입니다. 밭 오브젝트만이 사용할 수 있는 기능을 위하여 IFieldObjClickCheck라는 인터페이스 구현체를 갖고 있습니다. 중재자는
    사용하지 않는 방식으로 갖고 있으며, 필요한 기능을 오버라이딩하여 사용합니다.
-----------------------
<br><br><br>
<h3 style="border-bottom: 1px solid #d8dee4; color: #282d33;"> < MVP > </h3>
- UI를 효율적으로 관리하고 확장성을 넓히기 위하여 MVP 패턴을 활용하였습니다. 처음에는 인스펙터 바인딩을 통해 강한 결합구조를 갖고 있었습니다만,
  확장성과 의존성의 문제를 해결하기 위하여 MVP 패턴을 통해 View 작업자와 Presenter 작업자가 나뉘었음을 가정하여 구조를 재설계하였습니다. <br><br>
  <h4 style="border-bottom: 1px solid #d8dee4; color: #000000;">ShopPopup </h4>
  ㄴ ShopPopupWealthContent <br>
  ㄴ ShopPopupGemContent <br>
  ㄴ ShopPopupPackageContent <br>
  ㄴ ShopPopupCostumeContent <br>
  (위의 Content는 모두 Monobehavior 오브젝트입니다)
<br><br>
  <h4 style="border-bottom: 1px solid #d8dee4; color: #000000;"> ShopPopupPresenter </h4>
  ㄴ ShopPopupPackagePresenter -> IShopPopupPackageContent <br>
  ㄴ ShopPopupGemPresenter     -> IShopPopupGemContent <br>
  ㄴ ShopPopupCostumePresenter -> IShopPopupCostumePresenter <br>
  (WealthContent는 구조가 단순하여 Presenter를 나누지 않는 방향을 선택하였습니다. MVP 구조 역시 복잡하고 확장성이 있는 구조에서 필요로 한 것이기 때문에 선택이라고 생각을 합니다.)
<br><br>
  Popup   -> Content O       PopupPresenter   -> ContentPresenter O
  Content -> Popup X         ContentPresenter -> PopupPresenter X
<br><br>
  부모와 자식간의 참조는 아래 단방향만 가능하도록 하였으며, 최대한 외부를 모르도록 구성을 하였습니다.
<br><br>
  외부에서 팝업이 열릴 경우 PopupPresenter를 생성하며, 탭에 따라 PopupPresenter에서 ContentPresenter가 생겨납니다. 닫거나 탭이 바뀔 경우 이전 Presenter의
  연결을 모두 끊고 참조를 null로 하여 가비지 컬렉터(GC)에 수집되게 하여 처리를 합니다. PopupPresenter는 외부에서 생성될 때 ShopPopup의 OnDisable시 호출되는
  Action에 자기 자신의 연결을 끊는 Dispose 함수를 구독하게 하여, OnDisable 생명주기 발동 시 처리가 되도록 구성을 하였습니다.

  
    

