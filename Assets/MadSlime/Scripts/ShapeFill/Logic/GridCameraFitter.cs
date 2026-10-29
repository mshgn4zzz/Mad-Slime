using System;
using System.Collections.Generic;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.ShapeFill.Logic
{
    public class GridCameraFitter : MonoBehaviour
    {
        private const float FormPadding = 1.1f;
        private const float ClipPadding = 0.01f;
        private readonly List<Vector3> _corners = new List<Vector3>();

        [SerializeField] private Camera _camera;
        [SerializeField] private Rect _formViewport = new Rect(0.08f, 0.34f, 0.84f, 0.54f);

        private Vector3 _formCenter;
        private float _distance;

        public float Distance => _distance;

        public Rect GetSafeViewport()
        {
            Rect pixels = _camera.pixelRect;
            Rect safeArea = Screen.safeArea;

            if (_camera.targetTexture != null || safeArea.width <= 0f || safeArea.height <= 0f)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            float minX = Mathf.Clamp01((safeArea.xMin - pixels.xMin) / pixels.width);
            float minY = Mathf.Clamp01((safeArea.yMin - pixels.yMin) / pixels.height);
            float maxX = Mathf.Clamp01((safeArea.xMax - pixels.xMin) / pixels.width);
            float maxY = Mathf.Clamp01((safeArea.yMax - pixels.yMin) / pixels.height);

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        public Rect GetFormViewport()
        {
            Rect safeViewport = GetSafeViewport();

            return new Rect(
                safeViewport.xMin + (_formViewport.xMin * safeViewport.width),
                safeViewport.yMin + (_formViewport.yMin * safeViewport.height),
                _formViewport.width * safeViewport.width,
                _formViewport.height * safeViewport.height);
        }

        public void Fit(GridBuilder grid)
        {
            ArgumentGuard.CheckNotNull(grid, nameof(grid));

            Bounds bounds = grid.GetFormBounds();

            FitBounds(grid.transform.position, bounds.size, grid);
        }

        private void FitBounds(Vector3 formCenter, Vector3 formSize, GridBuilder grid)
        {
            if (_camera == null)
            {
                throw new InvalidOperationException($"{name}: Fill camera is not assigned.");
            }

            if (formSize.x <= 0f || formSize.y <= 0f || formSize.z <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(formSize), "Fit requires positive world-space bounds.");
            }

            _formCenter = formCenter;
            _corners.Clear();

            void AddCube(Vector3 offset, Vector3 extents)
            {
                for (int xSign = -1; xSign <= 1; xSign += 2)
                {
                    for (int ySign = -1; ySign <= 1; ySign += 2)
                    {
                        for (int zSign = -1; zSign <= 1; zSign += 2)
                        {
                            _corners.Add(offset + Vector3.Scale(extents, new Vector3(xSign, ySign, zSign)));
                        }
                    }
                }
            }

            foreach (Vector2Int cell in grid.BorderCells)
            {
                AddCube(
                    (grid.GridToWorld(cell.x, cell.y) - formCenter) * FormPadding,
                    Vector3.one * (grid.CellSize * 0.5f * FormPadding));
            }

            ApplyFit();
        }

        private void ApplyFit()
        {
            Rect viewport = GetFormViewport();

            if (viewport.width <= 0f || viewport.height <= 0f)
            {
                throw new InvalidOperationException(
                    $"{name}: form viewport must have a positive size inside the safe area.");
            }

            Quaternion worldToCamera = Quaternion.Inverse(_camera.transform.rotation);
            float tanY = Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
            float tanX = tanY * _camera.aspect;
            float minDepth = float.PositiveInfinity;
            float maxDepth = float.NegativeInfinity;
            float lowerX = float.NegativeInfinity, upperX = float.PositiveInfinity;
            float lowerY = float.NegativeInfinity, upperY = float.PositiveInfinity;
            float minSlopeX = tanX * ((viewport.xMin * 2f) - 1f);
            float maxSlopeX = tanX * ((viewport.xMax * 2f) - 1f);
            float minSlopeY = tanY * ((viewport.yMin * 2f) - 1f);
            float maxSlopeY = tanY * ((viewport.yMax * 2f) - 1f);

            foreach (Vector3 offset in _corners)
            {
                Vector3 corner = worldToCamera * offset;

                minDepth = Mathf.Min(minDepth, corner.z);
                maxDepth = Mathf.Max(maxDepth, corner.z);
                lowerX = Mathf.Max(lowerX, corner.x - (maxSlopeX * corner.z));
                upperX = Mathf.Min(upperX, corner.x - (minSlopeX * corner.z));
                lowerY = Mathf.Max(lowerY, corner.y - (maxSlopeY * corner.z));
                upperY = Mathf.Min(upperY, corner.y - (minSlopeY * corner.z));
            }

            float distance = Mathf.Max(
                _camera.nearClipPlane + ClipPadding - minDepth,
                (lowerX - upperX) / (maxSlopeX - minSlopeX),
                (lowerY - upperY) / (maxSlopeY - minSlopeY));
            float centerX = (lowerX + upperX - ((maxSlopeX + minSlopeX) * distance)) * 0.5f;
            float centerY = (lowerY + upperY - ((maxSlopeY + minSlopeY) * distance)) * 0.5f;

            _distance = distance;

            _camera.transform.position = _formCenter - (_camera.transform.forward * distance)
                                         + (_camera.transform.right * centerX) + (_camera.transform.up * centerY);
            _camera.farClipPlane = Mathf.Max(_camera.farClipPlane, distance + maxDepth + 1f);
        }
    }
}
