using System;
using System.Collections.Generic;
using UnityEngine;

public class DwgWallMaker
{
    private Material _wallMaterial;
    private Transform _transform;
    private float _wallHeight;
    private float _wallThickness;
    private float _maginficationRate;
    private Vector3 _medianPosition = Vector3.zero;
    
    private HashSet<LineKey> _generatedLines = new HashSet<LineKey>();
    private HashSet<Vector2Int> _alreadyCorrectedPoints;
    private Dictionary<Vector2Int, List<WallIntersectionData>> _intersectionsByPoint;
    private Vector3 V2ToV3(Vector2 v) => new Vector3(v.x, 0f, v.y);
    private Vector2Int ToGridKey(Vector2 v, float scale = 1000f) =>
        new Vector2Int(Mathf.RoundToInt(v.x * scale), Mathf.RoundToInt(v.y * scale));

    public Dictionary<int, List<WallSegment>> WallSegmentsByPath { get; private set; } = new Dictionary<int, List<WallSegment>>();
    
    public DwgWallMaker()
    {
        _intersectionsByPoint = new Dictionary<Vector2Int, List<WallIntersectionData>>();
        _generatedLines = new HashSet<LineKey>();
        _alreadyCorrectedPoints = new HashSet<Vector2Int>();
    }
    
    public void Initialize(WallDataSO data, Transform transform)
    {
        _wallMaterial = data.wallMaterial;
        _transform = transform;
        _wallHeight = data.wallHeight;
        _wallThickness = data.wallThickness;
        _maginficationRate = data.magnificationRate;
    }
    
    public void CreateSimpleWalls(List<List<Vector2>> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            CreateWallByPathWithId(paths[i], i);
        }
    }
    
    private void CreateWallByPathWithId(List<Vector2> path, int pathId)
    {
        int cnt = path.Count;
        for (int i = 0; i < cnt - 1; i++)
        {
            Vector2 a = path[i] * _maginficationRate;
            Vector2 b = path[i + 1] * _maginficationRate;
            var key = new LineKey(a, b);
            if (_generatedLines.Add(key) == false)
                continue;

            Vector3 start = V2ToV3(a);
            Vector3 end   = V2ToV3(b);
            Vector2Int keyA = ToGridKey(a);
            Vector2Int keyB = ToGridKey(b);

            Vector3 direction    = (end - start).normalized;
            Vector3 perpendicular= Vector3.Cross(Vector3.up, direction).normalized;
            float halfThickness  = _wallThickness / 2f;

            WallSegment segment = CreateWallMeshBasic(start, end, perpendicular, halfThickness);

            if (!WallSegmentsByPath.ContainsKey(pathId))
                WallSegmentsByPath[pathId] = new List<WallSegment>();
            WallSegmentsByPath[pathId].Add(segment);

            StoreIntersectionData(keyA, start, direction, perpendicular, halfThickness, segment, true);
            StoreIntersectionData(keyB, end,   direction, perpendicular, halfThickness, segment, false);
        }
    }
    
    private WallSegment CreateWallMeshBasic(Vector3 start, Vector3 end, Vector3 perpendicular, float halfThickness)
    {
        Vector3 up = Vector3.up * _wallHeight;

        // 기본 직사각형 벽 생성
        Vector3 p0 = start - perpendicular * halfThickness;  // 안쪽 시작
        Vector3 p1 = start + perpendicular * halfThickness;  // 바깥쪽 시작
        Vector3 p2 = end + perpendicular * halfThickness;    // 바깥쪽 끝
        Vector3 p3 = end - perpendicular * halfThickness;    // 안쪽 끝

        Vector3 p4 = p0 + up;
        Vector3 p5 = p1 + up;
        Vector3 p6 = p2 + up;
        Vector3 p7 = p3 + up;

        var verts = new Vector3[] { p0, p1, p2, p3, p4, p5, p6, p7 };

        // 간단 UV (사각 투영): 필요 시 나중에 면별로 세분화 가능
        var uvs = new Vector2[]
        {
            new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
            new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
        };
        
        // 상단 캡만 outline용 uv2 매핑 (0~1 사각), 나머지는 비활성(-1,-1)
        var uv2 = new Vector2[]
        {
            new Vector2(-1,-1), new Vector2(-1,-1), new Vector2(-1,-1), new Vector2(-1,-1), // p0~p3 bottom ring => disabled
            new Vector2(0,0),   new Vector2(1,0),   new Vector2(1,1),   new Vector2(0,1),   // p4~p7 top ring => outline target
        };

        // 버텍스 컬러: 아래(연회색), 위(흰색)
        // 회색 농도는 취향에 따라 (0.6~0.85) 사이로 조절
        Color bottomCol = new Color(0.5f, 0.5f, 0.5f, 1f);
        Color topCol    = Color.white;

        var cols = new Color[]
        {
            bottomCol, bottomCol, bottomCol, bottomCol, // p0~p3
            topCol,    topCol,    topCol,    topCol     // p4~p7
        };

        // 면 인덱스 (한 서브메시, 머티리얼 1개 사용)
        int[] tris = new int[]
        {
            0,1,2, 2,3,0,     // bottom
            4,6,5, 4,7,6,     // top
            0,4,5, 5,1,0,     // front
            3,2,6, 6,7,3,     // back
            0,3,7, 7,4,0,     // left
            1,5,6, 6,2,1      // right
        };
        var mesh = new Mesh { name = "WallSegmentMesh" };
        mesh.SetVertices(verts);
        mesh.SetUVs(0, new List<Vector2>(uvs));
        mesh.SetUVs(1, new List<Vector2>(uv2)); // uv2 셋팅
        mesh.SetColors(new List<Color>(cols));
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject wall = new GameObject("WallSegment");
        wall.transform.SetParent(_transform, false);
        MeshFilter mf = wall.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        wall.AddComponent<MeshRenderer>().sharedMaterial = _wallMaterial;

        return new WallSegment
        {
            go = wall,
            meshFilter = mf,
            vertices = mesh.vertices,
        };
    }
    
