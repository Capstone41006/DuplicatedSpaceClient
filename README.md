# DuplicatedSpaceClient
VR 사용자가 사용할 클라이언트인 Duplicated Space Server의 클라이언트 역할부   
Firebase와의 연동을 위해 자신의 Firebase 정보를 입력해 사용해야 함  

   
## Setup
### 1. 파일 추가
깃허브 100mb 제한으로 인해 일부 파일은 아래의 링크를 통해서 다운받아야 함.   
압축해제 후, 내부 파일들 Asset/ 경로에 넣어야 정상 작동.   
[File Link](https://drive.google.com/file/d/1TI6cAkWc2OQZVlsIt0YMYmfknpkX3GJx/view?usp=drive_link)

### 2. Firebase 연결
유니티 스크립트에서의 Firebase___.cs 스크립트들에서의
```bash
database = FirebaseDatabase.GetInstance("https://fir-***-rtdb.firebaseio.com/");
```
에서의 '***'에 자신의 데이터 입력   

### 3. Firebase Storage 연결
유니티 스크립트에서의 Firebase___.cs 스크립트들에서의
```bash
storageRef = storage.GetReferenceFromUrl("gs://fir-***.appspot.com");
```
에서의 '***'에 자신의 데이터 입력   

### 4. Firebase Admin 설정
GaussianSplattingVRViewer 폴더에 adminsdk 파일을 넣음


## 참조 Github Repo
[GaussianSplattingVRViewer](https://github.com/clarte53/GaussianSplattingVRViewerUnity)


## Example Video
https://youtu.be/9ZbRM-3eRyU
