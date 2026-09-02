using System.Collections.Generic;
using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.Types.Units;
using CadToUnityPlugin;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Color = UnityEngine.Color;

public class DwgManager : MonoBehaviour
{
    [Header("[DWG Settings]")]
    [SerializeField] private string _fileName = "";
    [SerializeField] private float _unitScale = 0.001f; // mm -> m 단위 변환

    [Header("[LineRenderer Settings]")]
    [SerializeField] private Transform _lineContainer;
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private float _lineWidth = 0.2f;
    [SerializeField] private Color _lineColor = Color.green;
    
    [Header("[Wall Settings]")]
    [SerializeField] private WallDataSO _wallData;

    [SerializeField] private Transform _wallParent;

    private DwgLoader _dwgLoader;
    private DwgWallMaker _wallMaker;

    private async void Start()
    {
        await LoadAndDrawDwgLinesAsync();
    }

    private async UniTask LoadAndDrawDwgLinesAsync()
    {
        _dwgLoader ??= new DwgLoader();
        _wallMaker ??= new DwgWallMaker();
        _wallMaker.Initialize(_wallData,_wallParent);

        // 1. StreamingAssets에서 DWG 파싱
        CadDocument cadDoc = await _dwgLoader.LoadStreamingAssetsFolderDwgAsync(_fileName);
        if (cadDoc == null)
        {
            Debug.LogError($"[DwgLineVisualizer] 도면 파일을 찾을 수 없습니다: {_fileName}");
            return;
        }
       
        // 2. 오브젝트 생성 없이 CadDocument 엔티티에서 직접 선분 좌표 추출
        List<List<Vector2>> pathList = ExtractPathsFromCadDoc(cadDoc);

        // 3. 중심점(Median) 계산 후 원점(0,0) 정렬
        Vector2 median = CalculateMedian(pathList);
        AdjustToCenter(pathList, median);

        // 4. 유니티 씬에 최종 LineRenderer 생성
        CreateSceneLines(pathList);
        
        // 5. 벽 생성
        _wallMaker.CreateSimpleWalls(pathList);
        
        // 6. 교차점 처리 샘플
        //_wallMaker.FixAllIntersections();
        Debug.Log($"[DwgLineVisualizer] 총 {pathList.Count}개의 선분 추출 및 생성 완료!");
    }

    // ACadSharp Entities에서 Line 및 Polyline 좌표만 직접 추출
    private List<List<Vector2>> ExtractPathsFromCadDoc(CadDocument cadDoc)
    {
        var paths = new List<List<Vector2>>();

        foreach (var entity in cadDoc.Entities)
        {
            // 단일 직선 (Line) 처리
            if (entity is Line line)
            {
                var segment = new List<Vector2>
                {
                    new Vector2((float)line.StartPoint.X * _unitScale, (float)line.StartPoint.Y * _unitScale),
                    new Vector2((float)line.EndPoint.X * _unitScale, (float)line.EndPoint.Y * _unitScale)
                };
                paths.Add(segment);
            }
            // 연결된 선 (LightWeight Polyline) 처리
            else if (entity is LwPolyline polyline)
            {
                var segment = new List<Vector2>();
                foreach (var vertex in polyline.Vertices)
                {
                    segment.Add(new Vector2((float)vertex.Location.X * _unitScale, (float)vertex.Location.Y * _unitScale));
                }

                if (polyline.IsClosed && segment.Count > 0)
                {
                    segment.Add(segment[0]); // 닫힌 폴리라인 처리
                }

                if (segment.Count >= 2)
                {
                    paths.Add(segment);
                }
            }
        }

        return paths;
    }
    
    /*private List<List<Vector2>> ExtractPathsFromCadDoc(CadDocument cadDoc)
    {
        List<LineSegment2D> rawSegments = new List<LineSegment2D>();

        // 1. 모든 Line과 Polyline을 최소 단위의 개별 선분(Segment)으로 수집
        foreach (var entity in cadDoc.Entities)
        {
            if (entity is Line line)
            {
                Vector2 p1 = new Vector2((float)line.StartPoint.X * _unitScale, (float)line.StartPoint.Y * _unitScale);
                Vector2 p2 = new Vector2((float)line.EndPoint.X * _unitScale, (float)line.EndPoint.Y * _unitScale);
                
                if (Vector2.Distance(p1, p2) > 0.001f)
                    rawSegments.Add(new LineSegment2D(p1, p2));
            }
            else if (entity is LwPolyline polyline)
            {
                var vertices = polyline.Vertices;
                int count = vertices.Count;
                if (count < 2) continue;

                for (int i = 0; i < count - 1; i++)
                {
                    Vector2 p1 = new Vector2((float)vertices[i].Location.X * _unitScale, (float)vertices[i].Location.Y * _unitScale);
                    Vector2 p2 = new Vector2((float)vertices[i + 1].Location.X * _unitScale, (float)vertices[i + 1].Location.Y * _unitScale);
                    
                    if (Vector2.Distance(p1, p2) > 0.001f)
                        rawSegments.Add(new LineSegment2D(p1, p2));
                }

                if (polyline.IsClosed && count > 2)
                {
                    Vector2 pLast = new Vector2((float)vertices[count - 1].Location.X * _unitScale, (float)vertices[count - 1].Location.Y * _unitScale);
                    Vector2 pFirst = new Vector2((float)vertices[0].Location.X * _unitScale, (float)vertices[0].Location.Y * _unitScale);
                    
                    if (Vector2.Distance(pLast, pFirst) > 0.001f)
                        rawSegments.Add(new LineSegment2D(pLast, pFirst));
                }
            }
        }

        // 2. 일직선상에서 겹치는 선분들을 하나로 병합
        List<LineSegment2D> mergedSegments = MergeCollinearSegments(rawSegments);

        // 3. 기존 반환 포맷인 List<List<Vector2>>로 변환
        var paths = new List<List<Vector2>>();
        foreach (var seg in mergedSegments)
        {
            paths.Add(new List<Vector2> { seg.Start, seg.End });
        }

        return paths;
    }*/