#region Intersection 
    private void StoreIntersectionData(Vector2Int key, Vector3 point, Vector3 direction, Vector3 perpendicular, 
        float halfThickness, WallSegment segment, bool isStart)
    {
        if (_intersectionsByPoint.ContainsKey(key) == false)
            _intersectionsByPoint[key] = new List<WallIntersectionData>();
        
        _intersectionsByPoint[key].Add(new WallIntersectionData {
            Point = point,
            Direction = direction,
            Perpendicular = perpendicular,
            HalfThickness = halfThickness,
            Segment = segment,
            IsStart = isStart
        });
    }
    
    public void FixAllIntersections()
    {
        foreach (var kvp in _intersectionsByPoint)
        {
            Vector2Int pointKey = kvp.Key;
            if (_alreadyCorrectedPoints.Contains(pointKey) == true)
                continue;
            
            List<WallIntersectionData> intersections = kvp.Value;

            if (intersections.Count >= 2)
            {
                ProcessIntersection(intersections);
                _alreadyCorrectedPoints.Add(pointKey);
            }
        }
    }
    
    private void ProcessIntersection(List<WallIntersectionData> intersections)
    {
        int count = intersections.Count;

        for (int i = 0; i < count; i++)
        {
            for (int j = i + 1; j < count; j++)
            {
                var dataA = intersections[i];
                var dataB = intersections[j];

                // 1. 교차점 기준 벽이 뻗어나가는 방향 (Outward Direction)
                Vector3 outDirA = dataA.IsStart ? dataA.Direction : -dataA.Direction;
                Vector3 outDirB = dataB.IsStart ? dataB.Direction : -dataB.Direction;

                // 두 벽이 거의 평행(180도 맞은편 또는 0도 동일선상)이면 건너뜀
                if (Mathf.Abs(Vector3.Dot(outDirA, outDirB)) > 0.999f)
                    continue;

                // 2. Outward 방향 기준의 외적 수직 벡터 (오른쪽/왼쪽 정의 일치화)
                Vector3 rightA = Vector3.Cross(Vector3.up, outDirA).normalized * dataA.HalfThickness;
                Vector3 rightB = Vector3.Cross(Vector3.up, outDirB).normalized * dataB.HalfThickness;

                // 벽 A의 양 모서리 광선
                Vector3 aRightP1 = dataA.Point + rightA;
                Vector3 aRightP2 = aRightP1 + outDirA * 50f;
                Vector3 aLeftP1  = dataA.Point - rightA;
                Vector3 aLeftP2  = aLeftP1 + outDirA * 50f;

                // 벽 B의 양 모서리 광선
                Vector3 bRightP1 = dataB.Point + rightB;
                Vector3 bRightP2 = bRightP1 + outDirB * 50f;
                Vector3 bLeftP1  = dataB.Point - rightB;
                Vector3 bLeftP2  = bLeftP1 + outDirB * 50f;

                // 3. 두 벽의 회전 방향(Turn Angle) 판별
                // A -> B 로 꺾이는 방향이 시계방향인지 반시계방향인지 확인
                float crossY = Vector3.Cross(outDirA, outDirB).y;

                if (crossY > 0) 
                {
                    // 좌회전(Left turn): A의 오른쪽과 B의 왼쪽이 안쪽 코너(Inner Corner)가 됨
                    if (CalculateLineIntersection(aRightP1, aRightP2, bLeftP1, bLeftP2, out Vector3 innerPt))
                    {
                        ApplyOffsetIntersection(dataA, innerPt, useRightOffset: true);
                        ApplyOffsetIntersection(dataB, innerPt, useRightOffset: false);
                    }

                    // A의 왼쪽과 B의 오른쪽이 바깥쪽 코너(Outer Corner)
                    if (CalculateLineIntersection(aLeftP1, aLeftP2, bRightP1, bRightP2, out Vector3 outerPt))
                    {
                        ApplyOffsetIntersection(dataA, outerPt, useRightOffset: false);
                        ApplyOffsetIntersection(dataB, outerPt, useRightOffset: true);
                    }
                }
                else 
                {
                    // 우회전(Right turn): A의 왼쪽과 B의 오른쪽이 안쪽 코너(Inner Corner)가 됨
                    if (CalculateLineIntersection(aLeftP1, aLeftP2, bRightP1, bRightP2, out Vector3 innerPt))
                    {
                        ApplyOffsetIntersection(dataA, innerPt, useRightOffset: false);
                        ApplyOffsetIntersection(dataB, innerPt, useRightOffset: true);
                    }

                    // A의 오른쪽과 B의 왼쪽이 바깥쪽 코너(Outer Corner)
                    if (CalculateLineIntersection(aRightP1, aRightP2, bLeftP1, bLeftP2, out Vector3 outerPt))
                    {
                        ApplyOffsetIntersection(dataA, outerPt, useRightOffset: true);
                        ApplyOffsetIntersection(dataB, outerPt, useRightOffset: false);
                    }
                }
            }
        }
    }

    // Outward 방향 기준 Right인지 여부에 따라 버텍스 인덱스를 매핑
    private void ApplyOffsetIntersection(WallIntersectionData data, Vector3 intersection, bool useRightOffset)
    {
        Vector3 up = Vector3.up * _wallHeight;

        // CreateWallMeshBasic 기준 버텍스 정의:
        // p0: Start - Perp (Start 기준 왼쪽)
        // p1: Start + Perp (Start 기준 오른쪽)
        // p2: End + Perp   (End 기준 오른쪽)
        // p3: End - Perp   (End 기준 왼쪽)
        
        int bottomIdx;
        int topIdx;

        if (data.IsStart)
        {
            // Start점에서는 Outward 방향이 벽 진행방향(Direction)과 같음
            // 따라서 RightOffset == (+Perpendicular) == p1
            bottomIdx = useRightOffset ? 1 : 0;
            topIdx    = useRightOffset ? 5 : 4;
        }
        else
        {
            // End점에서는 Outward 방향이 벽 진행 반대방향(-Direction)임
            // 따라서 Outward의 Right는 벽 진행방향 기준 Left(-Perpendicular)인 p3이 됨
            bottomIdx = useRightOffset ? 3 : 2;
            topIdx    = useRightOffset ? 7 : 6;
        }

        data.Segment.vertices[bottomIdx] = intersection;
        data.Segment.vertices[topIdx]    = intersection + up;

        data.Segment.meshFilter.mesh.vertices = data.Segment.vertices;
        data.Segment.meshFilter.mesh.RecalculateNormals();
        data.Segment.meshFilter.mesh.RecalculateBounds();
    }
    
    private bool CalculateLineIntersection(Vector3 line1Start, Vector3 line1End, 
        Vector3 line2Start, Vector3 line2End, out Vector3 intersection)
    {
        intersection = Vector3.zero;

        // 2D 평면에서 계산 (Y축은 무시)
        Vector2 p1 = new Vector2(line1Start.x, line1Start.z);
        Vector2 p2 = new Vector2(line1End.x, line1End.z);
        Vector2 p3 = new Vector2(line2Start.x, line2Start.z);
        Vector2 p4 = new Vector2(line2End.x, line2End.z);

        Vector2 dir1 = (p2 - p1).normalized;
        Vector2 dir2 = (p4 - p3).normalized;

        // 평행선 체크
        float cross = dir1.x * dir2.y - dir1.y * dir2.x;
        if (Mathf.Abs(cross) < 0.0001f) return false;

        // 교점 계산
        Vector2 dp = p3 - p1;
        float t = (dp.x * dir2.y - dp.y * dir2.x) / cross;
        
        Vector2 intersectionPoint = p1 + dir1 * t;
        intersection = new Vector3(intersectionPoint.x, 0, intersectionPoint.y);
        
        return true;
    }
    
