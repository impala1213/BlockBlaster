# BlockBlaster 모작

1 블록 데이터 정의
파일 - PieceDefinition.cs
주요 필드
- pieceId - 조각 식별용 문자열
- blocks - 조각이 차지하는 셀 오프셋 목록 배치 프리뷰 렌더링의 기준 데이터 #ex) 3칸 가로 블록이면 (0,0) (1,0) (2,0)
- dragAnchor - 드래그 스냅 기준 셀 origin 계산에 사용 중심이 되는 부분 3칸 가로 블록 기준 Anchor는 (1,0)으로 해야 중앙!
- tileSprite - 타일 스프라이트 손패 프리뷰 보드 배치에 동일 적용
- tileColor - 타일 색상 손패 프리뷰 보드 배치에 동일 적용 - 설정 필요 없음
- tileMaterial - 타일 머티리얼 선택 항목 - 설정 필요 없음

2 좌표 변환 스크린 좌표와 그리드 좌표 변환
파일 - ScreenToGrid.cs
주요 필드
- boardRect - 실제 칸 영역인 GridArea를 지정 BoardBG를 지정하면 배치와 프리뷰가 어긋남!
- CellSize - 한 칸의 픽셀 크기

주요 함수
- Recalculate - boardRect 크기 기준으로 CellSize 재계산 OnRecalculated 이벤트 발생
- TryGetGridPos - 화면 좌표가 어느 셀인지 계산 드래그 중 hover 셀 계산에 사용
- GridToBoardLocalCenter - 셀 중심의 boardRect 로컬 좌표 반환 타일 배치 위치 계산에 사용
- GridCenterToDragLayerLocal - 셀 중심을 DragLayer 로컬 좌표로 변환 스냅 이동에 사용

3 손패 생성 및 리필
파일 - HandController.cs
주요 필드
- slots - 손패 슬롯 3개 지정
- piecePool - 손패에 지급할 조각 목록
- rootCanvas - 캔버스 참조
- dragLayer - 드래그 중 레이어 참조
- game - 게임 로직 참조
- screenToGrid - 좌표 변환 참조

주요 함수
- SpawnInitialHand - 시작 시 슬롯 3개에 런타임으로 HandPiece 생성 후 PieceDragView 추가 DragBlock 프리팹 미사용
- HandlePlaced - 배치 성공 이벤트 수신 시 같은 슬롯 즉시 리필 view SetPiece RandomPiece
- RandomPiece - piecePool에서 랜덤 조각 반환

4 드래그 스냅 프리뷰 배치 시도
파일 - PieceDragView.cs
외부에서 호출하는 API
- Initialize - HandController가 생성 후 반드시 호출 의존성 주입
- SetHomeSlot - 드래그 종료 후 복귀할 슬롯 지정
- SetPiece - 조각 교체 시 시각화 재구성과 레이아웃 재적용

드래그 처리 흐름
- OnBeginDrag - DragLayer로 이동 보드 셀 크기 기준 레이아웃 전환 blocksRaycasts false 설정
- OnDrag - TryGetGridPos로 hover 셀 계산 origin equals hover minus dragAnchor 계산 CanPlace로 가능 여부 판단 tint 적용 필요 시 UpdatePreview로 고스트 표시
- OnEndDrag - TryPlace로 실제 배치 시도 후 슬롯 복귀 성공 시 OnPlaced 이벤트 발생

5 보드 타일 렌더링
파일 - BoardView.cs
주요 내용
- 배치 성공 시 placedCells 기반으로 TileView 생성 또는 재사용 후 배치
- 클리어 시 clearedCells 기반으로 타일 제거 또는 비활성 처리

파일 - TileView.cs
주요 함수
- ApplyLayout - 셀 중심 위치와 셀 크기 적용
- ApplyVisual - sprite color material을 매번 덮어써서 풀링 잔상 방지 기본값 복구 로직 유지

6 게임 로직 엔트리
파일 - GameController.cs
주요 함수
- CanPlace - 배치 가능 여부만 판단 프리뷰와 tint 판단에 사용
- TryPlace - 실제 배치 수행 배치 반영 라인 클리어 점수 갱신 포함 TurnResult 반환
- ResetGame - 보드와 점수 초기화

7 UI 영역과 레이어 구성 원리

7.1 GridArea 
- GridArea는 보드에서 실제로 블록이 놓이는 기준 좌표계 영역
- ScreenToGrid.boardRect는 반드시 GridArea를 참조한다
- TryGetGridPos와 CellSize 계산은 GridArea의 RectTransform 크기와 좌표계를 기준으로 한다
- GridArea가 올바르면 배치 위치 스냅 위치 프리뷰 위치가 모두 같은 기준으로 맞는다

7.2 HandArea 
- HandArea는 손패 슬롯들이 배치되는 UI 영역이다
- HandController.slots는 HandArea 아래의 Slot 오브젝트들을 참조한다
- HandArea는 보드 좌표계와 별개이며 드래그 중에는 DragLayer로 이동하므로 HandArea 자체는 드래그 좌표 계산에 관여하지 않는다
- 슬롯 크기 변화에 따라 PieceDragView.ApplyHandLayoutInSlot이 조각을 슬롯 안에 자동으로 맞춘다

7.3 TileLayer 
- TileLayer는 실제 보드에 확정 배치된 타일을 표시하는 레이어다
- BoardView가 placedCells를 기반으로 TileView를 생성 또는 재사용하여 TileLayer에 배치한다
- TileLayer는 일반적으로 GridArea 하위에 둔다

7.4 PreviewLayer 
- PreviewLayer는 드래그 중 미리보기를 표시하는 레이어다
- PieceDragView.UpdatePreview가 origin과 piece.blocks를 기준으로 각 셀 위치에 previewBlocks를 배치한다
- PreviewLayer는 보통 GridArea 하위에 둔다

7.5 DragLayer 
- DragLayer는 드래그 중인 조각을 올려두는 레이어다
- PieceDragView.OnBeginDrag에서 조각 오브젝트를 DragLayer로 이동시켜 보드 위에서 자유롭게 움직이게 한다
- dragLayer는 Canvas 전체를 덮는 full stretch RectTransform을 권장한다