    private Vector2 CalculateMedian(List<List<Vector2>> paths)
    {
        float totalX = 0f, totalY = 0f;
        int count = 0;
        foreach (var path in paths)
        {
            foreach (var pt in path)
            {
                totalX += pt.x;
                totalY += pt.y;
                count++;
            }
        }
        return count > 0 ? new Vector2(totalX / count, totalY / count) : Vector2.zero;
    }

    private void AdjustToCenter(List<List<Vector2>> paths, Vector2 center)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            for (int j = 0; j < paths[i].Count; j++)
            {
                paths[i][j] -= center;
            }
        }
    }

    private float GetUnit(CadDocument cadDoc)
    {
        var units = cadDoc.Header.InsUnits;
        Debug.Log($"Units => {units}");
        float unit;
        switch (units)
        {
            case UnitsType.Millimeters:
                unit = 0.001f;
                break;
            case UnitsType.Inches:
                unit = 0.0254f;
                break;
            case UnitsType.Centimeters:
                unit = 0.01f;
                break;
            case UnitsType.Meters:
                unit = 1f;
                break;
            case UnitsType.Kilometers:
                unit = 1000f;
                break;
            case UnitsType.Unitless:
                unit = 0.0254f;
                break;
            default:
                unit = 1f;
                break;
        }
        Debug.Log($"new Unit : {unit}");
        return unit;
    }

    private void CreateSceneLines(List<List<Vector2>> paths)
    {
        foreach (var path in paths)
        {
            var lineObj = new GameObject("Dwg_Line");
            lineObj.transform.SetParent(_lineContainer, false);

            var lr = lineObj.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = path.Count;
            lr.material = _lineMaterial;
            lr.startColor = _lineColor;
            lr.endColor = _lineColor;
            lr.startWidth = _lineWidth;
            lr.endWidth = _lineWidth;

            var positions = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                // 2D 좌표 (X, Y) -> 유니티 3D 바닥면 (X, 0, Z) 매핑
                positions[i] = new Vector3(path[i].x, 0, path[i].y);
            }
            lr.SetPositions(positions);
        }
    }
    
    private List<LineSegment2D> MergeCollinearSegments(List<LineSegment2D> segments)
    {
        List<LineSegment2D> result = new List<LineSegment2D>(segments);
        bool mergedAny = true;
        const float distEpsilon = 0.01f; // 동일 직선 판별 거리 오차 허용값 (미터/스케일 단위)

        while (mergedAny)
        {
            mergedAny = false;

            for (int i = 0; i < result.Count; i++)
            {
                for (int j = i + 1; j < result.Count; j++)
                {
                    if (TryMergeSegments(result[i], result[j], distEpsilon, out LineSegment2D merged))
                    {
                        result[i] = merged;
                        result.RemoveAt(j);
                        mergedAny = true;
                        break;
                    }
                }
                if (mergedAny) break;
            }
        }

        return result;
    }

    private bool TryMergeSegments(LineSegment2D a, LineSegment2D b, float epsilon, out LineSegment2D merged)
    {
        merged = default;

        Vector2 aDir = (a.End - a.Start).normalized;
        Vector2 bDir = (b.End - b.Start).normalized;

        // 1. 평행 여부 확인 (외적의 크기가 0에 가까운지)
        float cross = Mathf.Abs(aDir.x * bDir.y - aDir.y * bDir.x);
        if (cross > 0.005f) return false;

        // 2. 동일 무한 직선 상에 놓여 있는지 확인 (점과 직선 간 거리 판별)
        Vector2 toB1 = b.Start - a.Start;
        float distToLine = Mathf.Abs(toB1.x * aDir.y - toB1.y * aDir.x);
        if (distToLine > epsilon) return false;

        // 3. 선분 A의 방향 축으로 모든 점(4개)을 1D 투영(Dot Product)
        float aStartProj = 0f;
        float aEndProj = Vector2.Dot(a.End - a.Start, aDir);
        float bStartProj = Vector2.Dot(b.Start - a.Start, aDir);
        float bEndProj = Vector2.Dot(b.End - a.Start, aDir);

        float aMin = Mathf.Min(aStartProj, aEndProj);
        float aMax = Mathf.Max(aStartProj, aEndProj);
        float bMin = Mathf.Min(bStartProj, bEndProj);
        float bMax = Mathf.Max(bStartProj, bEndProj);

        // 4. 1D 축 상에서 두 선분이 겹치거나 맞닿아 있는지 검사
        if (aMax + epsilon < bMin || bMax + epsilon < aMin)
        {
            return false; // 분리된 독립 선분
        }

        // 5. 겹치므로 양 극단점을 찾아 새 선분으로 병합
        float newMinProj = Mathf.Min(aMin, bMin);
        float newMaxProj = Mathf.Max(aMax, bMax);

        Vector2 newStart = a.Start + aDir * newMinProj;
        Vector2 newEnd = a.Start + aDir * newMaxProj;

        merged = new LineSegment2D(newStart, newEnd);
        return true;
    }
}

public struct LineSegment2D
{
    public Vector2 Start;
    public Vector2 End;

    public LineSegment2D(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }
}
