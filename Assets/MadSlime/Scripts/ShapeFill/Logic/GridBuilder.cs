using System;
using System.Collections.Generic;
using UnityEngine;
namespace ShapeFill
{
    public class GridBuilder : MonoBehaviour
    {
        private const int BorderThickness = 2;

        private const float MinFillAlpha = 0.1f;

        [SerializeField] private Texture2D _shapeTexture;

        [SerializeField, Min(0.001f)] private float _cellSize = 0.6666667f;

        [SerializeField, Min(4)] private int _gridResolution = 96;

        private float _builtCellSize;
        private Bounds? _playArea;
        private Vector2 _gridCenter;

        private int _gridWidth;
        private int _gridHeight;
        private bool[,] _filledCells;
        private Color[,] _pixelColors;
        private List<Vector2Int> _borderCells = new List<Vector2Int>();
        private List<Vector2Int> _fillCells = new List<Vector2Int>();

        public IReadOnlyList<Vector2Int> BorderCells => _borderCells;
        public IReadOnlyList<Vector2Int> FillCells => _fillCells;
        public int Width => _gridWidth;
        public int Height => _gridHeight;
        public Texture2D ShapeTexture => _shapeTexture;
        public float CellSize => _builtCellSize > 0f ? _builtCellSize : _cellSize;

        public Vector3 TextureCenter => transform.TransformPoint(new Vector3(
            ((_gridWidth - 1) * 0.5f - _gridCenter.x) * CellSize,
            ((_gridHeight - 1) * 0.5f - _gridCenter.y) * CellSize, 0f));

        public void SetShapeTexture(Texture2D shapeTexture)
        {
            if (shapeTexture == null)
            {
                throw new ArgumentNullException(nameof(shapeTexture),
                    "GridBuilder.SetShapeTexture requires a non-null texture.");
            }

            _shapeTexture = shapeTexture;
        }

        public void SetPlayArea(Bounds playArea)
        {
            if (playArea.size.x <= 0f || playArea.size.z <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(playArea));
            }

            _playArea = playArea;
        }

        private static void ExpandBounds(List<Vector2Int> cells, ref Vector2 min, ref Vector2 max)
        {
            foreach (Vector2Int cell in cells)
            {
                min = Vector2.Min(min, cell);
                max = Vector2.Max(max, cell);
            }
        }

        public void Build()
        {
            _builtCellSize = _cellSize;
            ReadPixels();
            ClassifyBorder();
            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            ExpandBounds(_borderCells, ref min, ref max);
            ExpandBounds(_fillCells, ref min, ref max);
            _gridCenter = (min + max) * 0.5f;
            SortFillBottomToTop();
            FitToPlayArea();
        }

        public Vector3 GetFormSize()
        {
            return GetFormBounds().size;
        }

        // Bounds of occupied cells, including the world-aligned cube geometry, not transparent texture margins.
        public Bounds GetFormBounds()
        {
            bool hasCell = false;
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            EncapsulateCells(_borderCells, ref bounds, ref hasCell);
            EncapsulateCells(_fillCells, ref bounds, ref hasCell);
            if (hasCell == false)
            {
                throw new InvalidOperationException($"{name}: the shape has no opaque cells.");
            }

            bounds.Expand(CellSize);
            return bounds;
        }

        private void EncapsulateCells(List<Vector2Int> cells, ref Bounds bounds, ref bool hasCell)
        {
            foreach (Vector2Int cell in cells)
            {
                Vector3 position = GridToWorld(cell.x, cell.y);
                if (hasCell == false)
                {
                    bounds = new Bounds(position, Vector3.zero);
                    hasCell = true;
                }
                else
                {
                    bounds.Encapsulate(position);
                }
            }
        }

        private void FitToPlayArea()
        {
            if (_playArea.HasValue == false)
            {
                return;
            }

            Bounds area = _playArea.Value;
            Bounds form = GetFormBounds();
            // Keep the completion pulse within the floor as well as within the camera view.
            float scale = Mathf.Min(1f, area.size.x * 0.95f / (form.size.x * 1.1f),
                area.size.z * 0.95f / (form.size.z * 1.1f));
            _builtCellSize *= scale;
            form = GetFormBounds();
            Vector3 position = transform.position;
            position.x += area.center.x - form.center.x;
            position.z += area.center.z - form.center.z;
            transform.position = position;
        }