#endregion Intersection
   
}

public class WallIntersectionData
{
    public Vector3 Point;          // 교차점 좌표
    public Vector3 Direction;      // 벽의 방향 벡터
    public Vector3 Perpendicular;  // 수직 벡터
    public float HalfThickness;    // 벽 두께의 절반
    public WallSegment Segment;    // 해당 세그먼트
    public bool IsStart;           // true=시작점, false=끝점
}

public class WallSegment
{
    public GameObject go;
    public MeshFilter meshFilter;
    public Vector3[] vertices;
    
    public bool MatchesPath(Vector2 point1, Vector2 point2)
    {
        Vector2 start = (Vector2)vertices[0]; // 시작점
        Vector2 end = (Vector2)vertices[1];   // 끝점

        // 경로의 두 점이 Edge의 두 점과 일치하는지 확인 (순서포함)
        return (start == point1 && end == point2) || (start == point2 && end == point1);
    }
}

public struct LineKey : IEquatable<LineKey>
{
    private readonly Vector2 _a;
    private readonly Vector2 _b;

    public LineKey(Vector2 a, Vector2 b)
    {
        if (a.x < b.x || (Mathf.Approximately(a.x, b.x) && a.y < b.y))
        {
            _a = a;
            _b = b;
        }
        else
        {
            _a = b;
            _b = a;
        }
    }

    public bool Equals(LineKey other)
    {
        return _a == other._a && _b == other._b;
    }

    public override bool Equals(object obj)
    {
        return obj is LineKey other && Equals(other);
    }

    public override int GetHashCode() => _a.GetHashCode() ^ _b.GetHashCode();
}