using ACadSharp;
using ACadSharp.Entities;
using CadToUnityPlugin;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using ACadSharp.Types.Units;
using UnityEngine;
using Color = UnityEngine.Color;

public class DwgLineVisualizer : MonoBehaviour
{
    [Header("[DWG Settings]")]
    [SerializeField] private string _fileName = "";
    [SerializeField] private float _unitScale = 0.001f; // mm -> m 단위 변환

    [Header("[LineRenderer Settings]")]
    [SerializeField] private Transform _lineContainer;
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private float _lineWidth = 0.2f;
    [SerializeField] private Color _lineColor = Color.green;

    private DwgLoader _dwgLoader;

    private async void Start()
    {
        await LoadAndDrawDwgLinesAsync();
    }

    private async UniTask LoadAndDrawDwgLinesAsync()
    {
        _dwgLoader ??= new DwgLoader();

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
        /*foreach (var path in paths)
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
        }*/
    }
}
