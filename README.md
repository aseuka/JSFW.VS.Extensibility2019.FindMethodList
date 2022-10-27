# JSFW.VS.Extensibility2019.FindMethodList
함수 목록 보기  (( 💙 내가 가장 애정을 가진 프로그램 중 하나! 💙 ))

목적 : VS상에서 메소드만 보고 소스위치를 찾아갈 수 있게 만듬.<br />
 만들 당시엔 VS 자체에서 제공하는 부분에서는 필드 메소드 프로퍼티 등 다 섞여 있어서 원하는 위치로 찾아가기 힘들어서<br />
 메소드 목록만 보고 소스 위치로 찾아갈 수 있게 만들었다.<br />
 SI에서 여러 화면들을 동일한 패턴으로 구현하기에 특정 메서드들을 자주 찾아가야 하는데 이때 사용하기 좋다.<br />
 

- 마우스 우측버튼으로 컨텍스트 메뉴를 띄우면 [메소드 찾아가기 Ctrl+1] 메뉴가 뜬다<br />
![image](https://user-images.githubusercontent.com/116536524/198198649-72d3bb60-85be-4377-b13d-3cbfa26c365f.png)

- 메소드 목록 팝업<br />
![image](https://user-images.githubusercontent.com/116536524/198209144-eaf433f5-3728-4b73-9558-a9b7044a8847.png)<br />
: DragDrop 주황색 색상을 지정하여 목록에서 해당 키워드가 들어간 메소드는 주황색 글씨로 표시된다.<br />
: 생성자는 푸른색 굵은 글씨체가 기본 적용된다.<br />


- 키워드 색상설정 팝업 [메소드 목록 팝업:왼쪽 상단 톱니바퀴 버튼 클릭]<br />
![image](https://user-images.githubusercontent.com/116536524/198209189-07d9542d-2e97-4fe4-8413-9f497844879e.png)


---
- package 폴더가 없어 확인해보니<br />
  Visual Studio로 소스 열릴때 알아서 생긴다. 
  그리고 관리자 모드가 아니면 아래와 같은 오류가 발생한다.

- 소스 열었을때 오류 발생시<br />
```diff
-오류		프로젝트에 "GenerateFileManifest" 대상이 없습니다.	JSFW.VS.Extensibility2019.VariableUsingView	
```
해결방법 :: 관리자 모드로 Visual Studio를 열면된다. 

---
