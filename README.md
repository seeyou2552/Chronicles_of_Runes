# 🎮 FinalProject – Chronicle of Rune

  <img width="1601" height="897" alt="Image" src="https://github.com/user-attachments/assets/9b0366ee-88f6-4cb8-bc36-3ba0441f5d0e" />  

    
## 📖 목차
1. [프로젝트 소개](#프로젝트-소개)
2. [팀 소개](#팀-소개)
3. [프로젝트 계기](#프로젝트-계기)
4. [주요 기능](#주요-기능)
5. [개발 기간](#개발-기간)
6. [기술 스택](#기술-스택)
8. [와이어프레임](#와이어프레임)
9. [프로젝트 파일 구조](#프로젝트-파일-구조)
10. [Trouble Shooting](#trouble-shooting)

---

## 👨‍🏫 프로젝트 소개

**끝없는 던전, 무한한 전략! 당신만의 전투 스타일로 정복하라!**  
이 프로젝트는 **스테이지 기반 로그라이크 액션 RPG**입니다.

- 매번 새롭게 생성되는 던전에서 플레이어는 **개성 넘치는 몬스터와 강력한 보스**를 마주합니다.
- **스킬북**을 통해 다양한 스킬 습득 및 **룬 장착**으로 스킬의 효과를 강화하거나 변형할 수 있습니다.
- 같은 스킬도 사용자의 빌드에 따라 **완전히 다른 전략**이 가능하며, 매번 새로운 전투 경험을 제공합니다.

> 예측 불가능한 던전 구조, 무궁무진한 스킬 & 룬 조합,  
> 그리고 당신만의 독창적인 빌드로 매번 색다른 전투를 즐겨보세요.
  
<p align="center">
  <img src="https://github.com/user-attachments/assets/3b21f70d-248c-4c8d-a031-59c23008a306" width="300">
  <img src="https://github.com/user-attachments/assets/3a6f6a13-2f1d-4d12-9f24-af96ab746144" width="300"><br>
  <img src="https://github.com/user-attachments/assets/0c04f4b0-8641-4ecb-9e3f-cca5daff60ed" width="300">
  <img src="https://github.com/user-attachments/assets/57c036fb-d1db-4856-9461-efe8878410a7" width="300">
</p>  

  

---

## 👥 팀 소개
| 이름 | 역할 | GitHub |
| --- | --- | --- |
| 이동헌 | 데이터 관리 시스템 개발, CSV-SO 변환 툴 구현 | [GitHub](https://github.com/leedh1211) |
| 송준호 | ScriptableObject 기반 FSM 설계 및 몬스터 패턴 시스템 | [GitHub](https://github.com/3303twg) |
| 이시율 | 스킬 시스템 및 데이터 영속성 관리 구현 | [GitHub](https://github.com/seeyou2552) |
| 박나연 | 인벤토리 및 UI, 키 리바인딩 시스템 개발 | [GitHub](https://github.com/N-Y-P) |
| 이서형 | 절차적 맵 생성 알고리즘, 패시브 시스템 구현 | [GitHub](https://github.com/lsh000219) |

---

## 📌 프로젝트 계기
로그라이크 장르는 매번 다른 플레이 경험을 제공한다는 점에서 큰 매력을 가지고 있습니다.  
이 프로젝트는 “**같은 스킬도 어떤 조합을 하느냐에 따라 완전히 다른 빌드가 된다**”는 아이디어에서 출발했으며,  
다양한 스킬과 룬 시스템, 절차적 던전 생성으로 매번 새로운 전략을 만드는 경험을 제공합니다.

---

## 💜 주요 기능
- **랜덤 던전 생성 시스템**: 매번 다른 구조의 던전을 제공
- **룬 기반 스킬 커스터마이징**: 동일 스킬도 룬 조합에 따라 다른 성능
- **다양한 몬스터와 보스 패턴**: FSM 기반 패턴 제어로 유연한 AI
- **키 리바인딩 시스템**: 플레이어 맞춤형 조작 설정 가능
- **CSV-SO 기반 데이터 관리 툴**: 협업 시 데이터 충돌 최소화 및 생산성 향상

---

## ⏲ 개발 기간
- **2025-06-23 ~ 2025-08-14**

---

## 📚 기술 스택

| 구분 | 내용 |
| --- | --- |
| **게임 엔진** | Unity 2022.3.17f1 |
| **언어** | C# |
| **버전 관리** | Git / GitHub |
| **IDE** | Visual Studio / Rider |
| **데이터 관리** | CSV + ScriptableObject 변환 툴 |
| **배포** | PC 빌드 |

---


## 🎨 와이어프레임
  
<p align="center">
  <img src="https://github.com/user-attachments/assets/a4c870f2-84a6-4b5a-a008-f97848a5b708" width="300">
  <img src="https://github.com/user-attachments/assets/a1840aad-7ab2-419a-bf37-a8e6ee1f12e4" width="300">
  <img src="https://github.com/user-attachments/assets/581d2514-1b76-4ec7-82de-7eb1c9e4324a" width="300"><br>
  <img src="https://github.com/user-attachments/assets/ff945351-e9b7-450b-952b-ce3d42403c0a" width="300">
  <img src="https://github.com/user-attachments/assets/1ff99e7d-2ed5-4344-8368-b33c0d4931c7" width="300">
</p>
  
---


## 프로젝트 파일 구조
Assets/  
├── 📂 Scripts/                  # 게임 로직 스크립트  
│   ├── 📂 Enemy/                # 적 AI, 패턴, FSM 등  
│   ├── 📂 Player/               # 플레이어 이동, 전투, 입력 처리  
│   ├── 📂 Skills/               # 스킬, 룬, 효과 처리  
│   ├── 📂 Map/                  # 맵 생성, 방 연결, 포탈 처리  
│   └── 📂 UI/                   # UI 컨트롤러, HUD, 메뉴  
│  
├── 📂 ScriptableObjects/    # 스킬, 아이템, 몬스터 데이터  
├── 📂 Prefabs/                  # 게임 오브젝트 프리팹  
└── 📂 Scenes/                   # Unity 씬 파일  




## 🐞 Trouble Shooting

### 이동헌 – CSV → ScriptableObject 데이터 관리 개선
- **문제**: SO 방식은 Git 머지 충돌 잦음, Inspector 반복 작업 비효율
- **해결**: CSV 기반 데이터 관리 + 자동 SO 변환 툴
- **성과**: 충돌 최소화, 수백 개 SO 일괄 수정 가능

### 송준호 – ScriptableObject 기반 FSM
- **문제**: 코드 하드코딩으로 상태 확장 어려움
- **해결**: 상태를 SO로 분리, 인스펙터에서 수치 조정 가능
- **성과**: 코드 수정 없이 패턴 추가 가능, 디자이너 협업 효율 향상

### 이시율 – 스킬 커스터마이징 구조
- **문제**: 일부 스킬 구조에서 커스터마이징 적용이 어려운 현상
- **해결**: 특정 일부 스킬 구조에 별도 예외처리를 통해 변화를 주어 커스터마이징이 되도록 구현
- **성과**: 자유로운 커스터마이징 경험 제공

### 박나연 – 키 리바인딩 개선
- **문제**: 여러 슬롯이 동시에 입력 대기 상태로 진입 가능
- **해결**: 전역 상태 체크 후 버튼 잠금 처리
- **성과**: 안정적 키 리바인딩 제공

### 이서형 – 절차적 맵 생성
- **문제**: 로그라이크 특유의 랜덤요소를 구현 해야함
- **해결**: 길 방향 기반 방 배치 알고리즘 구현
- **성과**: 보스룸/상인 방 자동 배치 가능
