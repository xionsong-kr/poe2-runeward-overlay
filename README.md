# POE2 룬수호 오버레이

Path of Exile 2의 **룬수호(Rune Ward)** 수치를 화면에 실시간으로 표시하는 WPF 오버레이입니다.  
게임 HUD에서 눈에 잘 띄지 않는 룬수호 상태를 바 + 숫자로 항상 화면 위에 띄워줍니다.

---

## 기능

- **실시간 수치 표시** — BitBlt 캡처 + Tesseract OCR로 300ms마다 읽어 표시
- **상태별 색상**
  - 파란 바 — 정상
  - 금색 바 — 칼구르의 잔류물 오버플로우 (최대치 × 최대 1.5배)
  - 빨간 바 + 점멸 — 위험 (설정한 임계값 이하)
- **클릭 통과** — 오버레이가 게임 조작을 방해하지 않음
- **이동 모드** — `Ctrl+Shift+M`으로 토글, 드래그로 위치 변경 (황금 테두리 표시)
- **시스템 트레이** — 트레이 아이콘 우클릭으로 설정/종료
- **OCR 필터링** — 자릿수 검사, 급변 확인, 설정 최대치 기반 상한 필터로 오인식 억제

---

## 요구사항

- Windows 10/11
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- Tesseract 언어 데이터: `eng.traineddata`

---

## 설치 및 실행

### 1. 빌드

```bash
git clone https://github.com/xionsong-kr/poe2-runeward-overlay.git
cd poe2-runeward-overlay
dotnet build
```

### 2. tessdata 준비

Tesseract 영어 언어 데이터를 다운로드해 아래 경로에 배치합니다.

```
POE2RuneWardOverlay/tessdata/eng.traineddata
```

다운로드 링크:  
https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata

### 3. 실행

```
POE2RuneWardOverlay/bin/Debug/net8.0-windows/POE2RuneWardOverlay.exe
```

---

## 설정

실행 시 설정창이 자동으로 열립니다. 트레이 아이콘 우클릭 → 설정으로도 열 수 있습니다.

| 항목 | 설명 |
|---|---|
| 위험 임계값 | 이 비율 이하로 떨어지면 빨간 바 + 점멸 |
| 수호 최대치 | 내 캐릭터의 룬수호 최대값 입력 (0이면 OCR 자동 감지) |
| 캡처 영역 X/Y/W/H | 게임 화면에서 룬수호 텍스트가 표시되는 영역 좌표 |
| 캡처 영역 미리보기 | 버튼 클릭 시 설정된 캡처 영역을 3초간 빨간 테두리로 표시 |

### 캡처 영역 찾기

1. 설정창에서 대략적인 좌표 입력
2. **캡처 영역 미리보기** 버튼으로 게임 화면에서 위치 확인
3. 룬수호 숫자가 테두리 안에 들어오도록 조정 후 저장

> 1920×1080 기준 기본값: X=20, Y=832, W=220, H=35

---

## 단축키

| 단축키 | 동작 |
|---|---|
| `Ctrl+Shift+M` | 이동 모드 토글 (황금 테두리 표시 시 드래그 가능) |

---

## 주의사항

- `tessdata/eng.traineddata`는 용량 문제로 레포에 포함되지 않습니다. 위 링크에서 직접 다운로드하세요.
- 게임이 **전체화면(보더리스)** 모드일 때 정상 작동합니다.
- OCR 인식률은 캡처 영역 설정에 따라 달라집니다. 수호 숫자만 딱 맞게 잡는 것이 좋습니다.