        public Vector3 GridToWorld(int gridX, int gridY)
        {
            float localX = (gridX - _gridCenter.x) * CellSize;
            float localY = (gridY - _gridCenter.y) * CellSize;

            return transform.TransformPoint(new Vector3(localX, localY, 0f));
        }

        public Color GetPixelColor(int gridX, int gridY)
        {
            if (gridX < 0 || gridX >= _gridWidth || gridY < 0 || gridY >= _gridHeight)
            {
                return Color.white;
            }

            return _pixelColors[gridX, gridY];
        }

        private void ReadPixels()
        {
            int textureWidth = _shapeTexture.width;
            int textureHeight = _shapeTexture.height;

            int longestSide = Mathf.Max(textureWidth, textureHeight);
            float ratio = Mathf.Min(_gridResolution, longestSide) / (float)longestSide;
            _gridWidth = Mathf.Max(1, Mathf.RoundToInt(textureWidth * ratio));
            _gridHeight = Mathf.Max(1, Mathf.RoundToInt(textureHeight * ratio));

            _filledCells = new bool[_gridWidth, _gridHeight];
            _pixelColors = new Color[_gridWidth, _gridHeight];

            for (int gridX = 0; gridX < _gridWidth; gridX++)
            {
                for (int gridY = 0; gridY < _gridHeight; gridY++)
                {
                    int pixelX = Mathf.Min((int)((gridX + 0.5f) * textureWidth / _gridWidth), textureWidth - 1);
                    int pixelY = Mathf.Min((int)((gridY + 0.5f) * textureHeight / _gridHeight), textureHeight - 1);
                    Color pixelColor = _shapeTexture.GetPixel(pixelX, pixelY);

                    _filledCells[gridX, gridY] = pixelColor.a > MinFillAlpha;
                    _pixelColors[gridX, gridY] = pixelColor;
                }
            }
        }

        private void ClassifyBorder()
        {
            _borderCells.Clear();
            _fillCells.Clear();

            for (int gridX = 0; gridX < _gridWidth; gridX++)
            {
                for (int gridY = 0; gridY < _gridHeight; gridY++)
                {
                    if (_filledCells[gridX, gridY] == false)
                    {
                        continue;
                    }

                    if (IsBorderCell(gridX, gridY))
                    {
                        _borderCells.Add(new Vector2Int(gridX, gridY));
                    }
                    else
                    {
                        _fillCells.Add(new Vector2Int(gridX, gridY));
                    }
                }
            }
        }

        private bool IsBorderCell(int gridX, int gridY)
        {
            if (IsOnEdge(gridX, gridY) == true)
            {
                return true;
            }

            if (HasEmptyDirectNeighbor(gridX, gridY) == true)
            {
                return true;
            }

            return HasEmptyNeighborAtDistanceTwo(gridX, gridY);
        }

        private bool IsOnEdge(int gridX, int gridY)
        {
            return gridX < BorderThickness
                || gridX >= _gridWidth - BorderThickness
                || gridY < BorderThickness
                || gridY >= _gridHeight - BorderThickness;
        }

        private bool HasEmptyDirectNeighbor(int gridX, int gridY)
        {
            return _filledCells[gridX - 1, gridY] == false
                || _filledCells[gridX + 1, gridY] == false
                || _filledCells[gridX, gridY - 1] == false
                || _filledCells[gridX, gridY + 1] == false;
        }

        private bool HasEmptyNeighborAtDistanceTwo(int gridX, int gridY)
        {
            if (_filledCells[gridX - 1, gridY] == true && _filledCells[gridX - 2, gridY] == false)
            {
                return true;
            }

            if (_filledCells[gridX + 1, gridY] == true && _filledCells[gridX + 2, gridY] == false)
            {
                return true;
            }

            if (_filledCells[gridX, gridY - 1] == true && _filledCells[gridX, gridY - 2] == false)
            {
                return true;
            }

            if (_filledCells[gridX, gridY + 1] == true && _filledCells[gridX, gridY + 2] == false)
            {
                return true;
            }

            return false;
        }

        private void SortFillBottomToTop()
        {
            _fillCells.Sort(CompareFillCells);
        }

        private static int CompareFillCells(Vector2Int left, Vector2Int right)
        {
            if (left.y != right.y)
            {
                return left.y.CompareTo(right.y);
            }

            return left.x.CompareTo(right.x);
        }
    }
}
